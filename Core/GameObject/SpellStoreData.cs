using System;
using SpicyTemple.Core.Systems;

namespace SpicyTemple.Core.GameObject
{


public enum SpellStoreType : byte{
	spellStoreNone = 0,
	spellStoreKnown = 1,
	spellStoreMemorized = 2,
	spellStoreCast = 3,
	spellStoreAtWill // New! Todo implementation
}

public enum MetaMagicFlags : byte
{
	MetaMagic_Maximize = 1,
	MetaMagic_Quicken = 2,
	MetaMagic_Silent = 4,
	MetaMagic_Still = 8
}

public enum SpellComponentFlag : uint
{
	SpellComponent_Verbal = 1,
	SpellComponent_Somatic = 2,
	SpellComponent_XpCost = 4,
	SpellComponent_GpCost = 0x8,
}

public struct MetaMagicData{
	public byte metaMagicFlags; // 1 - Maximize Spell ; 2 - Quicken Spell ; 4 - Silent Spell;  8 - Still Spell
	public byte metaMagicEmpowerSpellCount;
	public byte metaMagicEnlargeSpellCount;
	public byte metaMagicExtendSpellCount;
	public byte metaMagicHeightenSpellCount;
	public byte metaMagicWidenSpellCount;

	/// <summary>
	/// Pack the data into a 32-bit unsigned integer.
	/// </summary>
	public uint Pack()
	{
		uint result = 0;
		result |= (uint) (metaMagicFlags & 0xF);
		result |= (uint) (metaMagicEmpowerSpellCount & 0xF) << 4;
		result |= (uint) (metaMagicEnlargeSpellCount & 0xF) << 8;
		result |= (uint) (metaMagicExtendSpellCount & 0xF) << 12;
		result |= (uint) (metaMagicHeightenSpellCount & 0xF) << 16;
		result |= (uint) (metaMagicWidenSpellCount & 0xF) << 20;
		return result;
	}

	/// <summary>
	/// Unpack the metamagic data from a 32-bit unsigned integer previously packed using the Pack method.
	/// </summary>
	public static MetaMagicData Unpack(uint raw)
	{
		var result = new MetaMagicData();
		result.metaMagicFlags = (byte) (raw & 0xF);
		result.metaMagicEmpowerSpellCount = (byte) ((raw & 0xF0) >> 4);
		result.metaMagicEnlargeSpellCount = (byte) ((raw & 0xF00) >> 8);
		result.metaMagicExtendSpellCount = (byte) ((raw & 0xF000) >> 12);
		result.metaMagicHeightenSpellCount = (byte) ((raw & 0xF0000) >> 16);
		result.metaMagicWidenSpellCount = (byte) ((raw & 0xF00000) >> 20);
		return result;
	}

}

public struct SpellStoreState {
	public SpellStoreType spellStoreType;
	public byte usedUp; // relevant only for spellStoreMemorized

	public static SpellStoreState Unpack(int packed)
	{
		var result = new SpellStoreState();
		result.spellStoreType = (SpellStoreType) (packed & 0xFF);
		result.usedUp = (byte) ((packed >> 8) & 0xFF);
		return result;
	}

	public int Pack()
	{
		return ((byte) spellStoreType) | (usedUp << 8);
	}

};

public enum SpontCastType : byte {
	spontCastNone = 0,
	spontCastGoodCleric = 2,
	spontCastEvilCleric = 4,
	spontCastDruid = 8
};

public enum AiSpellType : uint {
	ai_action_summon = 0,
	ai_action_offensive = 1,
	ai_action_defensive = 2,
	ai_action_flee = 3,
	ai_action_heal_heavy = 4,
	ai_action_heal_medium = 5,
	ai_action_heal_light = 6,
	ai_action_cure_poison = 7,
	ai_action_resurrect = 8
};

public enum SpellSourceType : int {
	Ability = 0,
	Arcane = 1,
	Divine = 2,
	Psionic = 3,
	Any = 4
};

public enum SpellReadyingType : int {
	Vancian = 0, // memorization slots
	Innate, // bards / sorcerers etc.
	Any
};

public enum SpellListType : int {
	None = 0,
	Any, // for prestige classes that stack spell progression with anything
	Arcane,
	Bardic, // subset of Arcane
	Clerical, // subset of Divine
	Divine,
	Druidic, // subset of divine
	Paladin, // subset of divine
	Psionic,
	Ranger, // subset of divine
	Special, // "independent" list
	Extender // extends an existing spell list
}

public struct SpellStoreData
    {
        public uint spellEnum;
        public uint classCode;
        public uint spellLevel;
        public SpellStoreState spellStoreState;
        public ushort padSpellStore;
        public MetaMagicData metaMagicData; // should be stored as 32bit value!
        public char pad0;
        public uint pad1; // these are actually related to MM indicator icons
        public uint pad2;
        public uint pad3;

        public SpellStoreData(int SpellEnum, int SpellLevel, int ClassCode, uint mmData, int SpellStoreData) : this()
        {
	        spellEnum = (uint) SpellEnum;
	        classCode = (uint) ClassCode;
	        spellLevel = (uint) SpellLevel;
	        metaMagicData = MetaMagicData.Unpack(mmData);
	        spellStoreState = SpellStoreState.Unpack(SpellStoreData);
        }

        public SpellStoreData(int SpellEnum, int SpellLevel, int ClassCode, MetaMagicData mmData, SpellStoreState spellStoreData) : this() {
            spellEnum = (uint) SpellEnum;
            classCode = (uint) ClassCode;
            spellLevel = (uint) SpellLevel;
            metaMagicData = mmData;
            spellStoreState = spellStoreData;
        }

        public SpellStoreData(int SpellEnum, int SpellLevel, int ClassCode, MetaMagicData mmData) : this() {
            spellEnum = (uint) SpellEnum;
            classCode = (uint) ClassCode;
            spellLevel = (uint) SpellLevel;
            metaMagicData = mmData;
        }

        public static bool operator <(SpellStoreData sp1, SpellStoreData sp2)
        {
	        int levelDelta = (int) sp1.spellLevel - (int) sp2.spellLevel;
	        if (levelDelta < 0)
		        return true;
	        else if (levelDelta > 0)
		        return false;

	        // if levels are equal
	        var name1 = GameSystems.Spell.GetSpellName(sp1.spellEnum);
	        var name2 = GameSystems.Spell.GetSpellName(sp2.spellEnum);
	        var nameCmp = string.CompareOrdinal(name1, name2);
	        return nameCmp < 0;
        }

        public static bool operator >(SpellStoreData a, SpellStoreData b)
        {
	        throw new System.NotImplementedException();
        }

    }

}