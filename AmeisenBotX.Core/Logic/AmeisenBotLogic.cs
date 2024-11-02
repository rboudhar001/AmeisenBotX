using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using AmeisenBotX.BehaviorTree;
using AmeisenBotX.BehaviorTree.Enums;
using AmeisenBotX.BehaviorTree.Objects;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Movement;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Engines.Movement.Providers.Basic;
using AmeisenBotX.Core.Engines.Movement.Providers.Special;
using AmeisenBotX.Core.Engines.Movement.StaticPath;
using AmeisenBotX.Core.Engines.Movement.StaticPath.Paths.Dungeons;
using AmeisenBotX.Core.Logic.Enums;
using AmeisenBotX.Core.Logic.Leafs;
using AmeisenBotX.Core.Logic.Routines;
using AmeisenBotX.Core.Managers.Character.Inventory.Objects;
using AmeisenBotX.Core.Objects;
using AmeisenBotX.Core.Objects.Enums;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Memory.Win32;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow.Shared.Lua;

namespace AmeisenBotX.Core.Logic
{
    public class AmeisenBotLogic : IAmeisenBotLogic
    {
        private readonly List<StaticPath> StaticPaths =
        [
            new ForgeOfSoulsDeathRoute(),
            new PitOfSaronDeathRoute()
        ];

        public AmeisenBotLogic(AmeisenBotConfig config, AmeisenBotInterfaces bot)
        {
            Config = config;
            Bot = bot;

            FirstStart = true;
            FirstLogin = true;
            Random = new();

            Mode = BotMode.None;

            AntiAfkEvent = new(TimeSpan.FromMilliseconds(1500));
            CharacterUpdateEvent = new(TimeSpan.FromMilliseconds(5000));
            EatBlockEvent = new(TimeSpan.FromMilliseconds(30000));
            EatEvent = new(TimeSpan.FromMilliseconds(250));
            IdleActionEvent = new(TimeSpan.FromMilliseconds(1000));
            LoginAttemptEvent = new(TimeSpan.FromMilliseconds(500));
            LootTryEvent = new(TimeSpan.FromMilliseconds(750));
            NeedToFightEvent = new(TimeSpan.FromMilliseconds(250));
            PartyMembersFightEvent = new(TimeSpan.FromMilliseconds(1000));
            RenderSwitchEvent = new(TimeSpan.FromMilliseconds(1000));
            UnitsLootedCleanupEvent = new(TimeSpan.FromMilliseconds(1000));
            UpdateFood = new(TimeSpan.FromMilliseconds(1000));

            UnitsLooted = [];
            UnitsToLoot = new();

            MovementManager = new
            (
                new List<IMovementProvider>()
                {
                    new DungeonMovementProvider(bot),
                    new SimpleCombatMovementProvider(bot),
                    new FollowMovementProvider(bot, config)
                }
            );

            // OPEN WORLD -----------------------------

            INode openWorldGhostNode = new Selector
            (
                CanUseStaticPaths,
                // prefer static paths
                new Leaf(RunToCorpseWithStaticPath),
                // run to corpse by position
                new Leaf(RunToCorpse)
            );

            INode combatNode = new Selector
            (
                () => Bot.CombatClass == null,
                // start auto-attacking if we have no combat class loaded
                new Selector
                (
                    () => Bot.Target == null,
                    new SuccessLeaf(() => Bot.Wow.StartAutoAttack()),
                    new Selector
                    (
                        () => !Bot.Player.IsInMeleeRange(Bot.Target),
                        new Leaf(() => MoveToPosition(Bot.Target.Position)),
                        new Selector
                        (
                            () => !BotMath.IsFacing(Bot.Player.Position, Bot.Player.Rotation, Bot.Target.Position),
                            new SuccessLeaf(() => Bot.Wow.FacePosition(Bot.Player.BaseAddress, Bot.Player.Position, Bot.Target.Position)),
                            new Selector
                            (
                                () => !Bot.Player.IsAutoAttacking,
                                new SuccessLeaf(() => { Bot.Wow.StartAutoAttack(); /*Bot.Wow.StopClickToMove();*/ }),
                                new SuccessLeaf()
                            )
                        )
                    )
                ),
                // TODO: handle tactics here run combat class logic
                new SuccessLeaf(() => Bot.CombatClass.Execute())
            );

            INode interactWithMerchantNode = new InteractWithUnitLeaf(Bot, () => Merchant, new SuccessLeaf(() => SpeakToMerchantRoutine.Run(Bot, Merchant)));
            INode interactWithClassTrainerNode = new InteractWithUnitLeaf(Bot, () => ClassTrainer, new SuccessLeaf(() => SpeakToClassTrainerRoutine.Run(Bot, ClassTrainer)));
            INode interactWithProfessionTrainerNode = new InteractWithUnitLeaf(Bot, () => ProfessionTrainer, new SuccessLeaf(() => SpeakToClassTrainerRoutine.Run(Bot, ProfessionTrainer)));

            INode jobsNode = new Waterfall
            (
                new SuccessLeaf(() => Bot.Jobs.Execute()),
                (() => Bot.Player.IsDead, new Leaf(Dead)),
                (() => Bot.Player.IsGhost, openWorldGhostNode),
                (() => !Bot.Player.IsMounted && NeedToFight(), combatNode),
                (NeedToRepairOrSell, interactWithMerchantNode),
                // (NeedToLoot, new Leaf(LootNearUnits)),
                (NeedToEat, new Leaf(Eat))
            );

            INode grindingNode = new Waterfall
            (
                new SuccessLeaf(() => Bot.Grinding.Execute()),
                (() => Bot.Player.IsDead, new Leaf(Dead)),
                (() => Bot.Player.IsGhost, openWorldGhostNode),
                (NeedToFight, combatNode),
                (NeedToRepairOrSell, interactWithMerchantNode),
                (NeedToTrainSpells, interactWithClassTrainerNode),
                (NeedToTrainSecondarySkills, interactWithProfessionTrainerNode),
                (NeedToLoot, new Leaf(LootNearUnits)),
                (NeedToEat, new Leaf(Eat))
            );

            INode questingNode = new Waterfall
            (
                new SuccessLeaf(() => Bot.Quest.Execute()),
                (() => Bot.Player.IsDead, new Leaf(Dead)),
                (() => Bot.Player.IsGhost, openWorldGhostNode),
                (NeedToFight, combatNode),
                (NeedToRepairOrSell, interactWithMerchantNode),
                (NeedToLoot, new Leaf(LootNearUnits)),
                (NeedToEat, new Leaf(Eat))
            );

            INode pvpNode = new Waterfall
            (
                new SuccessLeaf(() => Bot.Pvp.Execute()),
                (() => Bot.Player.IsDead, new Leaf(Dead)),
                (() => Bot.Player.IsGhost, openWorldGhostNode),
                (NeedToFight, combatNode),
                (NeedToRepairOrSell, interactWithMerchantNode),
                (NeedToLoot, new Leaf(LootNearUnits)),
                (NeedToEat, new Leaf(Eat))
            );

            INode testingNode = new Waterfall
            (
                new SuccessLeaf(() => Bot.Test.Execute()),
                (() => Bot.Player.IsDead, new Leaf(Dead)),
                (() => Bot.Player.IsGhost, openWorldGhostNode)
            );

            INode openWorldNode = new Waterfall
            (
                // do idle stuff as fallback
                new SuccessLeaf(() => Bot.CombatClass?.OutOfCombatExecute()),
                // handle main open world states
                (() => Bot.Player.IsDead, new Leaf(Dead)),
                (() => Bot.Player.IsGhost, openWorldGhostNode),
                (NeedToFight, combatNode),
                (NeedToRepairOrSell, interactWithMerchantNode),
                (NeedToLoot, new Leaf(LootNearUnits)),
                (NeedToEat, new Leaf(Eat)),
                (NeedToTalkToQuestgiver, new InteractWithUnitLeaf(Bot, () => QuestGiverToTalkTo)),
                (() => Config.IdleActions && IdleActionEvent.Run(), new SuccessLeaf(() => Bot.IdleActions.Tick(Config.Autopilot)))
            );

            // SPECIAL ENVIRONMENTS -----------------------------

            INode battlegroundNode = new Waterfall
            (
                new SuccessLeaf(() => { Bot.Battleground?.Execute(); }),
                // leave battleground once it is finished
                (IsBattlegroundFinished, new SuccessLeaf(() => { Bot.Wow.LeaveBattleground(); Bot.Battleground?.Reset(); })),
                (() => Bot.Player.IsDead, new Leaf(Dead)),
                (NeedToSaveMyself, new Leaf(SaveMySelf)),
                (NeedToEat, new Leaf(Eat)),
                (NeedToFight, combatNode)
            );

            INode dungeonNode = new Waterfall
            (
                new Selector
                (
                    () => Config.DungeonUsePartyMode,
                    // just follow when we use party mode in dungeon
                    openWorldNode,
                    new SuccessLeaf(() => Bot.Dungeon.Execute())
                ),
                (() => Bot.Player.IsDead, new Leaf(DeadDungeon)),
                (
                    NeedToFight,
                    new Selector
                    (
                        NeedToFollowTactic,
                        new SuccessLeaf(),
                        combatNode
                    )
                ),
                (NeedToLoot, new Leaf(LootNearUnits)),
                (NeedToEat, new Leaf(Eat))
            );

            INode raidNode = new Waterfall
            (
                new Selector
                (
                    () => Config.DungeonUsePartyMode,
                    // just follow when we use party mode in raid
                    new Leaf(Move),
                    new SuccessLeaf(() => Bot.Dungeon.Execute())
                ),
                (
                    NeedToFight,
                    new Selector
                    (
                        NeedToFollowTactic,
                        new SuccessLeaf(),
                        combatNode
                    )
                ),
                (NeedToLoot, new Leaf(LootNearUnits)),
                (NeedToEat, new Leaf(Eat))
            );

            // GENERIC -----------------------------

            INode mainLogicNode = new Annotator
            (
                // run the update stuff before we execute the main logic objects will be updated
                // here for example
                new SuccessLeaf(() => Bot.Wow.Tick()),
                new Selector
                (
                    () => Bot.Objects.IsWorldLoaded && Bot.Player != null && Bot.Objects != null,
                    new Annotator
                    (
                        // update stuff that needs us to be ingame
                        new SuccessLeaf(UpdateIngame),
                        new Waterfall
                        (
                            // open world auto behavior as fallback
                            openWorldNode,
                            // create a path
                            (NeedToCreatePath, new Leaf(CreatePath)),
                            // handle movement
                            //(MovementManager.NeedToMove, new Leaf(Move)),
                            // handle special environments
                            (() => Bot.Objects.MapId.IsBattlegroundMap(), battlegroundNode),
                            (() => Bot.Objects.MapId.IsDungeonMap(), dungeonNode),
                            (() => Bot.Objects.MapId.IsRaidMap(), raidNode),
                            // handle open world modes
                            (() => Mode == BotMode.Grinding, grindingNode),
                            (() => Mode == BotMode.Jobs, jobsNode),
                            (() => Mode == BotMode.Questing, questingNode),
                            (() => Mode == BotMode.PvP, pvpNode),
                            (() => Mode == BotMode.Testing, testingNode)
                        )
                    ),
                    // we are most likely in the loading screen or player/objects are null
                    new SuccessLeaf(() =>
                    {
                        // make sure we dont run after we leave the loading screen
                        Bot.Movement.StopMovement();
                    })
                )
            );

            Tree = new
            (
                new Waterfall
                (
                    // run the anti afk and main logic if wow is running, and we are logged in
                    new Annotator
                    (
                        new SuccessLeaf(AntiAfk),
                        mainLogicNode
                    ),
                    // accept tos and eula, start wow
                    (
                        () => Bot.Memory.Process == null || Bot.Memory.Process.HasExited,
                        new Sequence
                        (
                            new Leaf(CheckTosAndEula),
                            new Leaf(ChangeRealmlist),
                            new Leaf(StartWow)
                        )
                    ),
                    // setup interface and login
                    (() => !Bot.Wow.IsReady, new Leaf(SetupWowInterface)),
                    (NeedToLogin, new SuccessLeaf(Login))
                )
            );
        }

        // Create a path
        // ----------
        private static readonly int DurationInSeconds = 60 * 5;
        private static Vector3[] PathCreated = new Vector3[DurationInSeconds];
        private static int Index = 0;
        private static bool ResetCreatePath = false;
        // ----------

        public event Action OnWoWStarted;

        public BotMode Mode { get; private set; }

        private TimegatedEvent AntiAfkEvent { get; }

        private bool ArePartyMembersInFight { get; set; }

        private AmeisenBotInterfaces Bot { get; }

        private TimegatedEvent CharacterUpdateEvent { get; }

        private IWowUnit ClassTrainer { get; set; }

        private AmeisenBotConfig Config { get; }

        private DateTime DungeonDiedTimestamp { get; set; }

        private TimegatedEvent EatBlockEvent { get; }

        private TimegatedEvent EatEvent { get; }

        private bool FirstLogin { get; set; }

        private bool FirstStart { get; set; }

        private IEnumerable<IWowInventoryItem> Food { get; set; }

        private TimegatedEvent IdleActionEvent { get; }

        private DateTime IngameSince { get; set; }

        private TimegatedEvent LoginAttemptEvent { get; }

        private int LootTry { get; set; }

        private TimegatedEvent LootTryEvent { get; }

        private TimegatedEvent NeedToFightEvent { get; }

        private bool Fighting { get; set; } = false;

        private IWowUnit Merchant { get; set; }

        private MovementManager MovementManager { get; }

        private TimegatedEvent PartyMembersFightEvent { get; }

        private IWowUnit ProfessionTrainer { get; set; }

        private IWowUnit QuestGiverToTalkTo { get; set; }

        private Random Random { get; }

        private TimegatedEvent RenderSwitchEvent { get; }

        private bool SearchedStaticRoutes { get; set; }

        private IStaticPath StaticPath { get; set; }

        private Tree Tree { get; }

        private List<ulong> UnitsLooted { get; }

        private TimegatedEvent UnitsLootedCleanupEvent { get; }

        private Queue<ulong> UnitsToLoot { get; }

        private TimegatedEvent UpdateFood { get; }

        private bool SavingMyself { get; set; } = false;

        public static NpcSubType DecideClassTrainer(WowClass wowClass)
        {
            return wowClass switch
            {
                WowClass.Warrior => NpcSubType.WarriorTrainer,
                WowClass.Paladin => NpcSubType.PaladinTrainer,
                WowClass.Hunter => NpcSubType.HunterTrainer,
                WowClass.Rogue => NpcSubType.RougeTrainer,
                WowClass.Priest => NpcSubType.PriestTrainer,
                WowClass.Deathknight => NpcSubType.DeathKnightTrainer,
                WowClass.Shaman => NpcSubType.ShamanTrainer,
                WowClass.Mage => NpcSubType.MageTrainer,
                WowClass.Warlock => NpcSubType.WarlockTrainer,
                WowClass.Druid => NpcSubType.DruidTrainer,
                _ => throw new NotImplementedException(),
            };
        }

        public void ChangeMode(BotMode mode)
        {
            Mode = mode;

            switch (Mode)
            {
                case BotMode.Questing:
                    Bot.Quest.Enter();
                    break;

                default:
                    break;
            }
        }

        public void Tick()
        {
            Tree.Tick();
        }

        private void AntiAfk()
        {
            if (AntiAfkEvent.Run())
            {
                Bot.Memory.Write(Bot.Memory.Offsets.TickCount, Environment.TickCount);
                AntiAfkEvent.Timegate = TimeSpan.FromMilliseconds(Random.Next(300, 2300));
            }
        }

        /// <summary>
        /// This method searches for static paths, this is needed when pathfinding cannot
        /// find a good route from A to B.For example the ICC dungeons
        /// are only reachable by flying, it's easier to use static paths.
        /// </summary>
        /// <returns>True when a static path can be used, false if not</returns>
        private bool CanUseStaticPaths()
        {
            // already found a path, deleted when body is retrieved.
            if (StaticPath != null) return true;

            // UniversalDungeonEngine will set the dungeon profile when the player enters the dungeon,
            // and will only overwrite it if the player enters another dungeon, it never deletes the profile.

            if (Bot.Dungeon.Profile != null)
            {
                StaticPath staticPath = StaticPaths.FirstOrDefault(e => e.IsUsable(Bot.Objects.MapId));

                // no static path found
                if (staticPath == null) return false;

                StaticPath = staticPath;

                return true;
            }

            return false;
        }

        private BtStatus ChangeRealmlist()
        {
            if (!Config.AutoChangeRealmlist)
            {
                return BtStatus.Success;
            }

            try
            {
                AmeisenLogger.I.Log("StartWow", "Changing Realmlist");
                string configWtfPath = Path.Combine(Directory.GetParent(Config.PathToWowExe).FullName, "wtf", "config.wtf");

                if (File.Exists(configWtfPath))
                {
                    bool editedFile = false;
                    List<string> content = [.. File.ReadAllLines(configWtfPath)];

                    if (!content.Any(e => e.Contains($"SET REALMLIST {Config.Realmlist}", StringComparison.OrdinalIgnoreCase)))
                    {
                        bool found = false;

                        for (int i = 0; i < content.Count; ++i)
                        {
                            if (content[i].Contains("SET REALMLIST", StringComparison.OrdinalIgnoreCase))
                            {
                                editedFile = true;
                                content[i] = $"SET REALMLIST {Config.Realmlist}";
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            editedFile = true;
                            content.Add($"SET REALMLIST {Config.Realmlist}");
                        }
                    }

                    if (editedFile)
                    {
                        File.SetAttributes(configWtfPath, FileAttributes.Normal);
                        File.WriteAllLines(configWtfPath, content);
                        File.SetAttributes(configWtfPath, FileAttributes.ReadOnly);
                    }
                }

                return BtStatus.Success;
            }
            catch
            {
                AmeisenLogger.I.Log("StartWow", "Cannot write realmlist to config.wtf");
            }

            return BtStatus.Failed;
        }

        private BtStatus CheckTosAndEula()
        {
            try
            {
                string configWtfPath = Path.Combine(Directory.GetParent(Config.PathToWowExe).FullName, "wtf", "config.wtf");

                if (File.Exists(configWtfPath))
                {
                    bool editedFile = false;
                    string content = File.ReadAllText(configWtfPath);

                    if (!content.Contains("SET READEULA \"0\"", StringComparison.OrdinalIgnoreCase))
                    {
                        editedFile = true;

                        if (content.Contains("SET READEULA", StringComparison.OrdinalIgnoreCase))
                        {
                            content = content.Replace("SET READEULA \"0\"", "SET READEULA \"1\"", StringComparison.OrdinalIgnoreCase);
                        }
                        else
                        {
                            content += "\nSET READEULA \"1\"";
                        }
                    }

                    if (!content.Contains("SET READTOS \"0\"", StringComparison.OrdinalIgnoreCase))
                    {
                        editedFile = true;

                        if (content.Contains("SET READTOS", StringComparison.OrdinalIgnoreCase))
                        {
                            content = content.Replace("SET READTOS \"0\"", "SET READTOS \"1\"", StringComparison.OrdinalIgnoreCase);
                        }
                        else
                        {
                            content += "\nSET READTOS \"1\"";
                        }
                    }

                    if (!content.Contains("SET MOVIE \"0\"", StringComparison.OrdinalIgnoreCase))
                    {
                        editedFile = true;

                        if (content.Contains("SET MOVIE", StringComparison.OrdinalIgnoreCase))
                        {
                            content = content.Replace("SET MOVIE \"0\"", "SET MOVIE \"1\"", StringComparison.OrdinalIgnoreCase);
                        }
                        else
                        {
                            content += "\nSET MOVIE \"1\"";
                        }
                    }

                    if (editedFile)
                    {
                        File.SetAttributes(configWtfPath, FileAttributes.Normal);
                        File.WriteAllText(configWtfPath, content);
                        File.SetAttributes(configWtfPath, FileAttributes.ReadOnly);
                    }
                }

                return BtStatus.Success;
            }
            catch
            {
                AmeisenLogger.I.Log("StartWow", "Cannot write to config.wtf");
            }

            return BtStatus.Failed;
        }

        private BtStatus Dead()
        {
            SearchedStaticRoutes = false;

            if (Config.ReleaseSpirit || Bot.Objects.MapId.IsBattlegroundMap())
            {
                Bot.Wow.RepopMe();
                Bot.Movement.StopMovement();
                return BtStatus.Ongoing;
            }

            return BtStatus.Success;
        }

        private BtStatus DeadDungeon()
        {
            if (!ArePartyMembersInFight)
            {
                if (DungeonDiedTimestamp == default)
                {
                    DungeonDiedTimestamp = DateTime.UtcNow;
                }
                else if (DateTime.UtcNow - DungeonDiedTimestamp > TimeSpan.FromSeconds(30))
                {
                    Bot.Wow.RepopMe();
                    SearchedStaticRoutes = false;
                    return BtStatus.Success;
                }
            }

            if ((!ArePartyMembersInFight && DateTime.UtcNow - DungeonDiedTimestamp > TimeSpan.FromSeconds(30))
                || Bot.Objects.PartyMembers.Any(e => !e.IsDead
                    && (e.Class == WowClass.Paladin || e.Class == WowClass.Druid || e.Class == WowClass.Priest || e.Class == WowClass.Shaman)))
            {
                // if we died 30s ago or no one that can ress us is alive
                Bot.Wow.RepopMe();
                SearchedStaticRoutes = false;
                return BtStatus.Success;
            }

            return BtStatus.Ongoing;
        }

        private BtStatus SaveMySelf()
        {
            // TODO: I am in a battleground, move to nearest `OwnGraveyardPosition` until I leave combat to eat/drink
            if (Bot.Objects.MapId.IsBattlegroundMap() && !SavingMyself)
            {
                Bot.Wow.StopCasting();
                Bot.Movement.StopMovement();
                SavingMyself = Bot.Movement.SetMovementAction(MovementAction.Move, Bot.Battleground.Profile.EnemyGraveyardPosition);

                return SavingMyself ? BtStatus.Success : BtStatus.Failed;
            }

            return BtStatus.Success;
        }

        private BtStatus Eat()
        {
            if (Bot.Movement.RouteInProgress())
            {
                Bot.Movement.StopMovement();
                return BtStatus.Ongoing;
            }

            if (EatEvent.Run())
            {
                bool isEating = Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Food");
                bool isDrinking = Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Drink");

                if (isEating || isDrinking)
                {
                    return BtStatus.Ongoing;
                }

                bool needToEat = Bot.Player.HealthPercentage < Config.EatStartPercent;
                bool needToDrink = Bot.Player.ManaPercentage < Config.DrinkStartPercent;

                /*
                IWowInventoryItem refreshment = Food.FirstOrDefault(e => Enum.IsDefined(typeof(WowRefreshment), e.Id));
                if ((needToEat || needToDrink) && refreshment != null)
                {
                    Bot.Wow.UseItemByName(refreshment.Name);
                    return BtStatus.Ongoing;
                }
                */

                IWowInventoryItem water = Food.FirstOrDefault(e => Enum.IsDefined(typeof(WowWater), e.Id));
                if (needToDrink && water != null)
                {
                    Bot.Wow.UseItemByName(water.Name);
                    return BtStatus.Success;
                }

                IWowInventoryItem food = Food.FirstOrDefault(e => Enum.IsDefined(typeof(WowFood), e.Id));
                if (needToEat && food != null)
                {
                    Bot.Wow.UseItemByName(food.Name);
                    return BtStatus.Success;
                }

                return BtStatus.Failed;
            }

            return BtStatus.Ongoing;
        }

        private IEnumerable<IWowUnit> GetLootableUnits()
        {
            return Bot.Objects.All.OfType<IWowUnit>()
                .Where(e => e.IsLootable
                    && !UnitsLooted.Contains(e.Guid)
                    && e.Position.GetDistance(Bot.Player.Position) < Config.LootUnitsRadius);
        }

        private bool IsBattlegroundFinished()
        {
            return Bot.Memory.Read(Bot.Memory.Offsets.BattlegroundFinished, out int bgFinished)
                && bgFinished == 1;
        }

        private bool IsRepairNpcNear(out IWowUnit unit)
        {
            unit = Bot.Objects.All.OfType<IWowUnit>()
                    .FirstOrDefault(e => e.GetType() != typeof(IWowPlayer)
                                         && !e.IsDead
                                         && e.IsRepairer
                && Bot.Db.GetReaction(Bot.Player, e) != WowUnitReaction.Hostile
                && Bot.Player.DistanceTo(e) <= Config.RepairNpcSearchRadius);

            return unit != null;
        }

        private bool IsVendorNpcNear(out IWowUnit unit)
        {
            unit = Bot.Objects.All.OfType<IWowUnit>()
                .FirstOrDefault(e => e.GetType() != typeof(IWowPlayer)
                    && !e.IsDead
                    && e.IsVendor
                    && Bot.Db.GetReaction(Bot.Player, e) != WowUnitReaction.Hostile
                    && e.Position.GetDistance(Bot.Player.Position) < Config.RepairNpcSearchRadius);

            return unit != null;
        }

        private void LoadWowWindowPosition()
        {
            if (Config.SaveWowWindowPosition && !Config.AutoPositionWow)
            {
                if (Bot.Memory.Process.MainWindowHandle != nint.Zero && Config.WowWindowRect != new Rect() { Left = -1, Top = -1, Right = -1, Bottom = -1 })
                {
                    Bot.Memory.SetWindowPosition(Bot.Memory.Process.MainWindowHandle, Config.WowWindowRect);
                    AmeisenLogger.I.Log("AmeisenBot", $"Loaded window position: {Config.WowWindowRect}", LogLevel.Verbose);
                }
                else
                {
                    AmeisenLogger.I.Log("AmeisenBot", $"Unable to load window position of {Bot.Memory.Process.MainWindowHandle} to {Config.WowWindowRect}", LogLevel.Warning);
                }
            }
        }

        private void Login()
        {
            Bot.Wow.SetWorldLoadedCheck(true);

            if (FirstLogin)
            {
                FirstLogin = true;
                SetCVars();
            }

            // needed to prevent direct logout due to inactivity
            AntiAfk();

            if (LoginAttemptEvent.Run())
            {
                Bot.Wow.LuaDoString(LuaLogin.Get(Config.Username, Config.Password, Config.Realm, Config.CharacterSlot));
            }

            Bot.Wow.SetWorldLoadedCheck(false);
        }

        private BtStatus LootNearUnits()
        {
            IWowUnit unit = Bot.GetWowObjectByGuid<IWowUnit>(UnitsToLoot.Peek());

            if (unit == null || !unit.IsLootable || LootTry > 2)
            {
                UnitsLooted.Add(UnitsToLoot.Dequeue());
                LootTry = 0;
                return BtStatus.Failed;
            }

            if (unit.Position != Vector3.Zero && Bot.Player.DistanceTo(unit) > 3.0f)
            {
                Bot.Movement.SetMovementAction(MovementAction.Move, unit.Position);
                return BtStatus.Ongoing;
            }
            else if (LootTryEvent.Run())
            {
                if (Bot.Memory.Read(Bot.Memory.Offsets.LootWindowOpen, out byte lootOpen)
                    && lootOpen > 0)
                {
                    Bot.Wow.LootEverything();

                    UnitsLooted.Add(UnitsToLoot.Dequeue());
                    LootTry = 0;

                    Bot.Wow.ClickUiElement("LootCloseButton");
                    return BtStatus.Success;
                }
                else
                {
                    Bot.Wow.StopClickToMove();
                    Bot.Wow.InteractWithUnit(unit);
                    ++LootTry;
                }
            }

            return BtStatus.Ongoing;
        }

        private bool NeedToCreatePath()
        {
            if (Config.CreatePath)
            {
                ResetCreatePath = true;

                return true;
            }

            if (ResetCreatePath)
            {
                ResetCreatePath = false;
                PathCreated = new Vector3[DurationInSeconds];
                Index = 0;
            }

            return false;
        }

        private BtStatus CreatePath()
        {
            if (Index < 300)
            {
                // Save the  new player position every 1s in the array
                PathCreated[Index] = Bot.Player.Position;
                Index++;
            }
            return BtStatus.Ongoing;
        }

        private BtStatus Move()
        {
            return MoveToPosition(MovementManager.Target);
        }

        private BtStatus MoveToPosition(Vector3 position, MovementAction movementAction = MovementAction.Move)
        {
            if (position != Vector3.Zero && Bot.Player.DistanceTo(position) > 3.0f)
            {
                Bot.Movement.SetMovementAction(movementAction, position);
                return BtStatus.Ongoing;
            }

            return BtStatus.Success;
        }

        private bool NeedToSaveMyself()
        {
            bool needToSaveMyself = Bot.Player.IsInCombat && (Bot.Player.HealthPercentage < Config.FightUntilHealth || Bot.Player.ManaPercentage < Config.FightUntilMana);

            // I don't need to save myself, but I am saving myself
            if (!needToSaveMyself && SavingMyself)
            {
                Bot.Movement.StopMovement();
                SavingMyself = false;
                return false;
            }

            // I need to save myself, and I am already saving myself
            if (needToSaveMyself && SavingMyself)
            {
                return false;
            }

            return needToSaveMyself;
        }

        private bool NeedToEat()
        {
            if (Bot.Player.IsInCombat) return false;

            // is eating blocked, used to prevent shredding of food
            /*
            if (!EatBlockEvent.Ready)
            {
                return false;
            }

            // when we are in a group an they move too far away, abort eating and dont start eating for 30s
            if (Config.EatDrinkAbortFollowParty && Bot.Objects.RaidMemberGuids.Any() && Bot.Player.DistanceTo(Bot.Objects.CenterPartyPosition) > Config.EatDrinkAbortFollowPartyDistance)
            {
                EatBlockEvent.Run();
                return false;
            }
            */

            bool isEating = Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Food");
            bool isDrinking = Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Drink");

            // still eating/drinking, wait until threshold is reached
            if ((isEating && Bot.Player.HealthPercentage < Config.EatUntilPercent)
                || (isDrinking && Bot.Player.ManaPercentage < Config.DrinkUntilPercent))
            {
                return true;
            }

            if (UpdateFood.Run())
            {
                Food = Bot.Character.Inventory.Items
                    .Where(e => e.RequiredLevel <= Bot.Player.Level)
                    .OrderByDescending(e => e.ItemLevel);
            }

            return (Bot.Player.ManaPercentage < Config.DrinkStartPercent && Food.Any(e => Enum.IsDefined(typeof(WowWater), e.Id))
                || (Bot.Player.HealthPercentage < Config.EatStartPercent && Food.Any(e => Enum.IsDefined(typeof(WowFood), e.Id))));
        }

        private bool NeedToFight()
        {
            // check every 0.25s
            if (NeedToFightEvent.Run())
            {

                if (Bot.CombatClass?.Role == WowRole.Heal
                    && NeedToHeal())
                {
                    Fighting = true;
                }
                /*
                else if (NeedToFightWithPlayer())
                {
                    Fighting = true;
                }
                */
                /*
                else if (NeedToFightWithMobs())
                {
                    Fighting = true;
                }
                */
                else
                {
                    Fighting = false;
                }
            }

            return Fighting;
        }

        private bool NeedToHeal()
        {
            return Bot.Objects.PartyMembers.Any(e =>
                !e.IsDead
                && e.MaxHealth - e.Health > 1200
                && e.Position.GetDistance(Bot.Player.Position) < Config.SupportRange);
        }

        private bool NeedToFightWithPlayer()
        {
            return Bot.Objects.All.OfType<IWowUnit>().Any(e =>
                e.IsPlayer()
                && !e.IsDead
                && e.Position.GetDistance(Bot.Player.Position) < Config.SupportRange
                && Bot.Db.GetReaction(Bot.Player, e) == WowUnitReaction.Hostile);
        }

        private bool NeedToFightWithMobs()
        {
            if (Bot.Objects.MapId.IsBattlegroundMap()) { return false; }

            if (PartyMembersFightEvent.Run())
            {
                ArePartyMembersInFight = Bot.Objects.PartyMembers.Any(e => e.IsInCombat && e.DistanceTo(Bot.Player) < Config.SupportRange)
                    || Bot.Objects.All.OfType<IWowUnit>().Any(e => e.IsInCombat
                        && (e.IsTaggedByMe || !e.IsTaggedByOther)
                        && (e.TargetGuid == Bot.Player.Guid || Bot.Objects.PartyMembers.Any(x => x.Guid == e.TargetGuid))
                        && Bot.Db.GetReaction(Bot.Player, e) == WowUnitReaction.Hostile);
            }

            return Bot.Player.IsInCombat || ArePartyMembersInFight;
        }

        private bool NeedToFollowTactic()
        {
            return Bot.Tactic.Execute() && !Bot.Tactic.AllowAttacking;
        }

        private bool NeedToLogin()
        {
            return Bot.Memory.Read(Bot.Memory.Offsets.IsIngame, out int isIngame) && isIngame == 0;
        }

        private bool NeedToLoot()
        {
            if (UnitsLootedCleanupEvent.Run())
            {
                UnitsLooted.RemoveAll((guid) =>
                {
                    // remove unit from looted list when its gone or seen alive
                    IWowUnit unit = Bot.GetWowObjectByGuid<IWowUnit>(guid);
                    return unit != null && !unit.IsDead;
                });
            }

            foreach (IWowUnit unit in GetLootableUnits())
            {
                if (!UnitsLooted.Contains(unit.Guid) && !UnitsToLoot.Contains(unit.Guid))
                {
                    UnitsToLoot.Enqueue(unit.Guid);
                }
            }

            return UnitsToLoot.Count > 0;
        }

        private bool NeedToRepairOrSell()
        {
            bool needToRepair = Bot.Character.Equipment.Items.Any(e => e.Value.MaxDurability > 0 && e.Value.Durability / (double)e.Value.MaxDurability * 100.0 <= Config.ItemRepairThreshold);

            bool needToSell = Bot.Character.Inventory.FreeBagSlots < Config.BagSlotsToGoSell
                              && Bot.Character.Inventory.Items
                              .Any(e => e.Price > 0 && !Config.ItemSellBlacklist.Contains(e.Name)
                                      && ((Config.SellGrayItems && e.ItemQuality == (int)WowItemQuality.Poor)
                                      || (Config.SellWhiteItems && e.ItemQuality == (int)WowItemQuality.Common)
                                      || (Config.SellGreenItems && e.ItemQuality == (int)WowItemQuality.Uncommon)
                                      || (Config.SellBlueItems && e.ItemQuality == (int)WowItemQuality.Rare)
                                      || (Config.SellPurpleItems && e.ItemQuality == (int)WowItemQuality.Epic)));

            IWowUnit vendorRepair = null;
            IWowUnit vendorSell = null;

            if (Mode != BotMode.None && Bot.Grinding.Profile?.NpcsOfInterest == null)
            {
                return false;
            }

            switch (Mode)
            {
                case BotMode.Grinding:
                    {
                        Npc repairNpcEntry = Bot.Grinding.Profile.NpcsOfInterest.FirstOrDefault(e => e.Type == NpcType.VendorRepair);

                        if (repairNpcEntry != null)
                        {
                            vendorRepair = Bot.GetClosestVendorByEntryId(repairNpcEntry.EntryId);
                        }

                        Npc sellNpcEntry = Bot.Grinding.Profile.NpcsOfInterest.FirstOrDefault(e => e.Type is NpcType.VendorRepair or NpcType.VendorSellBuy);

                        if (sellNpcEntry != null)
                        {
                            vendorSell = Bot.GetClosestVendorByEntryId(sellNpcEntry.EntryId);
                        }

                        break;
                    }
                case BotMode.None:
                    IsRepairNpcNear(out IWowUnit repairNpc);
                    vendorRepair = repairNpc;

                    IsVendorNpcNear(out IWowUnit sellNpc);
                    vendorSell = sellNpc;
                    break;

                case BotMode.Questing:
                    break;

                case BotMode.PvP:
                    break;

                case BotMode.Testing:
                    break;

                case BotMode.Jobs:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (needToRepair && vendorRepair != null)
            {
                Merchant = vendorRepair;
                return true;
            }

            if (needToSell && vendorSell != null)
            {
                Merchant = vendorSell;
                return true;
            }

            return false;
        }

        private bool NeedToTalkToQuestgiver()
        {
            if (Config.AutoTalkToNearQuestgivers)
            {
                if (Bot.Objects.PartyMembers.Any())
                {
                    List<ulong> guids = [];

                    if (Bot.Objects.PartyLeader != null && Bot.Player.DistanceTo(Bot.Objects.PartyLeader) < 6.0f)
                    {
                        guids.Add(Bot.Objects.PartyLeader.TargetGuid);
                    }

                    foreach (ulong guid in guids)
                    {
                        if (Bot.TryGetWowObjectByGuid(guid, out IWowUnit unit)
                            && Bot.Player.DistanceTo(unit) < 5.6f
                            && unit.IsQuestgiver
                            && Bot.Db.GetReaction(Bot.Player, unit) != WowUnitReaction.Hostile)
                        {
                            QuestGiverToTalkTo = unit;
                            return true;
                        }
                    }
                }
            }

            QuestGiverToTalkTo = null;
            return false;
        }

        private bool NeedToTrainSecondarySkills()
        {
            IWowUnit professionTrainer = null;
            Npc profileTrainer = null;

            if (Bot.Grinding.Profile != null)
            {
                profileTrainer = Bot.Grinding.Profile.NpcsOfInterest?.FirstOrDefault(e =>
                    e.Type == NpcType.ProfessionTrainer);
            }

            if (profileTrainer != null)
            {
                professionTrainer = profileTrainer.SubType switch
                {
                    NpcSubType.FishingTrainer when !Bot.Character.Skills.ContainsKey("Fishing") => Bot
                        .GetClosestTrainerByEntryId(profileTrainer.EntryId),
                    NpcSubType.FirstAidTrainer when !Bot.Character.Skills.ContainsKey("First Aid") => Bot
                        .GetClosestTrainerByEntryId(profileTrainer.EntryId),
                    NpcSubType.CookingTrainer when !Bot.Character.Skills.ContainsKey("Cooking") => Bot
                        .GetClosestTrainerByEntryId(profileTrainer.EntryId),
                    _ => null
                };
            }

            if (professionTrainer == null)
            {
                return false;
            }

            ProfessionTrainer = professionTrainer;
            return ProfessionTrainer != null; // todo: Config.LearnSecondarySkills
        }

        private bool NeedToTrainSpells()
        {
            IWowUnit classTrainer = null;
            Npc profileTrainer = null;

            if (Bot.Grinding.Profile != null)
            {
                profileTrainer = Bot.Grinding.Profile.NpcsOfInterest?.FirstOrDefault(e =>
                    e.Type == NpcType.ClassTrainer && e.SubType == DecideClassTrainer(Bot.Player.Class));
            }

            if (profileTrainer != null)
            {
                classTrainer = Bot.GetClosestTrainerByEntryId(profileTrainer.EntryId);
            }

            if (classTrainer == null)
            {
                return false;
            }

            ClassTrainer = classTrainer;
            return Bot.Character.LastLevelTrained != 0 && Bot.Character.LastLevelTrained < Bot.Player.Level;
        }

        private BtStatus RunToCorpseWithStaticPath()
        {
            if (Bot.Movement.RouteInProgress())
            {
                return BtStatus.Success;
            }

            // get sub-path from static path where the start point is the nearest position to player position
            List<Vector3> pathToRun = StaticPath.GetPathFromWhereToStart(Bot.Player.Position);

            float distanceToStart = Bot.Player.Position.GetDistance(pathToRun.First());

            // If position from where the path start is more than 60m away, try to go there with Pathfinder
            bool onMyWay = distanceToStart > 60
                ? Bot.Movement.SetMovementAction(MovementAction.Move, pathToRun.First())
                : Bot.Movement.SetMovementFromPath(pathToRun);

            return onMyWay ? BtStatus.Success : BtStatus.Failed;
        }

        private BtStatus RunToCorpse()
        {
            if (!Bot.Memory.Read(Bot.Memory.Offsets.CorpsePosition, out Vector3 corpsePosition))
            {
                return BtStatus.Failed;
            }

            if (Bot.Player.Position.GetDistance(corpsePosition) > Config.GhostResurrectThreshold)
            {
                if (Bot.Movement.RouteInProgress())
                {
                    return BtStatus.Success;
                }

                Bot.Movement.SetMovementAction(MovementAction.Move, corpsePosition);
                return BtStatus.Ongoing;
            }

            Bot.Wow.RetrieveCorpse();
            return BtStatus.Success;
        }

        private void SetCVars()
        {
            List<(string, string)> cvars =
            [
                ("maxfps", $"{Config.MaxFps}"),
                ("maxfpsbk", $"{Config.MaxFps}"),
                ("AutoInteract", "1"),
                ("AutoLootDefault", "0"),
            ];

            if (Config.AutoSetUlowGfxSettings)
            {
                cvars.AddRange(new (string, string)[]
                {
                    ("alphalevel", "1"),
                    ("anisotropic", "0"),
                    ("basemip", "1"),
                    ("bitdepth", "16"),
                    ("characterAmbient", "1"),
                    ("detaildensity", "1"),
                    ("detailDoodadAlpha", "0"),
                    ("doodadanim", "0"),
                    ("environmentDetail", "0.5"),
                    ("extshadowquality", "0"),
                    ("farclip", "177"),
                    ("ffx", "0"),
                    ("fog", "0"),
                    ("fullalpha", "0"),
                    ("groundeffectdensity", "16"),
                    ("groundeffectdist", "1"),
                    ("gxcolorbits", "16"),
                    ("gxdepthbits", "16"),
                    ("horizonfarclip", "1305"),
                    ("hwPCF", "1"),
                    ("light", "0"),
                    ("lod", "0"),
                    ("loddist", "50"),
                    ("m2Faster", "1"),
                    ("mapshadows", "0"),
                    ("maxlights", "0"),
                    ("maxlod", "0"),
                    ("overridefarclip ", "0"),
                    ("particledensity", "0.3"),
                    ("pixelshader", "0"),
                    ("shadowlevel", "1"),
                    ("shadowlod", "0"),
                    ("showfootprintparticles", "0"),
                    ("showfootprints", "0"),
                    ("showshadow", "0"),
                    ("showwater", "0"),
                    ("skyclouddensity", "0"),
                    ("skycloudlod", "0"),
                    ("skyshow", "0"),
                    ("skysunglare", "0"),
                    ("smallcull", "1"),
                    ("specular", "0"),
                    ("textureloddist", "80"),
                    ("timingmethod", "1"),
                    ("unitdrawdist", "20"),
                    ("waterlod", "0"),
                    ("watermaxlod", "0"),
                    ("waterparticulates", "0"),
                    ("waterripples", "0"),
                    ("waterspecular", "0"),
                    ("waterwaves", "0"),
                });
            }

            StringBuilder sb = new();

            foreach ((string cvar, string value) in cvars)
            {
                sb.Append($"pcall(SetCVar,\"{cvar}\",\"{value}\");");
            }

            Bot.Wow.LuaDoString(sb);
        }

        private BtStatus SetupWowInterface()
        {
            return Bot.Wow.Setup() ? BtStatus.Success : BtStatus.Failed;
        }

        private BtStatus StartWow()
        {
            if (File.Exists(Config.PathToWowExe))
            {
                AmeisenLogger.I.Log("StartWow", "Starting WoW Process");
                Process p = Bot.Memory.StartProcessNoActivate($"\"{Config.PathToWowExe}\" -windowed -d3d9", out nint processHandle, out nint mainThreadHandle);
                p.WaitForInputIdle();

                Thread.Sleep(1000); // needed to spin up wow's window

                AmeisenLogger.I.Log("StartWow", $"Attaching XMemory to {p.ProcessName} ({p.Id})");

                if (Bot.Memory.Init(p, processHandle, mainThreadHandle))
                {
                    Bot.Memory.Offsets.Init(Bot.Memory.Process.MainModule.BaseAddress);

                    if (Config.SaveWowWindowPosition)
                    {
                        LoadWowWindowPosition();
                    }

                    OnWoWStarted?.Invoke();

                    return BtStatus.Success;
                }
                else
                {
                    AmeisenLogger.I.Log("StartWow", $"Attaching XMemory failed...");
                    p.Kill();
                    return BtStatus.Failed;
                }
            }

            return BtStatus.Failed;
        }

        private void UpdateIngame()
        {
            if (FirstStart)
            {
                FirstStart = false;
                IngameSince = DateTime.UtcNow;
            }

            if (Bot.Wow.Events != null)
            {
                if (!Bot.Wow.Events.IsActive && DateTime.UtcNow - IngameSince > TimeSpan.FromSeconds(2))
                {
                    // need to wait for the Frame setup
                    Bot.Wow.Events.Start();
                }

                Bot.Wow.Events.Tick();
            }

            Bot.Movement.Execute();

            if (CharacterUpdateEvent.Run())
            {
                Bot.Character.UpdateAll();
            }

            if (!Bot.Player.IsDead)
            {
                DungeonDiedTimestamp = default;
            }

            // auto disable rendering when not in focus
            if (Config.AutoDisableRender && RenderSwitchEvent.Run())
            {
                nint foregroundWindow = Bot.Memory.GetForegroundWindow();
                Bot.Wow.SetRenderState(foregroundWindow == Bot.Memory.Process.MainWindowHandle);
            }
        }
    }
}