using System;
using System.IO;
using SpicyTemple.Core.Config;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.Startup;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui;
using SpicyTemple.Core.Ui.Assets;
using SpicyTemple.Core.Ui.MainMenu;
using SpicyTemple.Core.Ui.Styles;

namespace SpicyTemple.Core
{
    public class SpicyTemple : IDisposable
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        private readonly SingleInstanceCheck _singleInstanceCheck = new SingleInstanceCheck();

        public void Run()
        {
            // Load the game configuration and - if necessary - write a default file
            var config = LoadConfig();

            Tig.Startup(config);

            // setMiles3dProvider();;

            // It's pretty unclear what this is used for
            // TODO bufferstuffFlag = bufferstuffFlag | 0x40;
            // TODO bufferstuffWidth = tig.GetConfig().width;
            // TODO bufferstuffHeight = tig.GetConfig().height;

            // Hides the cursor during loading
            Tig.Mouse.HideCursor();

            GameSystems.Init();

            /*
                Process options applicable after initialization of game systems
            */
            // TODO applyGameConfig();

            Tig.Mouse.SetCursor("art/interface/cursors/MainCursor.tga");

            Globals.UiManager = new UiManager();
            Globals.UiAssets = new UiAssets();
            Globals.WidgetTextStyles = new WidgetTextStyles();
            Globals.WidgetButtonStyles = new WidgetButtonStyles();

            UiSystems.Startup(config);

            // TODO gameSystems.LoadModule(config.defaultModule); // TODO: RAII

            // Python should now be initialized. Do the global hooks
            // TODO PythonGlobalExtension::installExtensions();

            // Notify the UI system that the module has been loaded
            // TODO UiModuleLoader uiModuleLoader(uiSystems);

            //temple::GetRef<BOOL(__cdecl)()>(0x10036720)(); // check dialog

            // TODO if (!config.skipIntro) {
                //movieFuncs.PlayMovie("movies\\introcinematic.bik", 0, 0, 0);
            //}

            // TODO uiSystems.ResizeViewport(config.renderWidth, config.renderHeight);

            // Show the main menu
            Tig.Mouse.ShowCursor();
            UiSystems.MainMenu.Show(MainMenuPage.MainMenu);
            // TODO temple::GetRef<int>(0x10BD3A68) = 1; // Purpose unknown and unconfirmed, may be able to remove

            // Run console commands from "startup.txt" (working dir)
// TODO             logger->info("[Running Startup.txt]");
// TODO             startupRelevantFuncs.RunBatchFile("Startup.txt");
// TODO             logger->info("[Beginning Game]");

// TODO             Updater updater;

        }

        private GameConfig LoadConfig()
        {
            var gameFolders = new GameFolders();

            var configPath = Path.Join(gameFolders.UserDataFolder, "config.json");
            var configManager = new GameConfigManager(configPath);

            Globals.ConfigManager = configManager;
            Globals.Config = configManager.Config;
            Globals.GameFolders = gameFolders;

            return configManager.Config;
        }

        public void Dispose()
        {
            _singleInstanceCheck.Dispose();
        }
    }
}