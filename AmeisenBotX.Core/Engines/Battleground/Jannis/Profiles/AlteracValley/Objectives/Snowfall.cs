using System.Collections.Generic;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Movement.StaticPath;
using AmeisenBotX.Core.Engines.Movement.StaticPath.Paths.Battlegrounds.AlteracValley;

namespace AmeisenBotX.Core.Engines.Battleground.Jannis.Profiles.AlteracValley.Objectives
{
    public class Snowfall : IObjective
    {
        public Vector3 Position { get; } = new Vector3(-203.10136f, -112.26751f, 78.5494f);

        public List<StaticPath> StartStaticPaths { get; } =
        [
            new FromSnowfallToStonehearthPath()
        ];

        public List<StaticPath> EndStaticPaths { get; } =
        [
            new FromIcebloodToSnowfallPath()
        ];
    }
}