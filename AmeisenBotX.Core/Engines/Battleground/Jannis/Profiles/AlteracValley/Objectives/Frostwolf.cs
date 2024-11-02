using System.Collections.Generic;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Movement.StaticPath;
using AmeisenBotX.Core.Engines.Movement.StaticPath.Paths.Battlegrounds.AlteracValley;

namespace AmeisenBotX.Core.Engines.Battleground.Jannis.Profiles.AlteracValley.Objectives
{
    public class Frostwolf : IObjective
    {
        public Vector3 Position { get; } = new Vector3(-1082.6345f, -346.05374f, 55.095543f);

        public List<StaticPath> StartStaticPaths { get; } =
        [
            new FromFrostwolfToIcebloodPath(),
        ];

        public List<StaticPath> EndStaticPaths { get; } =
        [
            new FromHordeGateToFrostwolfPath(),
            new FromFrostwolfReliefHuntToFrostwolfPath()
        ];
    }
}
