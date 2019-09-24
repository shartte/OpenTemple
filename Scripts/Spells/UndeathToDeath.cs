
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
    [SpellScript(587)]
    public class UndeathToDeath : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Undeath to Death OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-necromancy-conjure", spell.caster);
            // Sorts the targets by hit dice, so lowest hit dice are affected first
            spell.SortTargets(TargetListOrder.HitDiceThenDist, TargetListOrderDirection.Ascending);
            Logger.Info("target_list sorted by hitdice and dist from target_Loc (least to greatest): {0}", spell.Targets);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Undeath to Death OnSpellEffect");
            var remove_list = new List<GameObjectBody>();
            var kill_roll = Dice.D4;
            kill_roll = kill_roll.WithCount(Math.Min(20, spell.casterLevel));
            var hit_dice_max = kill_roll.Roll();
            foreach (var target_item in spell.Targets)
            {
                var obj_hit_dice = GameSystems.Critter.GetHitDiceNum(target_item.Object);
                // Only works on creatures of less than 9 hit dice, and only on undead
                if ((obj_hit_dice < 9) && (hit_dice_max >= obj_hit_dice) && (target_item.Object.IsMonsterCategory(MonsterCategory.undead)))
                {
                    hit_dice_max = hit_dice_max - obj_hit_dice;
                    // allow Will saving throw to negate
                    if (target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                    {
                        // saving throw successful
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                        AttachParticles("Fizzle", target_item.Object);
                    }
                    else
                    {
                        // saving throw unsuccessful
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                        // So you'll get awarded XP for the kill
                        if (!((SelectedPartyLeader.GetPartyMembers()).Contains(target_item.Object)))
                        {
                            target_item.Object.Damage(SelectedPartyLeader, DamageType.Unspecified, Dice.Parse("1d1"));
                        }

                        target_item.Object.Kill();
                        AttachParticles("sp-Holy Smite", target_item.Object);
                    }

                }

                remove_list.Add(target_item.Object);
            }

            spell.RemoveTargets(remove_list);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Undeath to Death OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Undeath to Death OnEndSpellCast");
        }

    }
}