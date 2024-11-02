using AmeisenBotX.BehaviorTree;
using AmeisenBotX.BehaviorTree.Enums;
using AmeisenBotX.BehaviorTree.Objects;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Logic.Routines;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AmeisenBotX.Core.Engines.Battleground.Jannis.Profiles.WarsongGulch
{
    public class WarsongGulchProfile : BattlegroundProfile
    {

        public WarsongGulchProfile(AmeisenBotInterfaces bot)
        {
            Bot = bot;

            if (Bot.Player.IsAlliance())
            {
                EnemyBasePosition = new(916, 1434, 346);
                EnemyBasePositionMapCoords = new(53, 90, 0);
                EnemyGraveyardPosition = new(1415, 1555, 343);
                FlagHidingSpot = new(1519, 1467, 374);
                GatePosition = new(1494, 1457 + (float)(new Random().NextDouble() * 16.0f - 8.0f), 343);
                OwnBasePosition = new(1539, 1481, 352);
                OwnBasePositionMapCoords = new(49, 15, 0);
                OwnGraveyardPosition = new(1029, 1387, 340);
            }
            else
            {
                EnemyBasePosition = new(1539, 1481, 352);
                EnemyBasePositionMapCoords = new(49, 15, 0);
                EnemyGraveyardPosition = new(1029, 1387, 340);
                FlagHidingSpot = new(949, 1449, 367);
                GatePosition = new(951, 1459 + (float)(new Random().NextDouble() * 16.0f - 8.0f), 342);
                OwnBasePosition = new(916, 1434, 346);
                OwnBasePositionMapCoords = new(53, 90, 0);
                OwnGraveyardPosition = new(1415, 1555, 343);
            }

            ActionEvent = new(TimeSpan.FromMilliseconds(500));
            LosCheckEvent = new(TimeSpan.FromMilliseconds(1000));

            var ctfBlackboard = new CtfBlackboard(UpdateBattlegroundInfo);
            Blackboard = ctfBlackboard;

            KillEnemyFlagCarrierSelector = new
            (
                (b) => b.EnemyTeamFlagCarrier != null,
                new Selector<CtfBlackboard>
                (
                    (b) => b.MyTeamHasFlag
                        || b.EnemyTeamFlagCarrier.Position.GetDistance(WsgDataset.EnemyBasePosition)
                         < Bot.Player.Position.GetDistance(WsgDataset.EnemyBasePosition),
                    new Leaf<CtfBlackboard>(KillEnemyFlagCarrier),
                    new Leaf<CtfBlackboard>(MoveToEnemyBaseAndGetFlag)
                ),
                new Selector<CtfBlackboard>
                (
                    (b) => b.EnemyTeamFlagPos.GetDistance2D(WsgDataset.EnemyBasePositionMapCoords)
                         < b.EnemyTeamFlagPos.GetDistance2D(WsgDataset.OwnBasePositionMapCoords),
                    new Leaf<CtfBlackboard>((b) => MoveToPosition(WsgDataset.EnemyBasePosition)),
                    new Leaf<CtfBlackboard>((b) => MoveToPosition(WsgDataset.OwnBasePosition))
                )
            );

            FlagSelector = new DualSelector<CtfBlackboard>
            (
                (b) => b.MyTeamHasFlag,
                (b) => b.EnemyTeamHasFlag,

                // no team has the flag
                new Leaf<CtfBlackboard>(MoveToEnemyBaseAndGetFlag),

                // only my team has the flag
                new Selector<CtfBlackboard>
                (
                    (b) => b.MyTeamFlagCarrier != null && b.MyTeamFlagCarrier.Guid == Bot.Wow.PlayerGuid,
                    // i'm the flag carrier
                    new Leaf<CtfBlackboard>((b) => MoveToPosition(WsgDataset.OwnBasePosition)),
                    new Selector<CtfBlackboard>
                    (
                        (b) => b.MyTeamFlagCarrier != null,
                        new Leaf<CtfBlackboard>((b) => MoveToPosition(b.MyTeamFlagCarrier.Position)),
                        new Leaf<CtfBlackboard>(DefendOwnBase)
                    )
                ),

                // only enemy team has the flag
                KillEnemyFlagCarrierSelector,

                // both teams have the flag
                new Selector<CtfBlackboard>
                (
                    (b) => b.MyTeamFlagCarrier != null && b.MyTeamFlagCarrier.Guid == Bot.Wow.PlayerGuid,
                    new Selector<CtfBlackboard>
                    (
                        (b) => Bot.Player.Position.GetDistance(WsgDataset.OwnBasePosition) < 128.0f,
                        new Leaf<CtfBlackboard>((b) => MoveToPosition(WsgDataset.FlagHidingSpot)),
                        new Leaf<CtfBlackboard>((b) => MoveToPosition(WsgDataset.OwnBasePosition))
                    ),
                    new Selector<CtfBlackboard>
                    (
                        (b) => b.MyTeamFlagCarrier != null
                            && b.MyTeamFlagCarrier.DistanceTo(Bot.Player) < 50.0f
                            && Bot.GetNearFriends<IWowPlayer>(b.MyTeamFlagCarrier.Position, 25.0f).Count() < 2,
                        // assist our flag carrier
                        new Leaf<CtfBlackboard>((b) => MoveToPosition(b.MyTeamFlagCarrier.Position)),
                        // go kill other flag carrier
                        KillEnemyFlagCarrierSelector
                    )
                )
            );

            MainSelector = new Selector<CtfBlackboard>
            (
                (b) => Bot.Player.IsDead,
                new Leaf<CtfBlackboard>(Dead),
                new Selector<CtfBlackboard>
                (
                    (b) => IsGateOpen(),
                    new Selector<CtfBlackboard>
                    (
                        (b) => IsFlagNear(),
                        new Leaf<CtfBlackboard>(UseNearestFlag),
                        new Selector<CtfBlackboard>
                        (
                            (b) => IsAnyBuffNearMeAndNoOneElseUsingIt(16.0f),
                            new Leaf<CtfBlackboard>(MoveToNearestBuff),
                            new Selector<CtfBlackboard>
                            (
                                (b) => DoWeOutnumberOurEnemies(b),
                                new Leaf<CtfBlackboard>(AttackNearWeakestEnemy),
                                FlagSelector
                            )
                        )
                    ),
                    new Leaf<CtfBlackboard>((b) => MoveToPosition(WsgDataset.GatePosition))
                 )
            );

            BehaviorTree = new
            (
                MainSelector,
                ctfBlackboard,
                TimeSpan.FromSeconds(1)
            );
        }

        public override Vector3 EnemyBasePosition { get; }

        public Vector3 EnemyBasePositionMapCoords { get; }

        public override Vector3 EnemyGraveyardPosition { get; }

        public Vector3 FlagHidingSpot { get; }

        public override Vector3 GatePosition { get; }

        public override Vector3 OwnBasePosition { get; }

        public Vector3 OwnBasePositionMapCoords { get; }

        public override Vector3 OwnGraveyardPosition { get; }

        public BehaviorTree<CtfBlackboard> BehaviorTree { get; }

        public DualSelector<CtfBlackboard> FlagSelector { get; }

        public override Blackboard Blackboard { get; }

        private CtfBlackboard CtfBlackboard => (CtfBlackboard)Blackboard; // Strongly-typed property

        public Selector<CtfBlackboard> MainSelector { get; }

        private TimegatedEvent ActionEvent { get; }

        private AmeisenBotInterfaces Bot { get; }

        private bool InLos { get; set; }

        private Selector<CtfBlackboard> KillEnemyFlagCarrierSelector { get; }

        private TimegatedEvent LosCheckEvent { get; }

        private IWsgDataset WsgDataset { get; set; }

        public override void Execute()
        {
            WsgDataset ??= Bot.Player.IsAlliance() ? new AllianceWsgDataset() : new HordeWsgDataset();

            BehaviorTree.Tick();
        }

        private bool AmINearOwnFlagCarrier(CtfBlackboard blackboard)
        {
            return Bot.GetNearEnemies<IWowUnit>(blackboard.MyTeamFlagCarrier.Position, 48.0f).Any(e => !e.IsDead);
        }

        private bool AmIOneOfTheClosestToOwnFlagCarrier(CtfBlackboard blackboard, int memberCount)
        {
            if (memberCount <= 0 || blackboard.MyTeamFlagCarrier == null)
            {
                return false;
            }

            // check whether i'm part of the closest x (memberCount) members to the flag carrier
            int index = Bot.Objects.PartyMembers.OfType<IWowPlayer>()
                            .Where(e => e.Guid != blackboard.MyTeamFlagCarrier.Guid)
                            .OrderBy(e => e.Position.GetDistance(Bot.Player.Position))
                            .Select((player, id) => new { Player = player, Index = id })
                            .FirstOrDefault(_ => _.Player.Guid == Bot.Wow.PlayerGuid)?.Index ?? -1;

            return index > -1 && index <= memberCount;
        }

        private BtStatus AttackNearWeakestEnemy(CtfBlackboard blackboard)
        {
            IWowPlayer weakestPlayer = Bot.GetNearEnemies<IWowPlayer>(Bot.Player.Position, 20.0f).OrderBy(e => e.Health).FirstOrDefault();

            if (weakestPlayer != null)
            {
                InitiateCombat(weakestPlayer);
                return BtStatus.Success;
            }

            return BtStatus.Failed;
        }

        private BtStatus DefendOwnBase(CtfBlackboard blackboard)
        {
            if (!CommonRoutines.MoveToTarget(Bot, WsgDataset.OwnBasePosition, 16.0f))
            {
                IWowUnit nearEnemy = Bot.GetNearEnemies<IWowUnit>(WsgDataset.OwnBasePosition, 16.0f).FirstOrDefault();

                if (nearEnemy != null)
                {
                    InitiateCombat(nearEnemy);
                }
            }

            return BtStatus.Ongoing;
        }

        private bool DoWeOutnumberOurEnemies(CtfBlackboard blackboard)
        {
            if (blackboard.MyTeamFlagCarrier != null && blackboard.MyTeamFlagCarrier.Guid == Bot.Wow.PlayerGuid)
            {
                return false;
            }

            int friends = Bot.GetNearFriends<IWowPlayer>(Bot.Player.Position, 18.0f).Count();
            int enemies = Bot.GetNearEnemies<IWowPlayer>(Bot.Player.Position, 18.0f).Count();

            return enemies > 0 && friends >= enemies;
        }

        private bool EnemiesNearFlagCarrier(CtfBlackboard blackboard)
        {
            return blackboard.MyTeamFlagCarrier.Position.GetDistance(Bot.Player.Position) < 32.0;
        }

        private BtStatus FleeFromComingEnemies()
        {
            IWowUnit nearestEnemy = Bot.GetNearEnemies<IWowUnit>(Bot.Player.Position, 48.0f).OrderBy(e => e.Position.GetDistance(Bot.Player.Position)).FirstOrDefault();

            if (nearestEnemy != null)
            {
                Bot.Movement.SetMovementAction(MovementAction.Flee, nearestEnemy.Position, nearestEnemy.Rotation);
                return BtStatus.Ongoing;
            }

            return BtStatus.Success;
        }

        private void InitiateCombat(IWowUnit unit)
        {
            if (Bot.Target == null)
            {
                Bot.Wow.ChangeTarget(unit.Guid);
                return;
            }

            MovementAction action = Bot.Player.DistanceTo(WsgDataset.OwnGraveyardPosition) < 24.0f
                || Bot.Player.DistanceTo(WsgDataset.EnemyGraveyardPosition) < 24.0f
                    ? MovementAction.DirectMove
                    : MovementAction.Chase;

            if (!CommonRoutines.MoveToTarget(Bot, Bot.Target.Position, Bot.CombatClass.IsMelee ? 3.0f : 28.0f, action))
            {
                Bot.CombatClass?.Execute();
            }
        }

        private bool IsAnyBuffNearMeAndNoOneElseUsingIt(float distance)
        {
            IWowGameobject buff = Bot.GetClosestGameObjectByDisplayId(Bot.Player.Position, IWsgDataset.BuffDisplayIds);
            return buff != null && buff.Position.GetDistance(Bot.Player.Position) < distance && !Bot.GetNearPartyMembers<IWowPlayer>(buff.Position, 8.0f).Any();
        }

        private bool IsFlagNear()
        {
            return CtfBlackboard.NearFlags != null && CtfBlackboard.NearFlags.Any(e => e.Position.GetDistance(Bot.Player.Position) < 8.0);
        }

        private bool IsGateOpen()
        {
            if (Bot.Player.IsAlliance())
            {
                IWowGameobject obj = Bot.Objects.All.OfType<IWowGameobject>()
                                    .FirstOrDefault(e => e.GameObjectType == WowGameObjectType.Door && e.DisplayId == 411);

                return obj == null || obj.Bytes0 == 0;
            }
            else
            {
                IWowGameobject obj = Bot.Objects.All.OfType<IWowGameobject>()
                                    .FirstOrDefault(e => e.GameObjectType == WowGameObjectType.Door && e.DisplayId == 850);

                return obj == null || obj.Bytes0 == 0;
            }
        }

        private BtStatus KillEnemyFlagCarrier(CtfBlackboard blackboard)
        {
            if (CtfBlackboard.EnemyTeamFlagCarrier == null)
            {
                return BtStatus.Failed;
            }

            InitiateCombat(CtfBlackboard.EnemyTeamFlagCarrier);
            return BtStatus.Ongoing;
        }

        private BtStatus MoveToEnemyBaseAndGetFlag(CtfBlackboard blackboard)
        {
            return !CommonRoutines.MoveToTarget(Bot, WsgDataset.EnemyBasePosition, 2.0f) && CtfBlackboard.NearFlags != null
                ? UseNearestFlag(blackboard)
                : BtStatus.Ongoing;
        }

        private BtStatus Dead(CtfBlackboard blackboard)
        {
            Bot.Wow.RepopMe();
            Bot.Movement.StopMovement();
            return BtStatus.Success;
        }

        private BtStatus MoveToNearestBuff(CtfBlackboard blackboard)
        {
            IWowGameobject buffObject = Bot.GetClosestGameObjectByDisplayId(Bot.Player.Position, IWsgDataset.BuffDisplayIds);

            return buffObject == null || !CommonRoutines.MoveToTarget(Bot, buffObject.Position, 3.0f) ? BtStatus.Failed : BtStatus.Ongoing;
        }

        private BtStatus MoveToOwnFlagCarrierAndHelp()
        {
            if (CtfBlackboard.MyTeamFlagCarrier == null)
            {
                return BtStatus.Failed;
            }

            if (!CommonRoutines.MoveToTarget(Bot, CtfBlackboard.MyTeamFlagCarrier.Position, 5.0f))
            {
                IWowUnit nearEnemy = Bot.GetNearEnemies<IWowUnit>(CtfBlackboard.MyTeamFlagCarrier.Position, 32.0f).FirstOrDefault();

                if (nearEnemy != null)
                {
                    InitiateCombat(nearEnemy);
                    return BtStatus.Success;
                }

                return BtStatus.Failed;
            }

            return BtStatus.Ongoing;
        }

        private BtStatus MoveToPosition(Vector3 position, float minDistance = 3.5f)
        {
            double distance = Bot.Player.Position.GetDistance(position);

            if (distance > minDistance)
            {
                if (LosCheckEvent.Run())
                {
                    InLos = Bot.Wow.IsInLineOfSight(Bot.Player.Position, position, 2.0f);
                }

                float zDiff = position.Z - Bot.Player.Position.Z;

                MovementAction action = zDiff < -4.0 && InLos
                    || Bot.Player.DistanceTo(WsgDataset.OwnGraveyardPosition) < 24.0f
                    || Bot.Player.DistanceTo(WsgDataset.EnemyGraveyardPosition) < 24.0f
                        ? MovementAction.DirectMove
                        : MovementAction.Move;

                Bot.Movement.SetMovementAction(action, position);

                return BtStatus.Ongoing;
            }

            return BtStatus.Success;
        }

        private void UpdateBattlegroundInfo()
        {
            try
            {
                if (Bot.Wow.ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=\"{{\"_,stateA,textA,_,_,_,_,_,_,_,_,_=GetWorldStateUIInfo(2)_,stateH,textH,_,_,_,_,_,_,_,_,_=GetWorldStateUIInfo(3)flagXA,flagYA=GetBattlefieldFlagPosition(1)flagXH,flagYH=GetBattlefieldFlagPosition(2){{v:0}}={{v:0}}..\"\\\"allianceState\\\" : \\\"\"..stateA..\"\\\",\"{{v:0}}={{v:0}}..\"\\\"allianceText\\\" : \\\"\"..textA..\"\\\",\"{{v:0}}={{v:0}}..\"\\\"hordeState\\\" : \\\"\"..stateH..\"\\\",\"{{v:0}}={{v:0}}..\"\\\"hordeText\\\" : \\\"\"..textH..\"\\\",\"{{v:0}}={{v:0}}..\"\\\"allianceFlagX\\\" : \\\"\"..flagXA..\"\\\",\"{{v:0}}={{v:0}}..\"\\\"allianceFlagY\\\" : \\\"\"..flagYA..\"\\\",\"{{v:0}}={{v:0}}..\"\\\"hordeFlagX\\\" : \\\"\"..flagXH..\"\\\",\"{{v:0}}={{v:0}}..\"\\\"hordeFlagY\\\" : \\\"\"..flagYH..\"\\\"\"{{v:0}}={{v:0}}..\"}}\""), out string result))
                {
                    Dictionary<string, dynamic> bgState = JsonSerializer.Deserialize<JsonElement>(result, Options).ToDyn();

                    string[] splittedScoreH = ((string)bgState["hordeText"]).Split('/');
                    string[] splittedScoreA = ((string)bgState["allianceText"]).Split('/');

                    float allianceFlagX = float.Parse((string)bgState["allianceFlagX"], NumberStyles.Any) * 100.0f;
                    float allianceFlagY = float.Parse((string)bgState["allianceFlagY"], NumberStyles.Any) * 100.0f;

                    float hordeFlagX = float.Parse((string)bgState["hordeFlagX"], NumberStyles.Any) * 100.0f;
                    float hordeFlagY = float.Parse((string)bgState["hordeFlagY"], NumberStyles.Any) * 100.0f;

                    if (Bot.Player.IsAlliance())
                    {
                        CtfBlackboard.MyTeamScore = int.Parse(splittedScoreA[0]);
                        CtfBlackboard.MyTeamMaxScore = int.Parse(splittedScoreA[1]);

                        CtfBlackboard.EnemyTeamScore = int.Parse(splittedScoreH[0]);
                        CtfBlackboard.EnemyTeamMaxScore = int.Parse(splittedScoreH[1]);

                        CtfBlackboard.MyTeamHasFlag = int.Parse((string)bgState["allianceState"]) == 2;
                        CtfBlackboard.EnemyTeamHasFlag = int.Parse((string)bgState["hordeState"]) == 2;

                        CtfBlackboard.MyTeamFlagPos = new(allianceFlagX, allianceFlagY, 0.0f);
                        CtfBlackboard.EnemyTeamFlagPos = new(hordeFlagX, hordeFlagY, 0.0f);

                        CtfBlackboard.MyTeamFlagCarrier = Bot.Objects.All.OfType<IWowPlayer>().FirstOrDefault(e => e.HasBuffById(23333));
                        CtfBlackboard.EnemyTeamFlagCarrier = Bot.Objects.All.OfType<IWowPlayer>().FirstOrDefault(e => e.HasBuffById(23335));
                    }
                    else
                    {
                        CtfBlackboard.MyTeamScore = int.Parse(splittedScoreH[0]);
                        CtfBlackboard.MyTeamMaxScore = int.Parse(splittedScoreH[1]);

                        CtfBlackboard.EnemyTeamScore = int.Parse(splittedScoreA[0]);
                        CtfBlackboard.EnemyTeamMaxScore = int.Parse(splittedScoreA[1]);

                        CtfBlackboard.MyTeamHasFlag = int.Parse((string)bgState["hordeState"]) == 2;
                        CtfBlackboard.EnemyTeamHasFlag = int.Parse((string)bgState["allianceState"]) == 2;

                        CtfBlackboard.MyTeamFlagPos = new(hordeFlagX, hordeFlagY, 0.0f);
                        CtfBlackboard.EnemyTeamFlagPos = new(allianceFlagX, allianceFlagY, 0.0f);

                        CtfBlackboard.MyTeamFlagCarrier = Bot.Objects.All.OfType<IWowPlayer>().FirstOrDefault(e => e.HasBuffById(23335));
                        CtfBlackboard.EnemyTeamFlagCarrier = Bot.Objects.All.OfType<IWowPlayer>().FirstOrDefault(e => e.HasBuffById(23333));
                    }

                    CtfBlackboard.NearFlags = Bot.Objects.All.OfType<IWowGameobject>()
                                                 .Where(e => e.DisplayId is ((int)WowGameObjectDisplayId.WsgAllianceFlag)
                                                          or ((int)WowGameObjectDisplayId.WsgHordeFlag));
                }
            }
            catch { }
        }

        private BtStatus UseNearestFlag(CtfBlackboard blackboard)
        {
            IWowGameobject nearestFlag = CtfBlackboard.NearFlags.OrderBy(e => e.Position.GetDistance(Bot.Player.Position)).FirstOrDefault();

            if (nearestFlag != null)
            {
                if (!CommonRoutines.MoveToTarget(Bot, nearestFlag.Position, 4.0f) && ActionEvent.Run())
                {
                    Bot.Wow.InteractWithObject(nearestFlag);
                    return BtStatus.Success;
                }

                return BtStatus.Ongoing;
            }

            return BtStatus.Failed;
        }

        private interface IWsgDataset
        {
            static readonly List<int> BuffDisplayIds = [5991, 5995, 5931];

            Vector3 EnemyBasePosition { get; }

            Vector3 EnemyBasePositionMapCoords { get; }

            Vector3 EnemyGraveyardPosition { get; }

            Vector3 FlagHidingSpot { get; }

            Vector3 GatePosition { get; }

            Vector3 OwnBasePosition { get; }

            Vector3 OwnBasePositionMapCoords { get; }

            Vector3 OwnGraveyardPosition { get; }
        }

        private class AllianceWsgDataset : IWsgDataset
        {
            public Vector3 EnemyBasePosition { get; } = new(916, 1434, 346);

            public Vector3 EnemyBasePositionMapCoords { get; } = new(53, 90, 0);

            public Vector3 EnemyGraveyardPosition { get; } = new(1415, 1555, 343);

            public Vector3 FlagHidingSpot { get; } = new(1519, 1467, 374);

            public Vector3 GatePosition { get; } = new(1494, 1457 + (float)(new Random().NextDouble() * 16.0f - 8.0f), 343);

            public Vector3 OwnBasePosition { get; } = new(1539, 1481, 352);

            public Vector3 OwnBasePositionMapCoords { get; } = new(49, 15, 0);

            public Vector3 OwnGraveyardPosition { get; } = new(1029, 1387, 340);
        }

        private class HordeWsgDataset : IWsgDataset
        {
            public Vector3 EnemyBasePosition { get; } = new(1539, 1481, 352);

            public Vector3 EnemyBasePositionMapCoords { get; } = new(49, 15, 0);

            public Vector3 EnemyGraveyardPosition { get; } = new(1029, 1387, 340);

            public Vector3 FlagHidingSpot { get; } = new(949, 1449, 367);

            public Vector3 GatePosition { get; private set; } = new(951, 1459 + (float)(new Random().NextDouble() * 16.0f - 8.0f), 342);

            public Vector3 OwnBasePosition { get; } = new(916, 1434, 346);

            public Vector3 OwnBasePositionMapCoords { get; } = new(53, 90, 0);

            public Vector3 OwnGraveyardPosition { get; } = new(1415, 1555, 343);
        }
    }
}