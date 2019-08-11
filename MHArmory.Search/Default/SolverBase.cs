using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MHArmory.Core;
using MHArmory.Core.DataStructures;
using MHArmory.Search.Contracts;

namespace MHArmory.Search.Default
{
    public abstract class SolverBase : ISolver
    {
        public abstract string Name { get; }
        public string Author { get; } = "TanukiSharp";
        public abstract string Description { get; }
        public abstract int Version { get; }

        public event Action<double> SearchProgress;

        private int currentCombinations;
        private double totalCombinations;

        private readonly ObjectPool<IEquipment[]> searchEquipmentsObjectPool = new ObjectPool<IEquipment[]>(() => new IEquipment[6]);

        private async Task UpdateSearchProgression(CancellationToken innerCancel, CancellationToken cancellationToken)
        {
            while (innerCancel.IsCancellationRequested == false && cancellationToken.IsCancellationRequested == false)
            {
                if (totalCombinations > 0)
                    SearchProgress?.Invoke(currentCombinations / totalCombinations);

                await Task.Delay(250);
            }
        }

        protected abstract ArmorSetSearchResult IsArmorSetMatching(IEquipment weapon, IEquipment[] equips, SolverDataJewelModel[] allJewels, IAbility[] desiredAbilities);

        private async Task<IList<ArmorSetSearchResult>> SearchArmorSetsInternal(
            ISolverData data,
            CancellationToken cancellationToken
        )
        {
            if (cancellationToken.IsCancellationRequested)
                return null;

            var heads = new List<IArmorPiece>();
            var chests = new List<IArmorPiece>();
            var gloves = new List<IArmorPiece>();
            var waists = new List<IArmorPiece>();
            var legs = new List<IArmorPiece>();
            var allCharms = new List<ICharmLevel>();

            var results = new List<ArmorSetSearchResult>();

            var generator = new EquipmentCombinationGenerator(
                searchEquipmentsObjectPool,
                data.AllHeads.Where(x => x.IsSelected).Select(x => x.Equipment),
                data.AllChests.Where(x => x.IsSelected).Select(x => x.Equipment),
                data.AllGloves.Where(x => x.IsSelected).Select(x => x.Equipment),
                data.AllWaists.Where(x => x.IsSelected).Select(x => x.Equipment),
                data.AllLegs.Where(x => x.IsSelected).Select(x => x.Equipment),
                data.AllCharms.Where(x => x.IsSelected).Select(x => x.Equipment)
            );

            long hh = data.AllHeads.Count(x => x.IsSelected);
            long cc = data.AllChests.Count(x => x.IsSelected);
            long gg = data.AllGloves.Count(x => x.IsSelected);
            long ww = data.AllWaists.Count(x => x.IsSelected);
            long ll = data.AllLegs.Count(x => x.IsSelected);
            long ch = data.AllCharms.Count(x => x.IsSelected);

            long combinationCount =
                Math.Max(hh, 1) *
                Math.Max(cc, 1) *
                Math.Max(gg, 1) *
                Math.Max(ww, 1) *
                Math.Max(ll, 1) *
                Math.Max(ch, 1);

            await Task.Yield();

            var parallelOptions = new ParallelOptions
            {
                //MaxDegreeOfParallelism = 1, // to ease debugging
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            currentCombinations = 0;
            totalCombinations = combinationCount;

            ParallelLoopResult parallelResult;

            try
            {
                OrderablePartitioner<IEquipment[]> partitioner = Partitioner.Create(generator.All(cancellationToken), EnumerablePartitionerOptions.NoBuffering);

                parallelResult = Parallel.ForEach(partitioner, parallelOptions, equips =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        searchEquipmentsObjectPool.PutObject(equips);
                        return;
                    }

                    ArmorSetSearchResult searchResult = IsArmorSetMatching(data.Weapon, equips, data.AllJewels, data.DesiredAbilities);

                    Interlocked.Increment(ref currentCombinations);

                    if (searchResult.IsMatch)
                    {
                        searchResult.ArmorPieces = new IArmorPiece[]
                        {
                            (IArmorPiece)equips[0],
                            (IArmorPiece)equips[1],
                            (IArmorPiece)equips[2],
                            (IArmorPiece)equips[3],
                            (IArmorPiece)equips[4],
                        };
                        searchResult.Charm = (ICharmLevel)equips[5];

                        lock (results)
                            results.Add(searchResult);
                    }

                    searchEquipmentsObjectPool.PutObject(equips);
                });
            }
            finally
            {
                generator.Reset();
            }

            return results;
        }

        public async Task<IList<ArmorSetSearchResult>> SearchArmorSets(ISolverData solverData, CancellationToken cancellationToken)
        {
            var innerCancellation = new CancellationTokenSource();

            Task updateTask = UpdateSearchProgression(innerCancellation.Token, cancellationToken);

            IList<ArmorSetSearchResult> result = await Task.Factory.StartNew(
                () => SearchArmorSetsInternal(solverData, cancellationToken),
                TaskCreationOptions.LongRunning
            ).Unwrap();

            innerCancellation.Cancel();

            await updateTask;

            if (cancellationToken.IsCancellationRequested == false)
                SearchProgress?.Invoke(1.0);

            return result;
        }
    }
}
