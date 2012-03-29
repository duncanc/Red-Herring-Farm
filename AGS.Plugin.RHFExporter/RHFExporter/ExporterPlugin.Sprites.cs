using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using RedHerringFarm.ImageSheets;

namespace RedHerringFarm
{
	public partial class ExporterPlugin
	{
        List<SpriteImageSheetEntry> spriteImageSheetEntries;
        private void PrepareSpriteImageSheets()
        {
            spriteImageSheetEntries = new List<SpriteImageSheetEntry>();

            List<ImageSheet> spriteImageSheets = new List<ImageSheet>();
            ExportSpriteFolder(editor.CurrentGame.Sprites, null, spriteImageSheets, false, true);
            ExportSpriteFolder(editor.CurrentGame.Sprites, null, spriteImageSheets, true, true);

            foreach (ImageSheet sheet in spriteImageSheets)
            {
                sheet.Pack();
                GameImageSheets.Add(sheet);
            }
        }
        private void ExportSpriteFolder(
            AGS.Types.ISpriteFolder folder,
            ImageSheet toMaskSheet,
            List<ImageSheet> completeImageSheets,
            bool alpha,
            bool topLevel)
        {
            if (toMaskSheet == null)
            {
                toMaskSheet = new ImageSheet(settings.MaxImageSheetWidth, settings.MaxImageSheetHeight, 0, 0);
                if (!alpha)
                {
                    toMaskSheet.ClearColor = HacksAndKludges.GetTransparencyColor();
                    toMaskSheet.MakeTransparent = true;
                }
                foreach (AGS.Types.Sprite sprite in folder.Sprites)
                {
                    if ((alpha && !sprite.AlphaChannel) || (!alpha && sprite.AlphaChannel))
                    {
                        continue;
                    }
                    if (sprite.Width > settings.MaxImageSheetWidth || sprite.Height > settings.MaxImageSheetHeight)
                    {
                        throw new Exception("Sprite #" + sprite.Number + " is bigger than the maximum image sheet size");
                    }
                    SpriteImageSheetEntry entry;
                    if (alpha)
                    {
                        entry = new SpriteImageSheetEntry(editor, sprite, Color.Transparent);
                    }
                    else
                    {
                        entry = new SpriteImageSheetEntry(editor, sprite, HacksAndKludges.GetTransparencyColor());
                    }
                    if (!toMaskSheet.AddEntry(entry))
                    {
                        if (!toMaskSheet.IsEmpty)
                        {
                            completeImageSheets.Add(toMaskSheet);
                        }
                        toMaskSheet = new ImageSheet(settings.MaxImageSheetWidth, settings.MaxImageSheetHeight, 0, 0);
                    }
                }
            }
            else
            {
                object maskSnapshot = toMaskSheet.Snapshot();

                foreach (AGS.Types.Sprite sprite in folder.Sprites)
                {
                    if ((alpha && !sprite.AlphaChannel) || (!alpha && sprite.AlphaChannel))
                    {
                        continue;
                    }
                    if (sprite.Width > settings.MaxImageSheetWidth || sprite.Height > settings.MaxImageSheetHeight)
                    {
                        throw new Exception("Sprite #" + sprite.Number + " is bigger than the maximum image sheet size");
                    }
                    SpriteImageSheetEntry entry;
                    if (alpha)
                    {
                        entry = new SpriteImageSheetEntry(editor, sprite, Color.Transparent);
                    }
                    else
                    {
                        entry = new SpriteImageSheetEntry(editor, sprite, HacksAndKludges.GetTransparencyColor());
                    }
                    if (!toMaskSheet.AddEntry(entry))
                    {
                        toMaskSheet.RestoreSnapshot(maskSnapshot);
                        ExportSpriteFolder(folder, null, completeImageSheets, alpha, true);
                        return;
                    }
                }
            }
            int insert = completeImageSheets.Count;
            foreach (AGS.Types.SpriteFolder subfolder in folder.SubFolders)
            {
                ExportSpriteFolder(subfolder, toMaskSheet, completeImageSheets, alpha, false);
            }
            if (topLevel && !toMaskSheet.IsEmpty)
            {
                completeImageSheets.Insert(insert, toMaskSheet);
            }
        }
        private void WriteSpritesJson(JsonWriter output)
        {
            WriteSpritesJson(output, null);
        }
        private void WriteSpritesJson(JsonWriter output, string key)
        {
            using (output.BeginArray(key))
            {
                foreach (SpriteImageSheetEntry entry in spriteImageSheetEntries)
                {
                    if (entry == null || entry.Width == 0 || entry.Height == 0)
                    {
                        output.WriteNull();
                    }
                    else
                    {
                        using (output.BeginObject())
                        {
                            output.WriteValue("s", GameImageSheets.IndexOf(entry.OwningSheet));
                            output.WriteValue("n", entry.EntryNumber);
                        }
                    }
                }
            }
        }

        private List<AGS.Types.Sprite> GetAllSprites()
        {
            List<AGS.Types.Sprite> allSprites = new List<AGS.Types.Sprite>();
            AddSpritesFromFolder(editor.CurrentGame.Sprites, allSprites);
            return allSprites;
        }
        private void AddSpritesFromFolder(AGS.Types.ISpriteFolder folder, List<AGS.Types.Sprite> list)
        {
            foreach (AGS.Types.Sprite sprite in folder.Sprites)
            {
                while (list.Count <= sprite.Number)
                {
                    list.Add(null);
                }
                list[sprite.Number] = sprite;
            }
            foreach (AGS.Types.ISpriteFolder subfolder in folder.SubFolders)
            {
                AddSpritesFromFolder(subfolder, list);
            }
        }
	}
    public class SpriteImageSheetEntry : ImageSheetEntry
    {
        public SpriteImageSheetEntry(AGS.Types.IAGSEditor editor, AGS.Types.Sprite sprite, Color bgColor)
        {
            TheSprite = sprite;
            bitmap = editor.GetSpriteImage(TheSprite.Number);
            Trim(bgColor);
        }
        private void Trim(Color backgroundColor)
        {
            if (bitmap == null)
            {
                return;
            }
            paddingLeft = paddingRight = paddingTop = paddingBottom = 0;
            for (int x = 0; x < bitmap.Width; x++)
            {
                bool colUsed = false;
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Color px = bitmap.GetPixel(x, y);
                    if (backgroundColor.A == 0)
                    {
                        if (px.A != 0)
                        {
                            colUsed = true;
                            break;
                        }
                    }
                    else if (backgroundColor.R != px.R || backgroundColor.G != px.G || backgroundColor.B != px.B)
                    {
                        colUsed = true;
                        break;
                    }
                }
                if (colUsed)
                {
                    break;
                }
                else
                {
                    paddingLeft++;
                }
            }
            if (paddingLeft == bitmap.Width)
            {
                paddingRight = paddingLeft;
                paddingLeft = 0;
                paddingTop = 0;
                paddingBottom = bitmap.Height;
                bitmap = null;
                return;
            }
            for (int x = bitmap.Width-1; x >= 0; x--)
            {
                bool colUsed = false;
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Color px = bitmap.GetPixel(x, y);
                    if (backgroundColor.A == 0)
                    {
                        if (px.A != 0)
                        {
                            colUsed = true;
                            break;
                        }
                    }
                    else if (backgroundColor.R != px.R || backgroundColor.G != px.G || backgroundColor.B != px.B)
                    {
                        colUsed = true;
                        break;
                    }
                }
                if (colUsed)
                {
                    break;
                }
                else
                {
                    paddingRight++;
                }
            }
            for (int y = 0; y < bitmap.Height; y++)
            {
                bool rowUsed = false;
                for (int x = paddingLeft; x < bitmap.Width - paddingRight; x++)
                {
                    Color px = bitmap.GetPixel(x, y);
                    if (backgroundColor.A == 0)
                    {
                        if (px.A != 0)
                        {
                            rowUsed = true;
                            break;
                        }
                    }
                    else if (backgroundColor.R != px.R || backgroundColor.G != px.G || backgroundColor.B != px.B)
                    {
                        rowUsed = true;
                        break;
                    }
                }
                if (rowUsed)
                {
                    break;
                }
                else
                {
                    paddingTop++;
                }
            }
            for (int y = bitmap.Height - 1; y >= 0; y--)
            {
                bool rowUsed = false;
                for (int x = paddingLeft; x < bitmap.Width - paddingRight; x++)
                {
                    Color px = bitmap.GetPixel(x, y);
                    if (backgroundColor.A == 0)
                    {
                        if (px.A != 0)
                        {
                            rowUsed = true;
                            break;
                        }
                    }
                    else if (backgroundColor != px)
                    {
                        rowUsed = true;
                        break;
                    }
                }
                if (rowUsed)
                {
                    break;
                }
                else
                {
                    paddingBottom++;
                }
            }
            if ((paddingLeft + paddingRight + paddingTop + paddingBottom) > 0)
            {
                bitmap = BitmapUtil.WindowBitmap(
                    bitmap,
                    paddingLeft,
                    paddingTop,
                    bitmap.Width - paddingLeft - paddingRight,
                    bitmap.Height - paddingTop - paddingBottom);
            }
        }
        private Bitmap bitmap;
        public readonly AGS.Types.Sprite TheSprite;
        public override int Width
        {
            get { return (bitmap == null) ? 0 : bitmap.Width; }
        }
        public override int Height
        {
            get { return (bitmap == null) ? 0 : bitmap.Height; }
        }
        public override void Draw(Graphics g)
        {
            if (bitmap != null)
            {
                g.DrawImage(bitmap, X, Y);
            }
        }
        public override void Draw(BitmapData bdata)
        {
            throw new NotImplementedException();
        }
    }
}
