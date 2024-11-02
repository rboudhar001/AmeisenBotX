using System.Linq;
using AmeisenBotX.Wow.Objects;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Validation.Basic
{
    public class IsThreatTargetValidator(AmeisenBotInterfaces bot) : ITargetValidator
    {
        private AmeisenBotInterfaces Bot { get; } = bot;

        public bool IsValid(IWowUnit unit)
        {
            // is tagged by me or my group
            return (unit.IsTaggedByMe || !unit.IsTaggedByOther)
                // has no target
                && (unit.TargetGuid == 0
                    // unit is targeting me, group or pets
                    || (unit.TargetGuid == Bot.Player.Guid || Bot.Objects.PartyMemberGuids.Contains(unit.TargetGuid) || Bot.Objects.PartyPetGuids.Contains(unit.TargetGuid)
                    // group or pets are targeting the unit
                    || (Bot.Objects.PartyMembers.Any(e => e.TargetGuid == unit.Guid) || Bot.Objects.PartyPets.Any(e => e.TargetGuid == unit.Guid))));
        }
    }
}