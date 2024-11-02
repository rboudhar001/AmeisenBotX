using System.Collections.Generic;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Movement.StaticPath;
using AmeisenBotX.Core.Engines.Movement.StaticPath.Paths.Battlegrounds.AlteracValley;

namespace AmeisenBotX.Core.Engines.Battleground.Jannis.Profiles.AlteracValley.Objectives
{
    public class HordeBase : IObjective
    {
        public Vector3 Position { get; } = new Vector3(-1437.9077f, -610.23004f, 51.161423f);

        public List<StaticPath> StartStaticPaths { get; } =
        [
            new FromHordeBaseToHordeGatePath()
        ];

        public List<StaticPath> EndStaticPaths { get; } =
        [
        ];
    }
}