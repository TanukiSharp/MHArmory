using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MHArmory.Search.Contracts
{
    public interface ISolver : IExtension
    {
        event Action<double> SearchProgress;

        Task<IList<ArmorSetSearchResult>> SearchArmorSets(ISolverData solverData, CancellationToken cancellationToken);
    }
}
