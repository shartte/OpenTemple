
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Location;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts
{
    [ObjectScript(160)]
    public class OgreChief : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            triggerer.BeginDialog(attachee, 1);
            return SkipDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalVar(14, GetGlobalVar(14) + 1);
            if ((attachee.GetStat(Stat.subdual_damage) >= attachee.GetStat(Stat.hp_current)))
            {
                SetGlobalVar(13, GetGlobalVar(13) - 1);
            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalVar(14, GetGlobalVar(14) - 1);
            return RunDefault;
        }
        public override bool OnEndCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetStat(Stat.subdual_damage) >= attachee.GetStat(Stat.hp_current)))
            {
                SetGlobalVar(13, GetGlobalVar(13) + 1);
            }

            return RunDefault;
        }


    }
}
