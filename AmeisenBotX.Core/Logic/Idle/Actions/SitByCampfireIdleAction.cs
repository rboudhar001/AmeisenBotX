﻿using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Linq;

namespace AmeisenBotX.Core.Logic.Idle.Actions
{
    public class SitByCampfireIdleAction(AmeisenBotInterfaces bot) : IIdleAction
    {
        public bool AutopilotOnly => false;

        public AmeisenBotInterfaces Bot { get; } = bot;

        public DateTime Cooldown { get; set; }

        public int MaxCooldown => 11 * 60 * 1000;

        public int MaxDuration => 2 * 60 * 1000;

        public int MinCooldown => 5 * 60 * 1000;

        public int MinDuration => 1 * 60 * 1000;

        private bool PlacedCampfire { get; set; }

        private Random Rnd { get; } = new Random();

        private bool SatDown { get; set; }

        public bool Enter()
        {
            PlacedCampfire = false;
            SatDown = false;

            return Bot.Character.SpellBook.IsSpellKnown("Basic Campfire");
        }

        public void Execute()
        {
            if (PlacedCampfire && SatDown)
            {
                return;
            }

            IWowGameobject nearCampfire = Bot.Objects.All.OfType<IWowGameobject>()
                .FirstOrDefault(e => e.DisplayId == (int)WowGameObjectDisplayId.CookingCampfire
                                  && Bot.Objects.PartyMemberGuids.Contains(e.CreatedBy));

            if (nearCampfire != null && !SatDown)
            {
                Bot.Wow.FacePosition(Bot.Player.BaseAddress, Bot.Player.Position, nearCampfire.Position, true);
                Bot.Wow.SendChatMessage(Rnd.Next(0, 2) == 1 ? "/sit" : "/sleep");
                SatDown = true;
            }
            else if (!PlacedCampfire)
            {
                Bot.Wow.CastSpell("Basic Campfire");
                PlacedCampfire = true;
            }
        }

        public override string ToString()
        {
            return $"{(AutopilotOnly ? "(🤖) " : "")}Place Campfire";
        }
    }
}