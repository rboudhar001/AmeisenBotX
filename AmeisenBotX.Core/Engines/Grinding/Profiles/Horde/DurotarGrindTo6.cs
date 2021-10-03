﻿using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Grinding.Objects;
using AmeisenBotX.Core.Objects;
using AmeisenBotX.Core.Objects.Enums;
using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Grinding.Profiles.Horde
{
    public class DurotarGrindTo6 : IGrindingProfile
    {
        public bool RandomizeSpots => true;

        public List<Npc> NpcsOfInterest { get; } = new()
        {
            new Npc("Duokna", 3158,
                WowMapId.Kalimdor, WowZoneId.ValleyofTrials, new Vector3(-565, -4214, 41),
                NpcType.VendorSellBuy),

            new Npc("Ken'jai", 3707,
                WowMapId.Kalimdor, WowZoneId.ValleyofTrials, new Vector3(-617, -4202, 38),
                NpcType.ClassTrainer, NpcSubType.PriestTrainer),

            new Npc("Shikrik", 3157,
                WowMapId.Kalimdor, WowZoneId.ValleyofTrials, new Vector3(-623, -4203, 38),
                NpcType.ClassTrainer, NpcSubType.ShamanTrainer)
        };

        public List<InteractableObject> ObjectsOfInterest { get; } = new()
        {
            new InteractableObject(3084,
                WowMapId.Kalimdor, WowZoneId.ValleyofTrials, new Vector3(-602, -4250, 37),
                InteractableObjectType.Fire)
        };

        public List<GrindingSpot> Spots { get; } = new()
        {
            // pigs
            new GrindingSpot(new Vector3(-546, -4308, 38), 40.0f, 1, 3),
            new GrindingSpot(new Vector3(-450, -4258, 48), 40.0f, 1, 3),
            // scorpids
            new GrindingSpot(new Vector3(-435, -4154, 52), 48.0f, 2, 7),
            new GrindingSpot(new Vector3(-379, -4096, 49), 48.0f, 2, 7),
            new GrindingSpot(new Vector3(-399, -4116, 50), 48.0f, 2, 7)
        };

        public override string ToString()
        {
            return "[H][Durotar] 1 To 6 Grinding";
        }
    }
}