﻿using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.StateMachine.Enums;
using AmeisenBotX.Core.StateMachine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.StateMachine.CombatClasses.Jannis
{
    public class PriestDiscipline : ICombatClass
    {
        // author: Jannis Höschele

        private readonly string bindingHealSpell = "Binding Heal";
        private readonly string flashHealSpell = "Flash Heal";
        private readonly string greaterHealSpell = "Greater Heal";
        private readonly string hymnOfHopeSpell = "Hymn of Hope";
        private readonly string innerFireSpell = "Inner Fire";
        private readonly string desperatePrayerSpell = "Desperate Prayer";
        private readonly string powerWordFortitudeSpell = "Power Word: Fortitude";
        private readonly string powerWordShieldSpell = "Power Word: Shield";
        private readonly string prayerOfHealingSpell = "Prayer of Healing";
        private readonly string prayerOfMendingSpell = "Prayer of Mending";
        private readonly string renewSpell = "Renew";
        private readonly string weakenedSoulSpell = "Weakened Soul";
        private readonly string penanceSpell = "Penance";
        private readonly string resurrectionSpell = "Resurrection";

        private readonly int buffCheckTime = 8;
        private readonly int deadPartymembersCheckTime = 4;

        public PriestDiscipline(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager)
        {
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;
            CooldownManager = new CooldownManager(characterManager.SpellBook.Spells);

            SpellUsageHealDict = new Dictionary<int, string>()
            {
                { 0, flashHealSpell },
                { 3000, penanceSpell },
                { 5000, greaterHealSpell },
            };

            Spells = new Dictionary<string, Spell>();
            CharacterManager.SpellBook.OnSpellBookUpdate += () =>
            {
                Spells.Clear();
                foreach (Spell spell in CharacterManager.SpellBook.Spells)
                {
                    Spells.Add(spell.Name, spell);
                }
            };
        }

        public bool HandlesMovement => false;

        public bool HandlesTargetSelection => true;

        public bool IsMelee => false;

        public IWowItemComparator ItemComparator { get; } = new BasicSpiritComparator();

        private CharacterManager CharacterManager { get; }

        private HookManager HookManager { get; }

        private DateTime LastBuffCheck { get; set; }

        private DateTime LastDeadPartymembersCheck { get; set; }

        private ObjectManager ObjectManager { get; }

        private CooldownManager CooldownManager { get; }

        private Dictionary<string, Spell> Spells { get; }

        private Dictionary<int, string> SpellUsageHealDict { get; }

        public string Displayname => "Priest Discipline";

        public string Version => "1.0";

        public string Author => "Jannis";

        public string Description => "FCFS based CombatClass for the Discipline Priest spec.";

        public WowClass Class => WowClass.Priest;

        public CombatClassRole Role => CombatClassRole.Heal;

        public Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public void Execute()
        {
            // we dont want to do anything if we are casting something...
            if (ObjectManager.Player.IsCasting)
            {
                return;
            }

            if (NeedToHealSomeone(out List<WowPlayer> playersThatNeedHealing))
            {
                HandleTargetSelection(playersThatNeedHealing);
                ObjectManager.UpdateObject(ObjectManager.Player.Type, ObjectManager.Player.BaseAddress);

                WowUnit target = ObjectManager.Target;
                if (target != null)
                {
                    ObjectManager.UpdateObject(target.Type, target.BaseAddress);

                    if (playersThatNeedHealing.Count > 4
                        && CastSpellIfPossible(prayerOfHealingSpell, true))
                    {
                        return;
                    }

                    if (target.Guid != ObjectManager.PlayerGuid
                        && target.HealthPercentage < 70
                        && ObjectManager.Player.HealthPercentage < 70
                        && CastSpellIfPossible(bindingHealSpell, true))
                    {
                        return;
                    }

                    if (ObjectManager.Player.ManaPercentage < 50
                        && CastSpellIfPossible(hymnOfHopeSpell))
                    {
                        return;
                    }

                    if (ObjectManager.Player.HealthPercentage < 20
                        && CastSpellIfPossible(desperatePrayerSpell))
                    {
                        return;
                    }

                    List<string> targetBuffs = HookManager.GetAuras(WowLuaUnit.Target);

                    if ((target.HealthPercentage < 85
                            && !targetBuffs.Any(e => e.Equals(weakenedSoulSpell, StringComparison.OrdinalIgnoreCase))
                            && !targetBuffs.Any(e => e.Equals(powerWordShieldSpell, StringComparison.OrdinalIgnoreCase))
                            && CastSpellIfPossible(powerWordShieldSpell, true))
                        || (target.HealthPercentage < 80
                            && !targetBuffs.Any(e => e.Equals(renewSpell, StringComparison.OrdinalIgnoreCase))
                            && CastSpellIfPossible(renewSpell, true)))
                    {
                        return;
                    }

                    double healthDifference = target.MaxHealth - target.Health;
                    List<KeyValuePair<int, string>> spellsToTry = SpellUsageHealDict.Where(e => e.Key <= healthDifference).ToList();

                    foreach (KeyValuePair<int, string> keyValuePair in spellsToTry.OrderByDescending(e => e.Value))
                    {
                        if (CastSpellIfPossible(keyValuePair.Value, true))
                        {
                            return;
                        }
                    }
                }
                else
                {
                    if (DateTime.Now - LastBuffCheck > TimeSpan.FromSeconds(buffCheckTime)
                        && HandleBuffing())
                    {
                        return;
                    }
                }
            }
        }

        public void OutOfCombatExecute()
        {
            if (DateTime.Now - LastBuffCheck > TimeSpan.FromSeconds(buffCheckTime)
                && HandleBuffing())
            {
                return;
            }

            if (DateTime.Now - LastDeadPartymembersCheck > TimeSpan.FromSeconds(deadPartymembersCheckTime)
                && HandleDeadPartymembers())
            {
                return;
            }
        }

        private bool HandleBuffing()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);
            if (!ObjectManager.Player.IsInCombat)
            {
                HookManager.TargetGuid(ObjectManager.PlayerGuid);
            }

            if ((!myBuffs.Any(e => e.Equals(powerWordFortitudeSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(powerWordFortitudeSpell, true))
                || (!myBuffs.Any(e => e.Equals(innerFireSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(innerFireSpell, true)))
            {
                return true;
            }

            LastBuffCheck = DateTime.Now;
            return false;
        }

        private bool HandleDeadPartymembers()
        {
            if (!Spells.ContainsKey(resurrectionSpell))
            {
                Spells.Add(resurrectionSpell, CharacterManager.SpellBook.GetSpellByName(resurrectionSpell));
            }

            if (Spells[resurrectionSpell] != null
                && !CooldownManager.IsSpellOnCooldown(resurrectionSpell)
                && Spells[resurrectionSpell].Costs < ObjectManager.Player.Mana)
            {
                IEnumerable<WowPlayer> players = ObjectManager.WowObjects.OfType<WowPlayer>();
                List<WowPlayer> groupPlayers = players.Where(e => e.IsDead && e.Health == 0 && ObjectManager.PartymemberGuids.Contains(e.Guid)).ToList();

                if (groupPlayers.Count > 0)
                {
                    HookManager.TargetGuid(groupPlayers.First().Guid);
                    HookManager.CastSpell(resurrectionSpell);
                    CooldownManager.SetSpellCooldown(resurrectionSpell, (int)HookManager.GetSpellCooldown(resurrectionSpell));
                    return true;
                }
            }

            return false;
        }

        private void HandleTargetSelection(List<WowPlayer> possibleTargets)
        {
            // select the one with lowest hp
            HookManager.TargetGuid(possibleTargets.OrderBy(e => e.HealthPercentage).First().Guid);
        }

        private bool CastSpellIfPossible(string spellName, bool needsMana = false)
        {
            if (!Spells.ContainsKey(spellName))
            {
                Spells.Add(spellName, CharacterManager.SpellBook.GetSpellByName(spellName));
            }

            if (Spells[spellName] != null
                && !CooldownManager.IsSpellOnCooldown(spellName)
                && (!needsMana || Spells[spellName].Costs < ObjectManager.Player.Mana))
            {
                HookManager.CastSpell(spellName);
                CooldownManager.SetSpellCooldown(spellName, (int)HookManager.GetSpellCooldown(spellName));
                return true;
            }

            return false;
        }

        private bool NeedToHealSomeone(out List<WowPlayer> playersThatNeedHealing)
        {
            IEnumerable<WowPlayer> players = ObjectManager.WowObjects.OfType<WowPlayer>();
            List<WowPlayer> groupPlayers = players.Where(e => !e.IsDead && e.Health > 1 && ObjectManager.PartymemberGuids.Contains(e.Guid) && e.Position.GetDistance2D(ObjectManager.Player.Position) < 35).ToList();

            groupPlayers.Add(ObjectManager.Player);

            playersThatNeedHealing = groupPlayers.Where(e => e.HealthPercentage < 90).ToList();

            return playersThatNeedHealing.Count > 0;
        }
    }
}