using System.Runtime.InteropServices;

namespace AmeisenBotX.Wow335a.Objects.Raw
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RawGroupGuids
    {
        public ulong GroupMemberGuid1 { get; set; }

        public ulong GroupMemberGuid2 { get; set; }

        public ulong GroupMemberGuid3 { get; set; }

        public ulong GroupMemberGuid4 { get; set; }

        public readonly ulong[] AsArray()
        {
            return
            [
                GroupMemberGuid1,
                GroupMemberGuid2,
                GroupMemberGuid3,
                GroupMemberGuid4,
            ];
        }
    }
}