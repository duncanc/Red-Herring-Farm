using System;
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using RedHerringFarm.TaskManaging;

namespace RedHerringFarm
{
    [AGS.Types.RequiredAGSVersion("3.2.0.0")]
    public partial class ExporterPlugin : AGS.Types.IAGSEditorPlugin, AGS.Types.IEditorComponent
    {
        public const string COMPONENT_ID = "RedHerringFarmExporter";
        public const string MENU_ID = "RHFExportMainMenu";
        public const string MENU_TITLE = "RHF Exporter";
        public const string COMMAND_EXPORT_ID = "RHFExportCommand";
        public const string COMMAND_EXPORT_TITLE = "Export Main Game Data";
        public const string COMMAND_EXPORT_ROOM_ID = "RHFExportRoomCommand";
        public const string COMMAND_EXPORT_ROOM_TITLE = "Export Current Room Data";
        public const string COMMAND_SETTINGS_ID = "RHFExportSettings";
        public const string COMMAND_SETTINGS_TITLE = "Settings...";
        public const string EXPORT_FOLDER_NAME = "Export";
        public const string GAME_DEF_FILENAME = "game.json";
        public const string ROOM_DEF_FILENAME = "room{0}.json";
        public const string ROOM_BACKGROUND_FILENAME = "room{0}background{1}.png";
        public const string FONT_SHEET_FILENAME = "font{0}.png";
        public const string TRANSLATION_FILENAME = "translation-{0}.json";
        public const string IMAGE_SHEET_FILENAME = "imageSheet{0}.png";
        public const string MAIN_ICON_FILENAME = "game.png";
        public const string SETUP_ICON_FILENAME = "setup.png";
        public const string GLOBAL_SCRIPTS_FILENAME = "globalScripts.js";
        public const string ROOM_SCRIPT_FILENAME = "room{0}script.js";

        private AGS.Types.IAGSEditor editor;

        private ExporterSettings settings = ExporterSettings.Default;
        public ExporterSettings Settings
        {
            get { return settings; }
            set { settings = value; }
        }

        SettingsPanel settingsPane;

        public ExporterPlugin(AGS.Types.IAGSEditor editor)
        {
            this.editor = editor;

            settingsPane = new SettingsPanel(this);
            settingsPane.contentDocument = new AGS.Types.ContentDocument(settingsPane, "RHF Export Settings", this);

            editor.AddComponent(this);

            editor.GUIController.AddMenu(this, MENU_ID, MENU_TITLE, editor.GUIController.FileMenuID);
            AGS.Types.MenuCommands mainMenuCommands = new AGS.Types.MenuCommands(MENU_ID);
            mainMenuCommands.Commands.Add(new AGS.Types.MenuCommand(COMMAND_EXPORT_ID, COMMAND_EXPORT_TITLE));
            mainMenuCommands.Commands.Add(new AGS.Types.MenuCommand(COMMAND_EXPORT_ROOM_ID, COMMAND_EXPORT_ROOM_TITLE));
            mainMenuCommands.Commands.Add(new AGS.Types.MenuCommand(COMMAND_SETTINGS_ID, COMMAND_SETTINGS_TITLE));
            editor.GUIController.AddMenuItems(this, mainMenuCommands);
        }

        public void Dispose()
        {
        }

        public void BeforeSaveGame()
        {
        }

        public const string TASK_EXPORT_MAIN_GAME_DATA = "Exporting main game data...";
        public const string TASK_EXPORT_GLOBAL_SCRIPTS = "Exporting global scripts...";
        public const string TASK_EXPORT_CURRENT_ROOM_DATA = "Exporting current room data...";
        public const string TASK_PREPARE_IMAGE_SHEETS = "Preparing image sheets...";
        public const string TASK_PREPARE_BITMAP_EXPORT = "Preparing bitmap export...";
        public const string TASK_EXPORT_GAME_DEF = "Exporting game definition...";
        public const string TASK_EXPORT_GAME_IMAGE_SHEETS = "Exporting image sheets...";
        public const string TASK_EXPORT_ICONS = "Exporting icons...";
        public const string TASK_EXPORT_TRANSLATIONS = "Exporting translations...";
        public const string TASK_EXPORT_ROOM_DEF = "Exporting room definition...";
        public const string TASK_EXPORT_ROOM_SCRIPT = "Exporting room script...";
        public const string TASK_EXPORT_ROOM_BACKGROUNDS = "Exporting room backgrounds...";
        public const string TASK_POSTPROCESS_BITMAPS = "Postprocessing bitmaps...";

        public void ExportMainGameData()
        {
            if (!HacksAndKludges.AreDialogScriptsCached(editor))
            {
                System.Windows.Forms.MessageBox.Show(
                    "Please build the project properly first.",
                    "Build Required",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Warning);
                return;
            }
            ExportProgress progressDialog = new ExportProgress();
            progressDialog.ExportBackgroundWorker.DoWork +=
                delegate(object sender, DoWorkEventArgs e)
                {
                    BackgroundWorker worker = (BackgroundWorker)sender;

                    TaskManager.Expect(TASK_EXPORT_MAIN_GAME_DATA);

                    using (TaskManager exportTask = TaskManager.Start(TASK_EXPORT_MAIN_GAME_DATA))
                    {
                        progressDialog.SetTaskManager(exportTask);

                        TaskManager.Expect(TASK_PREPARE_BITMAP_EXPORT);
                        TaskManager.Expect(TASK_EXPORT_GLOBAL_SCRIPTS);
                        TaskManager.Expect(TASK_PREPARE_IMAGE_SHEETS);
                        TaskManager.Expect(TASK_EXPORT_GAME_DEF);
                        TaskManager.Expect(TASK_EXPORT_ICONS);
                        TaskManager.Expect(TASK_EXPORT_GAME_IMAGE_SHEETS);
                        TaskManager.Expect(TASK_EXPORT_TRANSLATIONS);
                        TaskManager.Expect(TASK_POSTPROCESS_BITMAPS);

                        using (TaskManager.Start(TASK_PREPARE_BITMAP_EXPORT))
                        {
                            PrepareBitmapExport();
                        }
                        using (TaskManager.Start(TASK_PREPARE_IMAGE_SHEETS))
                        {
                            PrepareGameImageSheets();
                        }
                        using (TaskManager.Start(TASK_EXPORT_GAME_DEF))
                        {
                            ExportGameDef();
                        }
                        using (TaskManager.Start(TASK_EXPORT_GLOBAL_SCRIPTS))
                        {
                            ExportGlobalScripts();
                        }
                        using (TaskManager.Start(TASK_EXPORT_ICONS, true))
                        {
                            ExportIcons();
                        }
                        using (TaskManager.Start(TASK_EXPORT_GAME_IMAGE_SHEETS, true))
                        {
                            ExportGameImageSheets();
                        }
                        using (TaskManager.Start(TASK_EXPORT_TRANSLATIONS))
                        {
                            foreach (AGS.Types.Translation translation in editor.CurrentGame.Translations)
                            {
                                ExportTranslation(translation);
                            }
                        }
                        using (TaskManager.Start(TASK_POSTPROCESS_BITMAPS))
                        {
                            PostProcessBitmaps();
                        }
                    }
                };
            progressDialog.ShowDialog(HacksAndKludges.GetMainWindow());
        }

        public void ExportCurrentRoomData()
        {
            if (!HacksAndKludges.AreDialogScriptsCached(editor))
            {
                System.Windows.Forms.MessageBox.Show(
                    "Please build the project properly first.",
                    "Build Required",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Warning);
                return;
            }
            ExportProgress progressDialog = new ExportProgress();
            progressDialog.ExportBackgroundWorker.DoWork +=
                delegate(object sender, DoWorkEventArgs e)
                {
                    BackgroundWorker worker = (BackgroundWorker)sender;

                    TaskManager.Expect(TASK_EXPORT_CURRENT_ROOM_DATA);

                    using (TaskManager exportTask = TaskManager.Start(TASK_EXPORT_CURRENT_ROOM_DATA))
                    {
                        progressDialog.SetTaskManager(exportTask);

                        TaskManager.Expect(TASK_PREPARE_BITMAP_EXPORT);
                        TaskManager.Expect(TASK_EXPORT_ROOM_DEF);
                        TaskManager.Expect(TASK_EXPORT_ROOM_SCRIPT);
                        TaskManager.Expect(TASK_EXPORT_ROOM_BACKGROUNDS);
                        TaskManager.Expect(TASK_POSTPROCESS_BITMAPS);

                        using (TaskManager.Start(TASK_PREPARE_BITMAP_EXPORT))
                        {
                            PrepareBitmapExport();
                        }
                        using (TaskManager.Start(TASK_EXPORT_ROOM_DEF))
                        {
                            ExportCurrentRoomDef();
                        }
                        using (TaskManager.Start(TASK_EXPORT_ROOM_SCRIPT))
                        {
                            ExportCurrentRoomScript();
                        }
                        using (TaskManager.Start(TASK_EXPORT_ROOM_BACKGROUNDS))
                        {
                            ExportCurrentRoomBackgrounds();
                        }
                        using (TaskManager.Start(TASK_POSTPROCESS_BITMAPS))
                        {
                            PostProcessBitmaps();
                        }
                    }

                };
            progressDialog.ShowDialog(HacksAndKludges.GetMainWindow());
        }

        public void CommandClick(string command)
        {
            switch (command)
            {
                case COMMAND_EXPORT_ID:
                    ExportMainGameData();
                    break;
                case COMMAND_EXPORT_ROOM_ID:
                    if (editor.RoomController.CurrentRoom != null)
                    {
                        ExportCurrentRoomData();
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show(
                            HacksAndKludges.GetMainWindow(),
                            "You must be editing a room first.",
                            "No Room Loaded",
                            System.Windows.Forms.MessageBoxButtons.OK,
                            System.Windows.Forms.MessageBoxIcon.Warning);
                    }
                    break;
                case COMMAND_SETTINGS_ID:
                    editor.GUIController.AddOrShowPane(settingsPane.contentDocument);
                    break;
            }
        }

        public string ComponentID
        {
            get { return COMPONENT_ID; }
        }

        public void EditorShutdown()
        {
        }

        public void FromXml(XmlNode node)
        {
            if (node == null || node.NodeType != XmlNodeType.Element)
            {
                settings = ExporterSettings.Default;
            }
            else
            {
                XmlNode settingsNode = node.SelectSingleNode("Settings");
                if (settingsNode != null)
                {
                    settings = ExporterSettings.ReadXml(settingsNode);
                }
            }
        }

        public void GameSettingsChanged()
        {
        }

        public IList<AGS.Types.MenuCommand> GetContextMenu(string controlID)
        {
            List<AGS.Types.MenuCommand> commands = new List<AGS.Types.MenuCommand>();
            return commands;
        }

        public void PropertyChanged(string propertyName, object oldValue)
        {
        }

        public void RefreshDataFromGame()
        {
        }

        public void ToXml(XmlTextWriter writer)
        {
            settings.WriteXml(writer);
        }

        private string GetCurrentGameGuid()
        {
            return editor.CurrentGame.Settings.GUID.ToString("N").ToLower();
        }

        private string InGameFolder(string relative, params object[] format)
        {
            if (format != null && format.Length != 0)
            {
                relative = String.Format(relative, format);
            }
            string path = Path.Combine(editor.CurrentGame.DirectoryPath, relative);
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return path;
        }

        private string InExportFolder(string relative, params object[] format)
        {
            if (format != null && format.Length != 0)
            {
                relative = String.Format(relative, format);
            }
            string path = Path.Combine(InGameFolder(EXPORT_FOLDER_NAME), relative);
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return path;
        }

        private void ExportIcons()
        {
            string mainIconPath = InGameFolder("user.ico");
            if (File.Exists(mainIconPath))
            {
                Icon ico = new Icon(mainIconPath, new Size(16, 16));
                ExportBitmap(ico.ToBitmap(), InExportFolder(MAIN_ICON_FILENAME));
            }
            string setupIconPath = InGameFolder("setup.ico");
            if (File.Exists(setupIconPath))
            {
                Icon ico = new Icon(setupIconPath, new Size(16, 16));
                ExportBitmap(ico.ToBitmap(), InExportFolder(SETUP_ICON_FILENAME));
            }
        }
    }
}
