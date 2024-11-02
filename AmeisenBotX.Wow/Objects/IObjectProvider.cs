using AmeisenBotX.Common.Math;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow.Objects.Raw;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Wow.Objects
{
    public interface IObjectProvider
    {
        /// <summary>
        /// Gets fired when we updated all objects successfully.
        /// </summary>
        event Action<IEnumerable<IWowObject>> OnObjectUpdateComplete;

        IEnumerable<IWowObject> All { get; }

        /// <summary>
        /// Returns wow's camera information.
        /// </summary>
        RawCameraInfo Camera { get; }

        Vector3 CenterPartyPosition { get; }

        /// <summary>
        /// Contains the state of the game while in GlueXML start screen.
        /// </summary>
        string GameState { get; }

        bool IsTargetInLineOfSight { get; }

        bool IsWorldLoaded { get; }

        IWowUnit LastTarget { get; }

        WowMapId MapId { get; }

        int ObjectCount { get; }

        IWowUnit PartyLeader { get; }

        // group
        IEnumerable<IWowUnit> GroupMembers { get; }

        IEnumerable<ulong> GroupMemberGuids { get; }

        IEnumerable<IWowUnit> GroupPets { get; }

        IEnumerable<ulong> GroupPetGuids { get; }

        // raid
        IEnumerable<IWowUnit> RaidMembers { get; }

        IEnumerable<ulong> RaidMemberGuids { get; }

        IEnumerable<IWowUnit> RaidPets { get; }

        IEnumerable<ulong> RaidPetGuids { get; }

        // party
        IEnumerable<IWowUnit> PartyMembers { get; }

        IEnumerable<ulong> PartyMemberGuids { get; }

        IEnumerable<IWowUnit> PartyPets { get; }

        IEnumerable<ulong> PartyPetGuids { get; }

        IWowUnit Pet { get; }

        IWowPlayer Player { get; }

        nint PlayerBase { get; }

        IWowUnit Target { get; }

        IWowUnit Vehicle { get; }

        int ZoneId { get; }

        string ZoneName { get; }

        string ZoneSubName { get; }
    }
}