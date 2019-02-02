using MHArmory.Search.Contracts;
using MHArmory.Search.Cutoff.Models;

namespace MHArmory.Search.Cutoff.Services
{
    internal interface ISearchResultVerifier
    {
        bool TryGetSearchResult(Combination combination, bool hasSuperset, out ArmorSetSearchResult result);
    }
}
