﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Storage;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Combat.Helpers;
using AmeisenBotX.Core.Engines.Combat.Helpers.Aura;
using AmeisenBotX.Core.Engines.Combat.Helpers.Targets;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Managers.Character.Comparators;
using AmeisenBotX.Core.Managers.Character.Inventory.Objects;
using AmeisenBotX.Core.Managers.Character.Spells.Objects;
using AmeisenBotX.Core.Managers.Character.Talents.Objects;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Jannis
{
    public abstract class BasicCombatClass : SimpleConfigurable, ICombatClass
    {
        private readonly int[] useableHealingItems =
        [
            // potions
            118, 929, 1710, 2938, 3928, 4596, 5509, 13446, 22829, 33447,
            // healthstones
            5509, 5510, 5511, 5512, 9421, 19013, 22103, 36889, 36892,
        ];

        private readonly int[] useableManaItems =
        [
            // potions
            2245, 3385, 3827, 6149, 13443, 13444, 33448, 22832,
        ];

        protected BasicCombatClass(AmeisenBotInterfaces bot, AmeisenBotConfig config)
        {
            Bot = bot;
            Config = config;

            SpellAbortFunctions = [];

            CooldownManager = new CooldownManager(Bot.Character.SpellBook.Spells);
            ResurrectionTargets = [];

            TargetProviderDps = new TargetManager(Bot, Config, WowRole.Dps, TimeSpan.FromMilliseconds(250));
            TargetProviderTank = new TargetManager(Bot, Config, WowRole.Tank, TimeSpan.FromMilliseconds(250));
            TargetProviderHeal = new TargetManager(Bot, Config, WowRole.Heal, TimeSpan.FromMilliseconds(250));

            MyAuraManager = new AuraManager(Bot);
            TargetAuraManager = new AuraManager(Bot);
            GroupAuraManager = new GroupAuraManager(Bot);

            InterruptManager = new InterruptManager();

            EventCheckFacing = new TimegatedEvent(TimeSpan.FromMilliseconds(500));
            EventAutoAttack = new TimegatedEvent(TimeSpan.FromMilliseconds(500));

            Configurables.TryAdd("HealthItemThreshold", 30.0);
            Configurables.TryAdd("ManaItemThreshold", 30.0);
        }

        public string Author { get; } = "Jannis";

        public IEnumerable<int> BlacklistedTargetDisplayIds { get => TargetProviderDps.BlacklistedTargets; set => TargetProviderDps.BlacklistedTargets = value; }

        public CooldownManager CooldownManager { get; private set; }

        public abstract string Description { get; }

        public string DisplayName => $"[{WowVersion}] {DisplayName2}";

        public abstract string DisplayName2 { get; }

        public TimegatedEvent EventAutoAttack { get; private set; }

        public TimegatedEvent EventCheckFacing { get; set; }

        public GroupAuraManager GroupAuraManager { get; private set; }

        public bool HandlesFacing => true;

        public abstract bool HandlesMovement { get; }

        public InterruptManager InterruptManager { get; private set; }

        public abstract bool IsMelee { get; }

        public abstract IItemComparator ItemComparator { get; set; }

        public AuraManager MyAuraManager { get; private set; }

        public IEnumerable<int> PriorityTargetDisplayIds { get => TargetProviderDps.PriorityTargets; set => TargetProviderDps.PriorityTargets = value; }

        public Dictionary<string, DateTime> ResurrectionTargets { get; private set; }

        public abstract WowRole Role { get; }

        public abstract TalentTree Talents { get; }

        public AuraManager TargetAuraManager { get; private set; }

        public ITargetProvider TargetProviderDps { get; set; }

        public ITargetProvider TargetProviderHeal { get; set; }

        public ITargetProvider TargetProviderTank { get; set; }

        public abstract bool UseAutoAttacks { get; }

        public abstract string Version { get; }

        public abstract bool WalkBehindEnemy { get; }

        public abstract WowClass WowClass { get; }

        public abstract WowVersion WowVersion { get; }

        protected AmeisenBotInterfaces Bot { get; }

        protected AmeisenBotConfig Config { get; }

        protected DateTime LastSpellCast { get; private set; }

        protected List<Func<bool, bool>> SpellAbortFunctions { get; }

        private ulong CurrentCastTargetGuid { get; set; }

        public virtual bool TryToTravel()
        {
            return false;
        }

        public virtual void AttackTarget()
        {
            if (Bot.Target == null)
            {
                return;
            }

            if (Bot.Player.IsInMeleeRange(Bot.Target))
            {
                Bot.Wow.StopClickToMove();
                Bot.Movement.Reset();
                Bot.Wow.InteractWithUnit(Bot.Target);
            }
            else if (!Bot.Tactic.PreventMovement)
            {
                Bot.Movement.SetMovementAction(MovementAction.Move, Bot.Target.Position);
            }
        }

        public virtual void Execute()
        {
            if (Bot.Target != null && EventCheckFacing.Run())
            {
                CheckFacing(Bot.Target);
            }

            if (Bot.Player.IsCasting)
            {
                if (!Bot.Objects.IsTargetInLineOfSight || SpellAbortFunctions.Any(e => e(CurrentCastTargetGuid == Bot.Player.Guid)))
                {
                    Bot.Wow.StopCasting();
                }

                return;
            }

            // Update Priority Units
            // --------------------------- >
            if (Bot.Dungeon.Profile is { PriorityUnits.Count: > 0 })
            {
                TargetProviderDps.PriorityTargets = Bot.Dungeon.Profile.PriorityUnits;
            }

            // Auto-attacks
            // --------------------------- >
            if (UseAutoAttacks)
            {
                if (EventAutoAttack.Run()
                    //&& !Bot.Player.IsAutoAttacking
                    && Bot.Player.IsInMeleeRange(Bot.Target))
                {
                    Bot.Wow.StartAutoAttack();
                }
            }

            // Buffs, Debuffs, Interrupts
            // --------------------------- >
            if (MyAuraManager.Tick(Bot.Player.Auras)
                || GroupAuraManager.Tick())
            {
                return;
            }

            if (Bot.Target != null
                && TargetAuraManager.Tick(Bot.Target.Auras))
            {
                return;
            }

            if (InterruptManager.Tick(Bot.GetNearEnemies<IWowUnit>(Bot.Player.Position, IsMelee ? 5.0f : 30.0f)))
            {
                return;
            }

            // Usable items, potions, etc.
            // ---------------------------- >
            if (Bot.Player.HealthPercentage < Configurables["HealthItemThreshold"])
            {
                IWowInventoryItem healthItem = Bot.Character.Inventory.Items.FirstOrDefault(e => useableHealingItems.Contains(e.Id));

                if (healthItem != null)
                {
                    Bot.Wow.UseItemByName(healthItem.Name);
                }
            }

            if (Bot.Player.ManaPercentage < Configurables["ManaItemThreshold"])
            {
                IWowInventoryItem manaItem = Bot.Character.Inventory.Items.FirstOrDefault(e => useableManaItems.Contains(e.Id));

                if (manaItem != null)
                {
                    Bot.Wow.UseItemByName(manaItem.Name);
                }
            }

            // Race abilities
            // -------------- >
            if (Bot.Player.Race == WowRace.Human
                && (Bot.Player.IsDazed
                    || Bot.Player.IsFleeing
                    || Bot.Player.IsInfluenced
                    || Bot.Player.IsPossessed)
                && TryCastSpell("Every Man for Himself", 0))
            {
                return;
            }

            if (Bot.Player.HealthPercentage < 50.0
                && ((Bot.Player.Race == WowRace.Draenei && TryCastSpell("Gift of the Naaru", 0))
                    || (Bot.Player.Race == WowRace.Dwarf && TryCastSpell("Stoneform", 0))))
            {
                return;
            }
        }

        public virtual void OutOfCombatExecute()
        {
            if (Bot.Player.IsCasting)
            {
                if (!Bot.Objects.IsTargetInLineOfSight || SpellAbortFunctions.Any(e => e(CurrentCastTargetGuid == Bot.Player.Guid)))
                {
                    Bot.Wow.StopCasting();
                }

                return;
            }

            if ((Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Food") && Bot.Player.HealthPercentage < 100.0)
                || (Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Drink") && Bot.Player.ManaPercentage < 100.0))
            {
                return;
            }

            if (!Bot.Player.IsMounted
                && (MyAuraManager.Tick(Bot.Player.Auras) || GroupAuraManager.Tick()))
            {
                return;
            }
        }

        public override string ToString()
        {
            return $"[{WowClass}] [{Role}] {DisplayName} ({Author})";
        }

        protected bool CheckForWeaponEnchantment(WowEquipmentSlot slot, string enchantmentName, string spellToCastEnchantment)
        {
            if (Bot.Character.Equipment.Items.TryGetValue(slot, out IWowInventoryItem value))
            {
                int itemId = value.Id;

                if (itemId > 0)
                {
                    IWowItem item = Bot.Objects.All.OfType<IWowItem>().FirstOrDefault(e => e.EntryId == itemId);

                    if (item != null
                        && !item.GetEnchantmentStrings().Any(e => e.Contains(enchantmentName, StringComparison.OrdinalIgnoreCase))
                        && TryCastSpell(spellToCastEnchantment, 0, true))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected bool HandleDeadPartyMembers(string spellName)
        {
            Spell spell = Bot.Character.SpellBook.GetSpellByName(spellName);

            if (spell != null
                && spell.Costs < Bot.Player.Mana
                && !CooldownManager.IsSpellOnCooldown(spellName))
            {
                IEnumerable<IWowPlayer> groupPlayers = Bot.Objects.PartyMembers
                    .OfType<IWowPlayer>()
                    .Where(e => e.IsDead);

                if (groupPlayers.Any())
                {
                    IWowPlayer player = groupPlayers.FirstOrDefault(e => (Bot.Db.GetUnitName(e, out string name) && !ResurrectionTargets.ContainsKey(name)) || ResurrectionTargets[name] < DateTime.UtcNow);

                    if (player != null)
                    {
                        if (Bot.Db.GetUnitName(player, out string name))
                        {
                            if (!ResurrectionTargets.TryGetValue(name, out DateTime value))
                            {
                                value = DateTime.UtcNow + TimeSpan.FromSeconds(10);
                                ResurrectionTargets.Add(name, value);
                                return TryCastSpell(spellName, player.Guid, true);
                            }

                            if (value < DateTime.UtcNow)
                            {
                                return TryCastSpell(spellName, player.Guid, true);
                            }
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        protected bool IsInRange(Spell spell, IWowUnit wowUnit)
        {
            if ((spell.MinRange == 0 && spell.MaxRange == 0) || spell.MaxRange == 0)
            {
                return Bot.Player.IsInMeleeRange(wowUnit);
            }

            double distance = Bot.Player.Position.GetDistance(wowUnit.Position);
            return distance >= spell.MinRange && distance <= spell.MaxRange - 1.0;
        }

        protected bool TryCastAoeSpell(string spellName, ulong guid, bool needsResource = false, int currentResourceAmount = 0, bool forceTargetSwitch = false)
        {
            return TryCastSpell(spellName, guid, needsResource, currentResourceAmount, forceTargetSwitch)
                && CastAoeSpell(guid);
        }

        protected bool TryCastAoeSpellDk(string spellName, ulong guid, bool needsRunicPower = false, bool needsBloodrune = false, bool needsFrostrune = false, bool needsUnholyrune = false, bool forceTargetSwitch = false)
        {
            return TryCastSpellDk(spellName, guid, needsRunicPower, needsBloodrune, needsFrostrune, needsUnholyrune, forceTargetSwitch)
                && CastAoeSpell(guid);
        }

        protected bool TryCastSpell(string spellName, ulong guid, bool needsResource = false, int currentResourceAmount = 0, bool forceTargetSwitch = false, Func<bool> additionalValidation = null, Func<bool> additionalPreperation = null)
        {
            if (!Bot.Character.SpellBook.IsSpellKnown(spellName)) { return false; }

            if (ValidateTarget(guid, out IWowUnit target, out bool needToSwitchTarget))
            {
                if (currentResourceAmount == 0)
                {
                    currentResourceAmount = Bot.Player.Resource;
                }

                bool isTargetMyself = guid == 0 || guid == Bot.Player.Guid;
                Spell spell = Bot.Character.SpellBook.GetSpellByName(spellName);

                if (ValidateSpell(spell, target, currentResourceAmount, needsResource, isTargetMyself)
                    && (additionalValidation == null || additionalValidation()))
                {
                    if (additionalPreperation?.Invoke() == true)
                    {
                        return false;
                    }

                    PrepareCast(isTargetMyself, target, needToSwitchTarget || forceTargetSwitch, spell);
                    LastSpellCast = DateTime.UtcNow;
                    return CastSpell(spellName, isTargetMyself);
                }
            }

            return false;
        }

        protected bool TryCastSpellDk(string spellName, ulong guid, bool needsRunicPower = false, bool needsBloodrune = false, bool needsFrostrune = false, bool needsUnholyrune = false, bool forceTargetSwitch = false)
        {
            return TryCastSpell(spellName, guid, needsRunicPower, Bot.Player.RunicPower, forceTargetSwitch, () =>
            {
                Dictionary<int, int> runes = Bot.Wow.GetRunesReady();
                return (!needsBloodrune || runes[(int)WowRuneType.Blood] > 0 || runes[(int)WowRuneType.Death] > 0)
                    && (!needsFrostrune || runes[(int)WowRuneType.Frost] > 0 || runes[(int)WowRuneType.Death] > 0)
                    && (!needsUnholyrune || runes[(int)WowRuneType.Unholy] > 0 || runes[(int)WowRuneType.Death] > 0);
            });
        }

        protected bool TryCastSpellRogue(string spellName, ulong guid, bool needsEnergy = false, bool needsCombopoints = false, int requiredCombopoints = 1, bool forceTargetSwitch = false)
        {
            return TryCastSpell(spellName, guid, needsEnergy, Bot.Player.Energy, forceTargetSwitch, () =>
            {
                return !needsCombopoints || Bot.Player.ComboPoints >= requiredCombopoints;
            });
        }

        protected bool TryCastSpellWarrior(string spellName, string requiredStance, ulong guid, bool needsResource = false, bool forceTargetSwitch = false)
        {
            return TryCastSpell(spellName, guid, needsResource, Bot.Player.Rage, forceTargetSwitch, null, () =>
            {
                if (!Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == requiredStance)
                    && Bot.Character.SpellBook.IsSpellKnown(requiredStance)
                    && !CooldownManager.IsSpellOnCooldown(requiredStance))
                {
                    CastSpell(requiredStance, true);
                    return false;
                }

                return true;
            });
        }

        protected bool TryFindTarget(ITargetProvider targetProvider, out IEnumerable<IWowUnit> targets)
        {
            if (targetProvider.Get(out targets))
            {
                IWowUnit unit = targets.FirstOrDefault();

                if (unit != null)
                {
                    if (Bot.Player.TargetGuid == unit.Guid)
                    {
                        if (IWowUnit.IsValidAlive(Bot.Target))
                        {
                            CheckFacing(Bot.Target);
                            return true;
                        }
                    }
                    else
                    {
                        Bot.Wow.ChangeTarget(unit.Guid);
                        return false;
                    }
                }
            }

            Bot.Wow.ChangeTarget(0);
            return false;
        }

        private bool CastAoeSpell(ulong targetGuid)
        {
            if (ValidateTarget(targetGuid, out IWowUnit target, out bool _))
            {
                Bot.Wow.ClickOnTerrain(target.Position);
                LastSpellCast = DateTime.UtcNow;
                return true;
            }

            return false;
        }

        private bool CastSpell(string spellName, bool castOnSelf)
        {
            // spits out stuff like this "1;300" (1 or 0 whether the cast was successful or
            // not);(the cooldown in ms)
            if (Bot.Wow.ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:3}},{{v:4}}=GetSpellCooldown(\"{spellName}\"){{v:2}}=({{v:3}}+{{v:4}}-GetTime())*1000;if {{v:2}}<=0 then {{v:2}}=0;CastSpellByName(\"{spellName}\"{(castOnSelf ? ", \"player\"" : string.Empty)}){{v:5}},{{v:6}}=GetSpellCooldown(\"{spellName}\"){{v:1}}=({{v:5}}+{{v:6}}-GetTime())*1000;{{v:0}}=\"1;\"..{{v:1}} else {{v:0}}=\"0;\"..{{v:2}} end"), out string result))
            {
                AmeisenLogger.I.Log("CombatClass", $"[{DisplayName}]: COOLDOWN RESULT: \"{result}\"", LogLevel.Verbose);

                if (result.Length < 3)
                {
                    return false;
                }

                string[] parts = result.Split(";", StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length < 2)
                {
                    return false;
                }

                parts[1] = parts[1].Contains(',') ? parts[1].Split(',')[0] : parts[1].Split('.')[0];

                if (int.TryParse(parts[0], out int castSuccessful)
                    && int.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out int cooldown))
                {
                    if (cooldown < 0)
                    {
                        // TODO: find bug that causes negative cooldowns
                        cooldown = 1500;
                    }

                    CooldownManager.SetSpellCooldown(spellName, cooldown);

                    if (castSuccessful == 1)
                    {
                        CurrentCastTargetGuid = Bot.Target == null || castOnSelf ? Bot.Player.Guid : Bot.Target.Guid;
                        AmeisenLogger.I.Log("CombatClass", $"[{DisplayName}]: Casting Spell \"{spellName}\" on \"{(castOnSelf ? "self" : Bot.Target?.Guid)}\"", LogLevel.Verbose);
                        AmeisenLogger.I.Log("CombatClass", $"[{DisplayName}]: Spell \"{spellName}\" is on cooldown for {cooldown} ms", LogLevel.Verbose);
                        return true;
                    }
                    else
                    {
                        AmeisenLogger.I.Log("CombatClass", $"[{DisplayName}]: Unable to cast Spell \"{spellName}\" on \"{(castOnSelf ? "self" : Bot.Target?.Guid)}\"", LogLevel.Verbose);
                        AmeisenLogger.I.Log("CombatClass", $"[{DisplayName}]: Spell \"{spellName}\" is on cooldown for {cooldown} ms", LogLevel.Verbose);
                        return false;
                    }
                }
            }

            return false;
        }

        private void CheckFacing(IWowUnit target)
        {
            if (target != null
                && target.Guid != Bot.Wow.PlayerGuid
                && !BotMath.IsFacing(Bot.Player.Position, Bot.Player.Rotation, target.Position, 1.0f))
            {
                Bot.Wow.FacePosition(Bot.Player.BaseAddress, Bot.Player.Position, target.Position);
            }
        }

        private void PrepareCast(bool isTargetMyself, IWowUnit target, bool switchTarget, Spell spell)
        {
            if (!isTargetMyself && switchTarget)
            {
                Bot.Wow.ChangeTarget(target.Guid);
            }

            if (spell.CastTime > 0)
            {
                Bot.Movement.PreventMovement(spell.CastTime, PreventMovementType.SpellCast);
            }
        }

        private bool ValidateSpell(Spell spell, IWowUnit target, int resource, bool needsResource, bool isTargetMyself)
        {
            return spell != null
                && !CooldownManager.IsSpellOnCooldown(spell.Name)
                && (!needsResource || spell.Costs <= resource)
                && (isTargetMyself || IsInRange(spell, target));
        }

        private bool ValidateTarget(ulong guid, out IWowUnit target, out bool needToSwitchTargets)
        {
            if (guid == 0)
            {
                target = Bot.Player;
                needToSwitchTargets = false;
            }
            else if (guid == Bot.Target?.Guid)
            {
                target = Bot.Target;
                needToSwitchTargets = false;
            }
            else
            {
                target = Bot.GetWowObjectByGuid<IWowUnit>(guid);
                needToSwitchTargets = true;
            }

            return target != null;
        }
    }
}