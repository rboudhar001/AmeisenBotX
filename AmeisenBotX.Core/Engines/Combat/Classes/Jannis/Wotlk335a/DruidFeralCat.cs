﻿using AmeisenBotX.Core.Engines.Combat.Helpers.Aura.Objects;
using AmeisenBotX.Core.Managers.Character.Comparators;
using AmeisenBotX.Core.Managers.Character.Talents.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow335a.Constants;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Jannis.Wotlk335a
{
    public class DruidFeralCat : BasicCombatClass
    {
        public DruidFeralCat(AmeisenBotInterfaces bot, AmeisenBotConfig config) : base(bot, config)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Druid335a.MarkOfTheWild, () => TryCastSpell(Druid335a.MarkOfTheWild, Bot.Wow.PlayerGuid, true, 0, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Druid335a.CatForm, () => TryCastSpell(Druid335a.CatForm, 0, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Druid335a.SavageRoar, () => TryCastSpellRogue(Druid335a.SavageRoar, Bot.Wow.TargetGuid, true, true, 1)));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Druid335a.Rip, () => Bot.Player.ComboPoints == 5 && TryCastSpellRogue(Druid335a.Rip, Bot.Wow.TargetGuid, true, true, 5)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Druid335a.Rake, () => TryCastSpell(Druid335a.Rake, Bot.Wow.TargetGuid, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Druid335a.MangleCat, () => TryCastSpell(Druid335a.MangleCat, Bot.Wow.TargetGuid, true)));

            InterruptManager.InterruptSpells = new()
            {
                { 0, (x) => TryCastSpell(Druid335a.FaerieFire, x.Guid, true) },
            };

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((Druid335a.MarkOfTheWild, (spellName, guid) => TryCastSpell(spellName, guid, true)));
        }

        public override string Description => "FCFS based CombatClass for the Feral (Cat) Druid spec.";

        public override string DisplayName2 => "Druid Feral Cat";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IItemComparator ItemComparator { get; set; } = new BasicAgilityComparator([WowArmorType.Shield], [WowWeaponType.Sword, WowWeaponType.Mace, WowWeaponType.Axe]);

        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = [],
            Tree2 = new()
            {
                { 1, new(2, 1, 5) },
                { 2, new(2, 2, 5) },
                { 4, new(2, 4, 2) },
                { 6, new(2, 6, 2) },
                { 7, new(2, 7, 1) },
                { 8, new(2, 8, 3) },
                { 9, new(2, 9, 2) },
                { 10, new(2, 10, 3) },
                { 11, new(2, 11, 2) },
                { 12, new(2, 12, 2) },
                { 14, new(2, 14, 1) },
                { 17, new(2, 17, 5) },
                { 18, new(2, 18, 3) },
                { 19, new(2, 19, 1) },
                { 20, new(2, 20, 2) },
                { 23, new(2, 23, 3) },
                { 25, new(2, 25, 3) },
                { 26, new(2, 26, 1) },
                { 28, new(2, 28, 5) },
                { 29, new(2, 29, 1) },
                { 30, new(2, 30, 1) },
            },
            Tree3 = new()
            {
                { 1, new(3, 1, 2) },
                { 3, new(3, 3, 5) },
                { 4, new(3, 4, 5) },
                { 6, new(3, 6, 3) },
                { 8, new(3, 8, 1) },
                { 9, new(3, 9, 2) },
            },
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => true;

        public override WowClass WowClass => WowClass.Druid;

        public override WowVersion WowVersion => WowVersion.WotLK335a;

        public override void Execute()
        {
            base.Execute();

            if (TryFindTarget(TargetProviderDps, out _))
            {
                double distanceToTarget = Bot.Player.Position.GetDistance(Bot.Target.Position);

                if (distanceToTarget > 9.0
                    && TryCastSpell(Druid335a.FeralChargeBear, Bot.Wow.TargetGuid, true))
                {
                    return;
                }

                if (distanceToTarget > 8.0
                    && TryCastSpell(Druid335a.Dash, 0))
                {
                    return;
                }

                if (Bot.Player.HealthPercentage < 40
                    && TryCastSpell(Druid335a.SurvivalInstincts, 0, true))
                {
                    return;
                }

                if (TryCastSpell(Druid335a.Berserk, 0))
                {
                    return;
                }

                if (NeedToHealMySelf())
                {
                    return;
                }

                if ((Bot.Player.EnergyPercentage > 70
                        && TryCastSpell(Druid335a.Berserk, 0))
                    || (Bot.Player.Energy < 30
                        && TryCastSpell(Druid335a.TigersFury, 0))
                    || (Bot.Player.HealthPercentage < 70
                        && TryCastSpell(Druid335a.Barkskin, 0, true))
                    || (Bot.Player.HealthPercentage < 35
                        && TryCastSpell(Druid335a.SurvivalInstincts, 0, true))
                    || (Bot.Player.ComboPoints == 5
                        && TryCastSpellRogue(Druid335a.FerociousBite, Bot.Wow.TargetGuid, true, true, 5))
                    || TryCastSpell(Druid335a.Shred, Bot.Wow.TargetGuid, true))
                {
                    return;
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            base.OutOfCombatExecute();

            if (NeedToHealMySelf())
            {
                return;
            }
        }

        private bool NeedToHealMySelf()
        {
            return Bot.Player.HealthPercentage < 60
                && !Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == Druid335a.Rejuvenation)
                && TryCastSpell(Druid335a.Rejuvenation, 0, true)
|| Bot.Player.HealthPercentage < 40
                && TryCastSpell(Druid335a.HealingTouch, 0, true);
        }
    }
}