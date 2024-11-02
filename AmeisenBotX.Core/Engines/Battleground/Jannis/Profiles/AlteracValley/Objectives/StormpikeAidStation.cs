using System.Collections.Generic;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Movement.StaticPath;
using AmeisenBotX.Core.Engines.Movement.StaticPath.Paths.Battlegrounds.AlteracValley;

namespace AmeisenBotX.Core.Engines.Battleground.Jannis.Profiles.AlteracValley.Objectives
{
    public class StormpikeAidStation : IObjective
    {
        public Vector3 Position { get; } = new Vector3(638.6116f, -31.825853f, 46.170605f);

        public List<StaticPath> StartStaticPaths { get; } =
        [
            new FromStormpikeAidStationToDunBaldarKeepPath()
        ];

        public List<StaticPath> EndStaticPaths { get; } =
        [
            new FromStormpikeToStormpikeAidStationPath()
        ];
    }
}