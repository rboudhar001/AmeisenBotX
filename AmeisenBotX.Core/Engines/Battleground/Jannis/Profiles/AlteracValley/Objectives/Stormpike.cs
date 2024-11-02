using System.Collections.Generic;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Movement.StaticPath;
using AmeisenBotX.Core.Engines.Movement.StaticPath.Paths.Battlegrounds.AlteracValley;

namespace AmeisenBotX.Core.Engines.Battleground.Jannis.Profiles.AlteracValley.Objectives
{
    public class Stormpike : IObjective
    {
        public Vector3 Position { get; } = new Vector3(669.6398f, -294.3382f, 30.289236f);

        public List<StaticPath> StartStaticPaths { get; } =
        [
            new FromStormpikeToStormpikeAidStationPath()
        ];

        public List<StaticPath> EndStaticPaths { get; } =
        [
            new FromAllianceGateToStormpikePath(),
            new FromStonehearthToStormpikePath()
        ];
    }
}