﻿using AmeisenBotX.Wow.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Logic.Idle.Actions
{
    public class RandomEmoteIdleAction(AmeisenBotInterfaces bot) : IIdleAction
    {
        public bool AutopilotOnly => false;

        public AmeisenBotInterfaces Bot { get; } = bot;

        public DateTime Cooldown { get; set; }

        public List<string> Emotes { get; } =
            [
                "flex",
                "train",
                "joke",
                "laugh",
                "dance",
                "sit",
                "sleep",
            ];

        public List<string> EmotesWithInteraction { get; } =
            [
                "hi",
                "wink",
                "salute",
                "fart",
                "flex",
                "laugh",
                "rude",
                "roar",
                "applaud",
                "shy",
            ];

        public int MaxCooldown => 168 * 1000;

        public int MaxDuration => 0;

        public int MinCooldown => 42 * 1000;

        public int MinDuration => 0;

        private Random Rnd { get; } = new Random();

        public bool Enter()
        {
            return true;
        }

        public void Execute()
        {
            IEnumerable<IWowPlayer> friendsAroundMe = Bot.GetNearFriends<IWowPlayer>(Bot.Player.Position, 24.0f)
                .Where(e => e.Guid != Bot.Wow.PlayerGuid && Bot.Objects.PartyMemberGuids.Contains(e.Guid));

            if (friendsAroundMe.Any() && Rnd.NextDouble() > 0.5)
            {
                IWowPlayer player = friendsAroundMe.ElementAt(Rnd.Next(0, friendsAroundMe.Count()));

                if (Bot.Wow.TargetGuid != player.Guid)
                {
                    Bot.Wow.ChangeTarget(player.Guid);
                    Bot.Wow.FacePosition(Bot.Player.BaseAddress, Bot.Player.Position, player.Position, true);
                }

                Bot.Wow.SendChatMessage($"/{EmotesWithInteraction[Rnd.Next(0, EmotesWithInteraction.Count)]}");
            }
            else
            {
                Bot.Wow.SendChatMessage($"/{Emotes[Rnd.Next(0, Emotes.Count)]}");
            }
        }

        public override string ToString()
        {
            return $"{(AutopilotOnly ? "(🤖) " : "")}Random Emote";
        }
    }
}