using System.Collections.Generic;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Movement.StaticPath;
using AmeisenBotX.Core.Engines.Movement.StaticPath.Paths.Battlegrounds.AlteracValley;

namespace AmeisenBotX.Core.Engines.Battleground.Jannis.Profiles.AlteracValley.Objectives
{
    public class AllianceBase : IObjective
    {
        public Vector3 Position { get; } = new Vector3(873.08453f, -491.3031f, 96.54254f);

        public List<StaticPath> StartStaticPaths { get; } =
        [
            new FromAllianceBaseToAllianceGatePath()
        ];

        public List<StaticPath> EndStaticPaths { get; } =
        [
        ];
    }
}