using MHArmory.Core;
using MHArmory.Core.DataStructures;
using System;
using System.Collections.Generic;
using System.Text;

namespace MHArmory.Search.Contracts
{
    public class SolverDataEquipmentModel : ISolverDataEquipmentModel
    {
        public IEquipment Equipment { get; }

        public bool IsMatchingArmorSetSkill { get; set; }
        public int MatchingSkillTotalLevel { get; set; }
        public double AverageSkillCompletionRatio { get; set; }
        public bool ToBeRemoved { get; set; }

        public bool IsSelected { get; set; }

        public SolverDataEquipmentModel(IEquipment equipment)
        {
            Equipment = equipment;
        }

        public override string ToString()
        {
            return $"[{(IsSelected ? "O" : "X")}] {Localization.GetDefault(Equipment.Name)}";
        }
    }
}
