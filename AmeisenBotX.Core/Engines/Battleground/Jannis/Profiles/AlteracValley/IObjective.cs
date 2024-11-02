using System.Collections.Generic;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Movement.StaticPath;

namespace AmeisenBotX.Core.Engines.Battleground.Jannis.Profiles.AlteracValley
{
    public interface IObjective
    {
        /// <summary>
        /// Position of this objective.
        /// </summary>
        Vector3 Position { get; }

        /// <summary>
        /// Paths that start from this objective position.
        /// </summary>
        List<StaticPath> StartStaticPaths { get; }

        /// <summary>
        /// Paths that end in this objective position.
        /// </summary>
        List<StaticPath> EndStaticPaths { get; }
    }
}
