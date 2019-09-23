
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
    [SpellScript(556)]
    public class DazeMonster : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Daze Monster OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-enchantment-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Daze Monster OnSpellEffect");
            spell.duration = 1;
            var target = spell.Targets[0];
            // only creatures of < 7 HD affected
            if (GameSystems.Critter.GetHitDiceNum(target.Object) < 7)
            {
                // allow Will saving throw to negate
                if (target.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                {
                    // saving throw successful
                    target.Object.FloatMesFileLine("mes/spell.mes", 30001);
                    AttachParticles("Fizzle", target.Object);
                    spell.RemoveTarget(target.Object);
                }
                else
                {
                    // saving throw unsuccessful
                    target.Object.FloatMesFileLine("mes/spell.mes", 30002);
                    // HTN - apply condition Daze Monster
                    target.Object.AddCondition("sp-Daze", spell.spellId, spell.duration, 0);
                    target.ParticleSystem = AttachParticles("sp-Daze", target.Object);
                }

            }
            else
            {
                AttachParticles("Fizzle", target.Object);
                spell.RemoveTarget(target.Object);
            }

            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Daze Monster OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Daze Monster OnEndSpellCast");
        }

    }
}
