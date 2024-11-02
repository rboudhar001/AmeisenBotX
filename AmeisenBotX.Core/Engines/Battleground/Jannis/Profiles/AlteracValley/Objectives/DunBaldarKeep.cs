using System.Collections.Generic;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Movement.StaticPath;
using AmeisenBotX.Core.Engines.Movement.StaticPath.Paths.Battlegrounds.AlteracValley;

namespace AmeisenBotX.Core.Engines.Battleground.Jannis.Profiles.AlteracValley.Objectives
{
    public class DunBaldarKeep : IObjective
    {
        public Vector3 Position { get; } = new Vector3(722.4128f, -10.988703f, 50.62137f);

        public List<StaticPath> StartStaticPaths { get; } =
        [
        ];

        public List<StaticPath> EndStaticPaths { get; } =
        [
            new FromStormpikeAidStationToDunBaldarKeepPath()
        ];
    }
}