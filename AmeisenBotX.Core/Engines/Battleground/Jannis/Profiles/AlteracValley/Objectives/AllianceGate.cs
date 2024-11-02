using System.Collections.Generic;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Movement.StaticPath;
using AmeisenBotX.Core.Engines.Movement.StaticPath.Paths.Battlegrounds.AlteracValley;

namespace AmeisenBotX.Core.Engines.Battleground.Jannis.Profiles.AlteracValley.Objectives
{
    public class AllianceGate : IObjective
    {
        public Vector3 Position { get; } = new Vector3(794.6912f, -493.92212f, 99.766304f);

        public List<StaticPath> StartStaticPaths { get; } =
        [
            new FromAllianceGateToStormpikePath()
        ];

        public List<StaticPath> EndStaticPaths { get; } =
        [
            new FromAllianceBaseToAllianceGatePath()
        ];
    }
}