using System.Collections.Generic;
using MHArmory.Search.Contracts;
using MHArmory.Search.Cutoff.Models.Mapped;

namespace MHArmory.Search.Cutoff.Models
{
    internal struct MappedCutoffSearchParameters
    {
        public CutoffStatistics Statistics;
        public Combination Combination;
        public MappedEquipment[][] AllEquipment;
        public MappedEquipment[] Supersets;
        public IList<ArmorSetSearchResult> Results;
        public object Sync;
    }
}