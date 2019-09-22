
using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts
{
    [SpellScript(221)]
    public class HealingCircle : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Healing Circle OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-conjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Healing Circle OnSpellEffect");
            var remove_list = new List<GameObjectBody>();

            var dice = Dice.D8;

            dice = dice.WithModifier(Math.Min(20, spell.casterLevel));
            SpawnParticles("sp-Healing Circle", spell.aoeCenter);
            foreach (var target_item in spell.Targets)
            {
                AttachParticles("sp-Healing Circle Hit", target_item.Object);
                if (target_item.Object.IsMonsterCategory(MonsterCategory.undead))
                {
                    if (target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Fortitude, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                    {
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                        target_item.Object.DealReducedSpellDamage(spell.caster, DamageType.PositiveEnergy, dice, D20AttackPower.UNSPECIFIED, DAMAGE_REDUCTION_HALF, D20ActionType.CAST_SPELL, spell.spellId);
                    }
                    else
                    {
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                        target_item.Object.DealSpellDamage(spell.caster, DamageType.PositiveEnergy, dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
                    }

                }
                else
                {
                    target_item.Object.HealFromSpell(spell.caster, dice, D20ActionType.CAST_SPELL, spell.spellId);
                }

                remove_list.Add(target_item.Object);
            }

            spell.RemoveTargets(remove_list);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Healing Circle OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Healing Circle OnEndSpellCast");
        }


    }
}