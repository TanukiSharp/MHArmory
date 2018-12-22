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

        public event EventHandler SelectionChanged;

        private bool originalValue;

        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public void FreezeSelection()
        {
            originalValue = isSelected;
        }

        public void RestoreOriginalSelection()
        {
            IsSelected = originalValue;
        }

        public SolverDataEquipmentModel(IEquipment equipment)
        {
            Equipment = equipment;
        }

        public override string ToString()
        {
            return $"[{(IsSelected ? "O" : "X")}] {Equipment.Name}";
        }
    }
}
