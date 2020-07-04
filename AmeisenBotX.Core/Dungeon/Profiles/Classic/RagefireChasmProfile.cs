﻿using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Dungeon.Enums;
using AmeisenBotX.Core.Dungeon.Objects;
using AmeisenBotX.Core.Jobs.Profiles;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Dungeon.Profiles.Classic
{
    public class RagefireChasmProfile : IDungeonProfile
    {
        public string Author { get; } = "Jannis";

        public string Description { get; } = "Profile for the Dungeon in Orgrimmar, made for Level 13 to 18.";

        public DungeonFactionType FactionType { get; } = DungeonFactionType.Horde;

        public int GroupSize { get; } = 5;

        public MapId MapId { get; } = MapId.RagefireChasm;

        public int MaxLevel { get; } = 18;

        public string Name { get; } = "[13-18] Ragefire Chasm";

        public List<DungeonNode> Nodes { get; } = new List<DungeonNode>()
        {
			new DungeonNode(new Vector3(4, -15, -18)),
			new DungeonNode(new Vector3(1, -23, -20)),
			new DungeonNode(new Vector3(-1, -30, -22)),
			new DungeonNode(new Vector3(-4, -37, -22)),
			new DungeonNode(new Vector3(-7, -44, -22)),
			new DungeonNode(new Vector3(-16, -46, -22)),
			new DungeonNode(new Vector3(-24, -47, -22)),
			new DungeonNode(new Vector3(-32, -45, -22)),
			new DungeonNode(new Vector3(-41, -43, -22)),
			new DungeonNode(new Vector3(-49, -40, -22)),
			new DungeonNode(new Vector3(-57, -38, -20)),
			new DungeonNode(new Vector3(-64, -36, -18)),
			new DungeonNode(new Vector3(-72, -35, -19)),
			new DungeonNode(new Vector3(-81, -35, -20)),
			new DungeonNode(new Vector3(-88, -36, -24)),
			new DungeonNode(new Vector3(-95, -36, -28)),
			new DungeonNode(new Vector3(-104, -35, -30)),
			new DungeonNode(new Vector3(-113, -35, -32)),
			new DungeonNode(new Vector3(-122, -34, -33)),
			new DungeonNode(new Vector3(-130, -34, -33)),
			new DungeonNode(new Vector3(-139, -34, -33)),
			new DungeonNode(new Vector3(-148, -33, -35)),
			new DungeonNode(new Vector3(-156, -32, -38)),
			new DungeonNode(new Vector3(-165, -31, -41)),
			new DungeonNode(new Vector3(-174, -30, -43)),
			new DungeonNode(new Vector3(-183, -31, -45)),
			new DungeonNode(new Vector3(-192, -33, -46)),
			new DungeonNode(new Vector3(-200, -34, -48)),
			new DungeonNode(new Vector3(-207, -35, -51)),
			new DungeonNode(new Vector3(-215, -35, -53)),
			new DungeonNode(new Vector3(-224, -35, -56)),
			new DungeonNode(new Vector3(-233, -36, -57)),
			new DungeonNode(new Vector3(-241, -36, -58)),
			new DungeonNode(new Vector3(-248, -38, -60)),
			new DungeonNode(new Vector3(-256, -40, -61)),
			new DungeonNode(new Vector3(-263, -43, -61)),
			new DungeonNode(new Vector3(-270, -48, -61)),
			new DungeonNode(new Vector3(-277, -52, -61)),
			new DungeonNode(new Vector3(-285, -54, -61)),
			new DungeonNode(new Vector3(-292, -50, -61)),
			new DungeonNode(new Vector3(-298, -45, -61)),
			new DungeonNode(new Vector3(-301, -36, -61)),
			new DungeonNode(new Vector3(-304, -28, -60)),
			new DungeonNode(new Vector3(-306, -20, -58)),
			new DungeonNode(new Vector3(-306, -13, -55)),
			new DungeonNode(new Vector3(-305, -5, -52)),
			new DungeonNode(new Vector3(-302, 2, -49)),
			new DungeonNode(new Vector3(-295, 8, -47)),
			new DungeonNode(new Vector3(-286, 8, -46)),
			new DungeonNode(new Vector3(-279, 8, -49)),
			new DungeonNode(new Vector3(-271, 8, -50)),
			new DungeonNode(new Vector3(-263, 8, -50)),
			new DungeonNode(new Vector3(-254, 8, -50)),
			new DungeonNode(new Vector3(-245, 8, -48)),
			new DungeonNode(new Vector3(-237, 8, -45)),
			new DungeonNode(new Vector3(-228, 8, -44)),
			new DungeonNode(new Vector3(-219, 8, -43)),
			new DungeonNode(new Vector3(-211, 7, -40)),
			new DungeonNode(new Vector3(-204, 8, -37)),
			new DungeonNode(new Vector3(-197, 10, -34)),
			new DungeonNode(new Vector3(-189, 11, -33)),
			new DungeonNode(new Vector3(-181, 13, -32)),
			new DungeonNode(new Vector3(-172, 14, -31)),
			new DungeonNode(new Vector3(-165, 14, -28)),
			new DungeonNode(new Vector3(-157, 14, -26)),
			new DungeonNode(new Vector3(-149, 13, -23)),
			new DungeonNode(new Vector3(-142, 11, -21)),
			new DungeonNode(new Vector3(-135, 8, -21)),
			new DungeonNode(new Vector3(-127, 9, -20)),
			new DungeonNode(new Vector3(-120, 12, -19)),
			new DungeonNode(new Vector3(-114, 17, -19)),
			new DungeonNode(new Vector3(-111, 24, -19)),
			new DungeonNode(new Vector3(-108, 31, -18)),
			new DungeonNode(new Vector3(-106, 40, -18)),
			new DungeonNode(new Vector3(-106, 49, -18)),
			new DungeonNode(new Vector3(-109, 57, -19)),
			new DungeonNode(new Vector3(-114, 63, -20)),
			new DungeonNode(new Vector3(-119, 69, -21)),
			new DungeonNode(new Vector3(-127, 74, -22)),
			new DungeonNode(new Vector3(-134, 77, -22)),
			new DungeonNode(new Vector3(-143, 78, -21)),
			new DungeonNode(new Vector3(-152, 77, -21)),
			new DungeonNode(new Vector3(-161, 75, -21)),
			new DungeonNode(new Vector3(-170, 76, -21)),
			new DungeonNode(new Vector3(-179, 77, -22)),
			new DungeonNode(new Vector3(-187, 80, -23)),
			new DungeonNode(new Vector3(-192, 86, -24)),
			new DungeonNode(new Vector3(-195, 95, -25)),
			new DungeonNode(new Vector3(-204, 96, -25)),
			new DungeonNode(new Vector3(-212, 93, -25)),
			new DungeonNode(new Vector3(-220, 93, -25)),
			new DungeonNode(new Vector3(-229, 93, -23)),
			new DungeonNode(new Vector3(-237, 93, -22)),
			new DungeonNode(new Vector3(-246, 93, -23)),
			new DungeonNode(new Vector3(-254, 93, -25)),
			new DungeonNode(new Vector3(-261, 97, -25)),
			new DungeonNode(new Vector3(-264, 104, -25)),
			new DungeonNode(new Vector3(-261, 111, -25)),
			new DungeonNode(new Vector3(-258, 118, -22)),
			new DungeonNode(new Vector3(-253, 126, -20)),
			new DungeonNode(new Vector3(-250, 134, -19)),
			new DungeonNode(new Vector3(-247, 141, -19)),
			new DungeonNode(new Vector3(-245, 149, -19)),
			new DungeonNode(new Vector3(-242, 157, -19)),
			new DungeonNode(new Vector3(-240, 165, -19)),
			new DungeonNode(new Vector3(-238, 174, -19)),
			new DungeonNode(new Vector3(-236, 181, -21)),
			new DungeonNode(new Vector3(-234, 189, -24)),
			new DungeonNode(new Vector3(-232, 197, -25)),
			new DungeonNode(new Vector3(-234, 205, -25)),
			new DungeonNode(new Vector3(-241, 208, -25)),
			new DungeonNode(new Vector3(-249, 210, -23)),
			new DungeonNode(new Vector3(-256, 210, -20)),
			new DungeonNode(new Vector3(-264, 211, -22)),
			new DungeonNode(new Vector3(-271, 212, -25)),
			new DungeonNode(new Vector3(-279, 213, -25)),
			new DungeonNode(new Vector3(-287, 214, -25)),
			new DungeonNode(new Vector3(-296, 216, -25)),
			new DungeonNode(new Vector3(-305, 218, -25)),
			new DungeonNode(new Vector3(-314, 219, -22)),
			new DungeonNode(new Vector3(-322, 219, -21)),
			new DungeonNode(new Vector3(-330, 217, -20)),
			new DungeonNode(new Vector3(-338, 215, -21)),
			new DungeonNode(new Vector3(-345, 211, -21)),
			new DungeonNode(new Vector3(-351, 205, -22)),
			new DungeonNode(new Vector3(-357, 200, -22)),
			new DungeonNode(new Vector3(-360, 193, -22)),
			new DungeonNode(new Vector3(-363, 186, -22)),
			new DungeonNode(new Vector3(-371, 184, -22)),
			new DungeonNode(new Vector3(-374, 193, -22)),
			new DungeonNode(new Vector3(-374, 202, -22)),
			new DungeonNode(new Vector3(-374, 210, -22)),
			new DungeonNode(new Vector3(-366, 213, -22)),
			new DungeonNode(new Vector3(-357, 215, -22)),
			new DungeonNode(new Vector3(-348, 217, -21)),
			new DungeonNode(new Vector3(-340, 219, -21)),
			new DungeonNode(new Vector3(-332, 220, -20)),
			new DungeonNode(new Vector3(-324, 220, -21)),
			new DungeonNode(new Vector3(-315, 220, -22)),
			new DungeonNode(new Vector3(-307, 219, -25)),
			new DungeonNode(new Vector3(-298, 218, -26)),
			new DungeonNode(new Vector3(-289, 215, -25)),
			new DungeonNode(new Vector3(-281, 213, -25)),
			new DungeonNode(new Vector3(-273, 211, -25)),
			new DungeonNode(new Vector3(-264, 209, -22)),
			new DungeonNode(new Vector3(-255, 207, -21)),
			new DungeonNode(new Vector3(-247, 207, -24)),
			new DungeonNode(new Vector3(-239, 210, -25)),
			new DungeonNode(new Vector3(-233, 215, -25)),
			new DungeonNode(new Vector3(-232, 224, -25)),
			new DungeonNode(new Vector3(-237, 231, -24)),
			new DungeonNode(new Vector3(-243, 236, -23)),
			new DungeonNode(new Vector3(-249, 241, -21)),
			new DungeonNode(new Vector3(-257, 246, -20)),
			new DungeonNode(new Vector3(-264, 249, -18)),
			new DungeonNode(new Vector3(-272, 252, -17)),
			new DungeonNode(new Vector3(-280, 253, -17)),
			new DungeonNode(new Vector3(-288, 253, -16)),
			new DungeonNode(new Vector3(-297, 253, -14)),
			new DungeonNode(new Vector3(-306, 252, -13)),
			new DungeonNode(new Vector3(-314, 252, -12)),
			new DungeonNode(new Vector3(-323, 253, -11)),
			new DungeonNode(new Vector3(-332, 256, -10)),
			new DungeonNode(new Vector3(-341, 258, -8)),
			new DungeonNode(new Vector3(-350, 259, -7)),
			new DungeonNode(new Vector3(-359, 260, -6)),
			new DungeonNode(new Vector3(-368, 259, -5)),
			new DungeonNode(new Vector3(-375, 256, -5)),
			new DungeonNode(new Vector3(-382, 251, -5)),
			new DungeonNode(new Vector3(-388, 245, -5)),
			new DungeonNode(new Vector3(-392, 238, -5)),
			new DungeonNode(new Vector3(-396, 231, -3)),
			new DungeonNode(new Vector3(-400, 224, -2)),
			new DungeonNode(new Vector3(-404, 217, -1)),
			new DungeonNode(new Vector3(-407, 210, 1)),
			new DungeonNode(new Vector3(-409, 203, 3)),
			new DungeonNode(new Vector3(-410, 195, 4)),
			new DungeonNode(new Vector3(-410, 186, 6)),
			new DungeonNode(new Vector3(-409, 177, 7)),
			new DungeonNode(new Vector3(-406, 170, 7)),
			new DungeonNode(new Vector3(-403, 163, 8)),
			new DungeonNode(new Vector3(-398, 157, 8)),
			new DungeonNode(new Vector3(-392, 152, 8)),
			new DungeonNode(new Vector3(-386, 147, 8)),
		};

        public List<string> PriorityUnits { get; } = new List<string>();

        public int RequiredItemLevel { get; } = 10;

        public int RequiredLevel { get; } = 13;

        public Vector3 WorldEntry { get; } = new Vector3(1816, -4422, -19);

        public MapId WorldEntryMapId { get; } = MapId.Kalimdor;
    }
}