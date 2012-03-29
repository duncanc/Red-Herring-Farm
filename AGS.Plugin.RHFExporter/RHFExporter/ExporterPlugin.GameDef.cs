using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using RedHerringFarm.ImageSheets;

namespace RedHerringFarm
{
    public partial class ExporterPlugin
    {
        public void ExportGameDef()
        {
            using (JsonWriter output = JsonWriter.Create(InExportFolder(GAME_DEF_FILENAME)))
            {
                WriteGameDefJson(output);
            }
        }
        public void WriteGameDefJson(JsonWriter output)
        {
            WriteGameDef(output, null);
        }
        public void WriteGameDef(JsonWriter output, string key)
        {
            using (output.BeginObject(key))
            {
                output.WriteValue("guid", GetCurrentGameGuid());

                WriteSettingsJson(output, "settings");

                // mouseCursors
                using (output.BeginArray("mouseCursors"))
                {
                    foreach (AGS.Types.MouseCursor cursor in editor.CurrentGame.Cursors)
                    {
                        WriteMouseCursorJson(output, cursor);
                    }
                }

                // room numbers
                using (output.BeginArray("roomNumbers"))
                {
                    foreach (AGS.Types.IRoom room in editor.CurrentGame.Rooms)
                    {
                        output.WriteValue(room.Number);
                    }
                }

                // translation names
                using (output.BeginArray("translationNames"))
                {
                    foreach (AGS.Types.Translation translation in editor.CurrentGame.Translations)
                    {
                        output.WriteValue(translation.Name);
                    }
                }

                // views
                using (output.BeginArray("views"))
                {
                    foreach (AGS.Types.View view in GetAllViews())
                    {
                        WriteViewJson(output, view);
                    }
                }

                // dialogs
                using (output.BeginArray("dialogs"))
                {
                    foreach (AGS.Types.Dialog dialog in editor.CurrentGame.Dialogs)
                    {
                        WriteDialogJson(output, dialog);
                    }
                }

                // characters
                using (output.BeginArray("characters"))
                {
                    foreach (AGS.Types.Character character in editor.CurrentGame.Characters)
                    {
                        WriteCharacterJson(output, character);
                    }
                }

                // inventory items
                using (output.BeginArray("inventoryItems"))
                {
                    foreach (AGS.Types.InventoryItem inventoryItem in editor.CurrentGame.InventoryItems)
                    {
                        WriteInventoryItemJson(output, inventoryItem);
                    }
                }

                // initial inventory items
                using (output.BeginArray("initialInventoryItems"))
                {
                    foreach (AGS.Types.InventoryItem inventoryItem in editor.CurrentGame.InventoryItems)
                    {
                        if (inventoryItem.PlayerStartsWithItem)
                        {
                            output.WriteValue(inventoryItem.ID);
                        }
                    }
                }

                // image sheets
                using (output.BeginArray("imageSheets"))
                foreach (ImageSheet sheet in GameImageSheets)
                {
                    WriteImageSheetJson(output, sheet);
                }

                // numbered images (sprites)
                WriteSpritesJson(output, "numberedImages");

                // fonts
                WriteFontsJson(output, "fonts");

                // GUIs
                using (output.BeginArray("guis"))
                {
                    foreach (AGS.Types.GUI gui in editor.CurrentGame.GUIs)
                    {
                        WriteGuiJson(output, gui);
                    }
                }

                output.WriteValue("playerCharacter", HacksAndKludges.GetPlayerCharacter(editor.CurrentGame).ID);

                // global messages
                using (output.BeginArray("globalMessages"))
                {
                    foreach (string s in HacksAndKludges.GetGlobalMessages(editor.CurrentGame))
                    {
                        output.WriteValue(s);
                    }
                }

                // text parser
                using (output.BeginObject("textParserWords"))
                {
                    foreach (AGS.Types.TextParserWord word in HacksAndKludges.GetTextParser(editor.CurrentGame).Words)
                    {
                        output.WriteValue(word.Word, word.WordGroup);
                    }
                }

                // custom property schema
                using (output.BeginObject("customPropertySchema"))
                {
                    using (output.BeginArray("definitions"))
                    {
                        foreach (AGS.Types.CustomPropertySchemaItem item in HacksAndKludges.GetPropertySchema(editor.CurrentGame).PropertyDefinitions)
                        using (output.BeginObject())
                        {
                            output.WriteValue("appliesToCharacters", item.AppliesToCharacters);
                            output.WriteValue("appliesToHotspots", item.AppliesToHotspots);
                            output.WriteValue("appliesToInvItems", item.AppliesToInvItems);
                            output.WriteValue("appliesToObjects", item.AppliesToObjects);
                            output.WriteValue("appliesToRooms", item.AppliesToRooms);
                            output.WriteValue("name", item.Name);
                            output.WriteValue("description", item.Description);
                            output.WriteValue("type", EnumName(item.Type));
                            switch (item.Type)
                            {
                                case AGS.Types.CustomPropertyType.Boolean:
                                    output.WriteValue("defaultValue", item.DefaultValue == "true");
                                    break;
                                case AGS.Types.CustomPropertyType.Number:
                                    output.WriteValue("defaultValue", double.Parse(item.DefaultValue));
                                    break;
                                case AGS.Types.CustomPropertyType.Text:
                                    output.WriteValue("defaultValue", item.DefaultValue);
                                    break;
                            }
                        }
                    }
                }

                // lip sync
                using (output.BeginObject("lipSync"))
                {
                    output.WriteValue("type", EnumName(HacksAndKludges.GetLipSync(editor.CurrentGame).Type));
                    output.WriteValue("defaultFrame", HacksAndKludges.GetLipSync(editor.CurrentGame).DefaultFrame);
                    using (output.BeginArray("charactersPerFrame"))
                    {
                        foreach (string cpf in HacksAndKludges.GetLipSync(editor.CurrentGame).CharactersPerFrame)
                        {
                            output.WriteValue(cpf);
                        }
                    }
                }

                // old interaction variables
                using (output.BeginArray("oldInteractionVariables"))
                {
                    foreach (AGS.Types.OldInteractionVariable old in HacksAndKludges.GetOldInteractionVariables(editor.CurrentGame))
                    using (output.BeginObject())
                    {
                        output.WriteValue("scriptName", old.ScriptName);
                        output.WriteValue("value", old.Value);
                    }
                }
            }
        }

        public void WriteSettingsJson(JsonWriter output)
        {
            WriteSettingsJson(output, null);
        }
        public void WriteSettingsJson(JsonWriter output, string key)
        {
            using (output.BeginObject(key))
            {
                AGS.Types.Settings settings = editor.CurrentGame.Settings;

                switch (settings.ColorDepth)
                {
                    case AGS.Types.GameColorDepth.Palette:
                        output.WriteValue("colorDepth", 8);
                        break;
                    case AGS.Types.GameColorDepth.HighColor:
                        output.WriteValue("colorDepth", 16);
                        break;
                    case AGS.Types.GameColorDepth.TrueColor:
                        output.WriteValue("colorDepth", 32);
                        break;
                    default:
                        throw new Exception("unknown color depth: " + settings.ColorDepth);
                }

                switch (settings.Resolution)
                {
                    case AGS.Types.GameResolutions.R320x200:
                        output.WriteValue("width", 320);
                        output.WriteValue("height", 200);
                        break;
                    case AGS.Types.GameResolutions.R320x240:
                        output.WriteValue("width", 320);
                        output.WriteValue("height", 240);
                        break;
                    case AGS.Types.GameResolutions.R640x400:
                        output.WriteValue("width", 640);
                        output.WriteValue("height", 400);
                        break;
                    case AGS.Types.GameResolutions.R640x480:
                        output.WriteValue("width", 640);
                        output.WriteValue("height", 480);
                        break;
                    case AGS.Types.GameResolutions.R800x600:
                        output.WriteValue("width", 800);
                        output.WriteValue("height", 600);
                        break;
                    case AGS.Types.GameResolutions.R1024x768:
                        output.WriteValue("width", 1024);
                        output.WriteValue("height", 768);
                        break;
                    default:
                        throw new Exception("unknown resolution: " + settings.Resolution);
                }

                using (output.BeginObject("skipSpeech"))
                {
                    switch (settings.SkipSpeech)
                    {
                        case AGS.Types.SkipSpeechStyle.MouseOrKeyboardOrTimer:
                            output.WriteValue("mouse", true);
                            output.WriteValue("keyboard", true);
                            output.WriteValue("timer", true);
                            break;
                        case AGS.Types.SkipSpeechStyle.MouseOrKeyboard:
                            output.WriteValue("mouse", true);
                            output.WriteValue("keyboard", true);
                            output.WriteValue("timer", false);
                            break;
                        case AGS.Types.SkipSpeechStyle.MouseOnly:
                            output.WriteValue("mouse", true);
                            output.WriteValue("keyboard", false);
                            output.WriteValue("timer", true);
                            break;
                        case AGS.Types.SkipSpeechStyle.KeyboardOnly:
                            output.WriteValue("mouse", false);
                            output.WriteValue("keyboard", true);
                            output.WriteValue("timer", false);
                            break;
                        case AGS.Types.SkipSpeechStyle.TimerOnly:
                            output.WriteValue("mouse", false);
                            output.WriteValue("keyboard", false);
                            output.WriteValue("timer", true);
                            break;
                    }
                }

                output.WriteValue("name", settings.GameName);
                output.WriteValue("maxScore", settings.MaximumScore);
                output.WriteValue("mouseWheelSupport", settings.MouseWheelEnabled);
                output.WriteValue("gameWideSpeechAnimationSpeed", settings.LegacySpeechAnimationSpeed);
                output.WriteValue("useLowResCoordinates", settings.UseLowResCoordinatesInScript);
                output.WriteValue("autoMoveLookMode", settings.WalkInLookMode);
                output.WriteValue("autoMoveWalkMode", settings.AutoMoveInWalkMode);
                output.WriteValue("turnBeforeWalking", settings.TurnBeforeWalking);
                output.WriteValue("turnToFaceDirection", settings.TurnBeforeFacing);
                output.WriteValue("debug", settings.DebugMode);
                output.WriteValue("dialogBulletPointImage", settings.DialogOptionsBullet);
                output.WriteValue("dialogGapPixels", settings.DialogOptionsGap);
                output.WriteValue("numberDialogOptions", settings.NumberDialogOptions);
                output.WriteValue("dialogOptionsUpwards", settings.DialogOptionsBackwards);
                output.WriteValue("animateDuringDialogOptions", settings.RunGameLoopsWhileDialogOptionsDisplayed);
                output.WriteValue("speechPortraitSide", EnumName(settings.SpeechPortraitSide));
                output.WriteValue("speechStyle", EnumName(settings.SpeechStyle));
                output.WriteValue("dialogOptionsGui", settings.DialogOptionsGUI);
                output.WriteValue("multipleInventoryIcons", settings.DisplayMultipleInventory);

                AGS.Types.InventoryHotspotMarker marker = settings.InventoryHotspotMarker;
                if (marker == null)
                {
                    output.WriteNull("inventoryItemCursorHotspotMarker");
                }
                else
                {
                    switch (marker.Style)
                    {
                        case AGS.Types.InventoryHotspotMarkerStyle.None:
                            output.WriteNull("inventoryItemCursorHotspotMarker");
                            break;
                        case AGS.Types.InventoryHotspotMarkerStyle.Sprite:
                            using (output.BeginObject("inventoryItemCursorHotspotMarker"))
                            {
                                output.WriteValue("type", "sprite");
                                output.WriteValue("image", marker.Image);
                            }
                            break;
                        case AGS.Types.InventoryHotspotMarkerStyle.Crosshair:
                            using (output.BeginObject("inventoryItemCursorHotspotMarker"))
                            {
                                output.WriteValue("type", "crosshair");
                                output.WriteValue("crosshairColor", marker.CrosshairColor);
                                output.WriteValue("dotColor", marker.DotColor);
                            }
                            break;
                        default:
                            throw new Exception("Unknown InventoryHotspotMarkerStyle: " + marker.Style);
                    }
                }

                output.WriteValue("overrideInventoryWindowClick", settings.HandleInvClicksInScript);
                output.WriteValue("inventoryItemCursor", settings.InventoryCursors);
                output.WriteValue("scoreSound", settings.PlaySoundOnScore);
                output.WriteValue("alwaysDisplaySpeech", settings.AlwaysDisplayTextAsSpeech);
                output.WriteValue("antiAliasTtf", settings.AntiAliasFonts);
                output.WriteValue("textWindowGui", settings.TextWindowGUI);
                output.WriteValue("thoughtBubbleGui", settings.ThoughtGUI);
                output.WriteValue("fontsHiRes", settings.FontsForHiRes);
                output.WriteValue("gameTextLeftToRight", settings.BackwardsText);
                output.WriteValue("roomTransition", EnumName(settings.RoomTransition));
                output.WriteValue("guiAlphaStyle", EnumName(settings.GUIAlphaStyle));
                output.WriteValue("pixelPerfect", settings.PixelPerfect);
                output.WriteValue("whenInterfaceDisabled", EnumName(settings.WhenInterfaceDisabled));
            }
        }

        public void WriteMouseCursorJson(JsonWriter output, AGS.Types.MouseCursor cursor)
        {
            WriteMouseCursorJson(output, null, cursor);
        }
        public void WriteMouseCursorJson(JsonWriter output, string key, AGS.Types.MouseCursor cursor)
        {
            if (cursor == null)
            {
                output.WriteNull(key);
                return;
            }
            using (output.BeginObject(key))
            {
                output.WriteValue("animate", cursor.Animate);
                output.WriteValue("animateOnlyOnHotspots", cursor.AnimateOnlyOnHotspots);
                output.WriteValue("animateOnlyWhenMoving", cursor.AnimateOnlyWhenMoving);
                output.WriteValue("image", cursor.Image);
                output.WriteValue("view", cursor.View);
                output.WriteValue("hotspotX", cursor.HotspotX);
                output.WriteValue("hotspotY", cursor.HotspotY);
                output.WriteValue("name", cursor.Name);
                output.WriteValue("standardMode", cursor.StandardMode);
            }
        }

        public void WriteViewJson(JsonWriter output, AGS.Types.View view)
        {
            WriteView(output, null, view);
        }
        private void WriteView(JsonWriter output, string key, AGS.Types.View view)
        {
            if (view == null)
            {
                output.WriteNull(key);
                return;
            }
            using (output.BeginObject(key))
            {
                using (output.BeginArray("loops"))
                {
                    foreach (AGS.Types.ViewLoop loop in view.Loops)
                    {
                        WriteLoopJson(output, loop);
                    }
                }
            }
        }

        public void WriteLoopJson(JsonWriter output, AGS.Types.ViewLoop loop)
        {
            WriteLoopJson(output, null, loop);
        }
        public void WriteLoopJson(JsonWriter output, string key, AGS.Types.ViewLoop loop)
        {
            if (loop == null)
            {
                output.WriteNull(key);
                return;
            }
            using (output.BeginObject(key))
            {
                output.WriteValue("runNextLoop", loop.RunNextLoop);
                using (output.BeginArray("frames"))
                {
                    foreach (AGS.Types.ViewFrame frame in loop.Frames)
                    {
                        WriteFrameJson(output, frame);
                    }
                }
            }
        }

        public void WriteFrameJson(JsonWriter output, AGS.Types.ViewFrame frame)
        {
            WriteFrameJson(output, null, frame);
        }
        public void WriteFrameJson(JsonWriter output, string key, AGS.Types.ViewFrame frame)
        {
            if (frame == null)
            {
                output.WriteNull(key);
                return;
            }
            using (output.BeginObject(key))
            {
                output.WriteValue("image", frame.Image);
                output.WriteValue("delay", frame.Delay);
                output.WriteValue("flipped", frame.Flipped);
                output.WriteValue("sound", frame.Sound);
            }
        }

        public void WriteDialogJson(JsonWriter output, AGS.Types.Dialog dialog)
        {
            WriteDialogJson(output, null, dialog);
        }
        public void WriteDialogJson(JsonWriter output, string key, AGS.Types.Dialog dialog)
        {
            if (dialog == null)
            {
                output.WriteNull(key);
                return;
            }
            using (output.BeginObject(key))
            {
                output.WriteValue("scriptName", dialog.Name);
                output.WriteValue("showTextParser", dialog.ShowTextParser);
                using (output.BeginArray("options"))
                {
                    foreach (AGS.Types.DialogOption option in dialog.Options)
                    {
                        WriteDialogOptionJson(output, option);
                    }
                }
            }
        }

        public void WriteDialogOptionJson(JsonWriter output, AGS.Types.DialogOption option)
        {
            WriteDialogOptionJson(output, null, option);
        }
        public void WriteDialogOptionJson(JsonWriter output, string key, AGS.Types.DialogOption option)
        {
            if (option == null)
            {
                output.WriteNull();
                return;
            }
            using (output.BeginObject(key))
            {
                output.WriteValue("text", option.Text);
                output.WriteValue("show", option.Show);
                output.WriteValue("say", option.Say);
            }
        }

        public void WriteCharacterJson(JsonWriter output, AGS.Types.Character character)
        {
            WriteCharacterJson(output, null, character);
        }
        public void WriteCharacterJson(JsonWriter output, string key, AGS.Types.Character character)
        {
            using (output.BeginObject(key))
            {
                output.WriteValue("scriptName", character.ScriptName);
                output.WriteValue("room", character.StartingRoom);
                output.WriteValue("clickable", character.Clickable);
                output.WriteValue("x", character.StartX);
                output.WriteValue("y", character.StartY);
                output.WriteValue("name", character.RealName);
                output.WriteValue("speechColor", character.SpeechColor);
                output.WriteValue("animationDelay", character.AnimationDelay);
                output.WriteValue("normalView", character.NormalView);

                WriteCustomPropertiesJson(output, "properties", character.Properties);
                WriteInteractionsJson(output, "interactions", character.Interactions);
            }
        }

        public void WriteInteractionsJson(JsonWriter output, AGS.Types.Interactions interactions)
        {
            WriteInteractionsJson(output, null, interactions);
        }
        public void WriteInteractionsJson(JsonWriter output, string key, AGS.Types.Interactions interactions)
        {
            using (output.BeginObject(key))
            {
                for (int i = 0; i < interactions.ScriptFunctionNames.Length; i++)
                {
                    if (!String.IsNullOrEmpty(interactions.ScriptFunctionNames[i]))
                    {
                        using (output.BeginObject(i.ToString()))
                        {
                            output.WriteValue("func", interactions.ScriptFunctionNames[i]);
                            if (!String.IsNullOrEmpty(interactions.ImportedScripts[i]))
                            {
                                output.WriteValue("script", interactions.ImportedScripts[i]);
                            }
                        }
                    }
                }
            }
        }

        public void WriteInventoryItemJson(JsonWriter output, AGS.Types.InventoryItem inventoryItem)
        {
            WriteInventoryItemJson(output, null, inventoryItem);
        }

        public void WriteInventoryItemJson(JsonWriter output, string key, AGS.Types.InventoryItem inventoryItem)
        {
            using (output.BeginObject(key))
            {
                output.WriteValue("scriptName", inventoryItem.Name);
                output.WriteValue("name", inventoryItem.Description);
                output.WriteValue("image", inventoryItem.Image);
                output.WriteValue("hotspotX", inventoryItem.HotspotX);
                output.WriteValue("hotspotY", inventoryItem.HotspotY);
                output.WriteValue("cursorImage", inventoryItem.CursorImage);
                WriteCustomPropertiesJson(output, "properties", inventoryItem.Properties);
                WriteInteractionsJson(output, "interactions", inventoryItem.Interactions);
            }
        }

        public void WriteCustomPropertiesJson(JsonWriter output, AGS.Types.CustomProperties customProperties)
        {
            WriteCustomPropertiesJson(output, null, customProperties);
        }

        public void WriteCustomPropertiesJson(JsonWriter output, string key, AGS.Types.CustomProperties customProperties)
        {
            using (output.BeginObject(key))
            {
                foreach (AGS.Types.CustomProperty property in customProperties.PropertyValues.Values)
                {
                    output.WriteValue(property.Name, property.Value);
                }
            }
        }

        public void WriteGuiJson(JsonWriter output, AGS.Types.GUI gui)
        {
            WriteGuiJson(output, null, gui);
        }
        public void WriteGuiJson(JsonWriter output, string key, AGS.Types.GUI gui)
        {
            if (gui == null)
            {
                output.WriteNull(key);
                return;
            }
            using (output.BeginObject(key))
            {
                output.WriteValue("scriptName", gui.Name);
                output.WriteValue("backgroundColor", gui.BackgroundColor);
                output.WriteValue("backgroundImage", gui.BackgroundImage);

                if (gui is AGS.Types.NormalGUI)
                {
                    output.WriteValue("type", "normal");
                    AGS.Types.NormalGUI normalGui = (AGS.Types.NormalGUI)gui;

                    output.WriteValue("borderColor", normalGui.BorderColor);
                    output.WriteValue("clickable", normalGui.Clickable);
                    output.WriteValue("left", normalGui.Left);
                    output.WriteValue("top", normalGui.Top);
                    output.WriteValue("width", normalGui.Width);
                    output.WriteValue("height", normalGui.Height);
                    output.WriteValue("onClick", normalGui.OnClick);
                    output.WriteValue("popupYPos", normalGui.PopupYPos);
                    output.WriteValue("transparency", normalGui.Transparency);
                    switch (normalGui.Visibility)
                    {
                        case AGS.Types.GUIVisibility.MouseYPos:
                            output.WriteValue("visibility", "mouseYPos");
                            break;
                        case AGS.Types.GUIVisibility.Normal:
                            output.WriteValue("visibility", "normal");
                            break;
                        case AGS.Types.GUIVisibility.NormalButInitiallyOff:
                            output.WriteValue("visibility", "normalButInitiallyOff");
                            break;
                        case AGS.Types.GUIVisibility.Persistent:
                            output.WriteValue("visibility", "persistent");
                            break;
                        case AGS.Types.GUIVisibility.PopupModal:
                            output.WriteValue("visibility", "popupModal");
                            break;
                        default:
                            throw new Exception("Unknown GUIVisibility: " + normalGui.Visibility);
                    }
                    output.WriteValue("zOrder", normalGui.ZOrder);
                }
                else if (gui is AGS.Types.TextWindowGUI)
                {
                    output.WriteValue("type", "textWindow");
                    AGS.Types.TextWindowGUI textWindowGui = (AGS.Types.TextWindowGUI)gui;

                    output.WriteValue("borderColor", textWindowGui.BorderColor);
                    output.WriteValue("textColor", textWindowGui.TextColor);
                }
                else
                {
                    throw new Exception("Unknown GUI Type: " + gui);
                }

                using (output.BeginArray("controls"))
                {
                    foreach (AGS.Types.GUIControl control in gui.Controls)
                    {
                        WriteGuiControlJson(output, control);
                    }
                }
            }
        }

        public void WriteGuiControlJson(JsonWriter output, AGS.Types.GUIControl control)
        {
            WriteGuiControlJson(output, null, control);
        }
        public void WriteGuiControlJson(JsonWriter output, string key, AGS.Types.GUIControl control)
        {
            if (control == null)
            {
                output.WriteNull(key);
                return;
            }
            using (output.BeginObject(key))
            {
                output.WriteValue("scriptName", control.Name);
                output.WriteValue("left", control.Left);
                output.WriteValue("top", control.Top);
                output.WriteValue("width", control.Width);
                output.WriteValue("height", control.Height);

                output.WriteValue("zOrder", control.ZOrder);

                switch (control.ControlType)
                {
                    case "Label":
                        output.WriteValue("type", "label");
                        AGS.Types.GUILabel label = (AGS.Types.GUILabel)control;
                        output.WriteValue("font", label.Font);
                        output.WriteValue("text", label.Text);
                        output.WriteValue("textAlignment", EnumName(label.TextAlignment));
                        output.WriteValue("textColor", label.TextColor);
                        break;

                    case "Button":
                        output.WriteValue("type", "button");
                        AGS.Types.GUIButton button = (AGS.Types.GUIButton)control;
                        output.WriteValue("font", button.Font);
                        output.WriteValue("buttonAction", EnumName(button.ClickAction));
                        output.WriteValue("clipImage", button.ClipImage);
                        output.WriteValue("image", button.Image);
                        output.WriteValue("mouseoverImage", button.MouseoverImage);
                        output.WriteValue("newModeNumber", button.NewModeNumber);
                        output.WriteValue("onClick", button.OnClick);
                        output.WriteValue("pushedImage", button.PushedImage);
                        output.WriteValue("text", button.Text);
                        output.WriteValue("textAlignment", EnumName(button.TextAlignment));
                        output.WriteValue("textColor", button.TextColor);
                        break;

                    case "InventoryWindow":
                        output.WriteValue("type", "inventoryWindow");
                        AGS.Types.GUIInventory inv = (AGS.Types.GUIInventory)control;
                        output.WriteValue("characterId", inv.CharacterID);
                        output.WriteValue("itemHeight", inv.ItemHeight);
                        output.WriteValue("itemWidth", inv.ItemWidth);
                        break;

                    case "ListBox":
                        output.WriteValue("type", "listBpx");
                        AGS.Types.GUIListBox listBox = (AGS.Types.GUIListBox)control;
                        output.WriteValue("font", listBox.Font);
                        output.WriteValue("onSelectionChanged", listBox.OnSelectionChanged);
                        output.WriteValue("selectedBackgroundColor", listBox.SelectedBackgroundColor);
                        output.WriteValue("selectedTextColor", listBox.SelectedTextColor);
                        output.WriteValue("showBorder", listBox.ShowBorder);
                        output.WriteValue("showScrollArrows", listBox.ShowScrollArrows);
                        output.WriteValue("textAlignment", EnumName(listBox.TextAlignment));
                        output.WriteValue("textColor", listBox.TextColor);
                        break;

                    case "Slider":
                        output.WriteValue("type", "slider");
                        AGS.Types.GUISlider slider = (AGS.Types.GUISlider)control;
                        output.WriteValue("backgroundImage", slider.BackgroundImage);
                        output.WriteValue("handleImage", slider.HandleImage);
                        output.WriteValue("handleOffset", slider.HandleOffset);
                        output.WriteValue("maxValue", slider.MaxValue);
                        output.WriteValue("minValue", slider.MinValue);
                        output.WriteValue("onChange", slider.OnChange);
                        output.WriteValue("value", slider.Value);
                        break;

                    case "TextBox":
                        output.WriteValue("type", "textBox");
                        AGS.Types.GUITextBox textBox = (AGS.Types.GUITextBox)control;
                        output.WriteValue("font", textBox.Font);
                        output.WriteValue("onActivate", textBox.OnActivate);
                        output.WriteValue("showBorder", textBox.ShowBorder);
                        output.WriteValue("text", textBox.Text);
                        output.WriteValue("textColor", textBox.TextColor);
                        break;

                    case "TextWindowEdge":
                        output.WriteValue("type", "textWindowEdge");
                        AGS.Types.GUITextWindowEdge edge = (AGS.Types.GUITextWindowEdge)control;
                        output.WriteValue("image", edge.Image);
                        break;

                    default:
                        throw new Exception("Unknown GUI Control Type: " + control.ControlType);
                }
            }
        }

        // helper/util functions
        private string EnumName(Enum enumValue)
        {
            string ret = enumValue.ToString();
            return ret.Substring(0,1).ToLower() + ret.Substring(1);
        }
        private void WriteStringArrayJson(JsonWriter output, IEnumerable strings)
        {
            WriteStringArrayJson(output, null, strings);
        }
        private void WriteStringArrayJson(JsonWriter output, string key, IEnumerable strings)
        {
            using (output.BeginArray(key))
            {
                foreach (string str in strings)
                {
                    output.WriteValue(str);
                }
            }
        }
        private List<AGS.Types.View> GetAllViews()
        {
            List<AGS.Types.View> views = new List<AGS.Types.View>();
            GetViewsFromFolder(views, editor.CurrentGame.Views);
            return views;
        }

        private void GetViewsFromFolder(List<AGS.Types.View> list, AGS.Types.IViewFolder folder)
        {
            foreach (AGS.Types.View view in folder.Views)
            {
                while (list.Count <= view.ID)
                {
                    list.Add(null);
                }
                list[view.ID] = view;
            }
            foreach (AGS.Types.IViewFolder subfolder in folder.SubFolders)
            {
                GetViewsFromFolder(list, subfolder);
            }
        }
    }
}
