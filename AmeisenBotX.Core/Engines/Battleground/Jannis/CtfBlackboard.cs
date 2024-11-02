using AmeisenBotX.Common.Math;
using AmeisenBotX.Wow.Objects;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Battleground.Jannis
{
    public class CtfBlackboard : Blackboard
    {
        public IWowUnit EnemyTeamFlagCarrier { get; set; }

        public Vector3 EnemyTeamFlagPos { get; set; }

        public bool EnemyTeamHasFlag { get; set; }

        public override int EnemyTeamMaxScore { get; set; }

        public override int EnemyTeamScore { get; set; }

        public IWowUnit MyTeamFlagCarrier { get; set; }

        public Vector3 MyTeamFlagPos { get; set; }

        public bool MyTeamHasFlag { get; set; }

        public override int MyTeamMaxScore { get; set; }

        public override int MyTeamScore { get; set; }

        public IEnumerable<IWowGameobject> NearFlags { get; set; }

        public CtfBlackboard(Action updateAction) : base(updateAction)
        {
        }
    }
}