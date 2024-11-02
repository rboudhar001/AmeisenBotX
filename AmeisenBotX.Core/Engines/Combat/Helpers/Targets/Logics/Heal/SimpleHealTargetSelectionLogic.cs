using System.Collections.Generic;
using System.Linq;
using AmeisenBotX.Wow.Objects;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Logics.Heal
{
    public class SimpleHealTargetSelectionLogic(AmeisenBotInterfaces bot, AmeisenBotConfig config) : BasicTargetSelectionLogic(bot, config)
    {
        public override bool SelectTarget(out IEnumerable<IWowUnit> possibleTargets)
        {
            List<IWowUnit> healableUnits = [.. Bot.Objects.PartyMembers, Bot.Player];

            // healableUnits.AddRange(Bot.ObjectManager.PartyPets);

            possibleTargets = healableUnits
                .Where(e => TargetValidator.IsValid(e)
                            && !e.IsDead
                            && e.Health < e.MaxHealth
                            && e.Position.GetDistance(Bot.Player.Position) < Config.SupportRange
                )
                .OrderByDescending(e => e.Type)
                .ThenBy(e => e.Health);

            return possibleTargets.Any();
        }
    }
}