
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
    [ObjectScript(102)]
    public class Grud : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else if ((GetQuestState(35) == QuestState.Mentioned))
            {
                triggerer.BeginDialog(attachee, 70);
            }
            else if ((GetQuestState(35) == QuestState.Accepted))
            {
                triggerer.BeginDialog(attachee, 100);
            }
            else if ((GetQuestState(35) == QuestState.Completed))
            {
                triggerer.BeginDialog(attachee, 150);
            }
            else
            {
                triggerer.BeginDialog(attachee, 170);
            }

            return SkipDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetQuestState(35, QuestState.Botched);
            SetGlobalFlag(329, true);
            return RunDefault;
        }


    }
}
