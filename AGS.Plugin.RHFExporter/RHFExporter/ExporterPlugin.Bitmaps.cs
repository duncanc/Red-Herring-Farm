using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Text.RegularExpressions;
using System.IO;
using RedHerringFarm.TaskManaging;

namespace RedHerringFarm
{
    public partial class ExporterPlugin
    {
        private List<string> SaveBitmapPaths = new List<string>();

        private void PrepareBitmapExport()
        {
            SaveBitmapPaths.Clear();
        }

        private void ExportBitmap(Bitmap bitmap, string path)
        {
            SaveBitmapPaths.Add(path);
            bitmap.Save(path);
        }

        private void PostProcessBitmaps()
        {
            foreach (string path in SaveBitmapPaths)
            {
                if (!path.ToLower().EndsWith(".png") || String.IsNullOrEmpty(Settings.PngTool) || !File.Exists(Settings.PngTool))
                {
                    continue;
                }
                TaskManager.StatusUpdate("Postprocessing " + path + "...");
                ProcessStartInfo psi = new ProcessStartInfo(Settings.PngTool, "\"" + path + "\"");
                psi.CreateNoWindow = true;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.UseShellExecute = false;
                Process process = new Process();
                process.StartInfo = psi;
                process.Start();
                process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                TaskManager.StatusUpdate("Exit code: " + process.ExitCode);
            }
        }
    }
}
