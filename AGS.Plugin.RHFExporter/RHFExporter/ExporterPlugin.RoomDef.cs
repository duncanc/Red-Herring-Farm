using System;
using System.Collections.Generic;
using System.Text;

namespace RedHerringFarm
{
    public partial class ExporterPlugin
    {
        public void ExportCurrentRoomDef()
        {
            AGS.Types.ILoadedRoom room = editor.RoomController.CurrentRoom;
            if (room == null)
            {
                throw new Exception("No room loaded!");
            }
            using (JsonWriter output = JsonWriter.Create(InExportFolder(ROOM_DEF_FILENAME, room.Number)))
            {
                WriteRoomDefJson(output);
            }
        }
        public void WriteRoomDefJson(JsonWriter output)
        {
            WriteRoomDefJson(output, null);
        }
        public void WriteRoomDefJson(JsonWriter output, string key)
        {
            AGS.Types.ILoadedRoom room = editor.RoomController.CurrentRoom;
            if (room == null)
            {
                output.WriteNull(key);
                return;
            }
            AGS.Types.IRoom room2 = null;
            for (int i = 0; i < editor.CurrentGame.Rooms.Count; i++)
            {
                if (editor.CurrentGame.Rooms[i].Number == room.Number)
                {
                    room2 = editor.CurrentGame.Rooms[i];
                }
            }
            if (room2 == null)
            {
                throw new Exception("Room #" + room.Number + " not found in the rooms list!");
            }
            using (output.BeginObject(key))
            {
                output.WriteValue("game", GetCurrentGameGuid());
                output.WriteValue("name", room2.Description);
                output.WriteValue("number", room.Number);
                output.WriteValue("stateSaving", room2.StateSaving);
                output.WriteValue("bottomEdgeY", room.BottomEdgeY);
                output.WriteValue("leftEdgeX", room.LeftEdgeX);
                output.WriteValue("rightEdgeX", room.RightEdgeX);
                output.WriteValue("topEdgeY", room.TopEdgeY);
                output.WriteValue("backgroundAnimationDelay", room.BackgroundAnimationDelay);
                output.WriteValue("legacyMusicVolumeAdjustment", EnumName(room.MusicVolumeAdjustment));
                output.WriteValue("playerCharacterView", room.PlayerCharacterView);
                output.WriteValue("colorDepth", room.ColorDepth);
                output.WriteValue("width", room.Width);
                output.WriteValue("height", room.Height);
                output.WriteValue("numBackgrounds", room.BackgroundCount);

                Dictionary<int,bool> usedHotspots;
                Dictionary<int,bool> usedRegions;
                Dictionary<int,bool> usedWalkables;
                Dictionary<int,bool> usedWalkBehinds;

                using (output.BeginObject("masks"))
                {
                    usedHotspots = WriteRoomMaskJson(output, "hotspots", AGS.Types.RoomAreaMaskType.Hotspots);
                    usedRegions = WriteRoomMaskJson(output, "regions", AGS.Types.RoomAreaMaskType.Regions);
                    usedWalkables = WriteRoomMaskJson(output, "walkableAreas", AGS.Types.RoomAreaMaskType.WalkableAreas);
                    usedWalkBehinds = WriteRoomMaskJson(output, "walkBehinds", AGS.Types.RoomAreaMaskType.WalkBehinds);
                }

                using (output.BeginArray("hotspots"))
                {
                    foreach (AGS.Types.RoomHotspot hs in room.Hotspots)
                    {
                        WriteRoomHotspotJson(output, hs, usedHotspots.ContainsKey(hs.ID));
                    }
                }

                using (output.BeginArray("objects"))
                {
                    foreach (AGS.Types.RoomObject obj in room.Objects)
                    {
                        WriteRoomObjectJson(output, obj);
                    }
                }

                using (output.BeginArray("walkbehinds"))
                {
                    foreach (AGS.Types.RoomWalkBehind wb in room.WalkBehinds)
                    {
                        WriteRoomWalkbehindJson(output, wb, usedWalkBehinds.ContainsKey(wb.ID));
                    }
                }

                using (output.BeginArray("walkableAreas"))
                {
                    foreach (AGS.Types.RoomWalkableArea area in room.WalkableAreas)
                    {
                        WriteRoomWalkableAreaJson(output, area, usedWalkables.ContainsKey(area.ID));
                    }
                }

                using (output.BeginArray("regions"))
                {
                    foreach (AGS.Types.RoomRegion reg in room.Regions)
                    {
                        WriteRoomRegionJson(output, reg, usedRegions.ContainsKey(reg.ID));
                    }
                }

                WriteCustomPropertiesJson(output, "properties", room.Properties);
            }
        }
        public Dictionary<int,bool> WriteRoomMaskJson(JsonWriter output, AGS.Types.RoomAreaMaskType maskType)
        {
            return WriteRoomMaskJson(output, null, maskType);
        }
        public Dictionary<int,bool> WriteRoomMaskJson(JsonWriter output, string key, AGS.Types.RoomAreaMaskType maskType)
        {
            Dictionary<int,bool> usedZones = new Dictionary<int,bool>();
            using (output.BeginArray(key))
            {
                QuadTreeTools.ZoneGetter getZone =
                    delegate(int x, int y)
                    {
                        int zone;
                        if (x < 0 || y < 0 || y >= editor.RoomController.CurrentRoom.Height || x >= editor.RoomController.CurrentRoom.Width)
                        {
                            zone = 0;
                        }
                        else
                        {
                            zone = editor.RoomController.GetAreaMaskPixel(maskType, x, y);
                        }
                        usedZones[zone] = true;
                        return zone;
                    };

                int rootSize = QuadTreeTools.GetRootSize(
                    editor.RoomController.CurrentRoom.Width,
                    editor.RoomController.CurrentRoom.Height);

                QuadTreeNode rootNode = new QuadTreeNode(getZone, QuadTreeTools.RootSizeToPixels(rootSize), 0, 0);
                rootNode.process(editor.RoomController.CurrentRoom.Width, editor.RoomController.CurrentRoom.Height);
                WriteQuadTreeNode(output, rootNode);
            }
            return usedZones;
        }
        private void WriteQuadTreeNode(JsonWriter output, QuadTreeNode node)
        {
            if (node.children == null)
            {
                output.WriteValue(node.zone);
            }
            else
            {
                output.WriteValue(-1);
                foreach (QuadTreeNode child in node.children)
                {
                    WriteQuadTreeNode(output, child);
                }
            }
        }
        public void WriteRoomHotspotJson(JsonWriter output, AGS.Types.RoomHotspot hs, bool used)
        {
            WriteRoomHotspotJson(output, null, hs, used);
        }
        public void WriteRoomHotspotJson(JsonWriter output, string key, AGS.Types.RoomHotspot hs, bool used)
        {
            using (output.BeginObject(key))
            {
                output.WriteValue("used", used);
                output.WriteValue("name", hs.Description);
                output.WriteValue("scriptName", hs.Name);
                WriteCustomPropertiesJson(output, "properties", hs.Properties);
                WriteInteractionsJson(output, "clickModeHandlers", hs.Interactions);
                if (hs.WalkToPoint == null)
                {
                    output.WriteNull("walkToX");
                    output.WriteNull("walkToY");
                }
                else
                {
                    output.WriteValue("walkToX", hs.WalkToPoint.X);
                    output.WriteValue("walkToY", hs.WalkToPoint.Y);
                }
            }
        }
        public void WriteRoomObjectJson(JsonWriter output, AGS.Types.RoomObject obj)
        {
            WriteRoomObjectJson(output, null, obj);
        }
        public void WriteRoomObjectJson(JsonWriter output, string key, AGS.Types.RoomObject obj)
        {
            if (obj == null)
            {
                output.WriteNull(key);
                return;
            }
            using (output.BeginObject(key))
            {
                output.WriteValue("baseline", obj.Baseline);
                output.WriteValue("scriptName", obj.Name);
                output.WriteValue("name", obj.Description);
                output.WriteValue("image", obj.Image);
                output.WriteValue("useRoomAreaLighting", obj.UseRoomAreaLighting);
                output.WriteValue("useRoomAreaScaling", obj.UseRoomAreaScaling);
                output.WriteValue("x", obj.StartX);
                output.WriteValue("y", obj.StartY);
                output.WriteValue("visible", obj.Visible);

                WriteCustomPropertiesJson(output, "properties", obj.Properties);
                WriteInteractionsJson(output, "clickModeHandlers", obj.Interactions);
            }
        }
        public void WriteRoomWalkbehindJson(JsonWriter writer, AGS.Types.RoomWalkBehind wb, bool used)
        {
            WriteRoomWalkbehindJson(writer, null, wb, used);
        }
        public void WriteRoomWalkbehindJson(JsonWriter output, string key, AGS.Types.RoomWalkBehind wb, bool used)
        {
            if (wb == null)
            {
                output.WriteNull(key);
                return;
            }
            using (output.BeginObject(key))
            {
                output.WriteValue("used", used);
                output.WriteValue("baseline", wb.Baseline);
            }
        }
        public void WriteRoomWalkableAreaJson(JsonWriter writer, AGS.Types.RoomWalkableArea area, bool used)
        {
            WriteRoomWalkableAreaJson(writer, null, area, used);
        }
        public void WriteRoomWalkableAreaJson(JsonWriter output, string key, AGS.Types.RoomWalkableArea area, bool used)
        {
            if (area == null)
            {
                output.WriteNull(key);
                return;
            }
            using (output.BeginObject(key))
            {
                output.WriteValue("used", used);
                output.WriteValue("areaSpecificView", area.AreaSpecificView);
                output.WriteValue("maxScalingLevel", area.MaxScalingLevel);
                output.WriteValue("minScalingLevel", area.MinScalingLevel);
                output.WriteValue("scalingLevel", area.ScalingLevel);
                output.WriteValue("useContinuousScaling", area.UseContinuousScaling);
            }
        }
        public void WriteRoomRegionJson(JsonWriter output, AGS.Types.RoomRegion reg, bool used)
        {
            WriteRoomRegionJson(output, null, reg, used);
        }
        public void WriteRoomRegionJson(JsonWriter output, string key, AGS.Types.RoomRegion reg, bool used)
        {
            if (reg == null)
            {
                output.WriteNull(key);
                return;
            }
            using (output.BeginObject(key))
            {
                output.WriteValue("used", used);
                output.WriteValue("blueTint", reg.BlueTint);
                output.WriteValue("greenTint", reg.GreenTint);
                output.WriteValue("lightLevel", reg.LightLevel);
                output.WriteValue("redTint", reg.RedTint);
                output.WriteValue("tintSaturation", reg.TintSaturation);
                output.WriteValue("useColorTint", reg.UseColourTint);

                WriteInteractionsJson(output, "interactions", reg.Interactions);
            }
        }
    }
}
