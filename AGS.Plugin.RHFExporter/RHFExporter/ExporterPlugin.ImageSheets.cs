using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using RedHerringFarm.ImageSheets;
using RedHerringFarm.TaskManaging;

namespace RedHerringFarm
{
    partial class ExporterPlugin
    {
        static string TASK_PREPARE_SPRITE_IMAGE_SHEETS = "Preparing sprite image sheets...";
        static string TASK_PREPARE_FONT_IMAGE_SHEETS = "Preparing font image sheets...";
        private List<ImageSheet> GameImageSheets;
        private void PrepareGameImageSheets()
        {
            GameImageSheets = new List<ImageSheet>();

            TaskManager.Expect(TASK_PREPARE_SPRITE_IMAGE_SHEETS);
            TaskManager.Expect(TASK_PREPARE_FONT_IMAGE_SHEETS);

            using (TaskManager.Start(TASK_PREPARE_SPRITE_IMAGE_SHEETS))
            {
                PrepareSpriteImageSheets();
            }
            using (TaskManager.Start(TASK_PREPARE_FONT_IMAGE_SHEETS))
            {
                PrepareFontImageSheets();
            }
        }
        private void ExportGameImageSheets()
        {
            for (int i = 0; i < GameImageSheets.Count; i++)
            {
                Bitmap bmp = GameImageSheets[i].GetBitmap();
                ExportBitmap(bmp, InExportFolder(IMAGE_SHEET_FILENAME, i));
            }
        }
        private void WriteImageSheetJson(JsonWriter output, ImageSheet sheet)
        {
            WriteImageSheetJson(output, null, sheet);
        }
        private void WriteImageSheetJson(JsonWriter output, string key, ImageSheet sheet)
        {
            using (output.BeginObject(key))
            {
                using (output.BeginArray("entries"))
                {
                    foreach (ImageSheetEntry entry in sheet)
                    {
                        entry.WriteJson(output);
                    }
                }
            }
        }
    }
}
