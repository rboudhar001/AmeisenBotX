using System.Collections.Generic;
using System.Linq;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Logics.Dps
{
    public class BattlegroundTargetSelectionLogic(AmeisenBotInterfaces bot, AmeisenBotConfig config) : BasicTargetSelectionLogic(bot, config)
    {
        public override bool SelectTarget(out IEnumerable<IWowUnit> possibleTargets)
        {
            possibleTargets = Bot.Objects.All.OfType<IWowPlayer>()
                .Where(e => TargetValidator.IsValid(e)
                    && !e.IsDead
                    && e.Position.GetDistance(Bot.Player.Position) < Config.SupportRange
                    && Bot.Db.GetReaction(Bot.Player, e) == WowUnitReaction.Hostile
                )
                .OrderBy(e => e.HealthPercentage);

            return possibleTargets.Any();
        }
    }
}