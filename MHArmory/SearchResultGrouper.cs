using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.ViewModels;

namespace MHArmory
{
    [Flags]
    public enum SearchResultsGrouping
    {
        None = 0,
        RequiredDecorations = 1,
        Defense = 2,
        SpareSlots = 4,
        AdditionalSKills = 8,
        Resistances = 16,
    }

    public class SearchResultGrouper
    {
        public static readonly SearchResultGrouper Default = new SearchResultGrouper();

        private readonly StringBuilder builder = new StringBuilder();
        private SearchResultsGrouping grouping;

        private void AppendJewelsGroup(ArmorSetViewModel result)
        {
            string key = string.Join(
                ";",
                result.Jewels
                    .OrderBy(x => x.Jewel.Id)
                    .Select(x => $"{x.Jewel.Id},{x.Count}")
            );

            builder.Append($"{key}:");
        }

        private void AppendDefenseGroup(ArmorSetViewModel result)
        {
            int baseDef = 0;
            int maxDef = 0;
            int augDef = 0;

            for (int i = 0; i < result.ArmorPieces.Count; i++)
            {
                if (result.ArmorPieces[i] == null)
                    continue;

                baseDef += result.ArmorPieces[i].Defense.Base;
                maxDef += result.ArmorPieces[i].Defense.Max;
                augDef += result.ArmorPieces[i].Defense.Augmented;
            }

            builder.Append($"{baseDef},{maxDef},{augDef}:");
        }

        private void AppendSpareSlotsGroup(ArmorSetViewModel result)
        {
            if (result.SpareSlots != null)
                builder.Append($"{string.Join(",", result.SpareSlots)}:");
        }

        private void AppendAdditionalSkillsGroup(ArmorSetViewModel result)
        {
            string key = string.Join(
                ";",
                result.AdditionalSkills
                    .OrderBy(x => x.Skill.Id)
                    .Select(x => $"{x.Skill.Id},{x.TotalLevel}")
            );

            builder.Append($"{key}:");
        }

        private void AppendResistancesGroup(ArmorSetViewModel result)
        {
            int fire = 0;
            int water = 0;
            int thunder = 0;
            int ice = 0;
            int dragon = 0;

            for (int i = 0; i < result.ArmorPieces.Count; i++)
            {
                if (result.ArmorPieces[i] == null)
                    continue;

                fire += result.ArmorPieces[i].Resistances.Fire;
                water += result.ArmorPieces[i].Resistances.Water;
                thunder += result.ArmorPieces[i].Resistances.Thunder;
                ice += result.ArmorPieces[i].Resistances.Ice;
                dragon += result.ArmorPieces[i].Resistances.Dragon;
            }

            builder.Append($"{fire},{water},{thunder},{ice},{dragon}:");
        }

        private string CreateGroupKey(ArmorSetViewModel result)
        {
            if (grouping == SearchResultsGrouping.None)
                return null;

            builder.Clear();

            if ((grouping & SearchResultsGrouping.RequiredDecorations) == SearchResultsGrouping.RequiredDecorations)
                AppendJewelsGroup(result);

            if ((grouping & SearchResultsGrouping.Defense) == SearchResultsGrouping.Defense)
                AppendDefenseGroup(result);

            if ((grouping & SearchResultsGrouping.AdditionalSKills) == SearchResultsGrouping.AdditionalSKills)
                AppendAdditionalSkillsGroup(result);

            if ((grouping & SearchResultsGrouping.SpareSlots) == SearchResultsGrouping.SpareSlots)
                AppendSpareSlotsGroup(result);

            if ((grouping & SearchResultsGrouping.Resistances) == SearchResultsGrouping.Resistances)
                AppendResistancesGroup(result);

            return builder.ToString();
        }

        public IEnumerable<IGrouping<string, ArmorSetViewModel>> GroupBy(IEnumerable<ArmorSetViewModel> results, SearchResultsGrouping grouping)
        {
            this.grouping = grouping;
            return results.GroupBy(CreateGroupKey);
        }
    }
}
