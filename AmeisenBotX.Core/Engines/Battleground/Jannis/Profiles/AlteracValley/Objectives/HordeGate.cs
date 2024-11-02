using System.Collections.Generic;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Movement.StaticPath;
using AmeisenBotX.Core.Engines.Movement.StaticPath.Paths.Battlegrounds.AlteracValley;

namespace AmeisenBotX.Core.Engines.Battleground.Jannis.Profiles.AlteracValley.Objectives
{
    public class HordeGate : IObjective
    {
        public Vector3 Position { get; } = new Vector3(-1382.446f, -545.61957f, 54.837147f);

        public List<StaticPath> StartStaticPaths { get; } =
        [
            new FromHordeGateToFrostwolfPath(),
            new FromHordeGateToIcebloodPath()
        ];

        public List<StaticPath> EndStaticPaths { get; } =
        [
            new FromHordeBaseToHordeGatePath()
        ];
    }
}