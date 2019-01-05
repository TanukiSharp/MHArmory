using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHArmory.Configurations.Conversion
{
    public class ConverterV2ToV3 : IConverter
    {
        public int SourceVersion { get { return 2; } }
        public int TargetVersion { get { return 3; } }

        public object Convert(object input)
        {
            var inputConfig = input as ConfigurationV2;

            if (inputConfig == null)
                return null;

            var result = new ConfigurationV3
            {
                AcknowledgedVersion = inputConfig.AcknowledgedVersion,
                Version = 3,
                BackupLocations = inputConfig.BackupLocations,
                LastOpenedLoadout = inputConfig.LastOpenedLoadout,
                Language = inputConfig.Language,
            };

            result.SearchResultProcessing.ActiveSortingIndex = inputConfig.SearchResultProcessing.ActiveSortingIndex;
            result.SearchResultProcessing.Sorting = inputConfig.SearchResultProcessing.Sorting;

            CopyInParameters(inputConfig.InParameters, result.InParameters);

            foreach (KeyValuePair<string, WindowConfiguration> x in inputConfig.Windows)
                result.Windows[x.Key] = x.Value;

            foreach (KeyValuePair<string, SkillLoadoutItemConfigurationV2[]> x in inputConfig.SkillLoadouts)
            {
                result.SkillLoadouts[x.Key] = new SkillLoadoutItemConfigurationV3
                {
                    WeaponSlots = new int[0],
                    Skills = x.Value
                };
            }

            return result;
        }

        private void CopyInParameters(InParametersConfigurationV2 source, InParametersConfigurationV2 target)
        {
            target.DecorationOverride.Items.Clear();
            foreach (KeyValuePair<string, DecorationOverrideConfigurationItem> x in source.DecorationOverride.Items)
                target.DecorationOverride.Items[x.Key] = x.Value;
            target.DecorationOverride.UseOverride = source.DecorationOverride.UseOverride;

            target.EquipmentOverride.Items.Clear();
            foreach (string x in source.EquipmentOverride.Items)
                target.EquipmentOverride.Items.Add(x);
            target.EquipmentOverride.IsStoringPossessed = source.EquipmentOverride.IsStoringPossessed;
            target.EquipmentOverride.UseOverride = source.EquipmentOverride.UseOverride;

            target.Gender = source.Gender;

            target.Rarity = source.Rarity;

            target.WeaponSlots = source.WeaponSlots;
        }
    }
}
