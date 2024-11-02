using System.Collections.Generic;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Movement.StaticPath;
using AmeisenBotX.Core.Engines.Movement.StaticPath.Paths.Battlegrounds.AlteracValley;

namespace AmeisenBotX.Core.Engines.Battleground.Jannis.Profiles.AlteracValley.Objectives
{
    public class Iceblood : IObjective
    {
        public Vector3 Position { get; } = new Vector3(-611.561f, -396.5508f, 60.85842f);

        public List<StaticPath> StartStaticPaths { get; } =
        [
            new FromIcebloodToSnowfallPath(),
            new FromIcebloodToStonehearthPath()
        ];

        public List<StaticPath> EndStaticPaths { get; } =
        [
            new FromFrostwolfToIcebloodPath(),
            new FromHordeGateToIcebloodPath()
        ];
    }
}
