using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MHArmory.Core.DataStructures;
using MHArmory.Search.Contracts;
using MHArmory.Search.Cutoff.Models;

namespace MHArmory.Search.Cutoff.Services
{
    internal class MappedCutoffSearchService : IMappedCutoffSearchService
    {
        public int MaxResults { get; set; }

        public event Action<double> SearchProgress;

        private readonly ISearchResultVerifier verifier;

        public MappedCutoffSearchService(ISearchResultVerifier verifier)
        {
            MaxResults = 100000; // Seems like a reasonable default value? Idk.

            this.verifier = verifier;
        }

        private void InvokeProgressChanged(double value)
        {
            SearchProgress?.Invoke(value);
        }

        public void ParallelizedDepthFirstSearch(MappedCutoffSearchParameters parameters, int depth, CancellationToken cancellationToken)
        {
            Parallel.ForEach(parameters.AllEquipment[depth], equipment =>
            {
                MappedCutoffSearchParameters parametersCopy = parameters;
                parametersCopy.Combination = new Combination(parameters.Combination);
                parametersCopy.Combination.Replace(depth, equipment);
                DepthFirstSearch(parametersCopy, depth + 1, cancellationToken);
            });
        }

        public void DepthFirstSearch(MappedCutoffSearchParameters parameters, int depth, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested || parameters.Results.Count >= MaxResults)
            {
                return;
            }
            if (depth == parameters.AllEquipment.Length)
            {
                bool match = verifier.TryGetSearchResult(parameters.Combination, false, out ArmorSetSearchResult result);
                parameters.Statistics.RealSearch(match);
                if (match)
                {
                    lock (parameters.Sync)
                    {
                        parameters.Results.Add(result);
                    }
                }
                return;
            }

            bool supersetMatch = verifier.TryGetSearchResult(parameters.Combination, true, out _);
            parameters.Statistics.SupersetSearch(depth, supersetMatch);
            if (!supersetMatch)
            {
                return;
            }

            InvokeProgressChanged(parameters.Statistics.GetCurrentProgress());

            int depthLength = parameters.AllEquipment[depth].Length;
            for (int i = 0; i < depthLength; i++)
            {
                parameters.Combination.Replace(depth, parameters.AllEquipment[depth][i]);
                DepthFirstSearch(parameters, depth + 1, cancellationToken);
            }
            parameters.Combination.Replace(depth, parameters.Supersets[depth]);
        }
    }
}
