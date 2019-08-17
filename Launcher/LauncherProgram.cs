﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using SpicyTemple.Core;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO.MesFiles;
using SpicyTemple.Core.IO.SaveGames.Archive;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.TigSubsystems;

namespace Launcher
{
    public static class LauncherProgram
    {
        public static void Main(string[] args)
        {
/*
            using (var stream = new StreamWriter("D:/skills.txt"))
            {
                //foreach (var field in typeof(WellKnownSpells).GetFields())
                //{
                //   var constant = (int) field.GetRawConstantValue();
                //    stream.WriteLine($"{(int) constant}: 'WellKnownSpells.{field.Name}',");
                //}

                string[] names = Enum.GetNames(typeof(SkillId));
                foreach (var name in names)
                {
                    var literal = Enum.Parse<SkillId>(name);
                    stream.WriteLine($"{(int) literal}: 'SkillId.{name}',");
                }
            }

            return;*/

            if (args.Length > 0 && args[0] == "--extract-save")
            {
                ExtractSaveArchive.Main(args.Skip(1).ToArray());
                return;
            }

            if (args.Length == 2 && args[0] == "--mes-to-json")
            {
                var mesContent = MesFile.Read(args[1]);
                var newFile = Path.ChangeExtension(args[1], ".json");
                var options = new JsonSerializerOptions();
                options.WriteIndented = true;
                var jsonContent = JsonSerializer.Serialize(mesContent.ToDictionary(
                    kvp => kvp.Key.ToString(),
                    kvp => kvp.Value
                ), options);
                File.WriteAllText(newFile, jsonContent);
                return;
            }

            if (args.Length > 0 && args[0] == "--dump-addresses")
            {
                var dumper = new AddressDumper();
                dumper.DumpAddresses();
                return;
            }

            using var spicyTemple = new SpicyTemple.Core.MainGame();

            spicyTemple.Run();

            var camera = Tig.RenderingDevice.GetCamera();
            camera.CenterOn(0, 0, 0);

            var gameLoop = new GameLoop(
                Tig.MessageQueue,
                Tig.RenderingDevice,
                Tig.ShapeRenderer2d,
                Globals.Config.Rendering,
                Tig.DebugUI
            );
            gameLoop.Run();
        }
    }
}