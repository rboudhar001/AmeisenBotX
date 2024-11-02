using System.Collections.Generic;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Movement.StaticPath;

namespace AmeisenBotX.Core.Engines.Battleground.Jannis.Profiles.AlteracValley.Objectives
{
    public class FrostwolfKeep : IObjective
    {
        public Vector3 Position { get; } = new Vector3(-1371.4913f, -215.64049f, 99.37136f);

        public List<StaticPath> StartStaticPaths { get; } =
        [
        ];

        public List<StaticPath> EndStaticPaths { get; } =
        [
        ];
    }
}