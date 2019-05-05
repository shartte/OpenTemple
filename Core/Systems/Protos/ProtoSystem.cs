using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.IO.TabFiles;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Systems.Protos
{
    public class ProtoSystem : IGameSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        private const string UserProtoDir = "rules/protos/";

        private const string TemplePlusProtoFile = "rules/protos_override.tab";

        private const string VanillaProtoFile = "rules/protos.tab";

        private readonly struct ProtoIdRange
        {
            public readonly int Start;
            public readonly int End;

            public ProtoIdRange(int start, int end)
            {
                Start = start;
                End = end;
            }
        }

        private static readonly ProtoIdRange[] ProtoIdRanges =
        {
            new ProtoIdRange(0, 999), // portal
            new ProtoIdRange(1000, 1999), // container
            new ProtoIdRange(2000, 2999), // scenery
            new ProtoIdRange(3000, 3999), // projectile
            new ProtoIdRange(4000, 4999), // weapon
            new ProtoIdRange(5000, 5999), // ammo
            new ProtoIdRange(6000, 6999), // armor
            new ProtoIdRange(7000, 7999), // money
            new ProtoIdRange(8000, 8999), // food
            new ProtoIdRange(9000, 9999), // scroll
            new ProtoIdRange(10000, 10999), // key
            new ProtoIdRange(11000, 11999), // written
            new ProtoIdRange(12000, 12999), // generic
            new ProtoIdRange(13000, 13999), // pc
            new ProtoIdRange(14000, 14999), // npc
            new ProtoIdRange(15000, 15999), // trap
            new ProtoIdRange(16000, 16999), // bag
        };

        public ProtoSystem()
        {
            foreach (var protoFilename in Tig.FS.Search(UserProtoDir + "*.tab"))
            {
                ParsePrototypesFile(UserProtoDir + protoFilename);
            }

            if (Tig.FS.FileExists(TemplePlusProtoFile))
            {
                ParsePrototypesFile(TemplePlusProtoFile);
            }

            ParsePrototypesFile(VanillaProtoFile);
        }

        public void Dispose()
        {
            // Remove the prototype objects for each type
            foreach (var range in ProtoIdRanges)
            {
                for (var protoId = range.Start; protoId <= range.End; ++protoId)
                {
                    var id = ObjectId.CreatePrototype((ushort) protoId);
                    var obj = GameSystems.Object.GetObject(id);
                    if (obj != null)
                    {
                        GameSystems.Object.Remove(obj);
                    }
                }
            }
        }

        [TempleDllLocation(0x10039220)]
        private static bool GetObjectTypeFromProtoId(int protoId, out ObjectType type)
        {
            for (var i = 0; i < ProtoIdRanges.Length; i++)
            {
                if (protoId >= ProtoIdRanges[i].Start && protoId <= ProtoIdRanges[i].End)
                {
                    type = (ObjectType) i;
                    return true;
                }
            }

            type = default;
            return false;
        }

        [TempleDllLocation(0x10039120)]
        private static int GetOeNameIdForType(ObjectType type)
        {
            switch (type)
            {
                case ObjectType.portal:
                    return 800;
                case ObjectType.container:
                    return 1200;
                case ObjectType.scenery:
                    return 1600;
                case ObjectType.projectile:
                    return 1980;
                case ObjectType.weapon:
                    return 2000;
                case ObjectType.ammo:
                    return 2400;
                case ObjectType.armor:
                    return 2800;
                case ObjectType.money:
                    return 3200;
                case ObjectType.food:
                    return 3600;
                case ObjectType.scroll:
                    return 4000;
                case ObjectType.key:
                    return 4400;
                case ObjectType.written:
                    return 5200;
                case ObjectType.generic:
                    return 5600;
                case ObjectType.pc:
                    return 6000;
                case ObjectType.npc:
                    return 6400;
                case ObjectType.trap:
                    return 10001;
                case ObjectType.bag:
                    return 10401;
                default:
                    return 0;
            }
        }

        [TempleDllLocation(0x1003b640)]
        private void ParsePrototypesFile(string path)
        {
            void ProcessProtoRecord(TabFileRecord record)
            {
                var protoId = record[0].GetInt();

                if (!GetObjectTypeFromProtoId(protoId, out var type))
                {
                    Logger.Error("Failed to determine object type for proto id {0}", protoId);
                    return;
                }

                var obj = GameSystems.Object.CreateProto(type, ObjectId.CreatePrototype((ushort) protoId));

                obj.SetInt32(obj_f.name, GetOeNameIdForType(type));

                ProtoDefaultValues.SetDefaultValues(protoId, obj);

                ProtoColumns.ParseColumns(protoId, record, obj);

                /*
                 TODO
                if (obj_get_int32(objHandle, obj_f.type) == ObjectType.npc)
                {
                    v9 = GameSystems.Stat.ObjStatBaseGet(objHandle, Stat.race);
                    v10 = GameSystems.Stat.ObjStatBaseGet(objHandle, Stat.gender);
                    obj_set_int32_or_float32(objHandle, obj_f.sound_effect, 10 * (v10 + 2 * v9 + 1));
                }

                sub_10073420(objHandle);
                sub_1003AAC0(objHandle);
                sub_1003AC50(objHandle);
                */
            }

            TabFile.ParseFile(path, ProcessProtoRecord);

            Debugger.Break();
        }
    }
}