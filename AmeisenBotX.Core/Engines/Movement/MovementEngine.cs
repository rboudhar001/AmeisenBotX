using System;
using System.Collections.Generic;
using System.Linq;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Engines.Movement.Objects;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Constants;
using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Core.Engines.Movement
{
    public class MovementEngine(AmeisenBotInterfaces bot, AmeisenBotConfig config) : IMovementEngine
    {
        private AmeisenBotInterfaces Bot { get; } = bot;

        private AmeisenBotConfig Config { get; } = config;

        public MovementAction CurrentMovementAction { get; private set; }

        public float CurrentSpeed { get; set; }

        private float DistanceToDestination { get; set; }

        private TimegatedEvent DistanceMovedCheckEvent { get; } = new(TimeSpan.FromMilliseconds(500));

        private TimegatedEvent TravelEvent { get; } = new(TimeSpan.FromMilliseconds(1500));

        public bool IsUnstucking { get; private set; }

        public DateTime LastMovement { get; private set; }

        public Vector3 LastPosition { get; private set; }

        public IEnumerable<Vector3> Path => PathQueue;

        private Queue<Vector3> PathQueue { get; set; } = new();

        public IEnumerable<(Vector3 position, float radius)> PlacesToAvoid => PlacesToAvoidList.Where(e => DateTime.UtcNow <= e.until).Select(e => (e.position, e.radius));

        private List<(Vector3 position, float radius, DateTime until)> PlacesToAvoidList { get; set; } = [];

        private BasicVehicle PlayerVehicle { get; set; } = new(bot);

        private List<Vector3> PotentialUnstuckPositions { get; set; } = [];

        private PreventMovementType PreventMovementType { get; set; }

        private TimegatedEvent PreventMovementEvent { get; set; } = new(TimeSpan.FromMilliseconds(0));

        public Vector3 UnstuckPosition { get; private set; } = Vector3.Zero;

        private Random MyRandom { get; } = new Random();

        // Public

        public void AvoidPlace(Vector3 position, float radius, TimeSpan timeSpan)
        {
            DateTime now = DateTime.UtcNow;

            PlacesToAvoidList.Add((position, radius, now + timeSpan));
            PlacesToAvoidList.RemoveAll(e => now > e.until);
        }

        public void Execute()
        {
            if (!Config.Autopilot
                || Bot.Player.IsCasting
                || (!PreventMovementEvent.Ready && IsPreventMovementValid())) return;

            if (PathQueue.Count > 0)
            {
                Vector3 currentNode = IsUnstucking ? UnstuckPosition : PathQueue.Peek();
                float distanceToNode = Bot.Player.Position.GetDistance2D(currentNode);

                // only stop at `1.7f` meter of the last node, the rest of nodes stop at `3.5f` meters from them to get a smooth ride
                float howFarToStopFromTheNode = PathQueue.Count == 1 ? Config.MovementSettings.WaypointCheckThreshold : Config.MovementSettings.WaypointCheckThresholdMounted;

                // move to the node
                if (currentNode != Vector3.Zero && distanceToNode > howFarToStopFromTheNode)
                {
                    if (CurrentMovementAction == MovementAction.DirectMove)
                    {
                        // do not use Pathfinder, move directly
                        Bot.Character.MoveToPosition(currentNode, 20.9f, WowClickToMoveDistance.Move);
                    }
                    else
                    {
                        // move to the node with (force/acceleration/velocity) calculations and Pathfinder (if available)
                        PlayerVehicle.Update
                        (
                            MoveCharacter,
                            CurrentMovementAction,
                            currentNode,
                            Bot.Player.Rotation,
                            Bot.Player.IsInCombat ? Config.MovementSettings.MaxSteeringCombat : Config.MovementSettings.MaxSteering,
                            Config.MovementSettings.MaxVelocity,
                            Config.MovementSettings.SeperationDistance
                        );
                    }

                    if (Config.MovementSettings.EnableDistanceMovedJumpCheck)
                    {
                        DistanceMovedJumpCheck();
                    }

                    // use any travel method (mount/skill) every 1.5s
                    if (TravelEvent.Ready)
                    {
                        if (!Bot.Player.IsDead
                            && TryToTravel())
                        {
                            TravelEvent.Run();
                        }
                    }

                }
                else
                {
                    // we are at the node
                    if (IsUnstucking)
                    {
                        IsUnstucking = false;
                        PotentialUnstuckPositions = [];
                    }
                    else
                    {
                        PathQueue.Dequeue();
                    }
                }
            }
            else
            {
                if (AvoidAoeStuff(Bot.Player.Position, out Vector3 newPosition))
                {
                    SetMovementAction(MovementAction.Move, newPosition);
                }
            }
        }

        public void PreventMovement(double milliseconds, PreventMovementType preventMovementType = PreventMovementType.Hard /* TODO: implement here a callback? */)
        {
            Bot.Wow.StopClickToMove();

            PreventMovementType = preventMovementType;
            PreventMovementEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(milliseconds));
            PreventMovementEvent.Run();
        }

        public bool RouteInProgress()
        {
            return PathQueue.Count > 0;
        }

        public void Reset()
        {
            PathQueue.Clear();
        }

        public bool SetMovementAction(MovementAction movementAction, Vector3 position, float rotation = 0 /* @deprecated was only used for MovementAction.Evade scenario */)
        {
            IsUnstucking = false;
            PotentialUnstuckPositions = [];
            CurrentMovementAction = movementAction;

            // Get path to move to position with Pathfinder
            if (!Bot.Player.IsFlying
                && !Bot.Player.IsUnderwater
                && CurrentMovementAction != MovementAction.DirectMove
                && TryGetPath(position, out IEnumerable<Vector3> path))
            {
                Vector3[] p = path as Vector3[] ?? path.ToArray();
                CreateNewRoute(p);
                DistanceToDestination = Bot.Player.Position.GetDistance(p.Last());
            }
            // move to position directly
            else
            {
                CreateNewRoute(position);
                DistanceToDestination = Bot.Player.Position.GetDistance(position);
            }

            return true;
        }

        public bool SetMovementFromPath(List<Vector3> path)
        {
            IsUnstucking = false;
            PotentialUnstuckPositions = [];
            CurrentMovementAction = MovementAction.DirectMove;

            CreateNewRoute(path);
            DistanceToDestination = Bot.Player.Position.GetDistance(path.Last());

            return true;
        }

        public void StopMovement()
        {
            Reset();
            Bot.Wow.StopClickToMove();
        }

        public bool TryGetPath(Vector3 position, out IEnumerable<Vector3> path, float maxDistance = 5.0f)
        {
            // don't search a path into AoE effects
            if (AvoidAoeStuff(position, out Vector3 newPosition))
            {
                position = newPosition;
            }

            path = Bot.PathfindingHandler.GetPath((int)Bot.Objects.MapId, Bot.Player.Position, position);

            Vector3[] p = path as Vector3[] ?? path.ToArray();
            return path != null && p.Length != 0;
        }

        // Private

        private bool AvoidAoeStuff(Vector3 position, out Vector3 newPosition)
        {
            List<(Vector3 position, float radius)> places = PlacesToAvoid.ToList();

            // TODO: avoid dodging player aoe spells in sanctuaries, this may look suspect
            if (Config.AoeDetectionAvoid)
            {
                // add all AoE spells
                IEnumerable<IWowDynobject> aoeEffects = Bot.GetAoeSpells(position, Config.AoeDetectionExtends)
                    .Where(e => (Config.AoeDetectionIncludePlayers || Bot.GetWowObjectByGuid<IWowUnit>(e.Caster)?.Type == WowObjectType.Unit)
                             && Bot.Db.GetReaction(Bot.Player, Bot.GetWowObjectByGuid<IWowUnit>(e.Caster)) is WowUnitReaction.Hostile or WowUnitReaction.Neutral);

                places.AddRange(aoeEffects.Select(e => (e.Position, e.Radius)));
            }

            if (places.Count != 0)
            {
                // build mean position and move away x meters from it x is the biggest distance
                // we have to move
                Vector3 meanAoePos = BotMath.GetMeanPosition(places.Select(e => e.position));
                float distanceToMove = places.Max(e => e.radius) + Config.AoeDetectionExtends;

                // calculate the repel direction to move away from the aoe effects
                Vector3 repelDirection = position - meanAoePos;
                repelDirection.Normalize();

                // "repel" the position from the aoe spell
                newPosition = meanAoePos + (repelDirection * distanceToMove);
                return true;
            }

            newPosition = default;
            return false;
        }

        private void CreateNewRoute(IEnumerable<Vector3> path)
        {
            PathQueue.Clear();
            foreach (Vector3 position in path)
            {
                PathQueue.Enqueue(position);
            }
        }

        private void CreateNewRoute(Vector3 position)
        {
            PathQueue.Clear();
            PathQueue.Enqueue(position);
        }

        private void DistanceMovedJumpCheck()
        {
            if (DistanceMovedCheckEvent.Run())
            {
                if (LastMovement != default && DateTime.UtcNow - LastMovement < TimeSpan.FromSeconds(1))
                {
                    CurrentSpeed = LastPosition.GetDistance2D(Bot.Player.Position) / (float)(DateTime.UtcNow - LastMovement).TotalSeconds;

                    if (CurrentSpeed < 0.5f)
                    {
                        IsUnstucking = true;

                        // soft unstuck
                        Bot.Character.Jump();

                        // using Pathfinder for unstuck (usually don't work)
                        Vector3 node = Bot.PathfindingHandler.GetRandomPointAround((int)Bot.Objects.MapId, Bot.Player.Position, 10.0f);

                        if (node != Vector3.Zero)
                        {
                            UnstuckPosition = node;
                        }
                        else
                        {
                            // manually unstuck
                            GetUnstuckPositions(Bot.Player.Position);

                            // generate an index randomly
                            int index = MyRandom.Next(PotentialUnstuckPositions.Count);

                            // get potential unstuck position randomly
                            UnstuckPosition = PotentialUnstuckPositions[index];

                            // remove the selected position from the list
                            PotentialUnstuckPositions.RemoveAt(index);
                        }
                    }
                }

                LastMovement = DateTime.UtcNow;
                LastPosition = Bot.Player.Position;
            }
        }

        private bool IsPreventMovementValid()
        {
            return PreventMovementType switch
            {
                // cast maybe aborted, allow to move again
                PreventMovementType.SpellCast => Bot.Player.IsCasting,
                PreventMovementType.Hard => true,
                _ => false
            };
        }

        private void MoveCharacter(Vector3 positionToGoTo)
        {
            Vector3 node = Bot.PathfindingHandler.MoveAlongSurface((int)Bot.Objects.MapId, Bot.Player.Position, positionToGoTo);

            // Get new position for move to positionToGoTo evading obstacles with Pathfinder
            if (node != Vector3.Zero)
            {
                //Bot.Character.MoveToPosition(node, 20.9f, 0.1f); // Vehicle
                //Bot.Character.MoveToPosition(node, MathF.Tau, 0.25f); // Jane
                //Bot.Character.MoveToPosition(node, 20.9f, WowClickToMoveDistance.Move); // WowInterface335
                Bot.Character.MoveToPosition(node, 20.9f, 0.25f);
            }
            // If Pathfinders fails, move there directly
            else
            {
                //Bot.Character.MoveToPosition(positionToGoTo, 20.9f, 0.1f); // Vehicle
                //Bot.Character.MoveToPosition(positionToGoTo, MathF.Tau, 0.25f); // Jane
                //Bot.Character.MoveToPosition(positionToGoTo, 20.9f, WowClickToMoveDistance.Move); // WowInterface335
                Bot.Character.MoveToPosition(positionToGoTo, 20.9f, 0.25f);
            }
        }

        private bool TryToMountUp()
        {
            // already mounted
            if (Bot.Player.IsMounted) return true;

            // I can't mount
            if (!Bot.Player.IsOutdoors
                || Bot.Character.Mounts == null
                || !Bot.Character.Mounts.Any()
                // wsg flags
                || Bot.Player.HasBuffById(Bot.Player.IsAlliance() ? 23333 : 23335)) return false;

            IEnumerable<WowMount> filteredMounts = Bot.Character.Mounts;

            // use specific mount
            if (Config.UseOnlySpecificMounts)
            {
                filteredMounts = filteredMounts.Where(e => Config.Mounts.Split(",", StringSplitOptions.RemoveEmptyEntries).Any(x => x.Equals(e.Name.Trim(), StringComparison.OrdinalIgnoreCase)));
            }

            // there is no mounts to use
            WowMount[] wowMounts = filteredMounts as WowMount[] ?? filteredMounts.ToArray();
            if (filteredMounts == null || wowMounts.Length == 0) return false;

            // mount up
            PreventMovement(3000);
            WowMount mount = wowMounts.ElementAt(new Random().Next(0, wowMounts.Length));
            Bot.Wow.CallCompanion(mount.Index, "MOUNT");

            return true;
        }

        private bool TryToTravel()
        {
            // mount
            if (DistanceToDestination > 200
                && TryToMountUp())
            {
                return true;
            }

            // travel with a skill (`Travel Form`, `Ghost Wolf`, `Aspect of the Cheetah` ...)
            if (DistanceToDestination > 100
                && Bot.CombatClass != null
                && Bot.CombatClass.TryToTravel())
            {
                return true;
            }

            return false;
        }

        private void GetUnstuckPositions(Vector3 currentPosition, float radius = 10.0f)
        {
            if (PotentialUnstuckPositions.Count != 0) return;

            // generate potential positions around the current position
            for (int angle = 0; angle < 360; angle += 45)
            {
                float radians = angle * (MathF.PI / 180);
                Vector3 offset = new Vector3(MathF.Cos(radians), MathF.Sin(radians), 0) * radius;
                Vector3 potentialPosition = currentPosition + offset;

                PotentialUnstuckPositions.Add(potentialPosition);
            }
        }
    }
}
