
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
    [SpellScript(595)]
    public class BoltOfGlory : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Bolt of Glory OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-evocation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Bolt of Glory OnSpellEffect");
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Bolt of Glory OnBeginRound");
        }
        public override void OnBeginProjectile(SpellPacketBody spell, GameObjectBody projectile, int index_of_target)
        {
            Logger.Info("Bolt of Glory OnBeginProjectile");
            // spell.proj_partsys_id = game.particles( 'sp-Ray of Frost', projectile )
            SetProjectileParticles(projectile, AttachParticles("sp-Searing Light", projectile));
        }
        public override void OnEndProjectile(SpellPacketBody spell, GameObjectBody projectile, int index_of_target)
        {
            Logger.Info("Bolt of Glory OnEndProjectile");
            var target_item = spell.Targets[0];
            var damage_dice = Dice.D6;
            var alignment = target_item.Object.GetAlignment();
            if ((target_item.Object.IsMonsterCategory(MonsterCategory.outsider) && alignment.IsEvil())
                || target_item.Object.IsMonsterCategory(MonsterCategory.undead))
            {
                damage_dice = damage_dice.WithCount(Math.Min(15, spell.casterLevel));
            }
            else
            {
                damage_dice = damage_dice.WithCount(Math.Min(7, spell.casterLevel / 2));
            }

            spell.duration = 0;
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
                AttachParticles("sp-Searing Light-Hit", target_item.Object);
                // hit
                if ((target_item.Object.IsMonsterCategory(MonsterCategory.outsider) && ((alignment & Alignment.NEUTRAL_GOOD)) != 0))
                {
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 32060);
                    AttachParticles("Fizzle", target_item.Object);
                }
                else
                {
                    target_item.Object.DealSpellDamage(spell.caster, DamageType.PositiveEnergy, damage_dice, D20AttackPower.UNSPECIFIED, 100, D20ActionType.CAST_SPELL, spell.spellId, return_val, index_of_target);
                }

            }
            else
            {
                // missed
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30007);
                AttachParticles("Fizzle", target_item.Object);
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
            Logger.Info("Bolt of Glory OnEndSpellCast");
        }

    }
}
