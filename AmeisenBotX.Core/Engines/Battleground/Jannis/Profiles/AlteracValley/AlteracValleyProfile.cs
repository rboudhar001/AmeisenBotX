using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using AmeisenBotX.BehaviorTree;
using AmeisenBotX.BehaviorTree.Enums;
using AmeisenBotX.BehaviorTree.Objects;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Battleground.Jannis.Profiles.AlteracValley.Objectives;
using AmeisenBotX.Core.Engines.Combat.Helpers.Targets;
using AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Logics.Dps;
using AmeisenBotX.Core.Engines.Movement;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Engines.Movement.Providers.Special;
using AmeisenBotX.Core.Engines.Movement.StaticPath;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;

namespace AmeisenBotX.Core.Engines.Battleground.Jannis.Profiles.AlteracValley
{
    public class AlteracValleyProfile : BattlegroundProfile
    {
        // Public

        public override Vector3 EnemyBasePosition { get; }

        public override Vector3 EnemyGraveyardPosition { get; }

        public override Vector3 GatePosition { get; }

        public override Vector3 OwnBasePosition { get; }

        public override Vector3 OwnGraveyardPosition { get; }

        public override Blackboard Blackboard { get; }

        // Private

        private AmeisenBotInterfaces Bot { get; }

        private AmeisenBotConfig Config { get; set; }

        private MovementManager MovementManager { get; }

        private Selector<CtbBlackboard> MainSelector { get; }

        private BehaviorTree<CtbBlackboard> BehaviorTree { get; }

        private CtbBlackboard CtbBlackboard => (CtbBlackboard)Blackboard; // Strongly-typed property

        // Objectives

        private readonly IObjective AllianceGate;

        private readonly IObjective HordeGate;

        private readonly List<IObjective> Objectives;

        // Conditions

        private bool GateIsOpen { get; set; } = false;

        // Events

        private TimegatedEvent GateEvent { get; set; }

        private TimegatedEvent NeedToMoveEvent { get; }

        // Constructor

        public AlteracValleyProfile(AmeisenBotInterfaces bot, AmeisenBotConfig config)
        {
            Bot = bot;
            Config = config;

            // change movement provider for one more pvp
            MovementManager = new MovementManager(
                new List<IMovementProvider>()
                {
                    //new SimpleCombatMovementProvider(bot),
                    new BattlegroundMovementProvider(bot, config),
                    //new FollowMovementProvider(bot, config)
                }
            );

            // change dps target provider to one more pvp
            if (Bot.CombatClass != null)
            {
                BattlegroundTargetSelectionLogic targetSelectionLogic = new BattlegroundTargetSelectionLogic(Bot, Config);
                Bot.CombatClass.TargetProviderDps = new TargetManager(targetSelectionLogic, TimeSpan.FromMilliseconds(250));
            }

            // Map variables

            if (Bot.Player.IsAlliance())
            {
                EnemyBasePosition = new Vector3(0, 0, 0);
                EnemyGraveyardPosition = new Vector3(0, 0, 0);
                GatePosition = new Vector3(0, 0, 0);
                OwnBasePosition = new Vector3(0, 0, 0);
                OwnGraveyardPosition = new Vector3(0, 0, 0);
            }
            else
            {
                //EnemyBasePosition = new Vector3(728, -10, 50);
                EnemyBasePosition = new Vector3(652.0164f, -292.12424f, 30.207392f); // Bridge
                EnemyGraveyardPosition = new Vector3(677, -376, 29); // Stormpike Graveyard
                GatePosition = new Vector3(-1383, -546, 54);
                OwnBasePosition = new Vector3(-1383, -546, 54);
                OwnGraveyardPosition = new Vector3(0, 0, 0);
            }

            AllianceGate = new AllianceGate();
            HordeGate = new HordeGate();

            Objectives = new List<IObjective>
            {
                new AllianceBase(),
                AllianceGate,
                new DunBaldarKeep(),
                new StormpikeAidStation(),
                new Stormpike(),
                new Stonehearth(),
                new Snowfall(),
                new Iceblood(),
                new Frostwolf(),
                new FrostwolfReliefHunt(),
                new FrostwolfKeep(),
                HordeGate,
                new HordeBase()
            };

            // Events

            GateEvent = new(TimeSpan.FromMilliseconds(0));
            NeedToMoveEvent = new(TimeSpan.FromSeconds(5));

            // Logic

            MainSelector = new Selector<CtbBlackboard>
            (
                (_) => IsGateOpen(),
                new Selector<CtbBlackboard>
                (
                    (_) => NeedToMove(),
                    new Leaf<CtbBlackboard>((_) => Move(MovementManager.Target)),
                    new Selector<CtbBlackboard>
                    (
                        (_) => NeedToMoveToObjective(),
                        new Leaf<CtbBlackboard>(MoveToObjective),
                        new Leaf<CtbBlackboard>(InitiateCombat)
                    )
                ),
                new Leaf<CtbBlackboard>(MoveToGate)
            );

            // Init

            CtbBlackboard ctbBlackboard = new CtbBlackboard(UpdateBattlegroundInfo);
            BehaviorTree = new BehaviorTree<CtbBlackboard>
            (
                MainSelector,
                ctbBlackboard,
                TimeSpan.FromSeconds(1)
            );
            Blackboard = ctbBlackboard;
        }

        public override void Execute()
        {
            BehaviorTree.Tick();
        }

        private bool IsGateOpen()
        {
            if (!GateIsOpen && GateEvent.Ready)
            {
                double timeRunning = Bot.Wow.GetBattlefieldTimeRunning();

                // Gate open after 2 min the BG starts
                GateIsOpen = timeRunning > 120000;

                // check if gate is open the X time remaining to be opened
                GateEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(120000 - timeRunning));
                GateEvent.Run();
            }

            return GateIsOpen;
        }

        private BtStatus MoveToGate(CtbBlackboard blackboard)
        {
            IObjective objective = Bot.Player.IsAlliance() ? AllianceGate : HordeGate;

            if (!Bot.Movement.RouteInProgress() && Bot.Player.Position.GetDistance(objective.Position) > 20f)
            {
                Bot.Movement.SetMovementFromPath(objective.EndStaticPaths.First().Path);
            }

            return BtStatus.Success;
        }

        private bool NeedToMove()
        {
            // check to move somewhere every 5s
            if (NeedToMoveEvent.Ready)
            {
                bool needToMove = MovementManager.NeedToMove();

                if (needToMove)
                {
                    NeedToMoveEvent.Run();
                }

                return needToMove;
            }

            return false;
        }

        private BtStatus Move(Vector3 position)
        {
            if (position == Vector3.Zero) return BtStatus.Failed;

            return Bot.Movement.SetMovementAction(MovementAction.Follow, position) ? BtStatus.Success : BtStatus.Failed;
        }

        private bool NeedToMoveToObjective()
        {
            return !Bot.Player.IsDead && Bot.Player.Position.GetDistance(EnemyBasePosition) > 40f;
        }

        private BtStatus MoveToObjective(CtbBlackboard blackboard)
        {
            if (Bot.Movement.RouteInProgress())
            {
                return BtStatus.Success;
            }

            // Find the nearest objective to the player
            IObjective nearestObjective = Objectives.MinBy(e => e.Position.GetDistance(Bot.Player.Position));

            // No valid objectives found
            if (nearestObjective == null)
            {
                return BtStatus.Failed;
            }

            StaticPath nearestStaticPath;

            // Determine whether to move directly to the objective or towards the `EnemyBasePosition`
            bool moveToObjectiveFirst = nearestObjective.Position.GetDistance(EnemyBasePosition) < Bot.Player.Position.GetDistance(EnemyBasePosition);
            float distanceToObjective = Bot.Player.Position.GetDistance(nearestObjective.Position);

            // If we need to move to the objective first
            if (moveToObjectiveFirst && distanceToObjective > 20f)
            {
                // Find the nearest path to the objective
                nearestStaticPath = StaticPathFinder.FindNearestPath(Bot.Player.Position, nearestObjective.EndStaticPaths);
            }
            else
            {
                // Find the path that start from the objective that gets us closest to the `EnemyBasePosition`
                nearestStaticPath = nearestObjective.StartStaticPaths.MinBy(e => e.Path.Last().GetDistance(EnemyBasePosition));
            }

            // No valid paths found
            if (nearestStaticPath == null)
            {
                return BtStatus.Failed;
            }

            // Get sub-path where the start point is the nearest position to the player
            List<Vector3> pathToRun = nearestStaticPath.GetPathFromWhereToStart(Bot.Player.Position);

            // If I am further away than 20f try to move there with Pathfinder.
            return MoveAlongPath(pathToRun);
        }

        private BtStatus MoveAlongPath(List<Vector3> path)
        {
            float distanceToStart = Bot.Player.Position.GetDistance(path.First());

            bool onMyWay = distanceToStart > 20f
                ? Bot.Movement.SetMovementAction(MovementAction.Move, path.First())
                : Bot.Movement.SetMovementFromPath(path);

            return onMyWay ? BtStatus.Success : BtStatus.Failed;
        }

        private BtStatus InitiateCombat(CtbBlackboard blackboard)
        {
            if (Bot.CombatClass != null)
            {
                Bot.CombatClass.Execute();
                return BtStatus.Success;
            }

            return BtStatus.Failed;
        }

        private void UpdateBattlegroundInfo()
        {
            try
            {
                var luaCommand = BotUtils.ObfuscateLua("{v:0}='';" +
                    "_, stateA, textA = GetWorldStateUIInfo(1);" +
                    "_, stateH, textH = GetWorldStateUIInfo(2);" +
                    "{v:0}={v:0}..string.format('[\"%s\",\"%s\",\"%s\",\"%s\"]', stateA, textA, stateH, textH);");

                if (Bot.Wow.ExecuteLuaAndRead(luaCommand, out string result))
                {
                    List<string> bgState = JsonSerializer.Deserialize<List<string>>(result);
                    UpdateCtbBlackboard(bgState);
                }
                else
                {
                    AmeisenLogger.I.Log("AlteracValleyProfile", "Failed to execute Lua script", LogLevel.Warning);
                }
            }
            catch (Exception ex)
            {
                AmeisenLogger.I.Log("AlteracValleyProfile", $"Error updating battleground info: {ex.Message}", LogLevel.Error);
            }
        }

        private void UpdateCtbBlackboard(List<string> bgState)
        {
            if (Bot.Player.IsAlliance())
            {
                UpdateTeamScores(bgState);
            }
            else
            {
                UpdateTeamScores(bgState);
            }
        }

        private void UpdateTeamScores(List<string> bgState)
        {
            CtbBlackboard.MyTeamScore = int.Parse(bgState[0]);
            CtbBlackboard.MyTeamMaxScore = int.Parse(SubtractReinforcements(bgState[1]));
            CtbBlackboard.EnemyTeamScore = int.Parse(bgState[2]);
            CtbBlackboard.EnemyTeamMaxScore = int.Parse(SubtractReinforcements(bgState[3]));
        }

        public static string SubtractReinforcements(string input)
        {
            const string prefix = "Reinforcements: ";

            if (input.StartsWith(prefix))
            {
                return input.Substring(prefix.Length);
            }
            else
            {
                throw new ArgumentException("The input string does not start with the expected prefix.");
            }
        }
    }
}
