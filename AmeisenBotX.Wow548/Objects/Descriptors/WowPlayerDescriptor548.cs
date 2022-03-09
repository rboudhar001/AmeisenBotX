﻿using AmeisenBotX.Wow.Objects.Raw.SubStructs;
using System.Runtime.InteropServices;

namespace AmeisenBotX.Wow548.Objects.Descriptors
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct WowPlayerDescriptor548
    {
        public ulong DuelArbiter;
        public int Flags;
        public int GuildRankId;
        public int GuildDeleteDate;
        public int GuildLevel;
        public int HairColorId;
        public int RestState;
        public int ArenaFaction;
        public int DuelTeam;
        public int GuildTimestamp;
        public QuestlogEntry QuestlogEntry1;
        public QuestlogEntry QuestlogEntry2;
        public QuestlogEntry QuestlogEntry3;
        public QuestlogEntry QuestlogEntry4;
        public QuestlogEntry QuestlogEntry5;
        public QuestlogEntry QuestlogEntry6;
        public QuestlogEntry QuestlogEntry7;
        public QuestlogEntry QuestlogEntry8;
        public QuestlogEntry QuestlogEntry9;
        public QuestlogEntry QuestlogEntry10;
        public QuestlogEntry QuestlogEntry11;
        public QuestlogEntry QuestlogEntry12;
        public QuestlogEntry QuestlogEntry13;
        public QuestlogEntry QuestlogEntry14;
        public QuestlogEntry QuestlogEntry15;
        public QuestlogEntry QuestlogEntry16;
        public QuestlogEntry QuestlogEntry17;
        public QuestlogEntry QuestlogEntry18;
        public QuestlogEntry QuestlogEntry19;
        public QuestlogEntry QuestlogEntry20;
        public QuestlogEntry QuestlogEntry21;
        public QuestlogEntry QuestlogEntry22;
        public QuestlogEntry QuestlogEntry23;
        public QuestlogEntry QuestlogEntry24;
        public QuestlogEntry QuestlogEntry25;
        public VisibleItemEnchantment VisibleItemEnchantment1;
        public VisibleItemEnchantment VisibleItemEnchantment2;
        public VisibleItemEnchantment VisibleItemEnchantment3;
        public VisibleItemEnchantment VisibleItemEnchantment4;
        public VisibleItemEnchantment VisibleItemEnchantment5;
        public VisibleItemEnchantment VisibleItemEnchantment6;
        public VisibleItemEnchantment VisibleItemEnchantment7;
        public VisibleItemEnchantment VisibleItemEnchantment8;
        public VisibleItemEnchantment VisibleItemEnchantment9;
        public VisibleItemEnchantment VisibleItemEnchantment10;
        public VisibleItemEnchantment VisibleItemEnchantment11;
        public VisibleItemEnchantment VisibleItemEnchantment12;
        public VisibleItemEnchantment VisibleItemEnchantment13;
        public VisibleItemEnchantment VisibleItemEnchantment14;
        public VisibleItemEnchantment VisibleItemEnchantment15;
        public VisibleItemEnchantment VisibleItemEnchantment16;
        public VisibleItemEnchantment VisibleItemEnchantment17;
        public VisibleItemEnchantment VisibleItemEnchantment18;
        public VisibleItemEnchantment VisibleItemEnchantment19;
        public int ChosenTitle;
        public int FakeInebriation;
        public int VirtualPlayerRealm;
        public fixed ulong InventorySlots[23];
        public fixed ulong BackpackSlots[16];
        public fixed ulong BankSlots[28];
        public fixed ulong BankBagSlots[7];
        public fixed ulong VendorBuyBackSlots[12];
        public ulong Farsight;
        public ulong KnownTitles0;
        public ulong KnownTitles1;
        public ulong KnownTitles2;
        public ulong KnownTitles3;
        public ulong KnownTitles4;
        public ulong Coinage;
        public int Xp;
        public int NextLevelXp;
        public fixed short SkillInfos[896];
        public fixed int SkillLineIds[64];
        public fixed int SkillSteps[64];
        public fixed int SkillRanks[64];
        public fixed int SkillStartingRanks[64];
        public fixed int SkillMaxRanks[64];
        public fixed int SkillModifiers[64];
        public fixed int SkillTalents[64];
        public int CharacterPoints;
        public int MaxTalentTiers;
        public int TrackCreatures;
        public int TrackResources;
        public int MainhandExpertise;
        public int OffhandExpertise;
        public int RangedExpertise;
        public int CombatRatingExpertise;
        public float BlockPercentage;
        public float DodgePercentage;
        public float ParryPercentage;
        public float CritPercentage;
        public float RangedCritPercentage;
        public float OffhandCritPercentage;
        public float SpellCritPercentage;
        public int ShieldBlock;
        public float ShieldBlockPercentage;
        public int Mastery;
        public int PvpPowerDamage;
        public int PvpPowerHealing;
        public fixed byte ExploredZones[800];
        public int RestStateExpirience;
        public fixed int ModDamageDonePos[7];
        public fixed int ModDamageDoneNeg[7];
        public fixed int ModDamageDonePercentage[7];
        public int ModHealingDone;
        public float ModHealingPercentage;
        public float ModHealingDonePercentage;
        public float ModPeriodicHealingDonePercentage;
        public float WeaponDamageMultipliers;
        public float ModSpellPowerPercentage;
        public float ModResiliencePercentage;
        public float ModOverrideSpellPowerByApPercentage;
        public float ModOverrideApBySpellPowerPercentage;
        public int ModTargetResistance;
        public int ModTargetPhysicalResistance;
        public int LifetimeMaxRank;
        public int SelfResSpell;
        public int PvpMedals;
        public fixed int BuybackPrices[12];
        public fixed int BuybackTimestamps[12];
        public int YesterdayHonorableKills;
        public int LifetimeHonorableKills;
        public int WatchedFactionIndex;
        public fixed int CombatRatings[27];
        public fixed int ArenaTeamInfo[24];
        public int MaxLevel;
        public fixed float RuneRegens[4];
        public fixed int NoReagentCosts[4];
        public fixed int GlyphSlots[6];
        public fixed int Glyphs[6];
        public int GlyphsEnabled;
        public int PetSpellPower;
        public fixed int Researching[8];
        public fixed int ProfessionSkillLine[2];
        public int UiHitModifier;
        public int UiSpellHitModifier;
        public int HomeRealmTimeOffset;
        public int ModPetHaste;
        public fixed int SummonedBattlePetGuid[2];
        public int OverrideSpellsId;
        public int LfgBonusFactionId;
        public int LootSpecId;
        public int OverrideZonePvpType;
        public int ItemLevelDelta;
    }
}