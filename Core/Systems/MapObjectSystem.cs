using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.GFX.RenderMaterials;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.MapSector;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Systems
{
    using ReplacementSet = IImmutableDictionary<MaterialPlaceholderSlot, ResourceRef<IMdfRenderMaterial>>;

    public class MapObjectSystem : IGameSystem
    {
        private const bool IsEditor = false;

        private static readonly ILogger Logger = new ConsoleLogger();

        // Loaded from rules/materials.mes
        private readonly Dictionary<int, ReplacementSet> _replacementSets = new Dictionary<int, ReplacementSet>();

        private static readonly Dictionary<string, MaterialPlaceholderSlot> SlotMapping =
            new Dictionary<string, MaterialPlaceholderSlot>
            {
                {"CHEST", MaterialPlaceholderSlot.CHEST},
                {"BOOTS", MaterialPlaceholderSlot.BOOTS},
                {"GLOVES", MaterialPlaceholderSlot.GLOVES},
                {"HEAD", MaterialPlaceholderSlot.HEAD}
            };


        public MapObjectSystem()
        {
            LoadReplacementSets("rules/materials.mes");
            if (Tig.FS.FileExists("rules/materials_ext.mes"))
            {
                LoadReplacementSets("rules/materials_ext.mes");
            }
        }

        public void Dispose()
        {
            // Clear all referenced maintained by the replacement sets
            foreach (var set in _replacementSets.Values)
            {
                foreach (var value in set.Values)
                {
                    value.Dispose();
                }
            }

            _replacementSets.Clear();
        }

        [TempleDllLocation(0x10021930)]
        public void FreeRenderState(GameObjectBody obj)
        {
            obj.DestroyRendering();

            var sectorLoc = new SectorLoc(obj.GetLocation());
            if (obj.IsStatic() || GameSystems.MapSector.IsSectorLoaded(sectorLoc))
            {
                using var lockedSector = new LockedMapSector(sectorLoc);
                lockedSector.RemoveObject(obj);
            }
        }

        [TempleDllLocation(0x100219B0)]
        public void RemoveMapObj(GameObjectBody obj)
        {
            if (obj.HasFlag(ObjectFlag.TEXT))
            {
                GameSystems.TextBubble.Remove(obj);
            }

            if (obj.HasFlag(ObjectFlag.TEXT_FLOATER))
            {
                GameSystems.TextFloater.Remove(obj);
            }

            GameSystems.Anim.ClearForObject(obj);
            GameSystems.AI.RemoveAiTimer(obj);

            if (!IsEditor)
            {
                GameSystems.D20.RemoveDispatcher(obj);
            }

            GameUiBridge.OnObjectDestroyed(obj);
            obj.DestroyRendering();
            GameSystems.Object.Remove(obj);
        }

        [TempleDllLocation(0x1009f550)]
        public bool ValidateSector(bool requireHandles)
        {
            // Check all objects
            foreach (var obj in GameSystems.Object.EnumerateNonProtos())
            {
                // Primary keys for objects must be persistable ids
                if (!obj.id.IsPersistable())
                {
                    Logger.Error("Found non persistable object id {0}", obj.id);
                    return false;
                }

                if (obj.IsProto())
                {
                    continue;
                }

                if (GameSystems.Object.GetInventoryFields(obj.type, out var idxField, out var countField))
                {
                    ValidateInventory(obj, idxField, countField, requireHandles);
                }
            }

            return true;
        }

        [TempleDllLocation(0x1001fa80)]
        public string GetDisplayName(GameObjectBody obj, GameObjectBody observer)
        {
            if (obj == null)
            {
                return "OBJ_HANDLE_NULL";
            }

            if (obj.type == ObjectType.key)
            {
                var keyId = obj.GetInt32(obj_f.key_key_id);
                return GameSystems.Description.GetKeyName(keyId);
            }

            if (obj.IsItem())
            {
                if (IsEditor || obj == observer || GameSystems.Item.IsIdentified(obj))
                {
                    return GameSystems.Description.Get(obj.GetInt32(obj_f.description));
                }

                var unknownDescriptionId = obj.GetInt32(obj_f.item_description_unknown);
                return GameSystems.Description.Get(unknownDescriptionId);
            }

            if (obj.IsPC())
            {
                return obj.GetString(obj_f.pc_player_name);
            }

            if (obj.IsNPC())
            {
                return GameSystems.Description.Get(GameSystems.Critter.GetDescriptionId(obj, observer));
            }

            return GameSystems.Description.Get(obj.GetInt32(obj_f.description));
        }

        public string GetDisplayName(GameObjectBody obj) => GetDisplayName(obj, obj);

        private bool ValidateInventory(GameObjectBody container, obj_f idxField, obj_f countField, bool requireHandles)
        {
            var content = container.GetObjectIdArray(idxField);

            if (content.Count != container.GetInt32(countField))
            {
                Logger.Error("Count stored in {0} doesn't match actual item count of {1}.",
                    countField, idxField);
                return false;
            }

            for (var i = 0; i < content.Count; ++i)
            {
                var itemId = container.GetObjectId(idxField, i);

                var positional = $"Entry {itemId} in {idxField}@{i} of {container.id}";

                if (itemId.IsNull)
                {
                    Logger.Error("{0} is null", positional);
                    return false;
                }
                else if (!itemId.IsHandle)
                {
                    if (requireHandles)
                    {
                        Logger.Error("{0} is not a handle, but handles are required.", positional);
                        return false;
                    }

                    if (!itemId.IsPersistable())
                    {
                        Logger.Error("{0} is not a valid persistable id.", positional);
                        return false;
                    }
                }

                var itemObj = GameSystems.Object.GetObject(itemId);

                if (itemObj == null)
                {
                    Logger.Error("{0} does not resolve to a loaded object.", positional);
                    return false;
                }

                if (itemObj == container)
                {
                    Logger.Error("{0} is contained inside of itself.", positional);
                    return false;
                }

                // Only items are allowed in containers
                if (!itemObj.IsItem())
                {
                    Logger.Error("{0} is not an item.", positional);
                    return false;
                }
            }

            return true;
        }

        [TempleDllLocation(0x100C2110)]
        public void AddDynamicObjectsToSector(ref SectorObjects sectorObjects, SectorLoc loc, bool unknownFlag)
        {
            foreach (var obj in GameSystems.Object.SpatialIndex.EnumerateInSector(loc))
            {
                var flags = obj.GetFlags();
                if (!flags.HasFlag(ObjectFlag.INVENTORY) && !obj.IsStatic())
                {
                    sectorObjects.Insert(obj);
                    AddAiTimerOnSectorLoad(obj, unknownFlag);
                }
            }
        }

        [TempleDllLocation(0x1001e800)]
        public void StartAnimating(GameObjectBody obj)
        {
            var flags = obj.GetFlags();
            if (flags.HasFlag(ObjectFlag.OFF) || flags.HasFlag(ObjectFlag.DESTROYED))
            {
                return;
            }

            if (obj.type == ObjectType.scenery)
            {
                var sceneryFlags = obj.GetSceneryFlags();
                if (sceneryFlags.HasFlag(SceneryFlag.NO_AUTO_ANIMATE))
                {
                    if (!GameSystems.Anim.IsProcessing)
                    {
                        GameSystems.Anim.ClearForObject(obj);
                    }

                    return;
                }
            }

            GameSystems.Anim.PushIdleOrLoop(obj);
        }

        [TempleDllLocation(0x10020F50)]
        public void SetFlags(GameObjectBody obj, ObjectFlag flags)
        {
            if (flags.HasFlag(ObjectFlag.FLAT))
            {
                ChangeFlat(obj, true);
                flags &= ObjectFlag.FLAT;
            }

            var currentFlags = obj.GetFlags();
            if (flags.HasFlag(ObjectFlag.OFF) && !currentFlags.HasFlag(ObjectFlag.OFF))
            {
                if (currentFlags.HasFlag(ObjectFlag.TEXT))
                {
                    GameSystems.TextBubble.Remove(obj);
                }

                if (currentFlags.HasFlag(ObjectFlag.TEXT_FLOATER))
                {
                    GameSystems.TextFloater.Remove(obj);
                }

                GameSystems.MapSector.SetLightHandleFlag(obj, 0);
            }

            obj.SetFlags(obj.GetFlags() | flags);
        }

        [TempleDllLocation(0x10020F50)]
        public void ClearFlags(GameObjectBody obj, ObjectFlag flags)
        {
            if (flags.HasFlag(ObjectFlag.FLAT))
            {
                ChangeFlat(obj, true);
                flags &= ObjectFlag.FLAT;
            }

            var startAnimating = false;
            uint clearRenderFlags = 0;
            var currentFlags = obj.GetFlags();
            if (flags.HasFlag(ObjectFlag.OFF) && !currentFlags.HasFlag(ObjectFlag.OFF))
            {
                clearRenderFlags |= 0x7000000;
                GameSystems.MapSector.SetLightHandleFlag(obj, 0);
                startAnimating = true;
            }

            if (flags.HasFlag(ObjectFlag.STONED) && currentFlags.HasFlag(ObjectFlag.STONED))
            {
                clearRenderFlags |= 0x2000000;
            }

            if (flags.HasFlag(ObjectFlag.ANIMATED_DEAD) && currentFlags.HasFlag(ObjectFlag.ANIMATED_DEAD))
            {
                clearRenderFlags |= 0x2000000;
            }

            if (flags.HasFlag(ObjectFlag.DONTLIGHT) && currentFlags.HasFlag(ObjectFlag.DONTLIGHT))
            {
                clearRenderFlags |= 0x2000000;
            }

            obj.SetFlags(obj.GetFlags() & ~flags);
            obj.SetInt32(obj_f.render_flags, (int) (obj.GetInt32(obj_f.render_flags) & ~clearRenderFlags));

            if (startAnimating)
            {
                if (!IsEditor)
                {
                    StartAnimating(obj);
                }
            }
        }

        /**
         * Changes the FLAT flag of an object and re-sorts the object-list in the sector.
         */
        private void ChangeFlat(GameObjectBody obj, bool enabled)
        {
            obj.SetFlag(ObjectFlag.FLAT, enabled);

            var location = obj.GetLocation();
            var sectorLoc = new SectorLoc(location);
            if (GameSystems.MapSector.IsSectorLoaded(sectorLoc))
            {
                using var lockedSector = new LockedMapSector(sectorLoc);
                lockedSector.RemoveObject(obj);
                lockedSector.AddObject(obj);
            }
        }

        [TempleDllLocation(0x1001D840)]
        public void ClearRectList()
        {
            // VERY LIKELY UNUSED
            Stub.TODO();
        }

        [TempleDllLocation(0x10025950)]
        public void Move(GameObjectBody obj, LocAndOffsets loc)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10025f70)]
        public void MoveToMap(GameObjectBody gameObjectBody, int mapId, LocAndOffsets loc)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x1001ffe0)]
        public void AddAiTimerOnSectorLoad(GameObjectBody obj, bool unknownFlag)
        {
            if (!IsEditor)
            {
                if (obj.IsNPC())
                {
                    var flags = obj.GetFlags();
                    if (!flags.HasFlag(ObjectFlag.DESTROYED))
                    {
                        GameSystems.AI.AddOrReplaceAiTimer(obj, unknownFlag ? 1 : 0);
                    }
                }
            }
        }

        private void AddReplacementSetEntry(int id, string entry)
        {
            if (entry.Length == 0)
            {
                return;
            }

            var set = new Dictionary<MaterialPlaceholderSlot, ResourceRef<IMdfRenderMaterial>>();

            var elems = entry.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var elem in elems)
            {
                var subElems = elem.Split(':');

                if (subElems.Length < 2)
                {
                    Logger.Warn("Invalid material replacement set {0}: {1}", id, entry);
                    continue;
                }

                var slotName = subElems[0];
                var mdfName = subElems[1];

                // Map the slot name to the enum literal for it
                if (!SlotMapping.TryGetValue(slotName, out var slot))
                {
                    Logger.Warn("Invalid material replacement set {0}: {1}", id, entry);
                    continue;
                }

                // Resolve the referenced material
                var material = Tig.MdfFactory.LoadMaterial(mdfName);
                if (!material.IsValid)
                {
                    Logger.Warn("Material replacement set {0} references unknown MDF: {1}", id, mdfName);
                    continue;
                }

                set[slot] = material;
            }

            _replacementSets[id] = set.ToImmutableDictionary();
        }

        [TempleDllLocation(0x10022170)]
        public void ApplyReplacementMaterial(IAnimatedModel model, int id, int fallbackId = -1)
        {
            var replacementSet = GetReplacementSet(id, fallbackId);
            foreach (var entry in replacementSet)
            {
                model.AddReplacementMaterial(entry.Key, entry.Value.Resource);
            }
        }

        // Retrieves a material replacement set from rules/materials.mes
        private ReplacementSet GetReplacementSet(int id, int fallbackId = -1)
        {
            if (_replacementSets.TryGetValue(id, out var set))
            {
                return set;
            }

            if (fallbackId != -1)
            {
                if (_replacementSets.TryGetValue(id, out set))
                {
                    return set;
                }
            }

            return ImmutableDictionary<MaterialPlaceholderSlot, ResourceRef<IMdfRenderMaterial>>.Empty;
        }

        private void LoadReplacementSets(string filename)
        {
            var mapping = Tig.FS.ReadMesFile(filename);

            foreach (var entry in mapping)
            {
                AddReplacementSetEntry(entry.Key, entry.Value);
            }
        }

        [TempleDllLocation(0x100252d0)]
        public void MoveItem(GameObjectBody item, locXY loc)
        {
            var flags = item.GetFlags();
            if (flags.HasFlag(ObjectFlag.DESTROYED))
            {
                return;
            }

            item.SetFlag(ObjectFlag.INVENTORY, false);
            item.SetLocation(loc);
            item.SetFloat(obj_f.offset_x, 0f);
            item.SetFloat(obj_f.offset_y, 0f);

            var secLoc = new SectorLoc(loc);
            if (GameSystems.MapSector.IsSectorLoaded(secLoc))
            {
                using var sector = new LockedMapSector(secLoc);
                sector.AddObject(item);
            }

            item.SetInt32(obj_f.render_flags, 0);
            GameSystems.MapSector.MapSectorResetLightHandle(item);

            item.UpdateRenderingState(true);

            GameSystems.ObjectEvent.NotifyMoved(item, LocAndOffsets.Zero, new LocAndOffsets(loc, 0, 0));
        }


        /// <summary>
        /// Creates a new object with the given prototype at the given location.
        /// </summary>
        public GameObjectBody CreateObject(GameObjectBody protoObj, locXY location)
        {
            var obj = GameSystems.Object.CreateFromProto(protoObj, location);

            InitDynamic(obj, location);

            return obj;
        }

        public GameObjectBody CloneObject(GameObjectBody obj, locXY location)
        {

            var dest = GameSystems.Object.Clone(obj);

            dest.SetDispatcher(null);
            InitDynamic(dest, location);

            LocAndOffsets extendedLoc;
            extendedLoc.location = location;
            extendedLoc.off_x = 0;
            extendedLoc.off_y = 0;
            GameSystems.MapObject.Move(dest, extendedLoc);

            if (dest.IsNPC())
            {
                StandPoint standpoint = new StandPoint();
                standpoint.location.location = location;
                standpoint.location.off_x = 0;
                standpoint.location.off_y = 0;
                standpoint.mapId = GameSystems.Map.GetCurrentMapId();
                standpoint.jumpPointId = -1;

                GameSystems.Critter.SetStandPoint(dest, StandPointType.Day, standpoint);
                GameSystems.Critter.SetStandPoint(dest, StandPointType.Night, standpoint);
            }

            return dest;

        }

        private void InitDynamic(GameObjectBody obj, locXY location)
        {
            // Mark the object and all its children as dynamic
            obj.SetFlag(ObjectFlag.DYNAMIC, true);
            obj.ForEachChild(itemObj =>
            {
                itemObj.SetFlag(ObjectFlag.DYNAMIC, true);
            });

            // Add the new object to the sector system if needed
            var sectorLoc = new SectorLoc(location);
            if (GameSystems.MapSector.IsSectorLoaded(sectorLoc))
            {
                using var sector = new LockedMapSector(sectorLoc);
                sector.AddObject(obj);
            }

            GameSystems.MapSector.MapSectorResetLightHandle(obj);

            // Init NPC state
            if (obj.IsNPC())
            {
                GameSystems.AI.AddAiTimer(obj);
            }

            if (obj.IsCritter())
            {
                GameSystems.D20.StatusSystem.D20StatusInit(obj);
            }

            // Apply random sizing of the 3d model if requested
            var flags = obj.GetFlags();
            if (flags.HasFlag(ObjectFlag.RANDOM_SIZE))
            {
                var scale = obj.GetInt32(obj_f.model_scale);
                scale -= new Random().Next(0, 21);
                obj.SetInt32(obj_f.model_scale, scale);
            }

            GameSystems.Item.PossiblySpawnInvenSource(obj);

            obj.UpdateRenderingState(true);

            LocAndOffsets fromLoc;
            fromLoc.location.locx = 0;
            fromLoc.location.locy = 0;
            fromLoc.off_x = 0;
            fromLoc.off_y = 0;

            LocAndOffsets toLoc = fromLoc;
            toLoc.location = location;

            GameSystems.ObjectEvent.NotifyMoved(obj, fromLoc, toLoc);
        }

        [TempleDllLocation(0x10021d20)]
        public bool HasAnim(GameObjectBody obj, EncodedAnimId animId)
        {
            var animModel = obj.GetOrCreateAnimHandle();
            if (animModel != null)
            {
                return animModel.HasAnim(animId);
            }

            return false;
        }

        [TempleDllLocation(0x1001f770)]
        public void MakeItemParented(GameObjectBody item, GameObjectBody parent)
        {
            Trace.Assert(parent != null);

            if ( !item.HasFlag(ObjectFlag.DESTROYED) )
            {
                GameSystems.Light.RemoveAttachedTo(item);

                var sectorLoc = new SectorLoc(item.GetLocation());

                if ( GameSystems.MapSector.IsSectorLoaded(sectorLoc) )
                {
                    using var lockedSector = new LockedMapSector(sectorLoc);
                    lockedSector.RemoveObject(item);
                }

                item.SetFlag(ObjectFlag.INVENTORY, true);
                item.SetObject(obj_f.item_parent, parent);
            }
        }

    }
}