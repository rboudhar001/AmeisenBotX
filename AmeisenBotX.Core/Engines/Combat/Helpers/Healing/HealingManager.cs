﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Combat.Helpers.Healing.Enums;
using AmeisenBotX.Core.Managers.Character.Spells.Objects;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Wow.Objects;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Healing
{
    public class HealingManager
    {
        /// <summary>
        /// Create a new instance of the HealingManager that is used to choose healing spell in a
        /// smart way. It observes the heals done by the bot and remembers how much each spell
        /// healed. Based of that knowledge it is able to cancel spells to prevent over-heal and
        /// choose fast heals when the target is going to die in a few seconds.
        /// </summary>
        /// <param name="bot">AmeisenBotInterfaces collection</param>
        /// <param name="tryCastSpellAction">Function to cast a spell</param>
        /// <param name="damageMonitorSeconds">
        /// How many seconds to use for the incoming damage simulation
        /// </param>
        /// <param name="healthWeight">
        /// How much weight should the health of a target have on its priority
        /// </param>
        /// <param name="incomingDamageWeight">
        /// How much weight should the incoming damage have on its priority
        /// </param>
        /// <param name="targetDyingSeconds">
        /// How many seconds should we try to simulate damage to recognized dying targets
        /// </param>
        /// <param name="overhealingStopThreshold">
        /// How much percent of a spell needs to be overhealing to cancel it (0.0f - 1.0f)
        /// </param>
        /// <param name="overhealingStopThreshold">
        /// How much percent of a spell is allowed to be overheal (0.0f - 1.0f)
        /// </param>
        public HealingManager
        (
            AmeisenBotInterfaces bot,
            Func<string, ulong, bool> tryCastSpellAction,
            int damageMonitorSeconds = 10,
            float healthWeight = 0.7f,
            float incomingDamageWeight = 0.3f,
            int targetDyingSeconds = 4,
            float overhealingStopThreshold = 0.8f,
            float maxOverheal = 0.75f
        )
        {
            Bot = bot;
            TryCastSpellAction = tryCastSpellAction;

            // configureables
            DamageMonitorSeconds = damageMonitorSeconds;
            HealthWeightMod = healthWeight;
            IncomingDamageMod = incomingDamageWeight;
            TargetDyingSeconds = targetDyingSeconds;
            OverhealingStopThreshold = overhealingStopThreshold;
            MaxOverheal = maxOverheal;

            HealingSpells = [];
            MeasurementEvent = new(TimeSpan.FromSeconds(1));
            IncomingDamage = [];
            IncomingDamageBuffer = [];
            SpellHealingBuffer = [];
            SpellHealing = [];

            Bot.CombatLog.OnDamage += OnDamage;
            Bot.CombatLog.OnHeal += OnHeal;
        }

        public int DamageMonitorSeconds { get; set; }

        public float HealthWeightMod { get; set; }

        public float IncomingDamageMod { get; set; }

        public float MaxOverheal { get; set; }

        public float OverhealingStopThreshold { get; set; }

        public Dictionary<string, int> SpellHealing { get; set; }

        public int TargetDyingSeconds { get; set; }

        private AmeisenBotInterfaces Bot { get; }

        private List<(Spell, HealSpellType)> HealingSpells { get; }

        private Dictionary<ulong, int> IncomingDamage { get; }

        private Dictionary<ulong, Queue<(DateTime, int)>> IncomingDamageBuffer { get; }

        private TimegatedEvent MeasurementEvent { get; }

        private Dictionary<string, Queue<int>> SpellHealingBuffer { get; }

        private Func<string, ulong, bool> TryCastSpellAction { get; }

        /// <summary>
        /// Register a new spell that the bot can use to heal.
        /// </summary>
        /// <param name="spell">Spell object to register</param>
        public void AddSpell(Spell spell, HealSpellType spellType = HealSpellType.SingleTarget)
        {
            if (!SpellHealingBuffer.ContainsKey(spell.Name))
            {
                HealingSpells.Add((spell, spellType));
                SpellHealingBuffer.Add(spell.Name, new());

                if (!SpellHealing.ContainsKey(spell.Name))
                {
                    SpellHealing.Add(spell.Name, 0);
                }
            }
        }

        public void Load(Dictionary<string, JsonElement> s)
        {
            if (s.TryGetValue("SpellHealing", out JsonElement j)) { SpellHealing = j.To<Dictionary<string, int>>(); }
            if (s.TryGetValue("DamageMonitorSeconds", out j)) { DamageMonitorSeconds = j.To<int>(); }
            if (s.TryGetValue("HealthWeight", out j)) { HealthWeightMod = j.To<float>(); }
            if (s.TryGetValue("DamageWeight", out j)) { IncomingDamageMod = j.To<float>(); }
            if (s.TryGetValue("OverhealingStopThreshold", out j)) { OverhealingStopThreshold = j.To<float>(); }
            if (s.TryGetValue("MaxOverheal", out j)) { MaxOverheal = j.To<float>(); }
            if (s.TryGetValue("TargetDyingSeconds", out j)) { TargetDyingSeconds = j.To<int>(); }
        }

        public Dictionary<string, object> Save()
        {
            return new()
            {
                { "SpellHealing", SpellHealing },
                { "DamageMonitorSeconds", DamageMonitorSeconds },
                { "HealthWeight", HealthWeightMod },
                { "DamageWeight", IncomingDamageMod },
                { "OverhealingStopThreshold", OverhealingStopThreshold },
                { "MaxOverheal", MaxOverheal },
                { "TargetDyingSeconds", TargetDyingSeconds },
            };
        }

        /// <summary>
        /// Call this to determine wheter it would be useful to cancel the current cast, if
        /// overhealing would be too much or not.
        /// </summary>
        /// <param name="isTargetMyself">Spell casted on myself</param>
        /// <returns>True if you should cancel the cast, false if not</returns>
        public bool ShouldAbortCasting(bool isTargetMyself)
        {
            IWowUnit target = Bot.Target == null || isTargetMyself ? Bot.Player : Bot.Target;
            int spellId = Bot.Player.CurrentlyCastingSpellId > 0 ? Bot.Player.CurrentlyCastingSpellId : Bot.Player.CurrentlyChannelingSpellId;

            if (spellId > 0)
            {
                string castingSpell = Bot.Db.GetSpellName(spellId);

                if (SpellHealing.ContainsKey(castingSpell))
                {
                    int missingHealth = target.MaxHealth - target.Health;
                    int maxAllowedHeal = SpellHealing[castingSpell];
                    //int maxAllowedHeal = (int)(SpellHealing[castingSpell] * (1.0f + OverhealingStopThreshold));

                    // if the cast would be more than x% overheal, stop it
                    if (missingHealth < maxAllowedHeal)
                    {
                        AmeisenLogger.I.Log("HealingManager", $"Abort cast due to overhealing: {missingHealth} < {maxAllowedHeal}", LogLevel.Verbose);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Call this on a regular basis.
        /// </summary>
        /// <returns>True if the manager did cast a spell, false if not</returns>
        public bool Tick()
        {
            DateTime now = DateTime.UtcNow;

            // monitor damage per second on every healable target
            if (MeasurementEvent.Run())
            {
                UpdateIncomingDamage(now);
                UpdateSpellHealing();
            }

            List<IWowUnit> healableTargets = Bot.Wow.ObjectProvider.PartyMembers.Where(e =>
                !e.IsDead
                && e.HealthPercentage < 90
                && e.Position.GetDistance(Bot.Player.Position) < 40).ToList();

            //healableTargets.Add(Bot.Player);

            // is anyone going to die in the next seconds that we could save order by max health to
            // prioritize tanks
            IEnumerable<IWowUnit> dyingTargets = healableTargets
                .OrderBy(e => e.MaxHealth)
                .Where(e => e.HealthPercentage < 20.0
                            || (IncomingDamage.ContainsKey(e.Guid) && e.Health - (IncomingDamage[e.Guid] * TargetDyingSeconds) <= 0));

            if (dyingTargets != null)
            {
                // fastest heal possible
                IEnumerable<(Spell, HealSpellType)> fastestHeals = HealingSpells.OrderBy(e => e.Item1.CastTime);

                foreach (IWowUnit dyingTarget in dyingTargets)
                {
                    foreach ((Spell spell, HealSpellType spellType) in fastestHeals)
                    {

                        if (!dyingTarget.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == spell.Name)
                            && TryCastSpellAction(spell.Name, dyingTarget.Guid))
                        {
                            return true;
                        }
                    }
                }
            }

            // is there anyone that we could heal with max allowed over-heal
            IEnumerable<IWowUnit> targetsNeedToBeHealed = healableTargets.Where(e =>
                SpellHealing.Any(x => (e.MaxHealth - e.Health) >= x.Value * (1.0f + MaxOverheal)));

            if (targetsNeedToBeHealed.Any())
            {
                // prioritize target with the most incoming damage per second
                int maxDamage = IncomingDamage.Count > 0 ? IncomingDamage.Max(e => e.Value) : 0;
                List<(IWowUnit, double, int)> weightedTargets = [];

                foreach (IWowUnit target in targetsNeedToBeHealed)
                {
                    double damageWeight = maxDamage > 0 && IncomingDamage.ContainsKey(target.Guid) ? (IncomingDamage[target.Guid] / maxDamage) * IncomingDamageMod : 0.0;
                    double healthWeight = (target.Health / target.MaxHealth) * HealthWeightMod;

                    double weight = healthWeight + damageWeight;

                    // if target gets healed by other player, reduce weight by 50%
                    if (Bot.Wow.ObjectProvider.PartyMembers.Any(e => e.TargetGuid == target.Guid && e.IsCasting))
                    {
                        weight *= 0.5;
                    }

                    weightedTargets.Add((target, weight, target.MaxHealth - target.Health));
                }

                // sort by weight and process them
                foreach ((IWowUnit target, double weight, int missingHealth) in weightedTargets.OrderByDescending(e => e.Item2))
                {
                    // filter spell that would overheal us and then order by the amount of healing
                    IEnumerable<(Spell, HealSpellType)> heals = HealingSpells
                        .Where(e => missingHealth >= SpellHealing[e.Item1.Name])
                        .OrderByDescending(e => SpellHealing[e.Item1.Name]);

                    if (weightedTargets.Count > 2 && heals.Any(e => e.Item2 == HealSpellType.MultiTarget))
                    {
                        // try multi heals first
                        heals = heals.OrderByDescending(e => e.Item2);
                    }

                    foreach ((Spell spell, HealSpellType spellType) in heals)
                    {
                        if (!target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == spell.Name)
                            && TryCastSpellAction(spell.Name, target.Guid))
                        {
                            return true;
                        }
                    }
                }
            }

            // if not, simulate further incoming damage to see whether we could compensate that
            // damage in our cast time
            foreach (IWowUnit target in healableTargets)
            {
                if (!IncomingDamage.ContainsKey(target.Guid)) { continue; }

                int healthMissing = target.MaxHealth - target.Health;

                foreach ((Spell spell, HealSpellType spellType) in HealingSpells)
                {
                    // try to simulate what the health looks like when we finished casting
                    int simulatedHealthMissing = healthMissing + (IncomingDamage[target.Guid] * (spell.CastTime / 1000));

                    if (simulatedHealthMissing >= SpellHealing[spell.Name]
                        && !target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == spell.Name)
                        && TryCastSpellAction(spell.Name, target.Guid))
                    {
                        return true;
                    }
                }
            }

            // no need to heal anyone
            return false;
        }

        /// <summary>
        /// Fired when the player received damage.
        /// </summary>
        /// <param name="src">Source guid</param>
        /// <param name="dst">Destination guid</param>
        /// <param name="spellId">Spell id caused that damage</param>
        /// <param name="amount">Damage amount</param>
        /// <param name="over">Overdamage</param>
        private void OnDamage(ulong src, ulong dst, int spellId, int amount, int over)
        {
            if (dst == Bot.Wow.PlayerGuid || Bot.Wow.ObjectProvider.PartyMembers.Any(e => e.Guid == dst))
            {
                DateTime now = DateTime.UtcNow;

                if (!IncomingDamageBuffer.ContainsKey(dst))
                {
                    IncomingDamageBuffer.Add(dst, new());
                    IncomingDamage.Add(dst, 0);
                }

                IncomingDamageBuffer[dst].Enqueue((now, amount));
            }
        }

        /// <summary>
        /// Fired when a player received healing.
        /// </summary>
        /// <param name="src">Source guid</param>
        /// <param name="dst">Destination guid</param>
        /// <param name="spellId">Spell id caused that damage</param>
        /// <param name="amount">Haling amount</param>
        /// <param name="over">Overhealing</param>
        private void OnHeal(ulong src, ulong dst, int spellId, int amount, int over)
        {
            if (spellId > 0 && src == Bot.Wow.PlayerGuid)
            {
                string spellName = Bot.Db.GetSpellName(spellId);

                if (!SpellHealingBuffer.ContainsKey(spellName))
                {
                    SpellHealingBuffer.Add(spellName, new());
                    SpellHealing.Add(spellName, 0);
                }

                SpellHealingBuffer[spellName].Enqueue(amount);
            }
        }

        /// <summary>
        /// Updates the IncomingDamage dict with the new average damage incoming per unit.
        /// </summary>
        /// <param name="now">Current time</param>
        private void UpdateIncomingDamage(DateTime now)
        {
            foreach (ulong guid in IncomingDamageBuffer.Keys)
            {
                // remove too old stuff from buffer
                while (IncomingDamageBuffer[guid].Count > 0 && now - IncomingDamageBuffer[guid].Peek().Item1 > TimeSpan.FromSeconds(DamageMonitorSeconds))
                {
                    IncomingDamageBuffer[guid].Dequeue();
                }

                if (IncomingDamageBuffer[guid].Count > 0)
                {
                    TimeSpan totalTime = IncomingDamageBuffer[guid].First().Item1 - IncomingDamageBuffer[guid].Last().Item1;
                    int totalSeconds = (int)totalTime.TotalSeconds;

                    IncomingDamage[guid] = totalSeconds > 0 ? IncomingDamageBuffer[guid].Sum(e => e.Item2) / totalSeconds : 0;
                }
                else
                {
                    IncomingDamage[guid] = 0;
                }
            }

            // AmeisenLogger.I.Log("HealingManager", $"IncomingDamage:
            // {JsonSerializer.Serialize(IncomingDamage)}", LogLevel.Verbose);
        }

        /// <summary>
        /// Updates the SpellHealing dict with the new average healing done by the spells.
        /// </summary>
        /// <param name="now">Current time</param>
        private void UpdateSpellHealing()
        {
            foreach (string spell in SpellHealingBuffer.Keys)
            {
                // remove last entries
                while (SpellHealingBuffer[spell].Count > 32)
                {
                    SpellHealingBuffer[spell].Dequeue();
                }

                if (SpellHealingBuffer[spell].Count > 0)
                {
                    SpellHealing[spell] = (int)SpellHealingBuffer[spell].Average();
                }
            }

            // AmeisenLogger.I.Log("HealingManager", $"SpellHealing:
            // {JsonSerializer.Serialize(SpellHealing)}", LogLevel.Verbose);
        }
    }
}