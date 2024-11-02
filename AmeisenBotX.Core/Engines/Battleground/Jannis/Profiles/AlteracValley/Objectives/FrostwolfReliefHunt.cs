using System.Collections.Generic;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Movement.StaticPath;
using AmeisenBotX.Core.Engines.Movement.StaticPath.Paths.Battlegrounds.AlteracValley;

namespace AmeisenBotX.Core.Engines.Battleground.Jannis.Profiles.AlteracValley.Objectives
{
    public class FrostwolfReliefHunt : IObjective
    {
        public Vector3 Position { get; } = new Vector3(-1402.9949f, -307.766f, 89.41141f);

        public List<StaticPath> StartStaticPaths { get; } =
        [
            new FromFrostwolfReliefHuntToFrostwolfPath()
        ];

        public List<StaticPath> EndStaticPaths { get; } =
        [
        ];
    }
}
