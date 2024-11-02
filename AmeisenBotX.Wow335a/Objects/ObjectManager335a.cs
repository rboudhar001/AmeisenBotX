using AmeisenBotX.Common.Math;
using AmeisenBotX.Wow;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow335a.Objects.Raw;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Wow335a.Objects
{
    public class ObjectManager335a(WowMemoryApi memory) : ObjectManager<WowObject335a, WowUnit335a, WowPlayer335a, WowGameobject335a, WowDynobject335a, WowItem335a, WowCorpse335a, WowContainer335a>(memory)
    {
        protected override void ReadGroup()
        {
            PartyLeaderGuid = ReadLeaderGuid();

            if (PartyLeaderGuid > 0)
            {
                GroupMemberGuids = ReadGroupMemberGuids();
                GroupMembers = wowObjects.OfType<IWowUnit>().Where(e => GroupMemberGuids.Contains(e.Guid));

                Vector3 pos = new();
                var groupMembersAlive = GroupMembers.Where(e => !e.IsDead && e.Health > 1).ToList();
                foreach (Vector3 vec in groupMembersAlive.Select(e => e.Position))
                {
                    pos += vec;
                }
                CenterPartyPosition = pos / groupMembersAlive.Count();

                GroupPetGuids = GroupPets.Select(e => e.Guid);
                GroupPets = wowObjects.OfType<IWowUnit>().Where(e => GroupMemberGuids.Contains(e.SummonedByGuid));
            }
        }

        protected override void ReadRaid()
        {
            PartyLeaderGuid = ReadLeaderGuid();

            if (PartyLeaderGuid > 0)
            {
                RaidMemberGuids = ReadRaidMemberGuids();
                RaidMembers = wowObjects.OfType<IWowUnit>().Where(e => RaidMemberGuids.Contains(e.Guid));

                Vector3 pos = new();
                var raidMembersAlive = RaidMembers.Where(e => !e.IsDead && e.Health > 1).ToList();
                foreach (Vector3 vec in raidMembersAlive.Select(e => e.Position))
                {
                    pos += vec;
                }
                CenterPartyPosition = pos / raidMembersAlive.Count();

                RaidPetGuids = RaidPets.Select(e => e.Guid);
                RaidPets = wowObjects.OfType<IWowUnit>().Where(e => RaidMemberGuids.Contains(e.SummonedByGuid));
            }
        }

        protected override void ReadParty()
        {
            if (RaidMembers.Any())
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

        private ulong ReadLeaderGuid()
        {
            return Memory.Read(Memory.Offsets.RaidLeader, out ulong partyLeaderGuid)
                ? partyLeaderGuid == 0 && Memory.Read(Memory.Offsets.PartyLeader, out partyLeaderGuid)
                    ? partyLeaderGuid
                    : partyLeaderGuid
                : 0;
        }

        private IEnumerable<ulong> ReadGroupMemberGuids()
        {
            List<ulong> groupMemberGuids = [];

            if (Memory.Read(Memory.Offsets.PartyPlayerGuids, out RawGroupGuids groupMembers))
            {
                groupMemberGuids.AddRange(groupMembers.AsArray());
            }

            return groupMemberGuids.Where(e => e != 0 && e != PlayerGuid).Distinct();
        }

        private IEnumerable<ulong> ReadRaidMemberGuids()
        {
            List<ulong> raidMemberGuids = [];

            if (Memory.Read(Memory.Offsets.RaidGroupStart, out RawRaidStruct raidStruct))
            {
                foreach (nint raidPointer in raidStruct.GetPointers())
                {
                    if (Memory.Read(raidPointer, out ulong guid))
                    {
                        raidMemberGuids.Add(guid);
                    }
                }
            }

            return raidMemberGuids.Where(e => e != 0 && e != PlayerGuid).Distinct();
        }
    }
}