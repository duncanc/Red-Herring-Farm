
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;

// These are things I think I should probably be able
// to do properly through the plugin API

namespace RedHerringFarm
{
    internal static class HacksAndKludges
    {
        public static AGS.Types.Character GetPlayerCharacter(AGS.Types.IGame game)
        {
            return ((AGS.Types.Game)game).PlayerCharacter;
        }

        public static string[] GetGlobalMessages(AGS.Types.IGame game)
        {
            return ((AGS.Types.Game)game).GlobalMessages;
        }

        public static AGS.Types.TextParser GetTextParser(AGS.Types.IGame game)
        {
            return ((AGS.Types.Game)game).TextParser;
        }

        public static AGS.Types.CustomPropertySchema GetPropertySchema(AGS.Types.IGame game)
        {
            return ((AGS.Types.Game)game).PropertySchema;
        }

        public static AGS.Types.LipSync GetLipSync(AGS.Types.IGame game)
        {
            return ((AGS.Types.Game)game).LipSync;
        }

        public static List<AGS.Types.OldInteractionVariable> GetOldInteractionVariables(AGS.Types.IGame game)
        {
            return ((AGS.Types.Game)game).OldInteractionVariables;
        }

        public static IList<AGS.Types.AudioClipType> GetAudioClipTypes(AGS.Types.IGame game)
        {
            return ((AGS.Types.Game)game).AudioClipTypes;
        }

        public static AGS.Types.AudioClipFolder GetRootAudioClipFolder(AGS.Types.IGame game)
        {
            return ((AGS.Types.Game)game).RootAudioClipFolder;
        }

        public static IWin32Window GetMainWindow()
        {
            return Control.FromHandle(Process.GetCurrentProcess().MainWindowHandle);
        }

        public static Color GetTransparencyColor()
        {
            return Color.FromArgb(255, 0, 255);
        }

        public static string GetScriptText(AGS.Types.Script script)
        {
            if (script.FileName == "GlobalScript.asc" && !Regex.IsMatch(script.Text, @"function\s+dialog_request\s*\("))
            {
                return script.Text + Environment.NewLine + "function dialog_request(int param) {" + Environment.NewLine + "}";
            }
            return script.Text;
        }

        public static bool AreDialogScriptsCached(AGS.Types.IAGSEditor editor)
        {
            foreach (AGS.Types.Dialog dialog in editor.CurrentGame.Dialogs)
            {
                if (String.IsNullOrEmpty(dialog.CachedConvertedScript))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
