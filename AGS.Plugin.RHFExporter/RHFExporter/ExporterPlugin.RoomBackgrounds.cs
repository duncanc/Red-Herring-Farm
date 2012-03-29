using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using RedHerringFarm.ImageSheets;

namespace RedHerringFarm
{
    partial class ExporterPlugin
    {
        public void ExportCurrentRoomBackgrounds()
        {
            Bitmap bmp = new Bitmap(
                editor.RoomController.CurrentRoom.Width,
                editor.RoomController.CurrentRoom.Height);
            for (int i = 0; i < editor.RoomController.CurrentRoom.BackgroundCount; i++)
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    editor.RoomController.DrawRoomBackground(g, 0, 0, i, 1);
                }
                Bitmap saveBmp = bmp;
                Bitmap paletted;
                if (BitmapUtil.TryMakePaletted(bmp, out paletted))
                {
                    saveBmp = paletted;
                }
                ExportBitmap(saveBmp, InExportFolder(ROOM_BACKGROUND_FILENAME, editor.RoomController.CurrentRoom.Number, i));
            }
        }
    }
}
