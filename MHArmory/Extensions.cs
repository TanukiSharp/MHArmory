using System;
using MHArmory.Search.Default;
using MHArmory.Search.Testing;
using MHArmory.Search.Contracts;
using MHArmory.Search.Incremental;
using MHArmory.Search.Cutoff;

namespace MHArmory
{
    public static class AvailableExtensions
    {
        public static readonly ISolverData[] SolverData = new ISolverData[]
        {
            new SolverData(),
            new IncrementalSolverData(),
            new TestSolverData(),
        };

        public static readonly ISolver[] Solvers = new ISolver[]
        {
            new Solver(),
            CutoffSearch.Instance,
            new TestSolver(),
        };
    }
}
