
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

namespace Scripts
{
    [ObjectScript(32008)]
    public class Trap9Spikes : BaseObjectScript
    {
        public override bool OnTrap(TrapSprungEvent trap, GameObjectBody triggerer)
        {
            if ((trap.Type.Id == 9))
            {
                // numP = 210 / (game.party_npc_size() + game.party_pc_size())
                // for obj in game.obj_list_vicinity( triggerer.location, OLC_CRITTERS ):
                // obj.stat_base_set(stat_experience, (obj.stat_level_get(stat_experience) - numP))
                AttachParticles(trap.Type.ParticleSystemId, trap.Object);
                Sound(4026, 1);
                var result = trap.Attack(triggerer, 10, 20, false);
                if (((result & D20CAF.HIT)) != D20CAF.NONE)
                {
                    foreach (var dmg in trap.Type.Damage)
                    {
                        if ((dmg.Type == DamageType.Poison))
                        {
                            if ((!triggerer.SavingThrow(13, SavingThrowType.Fortitude, D20CO8_F_POISON, trap.Object)))
                            {
                                triggerer.AddCondition("Poisoned", dmg.Dice.Modifier, 0);
                            }

                        }
                        else
                        {
                            var d = dmg.Dice.Copy();
                            if (((result & D20CAF.CRITICAL)) != D20CAF.NONE)
                            {
                                d = d.WithCount(d.Count * 2);
                                d = d.WithModifier(d.Modifier * 2);
                            }

                            triggerer.Damage(trap.Object, dmg.Type, d);
                        }

                    }

                }

            }

            if ((trap.Type.Id == 17))
            {
                // numP = 210 / (game.party_npc_size() + game.party_pc_size())
                // for obj in game.obj_list_vicinity( triggerer.location, OLC_CRITTERS ):
                // obj.stat_base_set(stat_experience, (obj.stat_level_get(stat_experience) - numP))
                AttachParticles(trap.Type.ParticleSystemId, trap.Object);
                Sound(4026, 1);
                var result = trap.Attack(triggerer, 14, 18, false);
                if (((result & D20CAF.HIT)) != D20CAF.NONE)
                {
                    foreach (var dmg in trap.Type.Damage)
                    {
                        if ((dmg.Type == DamageType.Poison))
                        {
                            if ((!triggerer.SavingThrow(18, SavingThrowType.Fortitude, D20CO8_F_POISON, trap.Object)))
                            {
                                triggerer.AddCondition("Poisoned", dmg.Dice.Modifier, 0);
                            }

                        }
                        else
                        {
                            var d = dmg.Dice.Copy();
                            if (((result & D20CAF.CRITICAL)) != D20CAF.NONE)
                            {
                                d = d.WithCount(d.Count * 2);
                                d = d.WithModifier(d.Modifier * 2);
                            }

                            triggerer.Damage(trap.Object, dmg.Type, d);
                        }

                    }

                }

            }

            DetachScript();
            return SkipDefault;
        }

    }
}
