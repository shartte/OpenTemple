
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
    [SpellScript(170)]
    public class FingerOfDeath : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Finger of Death OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-necromancy-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Finger of Death OnSpellEffect");
            var damage_dice = Dice.Parse("3d6");
            damage_dice = damage_dice.WithModifier(Math.Min(25, spell.caster.GetStat(spell.spellClass)));
            var target = spell.Targets[0];
            AttachParticles("sp-Slay Living", target.Object);
            // damage target
            if (target.Object.SavingThrowSpell(spell.dc, SavingThrowType.Fortitude, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
            {
                target.Object.FloatMesFileLine("mes/spell.mes", 30001);
                // saving throw succesful, damage target
                target.Object.DealSpellDamage(spell.caster, DamageType.Unspecified, damage_dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
            }
            else
            {
                target.Object.FloatMesFileLine("mes/spell.mes", 30002);
                // saving throw unsuccesful, kill target
                // So you'll get awarded XP for the kill
                if (!((SelectedPartyLeader.GetPartyMembers()).Contains(target.Object)))
                {
                    target.Object.Damage(SelectedPartyLeader, DamageType.Unspecified, Dice.Parse("1d1"));
                }

                target.Object.KillWithDeathEffect();
            }

            spell.RemoveTarget(target.Object);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Finger of Death OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Finger of Death OnEndSpellCast");
        }

    }
}
