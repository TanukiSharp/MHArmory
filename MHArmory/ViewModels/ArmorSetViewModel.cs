using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MHArmory.Core;
using MHArmory.Core.DataStructures;
using MHArmory.Search;
using MHArmory.Search.Contracts;
using MHArmory.Services;
using MHArmory.Core.WPF;
using Microsoft.Win32;

namespace MHArmory.ViewModels
{
    public class FullAbilityDescriptionViewModel : ViewModelBase
    {
        private string description;
        public string Description
        {
            get { return description; }
            private set { SetValue(ref description, value); }
        }

        private bool isActive;
        public bool IsActive
        {
            get { return isActive; }
            set { SetValue(ref isActive, value); }
        }

        private bool isOver;
        public bool IsOver
        {
            get { return isOver; }
            set { SetValue(ref isOver, value); }
        }

        private readonly int level;
        private readonly Dictionary<string, string> descriptionLocalizations;

        public FullAbilityDescriptionViewModel(int level, Dictionary<string, string> description, bool isActive, bool isOver)
        {
            this.level = level;
            descriptionLocalizations = description;

            Core.Localization.RegisterLanguageChanged(this, self =>
            {
                ((FullAbilityDescriptionViewModel)self).UpdateDescription();
            });

            UpdateDescription();
            IsActive = isActive;
            if (isActive)
                IsOver = isOver;
        }

        private void UpdateDescription()
        {
            Description = $"{level}.  {Core.Localization.Get(descriptionLocalizations)}";
        }
    }

    public class FullSkillDescriptionViewModel : ViewModelBase
    {
        private ISkill skill;

        public Dictionary<string, string> GeneralDescription { get { return skill.Description; } }
        public FullAbilityDescriptionViewModel[] Abilities { get; }

        public FullSkillDescriptionViewModel(ISkill skill, int level)
        {
            this.skill = skill;

            int clampedLevel = Math.Max(0, Math.Min(level, skill.MaxLevel));

            Abilities = new FullAbilityDescriptionViewModel[skill.Abilities.Length];
            for (int i = 0; i < skill.Abilities.Length; i++)
            {
                IAbility ability = skill.Abilities[i];

                Abilities[i] = new FullAbilityDescriptionViewModel(
                    ability.Level,
                    ability.Description,
                    ability.Level == clampedLevel,
                    level > ability.Skill.MaxLevel
                );
            }
        }

        public void UpdateLevel(int level)
        {
            int clampedLevel = Math.Max(0, Math.Min(level, skill.MaxLevel));

            for (int i = 0; i < Abilities.Length; i++)
            {
                bool isActive = skill.Abilities[i].Level == clampedLevel;
                Abilities[i].IsActive = isActive;
                if (isActive)
                    Abilities[i].IsOver = level > skill.MaxLevel;
            }
        }
    }

    public class SearchResultSkillViewModel : ViewModelBase
    {
        public ISkill Skill { get; }
        public int TotalLevel { get; }
        public bool IsExtra { get; }
        public bool IsOver { get; }

        private FullSkillDescriptionViewModel description;
        public FullSkillDescriptionViewModel Description
        {
            get
            {
                if (description == null)
                    description = new FullSkillDescriptionViewModel(Skill, TotalLevel);
                return description;
            }
        }

        public SearchResultSkillViewModel(ISkill skill, int totalLevel, bool isExtra)
        {
            Skill = skill;
            TotalLevel = totalLevel;
            IsExtra = isExtra;
            IsOver = totalLevel > skill.MaxLevel;
        }
    }

    public class ArmorSetJewelViewModel : ViewModelBase
    {
        private IJewel jewel;
        public IJewel Jewel
        {
            get { return jewel; }
            private set { SetValue(ref jewel, value); }
        }

        private int count;
        public int Count
        {
            get { return count; }
            private set { SetValue(ref count, value); }
        }

        public ArmorSetJewelViewModel(IJewel jewel, int count)
        {
            this.jewel = jewel;
            this.count = count;
        }
    }

    public class GroupedArmorSetHeaderViewModel : ViewModelBase
    {
        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set { SetValue(ref isSelected, value); }
        }

        public ICommand SelectionCommand { get; }

        public IList<ArmorSetJewelViewModel> Jewels { get; }
        public SearchResultSkillViewModel[] AdditionalSkills { get; }

        public int[] SpareSlots { get; }

        public int TotalBaseDefense { get; }
        public int TotalMaxDefense { get; }
        public int TotalAugmentedDefense { get; }

        public int TotalFireResistance { get; }
        public int TotalWaterResistance { get; }
        public int TotalThunderResistance { get; }
        public int TotalIceResistance { get; }
        public int TotalDragonResistance { get; }

        public IList<ArmorSetViewModel> Items { get; }

        public bool HasRequiredDecorations { get; }
        public bool HasDefense { get; }
        public bool HasSpareSlots { get; }
        public bool HasAdditionalSkills { get; }
        public bool HasResistances { get; }

        private readonly SearchResultsViewModel parent;

        public GroupedArmorSetHeaderViewModel(SearchResultsViewModel parent, IList<ArmorSetViewModel> items)
        {
            this.parent = parent;
            SelectionCommand = new AnonymousCommand(OnSelection);

            foreach (EnumFlagViewModel<SearchResultsGrouping> flag in parent.GroupFlags)
            {
                if (flag.IsSet == false)
                    continue;

                switch (flag.EnumValue)
                {
                    case SearchResultsGrouping.RequiredDecorations:
                        HasRequiredDecorations = true;
                        break;
                    case SearchResultsGrouping.Defense:
                        HasDefense = true;
                        break;
                    case SearchResultsGrouping.SpareSlots:
                        HasSpareSlots = true;
                        break;
                    case SearchResultsGrouping.AdditionalSKills:
                        HasAdditionalSkills = true;
                        break;
                    case SearchResultsGrouping.Resistances:
                        HasResistances = true;
                        break;
                }
            }

            ArmorSetViewModel source = items[0];

            Jewels = source.Jewels;

            AdditionalSkills = source.AdditionalSkills;

            SpareSlots = source.SpareSlots;

            TotalBaseDefense = source.TotalBaseDefense;
            TotalMaxDefense = source.TotalMaxDefense;
            TotalAugmentedDefense = source.TotalAugmentedDefense;

            TotalFireResistance = source.TotalFireResistance;
            TotalWaterResistance = source.TotalWaterResistance;
            TotalThunderResistance = source.TotalThunderResistance;
            TotalIceResistance = source.TotalIceResistance;
            TotalDragonResistance = source.TotalDragonResistance;

            Items = items;
        }

        private void OnSelection()
        {
            parent.SelectedGroup = this;
        }
    }

    public class ArmorSetViewModel : ViewModelBase
    {
        private IList<IArmorPiece> armorPieces;
        public IList<IArmorPiece> ArmorPieces
        {
            get { return armorPieces; }
            private set { SetValue(ref armorPieces, value); }
        }

        private ICharmLevel charm;
        public ICharmLevel Charm
        {
            get { return charm; }
            private set { SetValue(ref charm, value); }
        }

        private IList<ArmorSetJewelViewModel> jewels;
        public IList<ArmorSetJewelViewModel> Jewels
        {
            get { return jewels; }
            private set { SetValue(ref jewels, value); }
        }

        private SearchResultSkillViewModel[] additionalSkills;
        public SearchResultSkillViewModel[] AdditionalSkills
        {
            get
            {
                SetSkills();
                return additionalSkills;
            }
        }

        private int additionalSkillsTotalLevel;
        public int AdditionalSkillsTotalLevel
        {
            get
            {
                SetSkills();
                return additionalSkillsTotalLevel;
            }
        }

        public int[] SpareSlots { get; }

        public int SpareSlotCount { get; }

        private int spareSlotSizeSquare = -1;
        public int SpareSlotSizeSquare
        {
            get
            {
                if (spareSlotSizeSquare < 0)
                    spareSlotSizeSquare = DataUtility.SlotSizeScoreSquare(SpareSlots);
                return spareSlotSizeSquare;
            }
        }

        private int spareSlotSizeCube = -1;
        public int SpareSlotSizeCube
        {
            get
            {
                if (spareSlotSizeCube < 0)
                    spareSlotSizeCube = DataUtility.SlotSizeScoreCube(SpareSlots);
                return spareSlotSizeCube;
            }
        }

        // Works because the code that uses this is single threaded.
        private static readonly IEquipment[] reusableEquipmentArray = new IEquipment[6];

        private IList<ICraftMaterial> craftMaterials;
        public IList<ICraftMaterial> CraftMaterials
        {
            get
            {
                SetCraftMaterials();
                return craftMaterials;
            }
        }

        private void SetCraftMaterials()
        {
            if (craftMaterials != null)
                return;

            for (int i = 0; i < armorPieces.Count; i++)
                reusableEquipmentArray[i] = armorPieces[i];
            reusableEquipmentArray[5] = charm;

            craftMaterials = reusableEquipmentArray
                .Where(x => x != null)
                .SelectMany(x => x.CraftMaterials)
                .GroupBy(x => x.LocalizedItem)
                .Select(g => (ICraftMaterial)new CraftMaterial(g.Key, g.Sum(x => x.Quantity)))
                .OrderBy(x => x.LocalizedItem.Id)
                .ToList();
        }

        public int TotalRarity { get; }

        public int TotalBaseDefense { get; }
        public int TotalMaxDefense { get; }
        public int TotalAugmentedDefense { get; }

        public int TotalFireResistance { get; }
        public int TotalWaterResistance { get; }
        public int TotalThunderResistance { get; }
        public int TotalIceResistance { get; }
        public int TotalDragonResistance { get; }

        private bool isOptimal;
        public bool IsOptimal
        {
            get
            {
                SetSkills();
                return isOptimal;
            }
        }

        private ICommand saveScreenshotToClipboardCommand;
        public ICommand SaveScreenshotToClipboardCommand
        {
            get
            {
                if (saveScreenshotToClipboardCommand == null)
                    saveScreenshotToClipboardCommand = new AnonymousCommand(OnSaveScreenshotToClipboard);
                return saveScreenshotToClipboardCommand;
            }
        }

        private ICommand saveScreenshotToFileCommand;
        public ICommand SaveScreenshotToFileCommand
        {
            get
            {
                if (saveScreenshotToFileCommand == null)
                    saveScreenshotToFileCommand = new AnonymousCommand(OnSaveScreenshotToFile);
                return saveScreenshotToFileCommand;
            }
        }

        private ICommand saveTextToClipboardCommand;
        public ICommand SaveTextToClipboardCommand
        {
            get
            {
                if (saveTextToClipboardCommand == null)
                    saveTextToClipboardCommand = new AnonymousCommand(OnSaveTextToClipboard);
                return saveTextToClipboardCommand;
            }
        }

        private ICommand saveTextToFileCommand;
        public ICommand SaveTextToFileCommand
        {
            get
            {
                if (saveTextToFileCommand == null)
                    saveTextToFileCommand = new AnonymousCommand(OnSaveTextToFile);
                return saveTextToFileCommand;
            }
        }

        public IAbility[] DesiredAbilities { get; }

        public SearchResultsViewModel Parent { get { return root.SearchResultsViewModel; } }

        private readonly RootViewModel root;

        public ArmorSetViewModel(RootViewModel root, ISolverData solverData, IList<IArmorPiece> armorPieces, ICharmLevel charm, IEnumerable<ArmorSetJewelViewModel> jewels, int[] spareSlots)
        {
            this.root = root;

            if (armorPieces.Count(x => x == null) > 0)
            {
            }

            if (charm == null)
            {
            }

            this.armorPieces = armorPieces;
            this.charm = charm;
            this.jewels = jewels.OrderBy(x => x.Jewel.Id).ToList();

            DesiredAbilities = solverData.DesiredAbilities;

            SpareSlots = spareSlots;

            TotalRarity = armorPieces.Sum(x => x.Rarity);

            SpareSlotCount = SpareSlots.Count(x => x > 0);

            TotalBaseDefense = armorPieces.Sum(x => x?.Defense.Base ?? 0);
            TotalMaxDefense = armorPieces.Sum(x => x?.Defense.Max ?? 0);
            TotalAugmentedDefense = armorPieces.Sum(x => x?.Defense.Augmented ?? 0);

            TotalFireResistance = armorPieces.Sum(a => a.Resistances.Fire);
            TotalWaterResistance = armorPieces.Sum(a => a.Resistances.Water);
            TotalThunderResistance = armorPieces.Sum(a => a.Resistances.Thunder);
            TotalIceResistance = armorPieces.Sum(a => a.Resistances.Ice);
            TotalDragonResistance = armorPieces.Sum(a => a.Resistances.Dragon);
        }

        private bool areSkillsSet = false;

        private void OnSaveScreenshotToClipboard()
        {
            ISearchResultScreenshotService service = ServicesContainer.GetService<ISearchResultScreenshotService>();
            Clipboard.SetImage(service.RenderToImage(this, root.InParameters.Slots.Select(x => x.Value)));
        }

        private void OnSaveScreenshotToFile()
        {
            ISearchResultScreenshotService service = ServicesContainer.GetService<ISearchResultScreenshotService>();

            var saveFileDialog = new SaveFileDialog
            {
                CheckFileExists = false,
                CheckPathExists = true,
                Filter = "PNG Files (*.png)|*.png|All Files (*.*)|*.*",
                InitialDirectory = AppContext.BaseDirectory,
                OverwritePrompt = true,
                Title = "Save screenshot or armor set search result"
            };

            if (saveFileDialog.ShowDialog() != true)
                return;

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(service.RenderToImage(this, root.InParameters.Slots.Select(x => x.Value))));

            using (FileStream fs = File.OpenWrite(saveFileDialog.FileName))
                encoder.Save(fs);
        }

        private void OnSaveTextToClipboard()
        {
            Clipboard.SetText(MakeString());
        }

        private void OnSaveTextToFile()
        {
            var saveFileDialog = new SaveFileDialog
            {
                AddExtension = true,
                CheckFileExists = false,
                CheckPathExists = true,
                Filter = "Markdown files (*.md)|*.md|Text files (*.txt)|*.txt|All Files (*.*)|*.*",
                InitialDirectory = AppContext.BaseDirectory,
                OverwritePrompt = true,
                Title = "Save search result to text file"
            };

            if (saveFileDialog.ShowDialog() != true)
                return;

            File.WriteAllText(saveFileDialog.FileName, MakeString());
        }

        private string MakeString()
        {
            var sb = new StringBuilder();

            const string newLine = "\r\n";

            sb.Append($"**Skills**{newLine}");
            if (DesiredAbilities.Length == 0)
                sb.Append($"- *none*{newLine}");
            else
            {
                foreach (IAbility x in DesiredAbilities)
                    sb.Append($"- {Core.Localization.Get(x.Skill.Name)}: {x.Level} / {x.Skill.MaxLevel}{newLine}");
            }

            sb.Append(newLine);

            sb.Append($"**Weapon slots**{newLine}");
            if (root.InParameters.Slots.All(x => x.Value <= 0))
                sb.Append($"- *none*{newLine}");
            else
            {
                IEnumerable<string> en = root.InParameters.Slots.Where(x => x.Value > 0).Select(x => $"[{x.Value}]");
                sb.Append($"- {string.Join(" ", en)}{newLine}");
            }

            sb.Append(newLine);

            sb.Append($"**Equipments**{newLine}");
            foreach (IArmorPiece x in ArmorPieces)
                sb.Append($"- {Core.Localization.Get(x.Name)}{newLine}");
            sb.Append($"- {Core.Localization.Get(Charm.Name)}{newLine}");

            sb.Append(newLine);

            sb.Append($"**Required decorations**{newLine}");
            if (Jewels.Count == 0)
                sb.Append($"- *none*{newLine}");
            else
            {
                foreach (ArmorSetJewelViewModel x in Jewels)
                    sb.Append($"- {Core.Localization.Get(x.Jewel.Name)} [{x.Jewel.SlotSize}] x{x.Count}{newLine}");
            }

            sb.Append(newLine);

            sb.Append($"**Defenses**{newLine}");
            sb.Append($"- Base: {TotalBaseDefense}{newLine}");
            sb.Append($"- Maximum: {TotalMaxDefense}{newLine}");
            sb.Append($"- Augmented: {TotalAugmentedDefense}{newLine}");

            sb.Append(newLine);

            sb.Append($"**Spare slots**{newLine}");
            if (SpareSlots.All(x => x == 0))
                sb.Append($"- *none*{newLine}");
            else
            {
                for (int i = 2; i >= 0; i--)
                {
                    if (SpareSlots[i] > 0)
                        sb.Append($"- [{i + 1}] x{SpareSlots[i]}{newLine}");
                }
            }

            sb.Append(newLine);

            sb.Append($"**Additional skills**{newLine}");
            if (AdditionalSkills.Length == 0)
                sb.Append($"- *none*{newLine}");
            else
            {
                foreach (SearchResultSkillViewModel x in AdditionalSkills)
                {
                    // skills that are not extra ones but still in the additional skills
                    // provide more than the desired one, thus displaying in italic
                    string sides = x.IsExtra == false ? "*" : string.Empty;
                    sb.Append($"- {sides}{Core.Localization.Get(x.Skill.Name)}: {x.TotalLevel} / {x.Skill.MaxLevel}{sides}{newLine}");
                }
            }

            sb.Append(newLine);

            sb.Append($"**Resistances**{newLine}");
            sb.Append($"- Fire: {TotalFireResistance}{newLine}");
            sb.Append($"- Water: {TotalWaterResistance}{newLine}");
            sb.Append($"- Thunder: {TotalThunderResistance}{newLine}");
            sb.Append($"- Ice: {TotalIceResistance}{newLine}");
            sb.Append($"- Dragon: {TotalDragonResistance}{newLine}");

            if (Parent.ShowCraftMaterials)
            {
                sb.Append(newLine);

                sb.Append($"**Craft materials**{newLine}");
                foreach (ICraftMaterial craft in CraftMaterials)
                    sb.Append($"- {Core.Localization.Get(craft.LocalizedItem)} x{craft.Quantity}{newLine}");
            }

            return sb.ToString();
        }

        private void SetSkills()
        {
            if (areSkillsSet)
                return;

            SetSkillsInternal();

            additionalSkillsTotalLevel = additionalSkills.Sum(x => x.TotalLevel);
            isOptimal = additionalSkills.All(x => x.IsOver == false);

            areSkillsSet = true;
        }

        private void SetSkillsInternal()
        {
            var skills = new Dictionary<int, int>();

            foreach (IArmorPiece armorPiece in armorPieces.Where(x => x != null))
            {
                foreach (IAbility ability in armorPiece.Abilities)
                    IncrementSkillLevel(skills, ability);
            }

            CheckAbilitiesOnArmorSet(skills);

            if (charm != null)
            {
                foreach (IAbility ability in charm.Abilities)
                    IncrementSkillLevel(skills, ability);
            }

            foreach (ArmorSetJewelViewModel jewelViewModel in jewels)
            {
                foreach (IAbility ability in jewelViewModel.Jewel.Abilities)
                {
                    for (int i = 0; i < jewelViewModel.Count; i++)
                        IncrementSkillLevel(skills, ability);
                }
            }

            // ------------------------------------

            var localAdditionalSkills = new List<SearchResultSkillViewModel>();

            foreach (KeyValuePair<int, int> skillKeyValue in skills)
            {
                ISkill skill = GlobalData.Instance.Skills.First(s => s.Id == skillKeyValue.Key);
                int totalLevel = skillKeyValue.Value;

                IAbility foundAbility = DesiredAbilities.FirstOrDefault(a => a.Skill.Id == skill.Id);
                if (foundAbility == null)
                    localAdditionalSkills.Add(new SearchResultSkillViewModel(skill, totalLevel, true));
                else if (totalLevel > foundAbility.Level)
                    localAdditionalSkills.Add(new SearchResultSkillViewModel(skill, totalLevel, false));
            }

            additionalSkills = localAdditionalSkills.OrderBy(x => x.Skill.Id).ToArray();
        }

        private void CheckAbilitiesOnArmorSet(Dictionary<int, int> skills)
        {
            var armorSetSkillParts = new Dictionary<IArmorSetSkillPart, int>();

            foreach (IArmorPiece armorPiece in armorPieces)
            {
                if (armorPiece.ArmorSetSkills == null)
                    continue;

                foreach (IArmorSetSkillPart armorSetSkillPart in armorPiece.ArmorSetSkills.SelectMany(x => x.Parts))
                {
                    if (armorSetSkillParts.TryGetValue(armorSetSkillPart, out int value) == false)
                        value = 0;

                    armorSetSkillParts[armorSetSkillPart] = value + 1;
                }
            }

            if (armorSetSkillParts.Count > 0)
            {
                foreach (KeyValuePair<IArmorSetSkillPart, int> armorSetSkillPartKeyValue in armorSetSkillParts)
                {
                    if (armorSetSkillPartKeyValue.Value >= armorSetSkillPartKeyValue.Key.RequiredArmorPieces)
                    {
                        foreach (IAbility ability in armorSetSkillPartKeyValue.Key.GrantedSkills)
                            IncrementSkillLevel(skills, ability);
                    }
                }
            }
        }

        private void IncrementSkillLevel(IDictionary<int, int> skills, IAbility ability)
        {
            if (skills.TryGetValue(ability.Skill.Id, out int level))
                skills[ability.Skill.Id] = level + ability.Level;
            else
                skills.Add(ability.Skill.Id, ability.Level);
        }
    }
}
