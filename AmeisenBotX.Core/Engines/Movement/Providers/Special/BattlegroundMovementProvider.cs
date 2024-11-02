using System;
using System.Collections.Generic;
using System.Linq;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Wow.Objects;

namespace AmeisenBotX.Core.Engines.Movement.Providers.Special
{
    public class BattlegroundMovementProvider(AmeisenBotInterfaces bot, AmeisenBotConfig config) : IMovementProvider
    {
        private AmeisenBotInterfaces Bot { get; } = bot;

        private AmeisenBotConfig Config { get; } = config;

        private Vector3 FollowOffset { get; set; }

        private TimegatedEvent OffsetCheckEvent { get; } = new(TimeSpan.FromSeconds(30));

        private Random Random { get; } = new();

        public bool Get(out Vector3 position, out MovementAction type)
        {
            if (!Bot.Player.IsDead && !Bot.Player.IsInCombat)
            {
                // calculate offset to use if dynamic position is enabled
                if (Config.FollowPositionDynamic && OffsetCheckEvent.Run())
                {
                    float factor = Bot.Player.IsOutdoors ? 2.0f : 1.0f;

                    FollowOffset = new Vector3()
                    {
                        X = ((float)Random.NextDouble() * (Config.MinFollowDistance * factor) - (Config.MinFollowDistance * (0.5f * factor))) * 0.7071f,
                        Y = ((float)Random.NextDouble() * (Config.MinFollowDistance * factor) - (Config.MinFollowDistance * (0.5f * factor))) * 0.7071f,
                        Z = 0.0f
                    };
                }

                // get player to follow
                if (TryGetPlayerToFollow(out IWowUnit player))
                {
                    type = MovementAction.Move;
                    position = BotMath.CalculatePositionBetween(Bot.Player.Position, player.Position, 30);
                    return true;
                }
            }

            type = MovementAction.None;
            position = Vector3.Zero;
            return false;
        }

        private bool TryGetPlayerToFollow(out IWowUnit playerToFollow)
        {
            IEnumerable<IWowPlayer> wowPlayers = Bot.Objects.All.OfType<IWowPlayer>().Where(e => !e.IsDead);

            IWowPlayer[] players = wowPlayers as IWowPlayer[] ?? wowPlayers.ToArray();
            if (players.Length != 0)
            {
                // follow specific
                if (Config.FollowSpecificCharacter)
                {
                    IWowUnit specificPlayer = players.FirstOrDefault(p => Bot.Db.GetUnitName(p, out string name) && name.Equals(Config.SpecificCharacterToFollow, StringComparison.OrdinalIgnoreCase));
                    if (ShouldIFollowPlayer(specificPlayer))
                    {
                        playerToFollow = specificPlayer;
                        return true;
                    }
                }

                // follow leader
                if (Config.FollowGroupLeader && ShouldIFollowPlayer(Bot.Objects.PartyLeader))
                {
                    playerToFollow = Bot.Objects.PartyLeader;
                    return true;
                }

                if (Config.FollowGroupMembers)
                {
                    // sort players based on their `HealthPercentage`
                    IEnumerable<IWowUnit> partyMembersSorted = Bot.Objects.PartyMembers
                        .Where(e => !e.IsDead && e.MaxHealth - e.Health > 1200)
                        .OrderBy(e => e.HealthPercentage);

                    foreach (IWowUnit player in partyMembersSorted)
                    {
                        if (ShouldIFollowPlayer(player))
                        {
                            playerToFollow = player;
                            return true;
                        }
                    }
                }
            }

            playerToFollow = null;
            return false;
        }

        private bool ShouldIFollowPlayer(IWowUnit playerToFollow)
        {
            Vector3 pos = Config.FollowPositionDynamic ? playerToFollow.Position + FollowOffset : playerToFollow.Position;
            double distance = Bot.Player.DistanceTo(pos);

            return distance > Config.MinFollowDistance
                   && distance < Config.MaxFollowDistance
                   && Math.Abs(playerToFollow.Position.Z - Bot.Player.Position.Z) < 5; // TODO: fix for AV bridge
        }
    }
}