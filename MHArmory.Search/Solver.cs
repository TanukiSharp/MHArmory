using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MHArmory.Core.DataStructures;

namespace MHArmory.Search
{
    public class Solver
    {
        private readonly SolverData data;

        public Solver(SolverData data)
        {
            this.data = data;
        }

        public event Action<string> DebugData;

        public Task<IList<ArmorSetSearchResult>> SearchArmorSets()
        {
            return SearchArmorSets(CancellationToken.None);
        }

        public Task<IList<ArmorSetSearchResult>> SearchArmorSets(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                return SearchArmorSetsInternal(
                    data.DesiredAbilities,
                    cancellationToken
                );
            });
        }

        private async Task<IList<ArmorSetSearchResult>> SearchArmorSetsInternal(
            IEnumerable<IAbility> desiredAbilities,
            CancellationToken cancellationToken
        )
        {
            // ========================================================
            // above are inputs
            // ========================================================

            var sw = Stopwatch.StartNew();

            if (cancellationToken.IsCancellationRequested)
                return null;

            var allCharms = new List<ICharmLevel>();

            if (cancellationToken.IsCancellationRequested)
                return null;

            var heads = new List<IArmorPiece>();
            var chests = new List<IArmorPiece>();
            var gloves = new List<IArmorPiece>();
            var waists = new List<IArmorPiece>();
            var legs = new List<IArmorPiece>();
            //var jewels = new List<IJewel>();

            var test = new List<ArmorSetSearchResult>();


            //jewels.Sort((a, b) => b.SlotSize.CompareTo(a.SlotSize));

            //var armorPieceWeights = new Dictionary<int, int>();

            //foreach (AbilityViewModel selectedAbility in SelectedAbilities.Where(x => x.IsChecked))
            //{
            //    IArmorPiece[] matchingArmorPieces = skillsToArmorsMap[selectedAbility.SkillId];

            //    foreach (IArmorPiece armorPiece in matchingArmorPieces)
            //    {
            //        if (armorPieceWeights.TryGetValue(armorPiece.Id, out int weight) == false)
            //            armorPieceWeights.Add(armorPiece.Id, 1);
            //        else
            //            armorPieceWeights[armorPiece.Id] = weight + 1;
            //    }

            //    //IArmorPiece temp = CreateArmorPieceSorter(matchingArmorPieces.Where(x => x.Type == ArmorPieceType.Head), sortCriterias).FirstOrDefault();
            //    //if (temp != null)
            //    //    heads.Add(temp);

            //    //temp = CreateArmorPieceSorter(matchingArmorPieces.Where(x => x.Type == ArmorPieceType.Chest), sortCriterias).FirstOrDefault();
            //    //if (temp != null)
            //    //    chests.Add(temp);

            //    //temp = CreateArmorPieceSorter(matchingArmorPieces.Where(x => x.Type == ArmorPieceType.Gloves), sortCriterias).FirstOrDefault();
            //    //if (temp != null)
            //    //    gloves.Add(temp);

            //    //temp = CreateArmorPieceSorter(matchingArmorPieces.Where(x => x.Type == ArmorPieceType.Waist), sortCriterias).FirstOrDefault();
            //    //if (temp != null)
            //    //    waists.Add(temp);

            //    //temp = CreateArmorPieceSorter(matchingArmorPieces.Where(x => x.Type == ArmorPieceType.Legs), sortCriterias).FirstOrDefault();
            //    //if (temp != null)
            //    //    legs.Add(temp);

            //    //if (skillsToCharmsMap.TryGetValue(selectedAbility.SkillId, out ICharm[] matchingCharms))
            //    //    charms.AddRange(matchingCharms.SelectMany(x => x.Levels));

            //    //if (skillsToJewelsMap.TryGetValue(selectedAbility.SkillId, out IJewel[] matchingJewels))
            //    //    jewels.AddRange(matchingJewels);
            //}

            IEquipment[] equipments = new IEquipment[6];

            var equipmentsList = new List<IEquipment[]>();

            foreach (IEquipment h in data.AllHeads.Where(x => x.IsSelected).Select(x => x.Equipment))
            {
                if (cancellationToken.IsCancellationRequested)
                    return null;

                equipments[0] = h;
                foreach (IEquipment c in data.AllChests.Where(x => x.IsSelected).Select(x => x.Equipment))
                {
                    if (cancellationToken.IsCancellationRequested)
                        return null;

                    equipments[1] = c;
                    foreach (IEquipment g in data.AllGloves.Where(x => x.IsSelected).Select(x => x.Equipment))
                    {
                        if (cancellationToken.IsCancellationRequested)
                            return null;

                        equipments[2] = g;
                        foreach (IEquipment w in data.AllWaists.Where(x => x.IsSelected).Select(x => x.Equipment))
                        {
                            if (cancellationToken.IsCancellationRequested)
                                return null;

                            equipments[3] = w;
                            foreach (IEquipment l in data.AllLegs.Where(x => x.IsSelected).Select(x => x.Equipment))
                            {
                                if (cancellationToken.IsCancellationRequested)
                                    return null;

                                equipments[4] = l;
                                foreach (IEquipment ch_ in data.AllCharms.Where(x => x.IsSelected).Select(x => x.Equipment))
                                {
                                    if (cancellationToken.IsCancellationRequested)
                                        return null;

                                    equipments[5] = ch_;
                                    equipmentsList.Add(equipments.ToArray());
                                }
                            }
                        }
                    }
                }
            }

            var sb = new StringBuilder();

            long hh = data.AllHeads.Count;
            long cc = data.AllChests.Count;
            long gg = data.AllGloves.Count;
            long ww = data.AllWaists.Count;
            long ll = data.AllLegs.Count;
            long ch = data.AllCharms.Count;

            var nfi = new System.Globalization.NumberFormatInfo();
            nfi.NumberGroupSeparator = "'";

            sb.AppendLine($"Heads count:  {hh}");
            sb.AppendLine($"Chests count: {cc}");
            sb.AppendLine($"Gloves count: {gg}");
            sb.AppendLine($"Waists count: {ww}");
            sb.AppendLine($"Legs count:   {ll}");
            sb.AppendLine($"Charms count:   {ch}");
            sb.AppendLine($"Combination count: {equipmentsList.Count.ToString("N0", nfi)}");
            sb.AppendLine($"Took: {sw.ElapsedMilliseconds.ToString("N0", nfi)} ms");

            DebugData?.Invoke(sb.ToString());

            await Task.Yield();

            ParallelOptions parallelOptions = new ParallelOptions
            {
                CancellationToken = cancellationToken,
                //MaxDegreeOfParallelism = 1, // to ease debugging
            };

            try
            {
                ParallelLoopResult parallelResult = Parallel.ForEach(equipmentsList, parallelOptions, equips =>
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    ArmorSetSearchResult searchResult = IsArmorSetMatching(data.WeaponSlots, equips, data.AllJewels, desiredAbilities);

                    searchResult.ArmorPieces = equips.Take(5).Cast<IArmorPiece>().ToList();
                    searchResult.Charm = (ICharmLevel)equips[5];

                    if (searchResult.IsMatch)
                    {
                        lock (test)
                        {
                            test.Add(searchResult);
                        }
                    }
                });
            }
            catch (OperationCanceledException)
            {
                return null;
            }

            //var indicesTruthTable = new IndicesTruthTable(6);
            //int[] output = new int[indicesTruthTable.IndicesCount];
            //int iterationsPerIndex = (int)Math.Pow(2.0, indicesTruthTable.IndicesCount);
            //int iterations = iterationsPerIndex * 100;


            //for (int i = 0; i < iterations; i++)
            //{
            //    indicesTruthTable.Next(output);

            //    //var foundArmorPieces = new List<IArmorPiece>();

            //    //if (heads.Count > 0)
            //    //    foundArmorPieces.Add(heads[output[0] % heads.Count]);
            //    //if (chests.Count > 0)
            //    //    foundArmorPieces.Add(chests[output[1] % chests.Count]);
            //    //if (gloves.Count > 0)
            //    //    foundArmorPieces.Add(gloves[output[2] % gloves.Count]);
            //    //if (waists.Count > 0)
            //    //    foundArmorPieces.Add(waists[output[3] % waists.Count]);
            //    //if (legs.Count > 0)
            //    //    foundArmorPieces.Add(legs[output[4] % legs.Count]);

            //    if (heads.Count > 0)
            //        equipments[0] = heads[output[0] % heads.Count];
            //    else
            //        equipments[0] = null;

            //    if (chests.Count > 0)
            //        equipments[1] = chests[output[1] % chests.Count];
            //    else
            //        equipments[1] = null;

            //    if (gloves.Count > 0)
            //        equipments[2] = gloves[output[2] % gloves.Count];
            //    else
            //        equipments[2] = null;

            //    if (waists.Count > 0)
            //        equipments[3] = waists[output[3] % waists.Count];
            //    else
            //        equipments[3] = null;

            //    if (legs.Count > 0)
            //        equipments[4] = legs[output[4] % legs.Count];
            //    else
            //        equipments[4] = null;

            //    if (charms.Count > 0)
            //        equipments[5] = charms[output[5] % charms.Count];
            //    else
            //        equipments[5] = null;

            //    if (IsArmorSetMatching(weaponSlots, equipments, selectedAbilities, skillsToJewelsMap))
            //    {
            //        test.Add(new ArmorSetViewModel
            //        {
            //            ArmorPieces = equipments.OfType<IArmorPiece>().ToList()
            //        });
            //    }
            //}

            sw.Stop();

            sb.AppendLine($"Matching result: {test.Count.ToString("N0", nfi)}");
            sb.AppendLine($"Took: {sw.ElapsedMilliseconds.ToString("N0", nfi)} ms");

            DebugData?.Invoke(sb.ToString());

            return test;
        }

        private ArmorSetSearchResult IsArmorSetMatching(
            int[] weaponSlots, IEquipment[] equipments,
            IList<IJewel> matchingJewels,
            IEnumerable<IAbility> desiredAbilities
        )
        {
            bool isOptimal = true;
            var requiredJewels = new List<ArmorSetJewelResult>();

            if (
                equipments.Where(x => x != null).Any(x => x.Name == "Bazel Helm Beta") &&
                equipments.Where(x => x != null).Any(x => x.Name == "Kushala Cista Beta") &&
                equipments.Where(x => x != null).Any(x => x.Name == "High Metal Braces Beta") &&
                equipments.Where(x => x != null).Any(x => x.Name == "Bazel Coil Beta") &&
                equipments.Where(x => x != null).Any(x => x.Name == "Death Stench Heel Beta") &&
                equipments.Where(x => x != null).Any(x => x.Name == "Attack Charm 3")
            )
            {
            }

            int[] availableSlots = new int[3];

            if (weaponSlots != null)
            {
                foreach (int slotSize in weaponSlots)
                {
                    if (slotSize > 0)
                        availableSlots[slotSize - 1]++;
                }
            }

            foreach (IEquipment equipment in equipments)
            {
                if (equipment == null)
                    continue;

                foreach (int slotSize in equipment.Slots)
                    availableSlots[slotSize - 1]++;
            }

            foreach (IAbility ability in desiredAbilities)
            {
                int armorAbilityTotal = 0;

                foreach (IEquipment equipment in equipments)
                {
                    if (equipment != null)
                    {
                        foreach (IAbility a in equipment.Abilities)
                        {
                            if (a.Skill.Id == ability.Skill.Id)
                                armorAbilityTotal += a.Level;
                        }
                    }
                }

                int remaingAbilityLevels = ability.Level - armorAbilityTotal;

                if (remaingAbilityLevels > 0)
                {
                    if (availableSlots.All(x => x <= 0))
                        return new ArmorSetSearchResult { IsMatch = false };

                    foreach (IJewel j in matchingJewels)
                    {
                        // bold assumption, will be fucked if they decide to create jewels with multiple skills
                        IAbility a = j.Abilities[0];

                        if (a.Skill.Id == ability.Skill.Id)
                        {
                            int requiredJewelCount = remaingAbilityLevels / a.Level;

                            if (ConsumeSlots(availableSlots, j.SlotSize, requiredJewelCount) == false)
                                return new ArmorSetSearchResult { IsMatch = false };

                            remaingAbilityLevels -= requiredJewelCount * a.Level;

                            requiredJewels.Add(new ArmorSetJewelResult { Jewel = j, Count = requiredJewelCount });

                            break;
                        }
                    }

                    if (remaingAbilityLevels > 0)
                        return new ArmorSetSearchResult { IsMatch = false };
                }

                if (remaingAbilityLevels < 0)
                    isOptimal = false;
            }

            return new ArmorSetSearchResult
            {
                IsMatch = true,
                IsOptimal = isOptimal,
                Jewels = requiredJewels
            };
        }

        private bool ConsumeSlots(int[] availableSlots, int jewelSize, int jewelCount)
        {
            for (int i = jewelSize - 1; i < availableSlots.Length; i++)
            {
                if (availableSlots[i] <= 0)
                    continue;

                if (availableSlots[i] >= jewelCount)
                {
                    availableSlots[i] -= jewelCount;
                    return true;
                }
                else
                {
                    jewelCount -= availableSlots[i];
                    availableSlots[i] = 0;
                }
            }

            return jewelCount <= 0;
        }
    }
}
