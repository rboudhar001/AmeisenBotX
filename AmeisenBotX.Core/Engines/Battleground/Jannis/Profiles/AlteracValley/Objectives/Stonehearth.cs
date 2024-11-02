using System.Collections.Generic;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Movement.StaticPath;
using AmeisenBotX.Core.Engines.Movement.StaticPath.Paths.Battlegrounds.AlteracValley;

namespace AmeisenBotX.Core.Engines.Battleground.Jannis.Profiles.AlteracValley.Objectives
{
    public class Stonehearth : IObjective
    {
        public Vector3 Position { get; } = new Vector3(77.42279f, -404.14465f, 46.35151f);

        public List<StaticPath> StartStaticPaths { get; } =
        [
            new FromStonehearthToStormpikePath()
        ];

        public List<StaticPath> EndStaticPaths { get; } =
        [
            new FromSnowfallToStonehearthPath(),
            new FromIcebloodToStonehearthPath()
        ];
    }
}