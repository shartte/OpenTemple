
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
    [ObjectScript(584)]
    public class Bethany : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.TurnTowards(triggerer);
            if ((attachee.GetMap() == 5070 || attachee.GetMap() == 5071 || attachee.GetMap() == 5072 || attachee.GetMap() == 5073 || attachee.GetMap() == 5074 || attachee.GetMap() == 5075 || attachee.GetMap() == 5076 || attachee.GetMap() == 5077))
            {
                if ((triggerer.GetRace() == RaceId.half_orc))
                {
                    triggerer.BeginDialog(attachee, 6);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 1);
                }

            }
            else if ((attachee.GetMap() == 5171))
            {
                if ((GetGlobalFlag(560) && GetGlobalFlag(561) && GetGlobalFlag(562)))
                {
                    if ((!GetGlobalFlag(549)))
                    {
                        triggerer.BeginDialog(attachee, 240);
                    }
                    else
                    {
                        if ((!GetGlobalFlag(962)))
                        {
                            triggerer.BeginDialog(attachee, 510);
                        }
                        else
                        {
                            triggerer.BeginDialog(attachee, 520);
                        }

                    }

                }
                else
                {
                    if ((GetGlobalFlag(563)))
                    {
                        triggerer.BeginDialog(attachee, 540);
                    }
                    else
                    {
                        if ((!ScriptDaemon.npc_get(attachee, 1)))
                        {
                            triggerer.BeginDialog(attachee, 530);
                            ScriptDaemon.npc_set(attachee, 1);
                        }
                        else
                        {
                            if ((!GetGlobalFlag(962)))
                            {
                                triggerer.BeginDialog(attachee, 550);
                            }
                            else
                            {
                                triggerer.BeginDialog(attachee, 560);
                            }

                        }

                    }

                }

            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5171))
            {
                if ((!GetGlobalFlag(826) && GetQuestState(62) == QuestState.Accepted))
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                }

            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() == null))
            {
                if (CombatStandardRoutines.should_modify_CR(attachee))
                {
                    CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
                }

            }

            SetGlobalFlag(826, true);
            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(826, false);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5071))
            {
                if ((!ScriptDaemon.npc_get(attachee, 3)))
                {
                    attachee.SetInt(obj_f.hp_damage, 50);
                    StartTimer(2000, () => talk_talk(attachee, triggerer));
                    ScriptDaemon.npc_set(attachee, 3);
                }

                if ((ScriptDaemon.npc_get(attachee, 2) && !ScriptDaemon.npc_get(attachee, 4)))
                {
                    StartTimer(200, () => beth_exit(attachee, triggerer));
                    ScriptDaemon.npc_set(attachee, 4);
                }

            }

            return RunDefault;
        }
        public static bool face_holly(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var holly = Utilities.find_npc_near(attachee, 8714);
            attachee.TurnTowards(holly);
            holly.TurnTowards(attachee);
            return RunDefault;
        }
        public static bool heal_beth(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var dice = Dice.Parse("1d10+1000");
            attachee.Heal(null, dice);
            attachee.HealSubdual(null, dice);
            Sound(4182, 1);
            AttachParticles("sp-Heal", attachee);
            return RunDefault;
        }
        public static int is_25_and_under(GameObjectBody speaker, GameObjectBody listener)
        {
            if ((speaker.DistanceTo(listener) <= 25))
            {
                return 1;
            }

            return 0;
        }
        public static bool run_off(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.RunOff();
            return RunDefault;
        }
        public static void talk_talk(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.TurnTowards(PartyLeader);
            PartyLeader.BeginDialog(attachee, 1);
            ScriptDaemon.npc_set(attachee, 2);
            return;
        }
        public static bool beth_exit(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.ClearNpcFlag(NpcFlag.WAYPOINTS_DAY);
            attachee.ClearNpcFlag(NpcFlag.WAYPOINTS_NIGHT);
            attachee.RunOff(new locXY(480, 480));
            return RunDefault;
        }

    }
}
