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
            };

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
    }
}
