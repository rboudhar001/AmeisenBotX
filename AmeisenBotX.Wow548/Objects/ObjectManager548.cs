using AmeisenBotX.Common.Math;
using AmeisenBotX.Wow;
using AmeisenBotX.Wow.Objects;

namespace AmeisenBotX.Wow548.Objects
{
    public class ObjectManager548(WowMemoryApi memory) : ObjectManager<WowObject548, WowUnit548, WowPlayer548, WowGameobject548, WowDynobject548, WowItem548, WowCorpse548, WowContainer548>(memory)
    {
        protected override void ReadGroup()
        {
            if (ReadLeaderGuid(out nint party)
                && Memory.Read(nint.Add(party, 0xC4), out int count) && count > 0)
            {
                GroupMemberGuids = ReadRaidMemberGuids(party);
                GroupMembers = wowObjects.OfType<IWowUnit>().Where(e => GroupMemberGuids.Contains(e.Guid));

                Vector3 pos = new();

                foreach (Vector3 vec in GroupMembers.Select(e => e.Position))
                {
                    pos += vec;
                }

                CenterPartyPosition = pos / GroupMembers.Count();

                GroupPetGuids = GroupPets.Select(e => e.Guid);
                GroupPets = wowObjects.OfType<IWowUnit>().Where(e => GroupMemberGuids.Contains(e.SummonedByGuid));
            }
        }

        protected override void ReadRaid()
        {
            if (ReadLeaderGuid(out nint party)
                && Memory.Read(nint.Add(party, 0xC4), out int count) && count > 0)
            {
                RaidMemberGuids = ReadRaidMemberGuids(party);
                RaidMembers = wowObjects.OfType<IWowUnit>().Where(e => RaidMemberGuids.Contains(e.Guid));

                Vector3 pos = new();

                foreach (Vector3 vec in RaidMembers.Select(e => e.Position))
                {
                    pos += vec;
                }

                CenterPartyPosition = pos / RaidMembers.Count();

                RaidPetGuids = RaidPets.Select(e => e.Guid);
                RaidPets = wowObjects.OfType<IWowUnit>().Where(e => RaidMemberGuids.Contains(e.SummonedByGuid));
            }
        }

        protected override void ReadParty()
        {
            if (RaidMembers.Count() == 0)
            {
                PartyMembers = RaidMembers;
                PartyMemberGuids = RaidMemberGuids;
                PartyPets = RaidPets;
                PartyPetGuids = RaidPetGuids;
            }
            else
            {
                PartyMembers = GroupMembers;
                PartyMemberGuids = GroupMemberGuids;
                PartyPets = GroupPets;
                PartyPetGuids = GroupPetGuids;
            }

        }

        private IEnumerable<ulong> ReadRaidMemberGuids(nint party)
        {
            List<ulong> raidMemberGuids = [];

            for (int i = 0; i < 40; i++)
            {
                if (Memory.Read(nint.Add(party, i * 4), out nint player) && player != nint.Zero
                    && Memory.Read(nint.Add(player, 0x10), out ulong guid) && guid > 0)
                {
                    raidMemberGuids.Add(guid);

                    if (Memory.Read(nint.Add(player, 0x4), out int status) && status == 2)
                    {
                        PartyLeaderGuid = guid;
                    }
                }
            }

            return raidMemberGuids.Where(e => e != 0 && e != PlayerGuid).Distinct();
        }

        private bool ReadLeaderGuid(out nint party)
        {
            return Memory.Read(Memory.Offsets.PartyLeader, out party) && party != nint.Zero;
        }
    }
}