using System;
using System.Collections.Generic;
using System.Drawing;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Fade;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.MainMenu
{
    public enum MainMenuPage
    {
        MainMenu,
        Difficulty,
        InGameNormal,
        InGameIronman,
        Options,
    }

    public class MainMenuUi : IDisposable
    {

        public MainMenuUi()
        {
            var mmLocalization = Tig.FS.ReadMesFile("mes/mainmenu.mes");

            var screenSize = Tig.RenderingDevice.GetCamera().ScreenSize;

            var widgetDoc = WidgetDoc.Load("ui/main_menu.json");
            mMainWidget = widgetDoc.TakeRootContainer();

            mViewCinematicsDialog = new ViewCinematicsDialog(this, mmLocalization);
            mSetPiecesDialog = new SetPiecesDialog(this);

            // This eats all mouse messages that reach the full-screen main menu
            mMainWidget.SetMouseMsgHandler(msg => { return true; });
            mMainWidget.SetWidgetMsgHandler(msg => { return true; });

            mMainWidget.SetKeyStateChangeHandler(msg =>
            {
                // Close the menu if it's the ingame menu
                if (msg.key == DIK.DIK_ESCAPE && !msg.down)
                {
                    if (mCurrentPage == MainMenuPage.InGameNormal || mCurrentPage == MainMenuPage.InGameIronman)
                    {
                        Hide();
                    }
                }

                return true;
            });

            mPagesWidget = widgetDoc.GetWindow("pages");

            mPageWidgets[MainMenuPage.MainMenu] = widgetDoc.GetWindow("page-main-menu");
            mPageWidgets[MainMenuPage.Difficulty] = widgetDoc.GetWindow("page-difficulty");
            mPageWidgets[MainMenuPage.InGameNormal] = widgetDoc.GetWindow("page-ingame-normal");
            mPageWidgets[MainMenuPage.InGameIronman] = widgetDoc.GetWindow("page-ingame-ironman");
            mPageWidgets[MainMenuPage.Options] = widgetDoc.GetWindow("page-options");
            //mPageWidgets[MainMenuPage.SetPieces] = widgetDoc.GetWindow("page-set-pieces");

            // Wire up buttons on the main menu
            widgetDoc.GetButton("new-game").SetClickHandler(() => { Show(MainMenuPage.Difficulty); });
            widgetDoc.GetButton("load-game").SetClickHandler(() =>
            {
                Hide();
                UiSystems.LoadGame.Show(true);
            });
            widgetDoc.GetButton("tutorial").SetClickHandler(LaunchTutorial);
            widgetDoc.GetButton("options").SetClickHandler(() => { Show(MainMenuPage.Options); });
            widgetDoc.GetButton("quit-game").SetClickHandler(() =>
            {
                Tig.MessageQueue.Enqueue(new Message(MessageType.EXIT));
            });

            // Wire up buttons on the difficulty selection page
            widgetDoc.GetButton("difficulty-normal").SetClickHandler(() =>
            {
                GameSystems.SetIronman(false);
                Hide();
                UiSystems.PCCreation.Start();
            });
            widgetDoc.GetButton("difficulty-ironman").SetClickHandler(() =>
            {
                GameSystems.SetIronman(true);
                Hide();
                UiSystems.PCCreation.Start();
            });
            widgetDoc.GetButton("difficulty-exit").SetClickHandler(() => { Show(MainMenuPage.MainMenu); });

            // Wire up buttons on the ingame menu (normal difficulty)
            widgetDoc.GetButton("ingame-normal-load").SetClickHandler(() =>
            {
                Hide();
                UiSystems.LoadGame.Show(false);
            });
            widgetDoc.GetButton("ingame-normal-save").SetClickHandler(() =>
            {
                Hide();
                UiSystems.SaveGame.Show(true);
            });
            widgetDoc.GetButton("ingame-normal-close").SetClickHandler(Hide);
            widgetDoc.GetButton("ingame-normal-quit").SetClickHandler(() =>
            {
                Hide();
                GameSystems.ResetGame();
                UiSystems.Reset();
                Show(MainMenuPage.MainMenu);
            });

            // Wire up buttons on the ingame menu (ironman difficulty)
            widgetDoc.GetButton("ingame-ironman-close").SetClickHandler(Hide);
            widgetDoc.GetButton("ingame-ironman-save-quit").SetClickHandler(() =>
            {
                if (GameSystems.SaveGameIronman())
                {
                    GameSystems.ResetGame();
                    UiSystems.Reset();
                    Show(MainMenuPage.MainMenu);
                }
            });

            // Wire up buttons on the ingame menu (ironman difficulty)
            widgetDoc.GetButton("options-show").SetClickHandler(() =>
            {
                Hide();
                UiSystems.Options.Show(true);
            });
            widgetDoc.GetButton("options-view-cinematics").SetClickHandler(() =>
            {
                Hide();
                UiSystems.UtilityBar.Hide();
                // TODO ui_mm_msg_ui4();
                mViewCinematicsDialog.Show();
            });
            widgetDoc.GetButton("options-credits").SetClickHandler(() =>
            {
                Hide();

                List<int> creditsMovies = new List<int> {100, 110, 111, 112, 113};
                foreach (var movieId in creditsMovies)
                {
                    GameSystems.GMovie.MovieQueueAdd(movieId);
                }

                GameSystems.GMovie.MovieQueuePlay();

                Show(MainMenuPage.Options);
            });
            widgetDoc.GetButton("options-back").SetClickHandler(() => { Show(MainMenuPage.MainMenu); });

            RepositionWidgets(screenSize.Width, screenSize.Height);

            Hide(); // Hide everything by default
        }

        public void Dispose()
        {
            // TODO
        }

        public void ResizeViewport(Size resizeArgs)
        {
            RepositionWidgets(resizeArgs.Width, resizeArgs.Height);
        }

        public string GetName()
        {
            return "MM-UI";
        }

        public bool IsVisible()
        {
            // The main menu is defined as visible, if any of the pages is visible
            foreach (var entry in mPageWidgets)
            {
                if (entry.Value.IsVisible())
                {
                    return true;
                }
            }

            return false;
        }

        public void Show(MainMenuPage page)
        {
            // Was previously @ 0x10116500

            // In case the main menu is shown in-game, we have to take care of some business
            if (!IsVisible())
            {
                if (page == MainMenuPage.InGameNormal || page == MainMenuPage.InGameIronman)
                {
                    GameSystems.TakeSaveScreenshots();
                    GameSystems.Anim.PushDisableFidget();
                }
            }

            // TODO: This seems wrong actually... This should come after hide()
            mCurrentPage = page;
            Hide();

            UiSystems.SaveGame.Hide();
            UiSystems.LoadGame.Hide();
            UiSystems.UtilityBar.HideOpenedWindows(false);
            UiSystems.CharSheet.Hide();

            mMainWidget.Show();
            mMainWidget.BringToFront();

            foreach (var entry in mPageWidgets)
            {
                entry.Value.SetVisible(entry.Key == page);
            }

            if (page != MainMenuPage.InGameNormal)
            {
                UiSystems.UtilityBar.Hide();
            }

            UiSystems.InGame.ResetInput();

            if (!UiSystems.UtilityBar.IsVisible())
                UiSystems.DungeonMaster.Hide();
        }

        public void Hide()
        {
            if (IsVisible())
            {
                if (mCurrentPage == MainMenuPage.InGameNormal || mCurrentPage == MainMenuPage.InGameIronman)
                {
                    GameSystems.Anim.PopDisableFidget();
                }
            }

            foreach (var entry in mPageWidgets)
            {
                entry.Value.SetVisible(false);
            }

            mMainWidget.Hide();

            if (mCurrentPage == MainMenuPage.InGameNormal)
            {
                UiSystems.UtilityBar.Show();
            }

            //if (UiSystems.UtilityBar.IsVisible())
            //	UiSystems.DungeonMaster.Show();
        }

        private MainMenuPage mCurrentPage = MainMenuPage.MainMenu;

        private ViewCinematicsDialog mViewCinematicsDialog;
        private SetPiecesDialog mSetPiecesDialog;

        private WidgetContainer mMainWidget;

        private Dictionary<MainMenuPage, WidgetContainer> mPageWidgets = new Dictionary<MainMenuPage, WidgetContainer>();

        // The widget that contains all pages
        private WidgetContainer mPagesWidget;

        private void RepositionWidgets(int width, int height)
        {
            // Attach the pages to the bottom of the screen
            mPagesWidget.SetY(height - mPagesWidget.GetHeight());
        }

        private void LaunchTutorial()
        {
            SetupTutorialMap();
            UiSystems.Party.UpdateAndShowMaybe();
            Hide();
            UiSystems.Party.Update();
        }

        [TempleDllLocation(0x10111AD0)]
        private void SetupTutorialMap()
        {
            if (!Tutorial.IsActive)
            {
                Tutorial.Toggle();
            }

            var tutorialMap = GameSystems.Map.GetMapIdByType(MapType.TutorialMap);
            TransitionToMap(tutorialMap);
        }

        internal void TransitionToMap(int mapId)
        {
            var fadeArgs = FadeArgs.Default;
            fadeArgs.countSthgUsually48 = 1;
            GameSystems.GFade.PerformFade(ref fadeArgs);
            GameSystems.Anim.StartFidgetTimer();

            var fadeTp = FadeAndTeleportArgs.Default;
            fadeTp.destLoc = GameSystems.Map.GetStartPos(mapId);
            fadeTp.destMap = mapId;
            fadeTp.flags = 4;
            fadeTp.somehandle = GameSystems.Party.GetLeader();

            var enterMovie = GameSystems.Map.GetEnterMovie(mapId, true);
            if (enterMovie != 0)
            {
                fadeTp.flags |= 1;
                fadeTp.field20 = 0;
                fadeTp.movieId = enterMovie;
            }

            fadeTp.field48 = 1;
            fadeTp.field4c = new PackedLinearColorA(0, 0, 0, 255);
            fadeTp.field50 = 64;
            fadeTp.somefloat2 = 3.0f;
            GameSystems.GFade.FadeAndTeleport(ref fadeTp);

            GameSystems.SoundGame.StopAll(false);
            UiSystems.WorldMapRandomEncounter.StartRandomEncounterTimer();
            GameSystems.Anim.PopDisableFidget();
        }

        public void InitializePlayerForTutorial()
        {
            var velkorProto = GameSystems.Object.GetProtoHandle(13105);

            var velkor = GameSystems.Object.CreateObject(velkorProto, new locXY(480, 40));
            var velkorObj = GameSystems.Object.GetObject(velkor);
            velkorObj.SetInt32(obj_f.pc_voice_idx, 11);
            GameSystems.Critter.GenerateHp(velkor);
            GameSystems.Party.AddToPCGroup(velkor);

            Tutorial.SpawnEquipment(velkor);

            // TODO var anim = objects.GetAnimHandle(velkor);
            // TODO objects.UpdateRenderHeight(velkor, *anim);
            // TODO objects.UpdateRadius(velkor, *anim);
        }
    }


    class ViewCinematicsDialog
    {
        private readonly MainMenuUi _mainMenu;

        public ViewCinematicsDialog(MainMenuUi mainMenu, IDictionary<int, string> mmMes)
        {
            _mainMenu = mainMenu;

            WidgetDoc doc = WidgetDoc.Load("ui/main_menu_cinematics.json");

            doc.GetButton("view").SetClickHandler(() =>
            {
                if (mSelection < 0 || mSelection >= seenIndices.Count)
                    return;
                var movieIdx = seenIndices[mSelection];
                if (movieIdx < 0 || movieIdx >= movieIds.Count)
                    return;
                var movieId = movieIds[movieIdx];
                GameSystems.GMovie.PlayMovieId(movieId, 0, 0);
            });
            doc.GetButton("cancel").SetClickHandler(() =>
            {
                mWidget.Hide();
                _mainMenu.Show(MainMenuPage.Options);
            });

            mListBox = doc.GetScrollView("cinematicsList");

            mWidget = doc.TakeRootContainer();
            mWidget.Hide();

            for (var i = 0; i < 24; i++)
            {
                mMovieNames[i] = mmMes[2000 + i];
            }
        }

        public void Show()
        {
            mListBox.Clear();
            btnIds.Clear();
            seenIndices.Clear();


            for (var i = 0; i < movieIds.Count; i++)
            {
                if (IsMovieSeen(movieIds[i]))
                {
                    seenIndices.Add(i);
                }
            }

            int y = 0;
            for (int i = 0; i < seenIndices.Count; i++)
            {
                var movieInd = seenIndices[i];

                var button = new WidgetButton();
                button.SetText(mMovieNames[movieInd]);
                button.SetId(mMovieNames[movieInd]);
                var innerWidth = mListBox.GetInnerWidth();
                button.SetWidth(innerWidth);
                button.SetAutoSizeWidth(false);
                button.SetStyle("mm-cinematics-list-button");
                button.SetY(y);
                //var pBtn = button.get();
                btnIds.Add(button.GetWidgetId());
                var selectIdx = i;
                button.SetClickHandler(() => Select(selectIdx));
                y += button.GetHeight();
                mListBox.Add(button);
            }

            mWidget.Show();
        }

        public void Select(int idx)
        {
            foreach (var it in btnIds)
            {
                var pBtn = (WidgetButton) Globals.UiManager.GetAdvancedWidget(it);
                pBtn.SetStyle("mm-cinematics-list-button");
            }

            mSelection = idx;
            if (mSelection >= 0 && mSelection < btnIds.Count)
            {
                var pBtn = (WidgetButton) Globals.UiManager.GetAdvancedWidget(btnIds[mSelection]);
                pBtn.SetStyle("mm-cinematics-list-button-selected");
            }
        } // changes scrollbox selection

        public bool IsMovieSeen(int movieId)
        {
            var moviesSeen = Globals.Config.GetVanillaString("movies_seen");
            var movieStr = $"({movieId},-1)";
            return moviesSeen.Contains(movieStr);
        }

        private int mSelection = 0;
        private WidgetContainer mWidget;
        private Dictionary<int, string> mMovieNames = new Dictionary<int, string>();
        private WidgetScrollView mListBox;
        private List<LgcyWidgetId> btnIds = new List<LgcyWidgetId>();
        private List<int> seenIndices = new List<int>(); // indices into movieIds / mMovieNames

        private List<int> movieIds = new List<int>
        {
            1000, 1009, 1007,
            1012, 1002, 1015,
            1005, 1010, 1004,
            1013, 1006, 1016,
            1001, 1011, 1008,
            1014, 1003, 1017,
            304, 300, 303,
            301, 302, 1009
        };
    };

    class SetPiecesDialog
    {
        private readonly MainMenuUi _mainMenuUi;

        public SetPiecesDialog(MainMenuUi mainMenuUi)
        {
            _mainMenuUi = mainMenuUi;

            WidgetDoc doc = WidgetDoc.Load("ui/main_menu_setpieces.json");

            doc.GetButton("go").SetClickHandler(() =>
            {
                mWidget.Hide();
                LaunchScenario();
            });
            doc.GetButton("cancel").SetClickHandler(() =>
            {
                mWidget.Hide();
                _mainMenuUi.Show(MainMenuPage.MainMenu);
            });

            mListBox = doc.GetScrollView("scenariosList");

            mWidget = doc.TakeRootContainer();
            mWidget.Hide();
        }

        public void Select(int i)
        {
            mSelection = i;
        }

        public void Show()
        {
            mListBox.Clear();

            int y = 0;
            const int NUM_SCENARIOS = 0;
            for (int i = 0; i < NUM_SCENARIOS; i++)
            {
                var button = new WidgetButton();
                button.SetText("Arena");
                button.SetId("Arena");
                var innerWidth = mListBox.GetInnerWidth();
                button.SetWidth(innerWidth);
                button.SetAutoSizeWidth(false);
                button.SetStyle("mm-setpieces-list-button");
                button.SetY(y);
                //var pBtn = button.get();
                btnIds.Add(button.GetWidgetId());
                var idx = i;
                button.SetClickHandler(() => Select(idx));
                y += button.GetHeight();
                mListBox.Add(button);
            }

            mWidget.Show();
        }

        public void LaunchScenario()
        {
            _mainMenuUi.InitializePlayerForTutorial();

            SetupScenario();
            UiSystems.Party.UpdateAndShowMaybe();
            _mainMenuUi.Hide();
            UiSystems.Party.Update();
        }

        public void SetupScenario()
        {
            var destMap = GameSystems.Map.GetMapIdByType(MapType.ArenaMap);
            _mainMenuUi.TransitionToMap(destMap);

            // TODO var args = PyTuple_New(0);

            // TODO var result = pythonObjIntegration.ExecuteScript("arena_script", "OnStartup", args);
            // TODO Py_DECREF(result);
            // TODO Py_DECREF(args);
        }

        private int mSelection = 0;
        private WidgetContainer mWidget;
        private WidgetScrollView mListBox;
        private List<LgcyWidgetId> btnIds;
    }
}