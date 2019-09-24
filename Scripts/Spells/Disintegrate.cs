
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
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts.Spells
{
    [SpellScript(127)]
    public class Disintegrate : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Disintegrate OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-transmutation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Disintegrate OnSpellEffect");
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Disintegrate OnBeginRound");
        }
        public override void OnBeginProjectile(SpellPacketBody spell, GameObjectBody projectile, int index_of_target)
        {
            Logger.Info("Disintegrate OnBeginProjectile");
            // spell.proj_partsys_id = game.particles( 'sp-Ray of Frost', projectile )
            SetProjectileParticles(projectile, AttachParticles("sp-Disintegrate", projectile));
        }
        public override void OnEndProjectile(SpellPacketBody spell, GameObjectBody projectile, int index_of_target)
        {
            Logger.Info("Disintegrate OnEndProjectile");
            var damage_dice = Dice.D6;
            damage_dice = damage_dice.WithCount(Math.Min(40, spell.casterLevel * 2));
            spell.duration = 0;
            var is_immune_to_crit = 0;
            var changed_con = 0;
            var target_item = spell.Targets[0];
            EndProjectileParticles(projectile);
            // WF Ray fix added by Shiningted (& two lines below)
            var has_it = 0;
            var x = 0;
            var y = 0;
            if (spell.caster.HasFeat(FeatId.WEAPON_FOCUS_RAY))
            {
                // game.particles( "sp-summon monster I", game.party[0] )
                has_it = 1;
                x = spell.caster.GetBaseStat(Stat.dexterity);
                y = x + 2;
                if (spell.caster.HasFeat(FeatId.GREATER_WEAPON_FOCUS_RAY))
                {
                    y = y + 2;
                }

                spell.caster.SetBaseStat(Stat.dexterity, y);
            }

            var return_val = spell.caster.PerformTouchAttack(target_item.Object);
            if ((return_val & D20CAF.HIT) != D20CAF.NONE)
            {
                AttachParticles("sp-Disintegrate-Hit", target_item.Object);
                if ((target_item.Object.GetNameId() == 14629 || target_item.Object.GetNameId() == 14621 || target_item.Object.GetNameId() == 14604) && !Co8.is_spell_flag_set(target_item.Object, Co8SpellFlag.FlamingSphere))
                {
                    // check for Otiluke's Resilient Sphere
                    if (target_item.Object.HasCondition(SpellEffects.SpellOtilukesResilientSphere))
                    {
                        target_item.Object.AddCondition("sp-Break Enchantment", spell.spellId, spell.duration, 0);
                        SpawnParticles("sp-Otilukes Resilient Sphere-END", target_item.Object.GetLocation());
                    }
                    else
                    {
                        SpawnParticles("sp-Stoneskin", target_item.Object.GetLocation());
                        target_item.Object.Destroy();
                    }

                }
                else
                {
                    // hit
                    if (target_item.Object.IsMonsterCategory(MonsterCategory.construct) || target_item.Object.IsMonsterCategory(MonsterCategory.undead))
                    {
                        if (target_item.Object.GetBaseStat(Stat.constitution) < 0)
                        {
                            target_item.Object.SetBaseStat(Stat.constitution, 10);
                            changed_con = 1;
                        }

                        is_immune_to_crit = 1;
                    }
                    else if (target_item.Object.IsMonsterCategory(MonsterCategory.plant) || target_item.Object.IsMonsterCategory(MonsterCategory.ooze) || target_item.Object.IsMonsterCategory(MonsterCategory.elemental))
                    {
                        is_immune_to_crit = 1;
                    }

                    // elif return_val == 2:
                    // damage_dice.num = damage_dice.num * 2 # handled internally now
                    if (target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Fortitude, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                    {
                        damage_dice = damage_dice.WithCount(5);
                        // if return_val == 2 and is_immune_to_crit == 0:
                        // damage_dice.num = 10
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                    }

                    target_item.Object.DealSpellDamage(spell.caster, DamageType.Force, damage_dice, D20AttackPower.UNSPECIFIED, 100, D20ActionType.CAST_SPELL, spell.spellId, return_val, index_of_target);
                    if (target_item.Object.GetStat(Stat.hp_current) < 1)
                    {
                        target_item.Object.KillWithDeathEffect();
                        target_item.Object.AddCondition("sp-Animate Dead", spell.spellId, spell.duration, 3);
                        AttachParticles("sp-Stoneskin", target_item.Object);
                    }

                    // check for Otiluke's Resilient Sphere
                    if (target_item.Object.HasCondition(SpellEffects.SpellOtilukesResilientSphere))
                    {
                        target_item.Object.AddCondition("sp-Break Enchantment", spell.spellId, spell.duration, 0);
                        AttachParticles("sp-Otilukes Resilient Sphere-END", target_item.Object);
                    }

                }

            }
            else
            {
                // missed
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30007);
                AttachParticles("Fizzle", target_item.Object);
            }

            if (changed_con == 1)
            {
                target_item.Object.SetBaseStat(Stat.constitution, -1);
            }

            if (has_it == 1)
            {
                spell.caster.SetBaseStat(Stat.dexterity, x);
            }

            spell.RemoveTarget(target_item.Object);
            spell.EndSpell();
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Disintegrate OnEndSpellCast");
        }

    }
}