
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
    [SpellScript(286)]
    public class MagicFang : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Magic Fang OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-transmutation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Magic Fang OnSpellEffect");
            spell.duration = 10 * spell.casterLevel;
            var target_item = spell.Targets[0];
            target_item.Object.AddCondition("sp-Magic Fang", spell.spellId, spell.duration, 0);
            target_item.ParticleSystem = AttachParticles("sp-Magic Fang", target_item.Object);
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Magic Fang OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Magic Fang OnEndSpellCast");
        }

    }
}
