using System;
using System.Collections.Generic;
using AmeisenBotX.Wow.Objects;

namespace AmeisenBotX.Core.Engines.Battleground.Jannis
{
    public class CtbBlackboard : Blackboard
    {
        public override int EnemyTeamMaxScore { get; set; }

        public override int EnemyTeamScore { get; set; }

        public override int MyTeamMaxScore { get; set; }

        public override int MyTeamScore { get; set; }

        public IEnumerable<IWowGameobject> NearBases { get; set; }

        public CtbBlackboard(Action updateAction) : base(updateAction)
        {
        }
    }
}