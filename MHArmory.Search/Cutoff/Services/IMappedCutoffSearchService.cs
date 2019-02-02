using System;
using System.Collections.Generic;
using System.Threading;
using MHArmory.Search.Contracts;
using MHArmory.Search.Cutoff.Models;
using MHArmory.Search.Cutoff.Models.Mapped;

namespace MHArmory.Search.Cutoff.Services
{
    internal interface IMappedCutoffSearchService
    {
        int MaxResults { get; set; }

        event Action<double> SearchProgress;

        void DepthFirstSearch(MappedCutoffSearchParameters parameters, int depth, CancellationToken cancellationToken);
        void ParallelizedDepthFirstSearch(MappedCutoffSearchParameters parameters, int depth, CancellationToken cancellationToken);
    }
}
