using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MHArmory.Core;
using MHArmory.Core.DataStructures;

namespace MHArmory.Search.Default
{
    public class EquipmentCombinationGenerator
    {
        private readonly object syncRoot = new object();
        private readonly ObjectPool<IEquipment[]> searchEquipmentsObjectPool;
        private readonly IList<IEquipment>[] allEquipments;
        private readonly int[] indexes;
        private bool isDone;

        public int CombinationCount { get; }

        public EquipmentCombinationGenerator(
            ObjectPool<IEquipment[]> searchEquipmentsObjectPool,
            IEnumerable<IEquipment> heads,
            IEnumerable<IEquipment> chests,
            IEnumerable<IEquipment> gloves,
            IEnumerable<IEquipment> waists,
            IEnumerable<IEquipment> legs,
            IEnumerable<IEquipment> charms
        )
        {
            this.searchEquipmentsObjectPool = searchEquipmentsObjectPool;

            allEquipments = new IList<IEquipment>[]
            {
                heads.ToList(),
                chests.ToList(),
                gloves.ToList(),
                waists.ToList(),
                legs.ToList(),
                charms.ToList()
            };

            indexes = new int[allEquipments.Length];

            if (allEquipments.All(x => x.Count == 0))
                CombinationCount = 0;
            else
            {
                int combinationCount = 1;
                for (int i = 0; i < allEquipments.Length; i++)
                    combinationCount *= Math.Max(allEquipments[i].Count, 1);
                CombinationCount = combinationCount;
            }
        }

        private bool Increment()
        {
            for (int i = 0; i < indexes.Length; i++)
            {
                indexes[i]++;

                if (indexes[i] < allEquipments[i].Count)
                    return true;

                indexes[i] = 0;
            }

            return false;
        }

        public IEquipment[] Next(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return null;

            lock (syncRoot)
            {
                if (isDone)
                    return null;

                IEquipment[] equipments = searchEquipmentsObjectPool.GetObject();

                for (int i = 0; i < equipments.Length; i++)
                {
                    IList<IEquipment> allCategoryEquipments = allEquipments[i];

                    if (allCategoryEquipments.Count > indexes[i])
                        equipments[i] = allCategoryEquipments[indexes[i]];
                    else
                        equipments[i] = null;
                }

                isDone = Increment() == false;

                return equipments;
            }
        }

        public IEnumerable<IEquipment[]> All(CancellationToken cancellationToken)
        {
            IEquipment[] result;

            while ((result = Next(cancellationToken)) != null)
                yield return result;
        }

        public void Reset()
        {
            for (int i = 0; i < indexes.Length; i++)
                indexes[i] = 0;

            isDone = false;
        }
    }
}
