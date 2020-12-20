using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.Script.Extensions;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    [AutoRegister]
    public class GrayElf
    {
        public const RaceId Id = RaceId.elf + (3 << 5);

        public static readonly RaceSpec RaceSpec = new RaceSpec(Id, RaceBase.elf, Subrace.gray_elf)
        {
            effectiveLevel = 0,
            helpTopic = "TAG_GRAY_ELF",
            flags = 0,
            conditionName = "Gray Elf",
            heightMale = (53, 65),
            heightFemale = (53, 65),
            weightMale = (87, 121),
            weightFemale = (82, 116),
            statModifiers = {(Stat.strength, -2), (Stat.dexterity, 2), (Stat.constitution, -2), (Stat.intelligence, 2)},
            ProtoId = 13026,
            materialOffset = 2, // offset into rules/material_ext.mes file,
            feats = {FeatId.SIMPLE_WEAPON_PROFICIENCY_ELF},
            useBaseRaceForDeity = true
        };

        public static readonly ConditionSpec Condition = ConditionSpec.Create(RaceSpec.conditionName)
            .AddAbilityModifierHooks(RaceSpec)
            .AddSkillBonuses(
                (SkillId.listen, 2),
                (SkillId.search, 2),
                (SkillId.spot, 2)
            )
            .AddBaseMoveSpeed(30)
            .AddHandler(DispatcherType.SaveThrowLevel, ElvenSaveBonusEnchantment)
            .AddFavoredClassHook(Stat.level_wizard)
            .AddHandler(DispatcherType.ConditionAddPre, ConditionImmunityOnPreAdd)
            .Build();

        public static void ElvenSaveBonusEnchantment(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoSavingThrow();
            var flags = dispIo.flags;
            if ((flags & D20SavingThrowFlag.SPELL_SCHOOL_ENCHANTMENT) != 0)
            {
                dispIo.bonlist.AddBonus(2, 31, 139); // Racial Bonus
            }
        }

        public static void ConditionImmunityOnPreAdd(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoCondStruct();
            if (dispIo.condStruct == SpellEffects.SpellSleep)
            {
                dispIo.outputFlag = false;
                evt.objHndCaller.FloatMesFileLine("mes/combat.mes", 5059, TextFloaterColor.Red); // "Sleep Immunity"
                GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(31, evt.objHndCaller, null);
            }
        }
    }
}