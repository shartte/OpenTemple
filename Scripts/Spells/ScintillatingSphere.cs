
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
    [SpellScript(802)]
    public class ScintillatingSphere : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Scintillating Sphere OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-evocation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Scintillating Sphere OnSpellEffect");
            AttachParticles("sp-evocation-conjure", spell.caster);
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Scintillating Sphere OnBeginRound");
        }
        public override void OnBeginProjectile(SpellPacketBody spell, GameObjectBody projectile, int index_of_target)
        {
            Logger.Info("Scintillating Sphere OnBeginProjectile");
            // spell.proj_partsys_id = game.particles( 'sp-Scintillating Sphere-proj', projectile )
            SetProjectileParticles(projectile, AttachParticles("sp-Scintillating Sphere-proj", projectile));
        }
        public override void OnEndProjectile(SpellPacketBody spell, GameObjectBody projectile, int index_of_target)
        {
            Logger.Info("Scintillating Sphere OnEndProjectile");
            var remove_list = new List<GameObjectBody>();
            spell.duration = 0;
            var dam = Dice.D6;
            dam = dam.WithCount(Math.Min(10, spell.casterLevel));
            EndProjectileParticles(projectile);
            SpawnParticles("sp-Scintillating Sphere-Hit", spell.aoeCenter);
            foreach (var target_item in spell.Targets)
            {
                if (target_item.Object.ReflexSaveAndDamage(spell.caster, spell.dc, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, dam, DamageType.Electricity, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId))
                {
                    // saving throw successful
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                }
                else
                {
                    // saving throw unsuccessful
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                }

                remove_list.Add(target_item.Object);
            }

            spell.RemoveTargets(remove_list);
            spell.EndSpell();
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Scintillating Sphere OnEndSpellCast");
        }

    }
}
