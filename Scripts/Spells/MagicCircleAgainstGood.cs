
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
    [SpellScript(284)]
    public class MagicCircleAgainstGood : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Magic Circle against Good OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-abjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Magic Circle against Good OnSpellEffect");
            spell.duration = 100 * spell.casterLevel;
            var target_item = spell.Targets[0];
            if (target_item.Object.IsFriendly(spell.caster))
            {
                target_item.Object.AddCondition("sp-Magic Circle Outward", spell.spellId, spell.duration, 2);
                target_item.ParticleSystem = AttachParticles("sp-Magic Circle against Good-OUT", target_item.Object);
            }
            else
            {
                target_item.Object.AddCondition("sp-Magic Circle Inward", spell.spellId, spell.duration, 2);
                target_item.ParticleSystem = AttachParticles("sp-Magic Circle against Good-IN", target_item.Object);
            }

        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Magic Circle against Good OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Magic Circle against Good OnEndSpellCast");
        }

    }
}