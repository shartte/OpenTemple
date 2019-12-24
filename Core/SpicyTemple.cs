using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
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

[assembly: InternalsVisibleTo("SpicyTemple.Tests")]

namespace SpicyTemple.Core
{
    public class MainGame : IDisposable
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        private readonly SingleInstanceCheck _singleInstanceCheck = new SingleInstanceCheck();

        public bool Run()
        {
            // Load the game configuration and - if necessary - write a default file
            var config = LoadConfig();

            if (!ValidateOrPickInstallation(config))
            {
                return false;
            }

            Tig.Startup(config);

            // Hides the cursor during loading
            Tig.Mouse.HideCursor();

            GameSystems.Init();

            Tig.Mouse.SetCursor("art/interface/cursors/MainCursor.tga");

            Globals.UiManager = new UiManager();
            Globals.UiAssets = new UiAssets();
            Globals.WidgetTextStyles = new WidgetTextStyles();
            Globals.WidgetButtonStyles = new WidgetButtonStyles();

            UiSystems.Startup(config);

            GameSystems.LoadModule("ToEE");

            if (!config.SkipIntro)
            {
                GameSystems.Movies.PlayMovie("movies/introcinematic.bik", 0, 0, 0);
            }

            // Show the main menu
            Tig.Mouse.ShowCursor();
            UiSystems.MainMenu.Show(MainMenuPage.MainMenu);
            return true;
        }

        private bool ValidateOrPickInstallation(GameConfig config)
        {
            var currentPath = config.InstallationFolder;

            // If the directory is initially not set, try auto-detection
            if (currentPath == null)
            {
                if (InstallationDirSelector.TryFind(out currentPath))
                {
                    Logger.Info("Auto-Detected ToEE installation directory: {0}", currentPath);
                }
            }

            while (true)
            {
                var validationReport = ToEEInstallationValidator.Validate(currentPath);
                if (validationReport.IsValid)
                {
                    break;
                }

                if (!InstallationDirSelector.Select(
                    validationReport,
                    currentPath,
                    out var selectedPath
                ))
                {
                    return false;
                }

                currentPath = selectedPath;
            }

            if (currentPath != config.InstallationFolder)
            {
                config.InstallationFolder = currentPath;
                Globals.ConfigManager.Save();
                Logger.Info("Set installation directory to {0}", currentPath);
            }

            return true;
        }

        private GameConfig LoadConfig()
        {
            var gameFolders = new GameFolders();
            var configManager = new GameConfigManager(gameFolders);

            Globals.ConfigManager = configManager;
            Globals.GameFolders = gameFolders;

            return configManager.Config;
        }

        public void Dispose()
        {
            _singleInstanceCheck.Dispose();
        }
    }
}