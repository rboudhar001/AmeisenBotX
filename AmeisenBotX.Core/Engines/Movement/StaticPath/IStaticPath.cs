using System.Collections.Generic;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Core.Engines.Movement.StaticPath
{
    /// <summary>
    /// Static path is used to describe a route from A to B that cannot be reached by the
    /// bots pathfinding at the moment.
    /// </summary>
    public interface IStaticPath
    {
        /// <summary>
        /// Map id where this path can be used.
        /// </summary>
        WowMapId MapId { get; }

        /// <summary>
        /// List of positions that represent a path to be able to go from A to B
        /// </summary>
        List<Vector3> Path { get; }

        /// <summary>
        /// Get a sub-path from the original path to run, the sub-path start position will be the nearest position to `playerPosition`
        /// </summary>
        /// <param name="playerPosition">Player position to use for find the nearest position in the path</param>
        List<Vector3> GetPathFromWhereToStart(Vector3 playerPosition);

        /// <summary>
        /// Returns whether the path can be used based on the map id.
        /// </summary>
        /// <param name="mapId">Current map id</param>
        /// <returns>True when the path is usable, false if not</returns>
        bool IsUsable(WowMapId mapId);
    }
}