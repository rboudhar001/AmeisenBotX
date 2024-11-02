using System;
using System.Collections.Generic;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Movement.Enums;

namespace AmeisenBotX.Core.Engines.Movement
{
    public interface IMovementEngine
    {
        /// <summary>
        /// Get the current movement engine state.
        /// </summary>
        MovementAction CurrentMovementAction { get; }

        /// <summary>
        /// Returns the current speed in meters per second.
        /// </summary>
        float CurrentSpeed { get; }

        /// <summary>
        /// Get the curren loaded path.
        /// </summary>
        IEnumerable<Vector3> Path { get; }

        /// <summary>
        /// Get the current blacklisted places.
        /// </summary>
        IEnumerable<(Vector3 position, float radius)> PlacesToAvoid { get; }

        /// <summary>
        /// Add a place to the blacklist. Used to avoid AOE effects.
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="radius">Radius</param>
        /// <param name="timeSpan">How long should the place be blacklisted</param>
        void AvoidPlace(Vector3 position, float radius, TimeSpan timeSpan);

        /// <summary>
        /// 
        /// Method called every `Config.StateMachineTickMs` milliseconds, this is
        /// and should be always the only function that manage the movement of the character
        /// making previous validations for not breaking workflow and functionality.
        ///
        /// Validations:
        /// - Only move if `Config.Autopilot` is enabled.
        /// - Character is not casting, if you need to move under this condition, first `Bot.Wow.StopCasting()` and then `Bot.Movement.SetMovementAction()`
        /// - Movement is not being prevented X milliseconds with `Bot.Movement.PreventMovement()`
        /// 
        /// IMPORTANT: Do not use `Bot.Character.MoveToPosition(position);`, instead use `Bot.Movement.SetMovementAction(MovementAction.DirectMove, position);`
        /// 
        /// </summary>
        void Execute();

        /// <summary>
        /// Prevent movement for a specified time.
        /// </summary>
        /// <param name="milliseconds">How long should movement be prevented</param>
        /// <param name="preventMovementType">
        /// Special movement blocker that may get freed early (example: when cast is aborted)
        /// </param>
        void PreventMovement(double milliseconds, PreventMovementType preventMovementType = PreventMovementType.Hard);

        /// <summary>
        /// Drop the current target position and path.
        /// </summary>
        void Reset();

        /// <returns>True if there is a route in process, false if not</returns>
        bool RouteInProgress();

        /// <summary>
        /// Set a new target position.
        /// </summary>
        /// <param name="state">How should the bot move</param>
        /// <param name="position">Where should the bot move</param>
        /// <param name="rotation">target rotation</param>
        /// <param name="distanceNode">distance to the target node</param>
        /// <param name="usePathfinder">whether to use pathfinding</param>
        /// <returns>True if instruction was set successful, false if not</returns>
        bool SetMovementAction(MovementAction state, Vector3 position, float rotation = 0 /* @deprecated was only used for MovementAction.Evade scenario */);

        /// <summary>
        /// Set a new target position.
        /// </summary>
        /// <param name="path">list of positions to follow</param>
        /// <param name="rotation">target rotation</param>
        /// <param name="distanceNode">distance to the target node</param>
        /// <param name="usePathfinder">whether to use pathfinding</param>
        /// <returns>True if instruction was set successful, false if not</returns>
        bool SetMovementFromPath(List<Vector3> path);

        /// <summary>
        /// Stop the bots current movement.
        /// </summary>
        void StopMovement();

        /// <summary>
        /// Determine whether a position can be reached by the player.
        /// </summary>
        /// <param name="position">Target position</param>
        /// <param name="path">The resulting path if the target position can be reached</param>
        /// <param name="maxDistance">Max distance to the target position</param>
        /// <returns>True if it can be reached, false if not</returns>
        bool TryGetPath(Vector3 position, out IEnumerable<Vector3> path, float maxDistance = 1.0f);
    }
}
