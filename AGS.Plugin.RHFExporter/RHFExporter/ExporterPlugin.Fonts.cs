using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using FreeType;
using RedHerringFarm.ImageSheets;

namespace RedHerringFarm
{
    public partial class ExporterPlugin
    {
        List<AgsFont> fonts;
        private void PrepareFontImageSheets()
        {
            ImageSheet outlineMonoCharSheet = new ImageSheet(settings.MaxImageSheetWidth, settings.MaxImageSheetHeight, 2, 1);
            ImageSheet normalMonoCharSheet = new ImageSheet(settings.MaxImageSheetWidth, settings.MaxImageSheetHeight, 0, 0);

            outlineMonoCharSheet.ClearColor = Color.Black;
            outlineMonoCharSheet.MakeTransparent = true;
            normalMonoCharSheet.ClearColor = Color.Black;
            normalMonoCharSheet.MakeTransparent = true;

            fonts = GetFonts();

            foreach (AgsFont font in fonts)
            {
                foreach (AgsFontChar c in font.Chars)
                {
                    if (c == null || c.Width == 0 || c.Height == 0)
                    {
                        continue;
                    }
                    if (font.font.OutlineStyle == AGS.Types.FontOutlineStyle.Automatic)
                    {
                        outlineMonoCharSheet.AddEntry(c);
                    }
                    else
                    {
                        normalMonoCharSheet.AddEntry(c);
                    }
                }
            }

            if (!normalMonoCharSheet.IsEmpty)
            {
                if (!normalMonoCharSheet.Pack())
                {
                    throw new Exception("Cannot pack normal fonts!");
                }
                GameImageSheets.Add(normalMonoCharSheet);
            }

            if (!outlineMonoCharSheet.IsEmpty)
            {
                if (!outlineMonoCharSheet.Pack())
                {
                    throw new Exception("Cannot pack outline fonts!");
                }
                GameImageSheets.Add(outlineMonoCharSheet);
            }

        }

        private void WriteFontsJson(JsonWriter output, string key)
        {
            using (output.BeginArray(key))
            {
                foreach (AgsFont font in fonts)
                {
                    WriteFontJson(output, font);
                }
            }
        }

        private void WriteFontJson(JsonWriter output, AgsFont font)
        {
            WriteFontJson(output, null, font);
        }
        private void WriteFontJson(JsonWriter output, string key, AgsFont font)
        {
            if (font == null)
            {
                output.WriteNull(key);
                return;
            }
            using (output.BeginObject(key))
            {
                using (output.BeginArray("chars"))
                {
                    foreach (AgsFontChar c in font.Chars)
                    {
                        WriteFontCharJson(output, c);
                    }
                }
            }
        }

        private void WriteFontCharJson(JsonWriter output, AgsFontChar c)
        {
            WriteFontCharJson(output, null, c);
        }
        private void WriteFontCharJson(JsonWriter output, string key, AgsFontChar c)
        {
            if (c == null || c.Width == 0 || c.Height == 0)
            {
                output.WriteNull(key);
                return;
            }
            using (output.BeginObject(key))
            {
                if (c.Width > 0 && c.Height > 0)
                {
                    output.WriteValue("s", GameImageSheets.IndexOf(c.OwningSheet));
                    output.WriteValue("n", c.EntryNumber);
                    if (c.Advance != c.Width)
                    {
                        output.WriteValue("a", c.Advance);
                    }
                }
            }
        }

        private List<AgsFont> GetFonts()
        {
            List<AgsFont> fonts = new List<AgsFont>();
            foreach (AGS.Types.Font font in editor.CurrentGame.Fonts)
            {
                if (font == null)
                {
                    fonts.Add(null);
                    continue;
                }
                string wfnPath = Path.Combine(editor.CurrentGame.DirectoryPath, font.WFNFileName);
                string ttfPath = Path.Combine(editor.CurrentGame.DirectoryPath, font.TTFFileName);
                if (!String.IsNullOrEmpty(font.WFNFileName) && File.Exists(wfnPath))
                {
                    fonts.Add(new AgsFont.Wfn(font, wfnPath));
                }
                else if (!String.IsNullOrEmpty(font.TTFFileName) && File.Exists(ttfPath))
                {
                    fonts.Add(new AgsFont.Ttf(font, ttfPath));
                }
                else
                {
                    fonts.Add(null);
                }
            }
            return fonts;
        }
    }

    internal class AgsFontChar : ImageSheetEntry
    {
        public int Advance;
        internal AgsFontChar(Bitmap bmp, int advance, int xOffset, int yOffset)
        {
            this.bmp = bmp;
            this.Advance = advance;
            this.paddingLeft = xOffset;
            this.paddingTop = yOffset;
            if (bmp == null)
            {
                return;
            }

            int removeBottomRows, removeTopRows, removeLeftRows, removeRightRows;
            removeBottomRows = removeTopRows = removeLeftRows = removeRightRows = 0;
            for (int y = bmp.Height - 1; y >= 0; y--)
            {
                bool rowUsed = false;
                for (int x = 0; x < bmp.Width; x++)
                {
                    if (bmp.GetPixel(x, y).R >= 128)
                    {
                        rowUsed = true;
                        break;
                    }
                }
                if (rowUsed) break;
                removeBottomRows++;
            }
            if (removeLeftRows == bmp.Width)
            {
                this.bmp = null;
                paddingLeft = xOffset;
                paddingRight = bmp.Width;
                paddingTop = yOffset;
                paddingBottom = bmp.Height;
                return;
            }
            for (int y = 0; y < bmp.Height; y++)
            {
                bool rowUsed = false;
                for (int x = 0; x < bmp.Width; x++)
                {
                    if (bmp.GetPixel(x, y).R >= 128)
                    {
                        rowUsed = true;
                        break;
                    }
                }
                if (rowUsed) break;
                removeTopRows++;
            }
            for (int x = 0; x < bmp.Width; x++)
            {
                bool colUsed = false;
                for (int y = 0; y < bmp.Height; y++)
                {
                    if (bmp.GetPixel(x, y).R >= 128)
                    {
                        colUsed = true;
                        break;
                    }
                }
                if (colUsed) break;
                removeLeftRows++;
            }
            for (int x = bmp.Width - 1; x >= 0; x--)
            {
                bool colUsed = false;
                for (int y = 0; y < bmp.Height; y++)
                {
                    if (bmp.GetPixel(x, y).R >= 128)
                    {
                        colUsed = true;
                        break;
                    }
                }
                if (colUsed) break;
                removeRightRows++;
            }
            paddingLeft += removeLeftRows;
            paddingTop += removeTopRows;
            if (removeLeftRows == bmp.Width || removeTopRows == bmp.Height)
            {
                this.bmp = null;
            }
            else if ((removeLeftRows + removeRightRows + removeTopRows + removeBottomRows) > 0)
            {
                this.bmp = BitmapUtil.WindowBitmap(
                    bmp,
                    removeLeftRows,
                    removeTopRows,
                    bmp.Width - removeLeftRows - removeRightRows,
                    bmp.Height - removeTopRows - removeBottomRows);
            }
        }
        Bitmap bmp;
        public override int Width
        {
            get { return (bmp == null) ? 0 : bmp.Width; }
        }
        public override int Height
        {
            get { return (bmp == null) ? 0 : bmp.Height; }
        }
        public override void Draw(Graphics g)
        {
            if (bmp != null) g.DrawImage(bmp, X, Y);
        }
        public override void Draw(BitmapData bdata)
        {
            if (bmp == null)
            {
                return;
            }
            if (bdata.PixelFormat != bmp.PixelFormat)
            {
                throw new Exception("BitmapData is " + bdata.PixelFormat
                    + ", must be " + bmp.PixelFormat);
            }
            if (bdata.Width != Width || bdata.Height != Height)
            {
                throw new Exception("BitmapData is " + bdata.Width + "x" + bdata.Height
                    + ", must be " + Width + "x" + Height);
            }
            BitmapData locked = bmp.LockBits(
                new Rectangle(0, 0, Width, Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format1bppIndexed);
            byte[] data = new byte[locked.Stride * locked.Height];
            Marshal.Copy(locked.Scan0, data, 0, data.Length);
            byte[] outData;
            if (locked.Stride == bdata.Stride)
            {
                outData = data;
            }
            else
            {
                outData = new byte[bdata.Stride * bdata.Height];
                for (int y = 0; y < bdata.Height; y++)
                {
                    Buffer.BlockCopy(
                        data,
                        y * locked.Stride,
                        outData,
                        y * bdata.Stride,
                        Math.Min(bdata.Stride, locked.Stride));
                }
            }
            bmp.UnlockBits(locked);
            Marshal.Copy(outData, 0, bdata.Scan0, outData.Length);
        }
        public override string UniqueKey
        {
            get
            {
                if (bmp == null) return null;
                return bmp.Width + "," + bmp.Height + "," + HashUtil.MD5Bitmap(bmp);
            }
        }
    }
    internal abstract class AgsFont
    {
        internal AgsFont(AGS.Types.Font font)
        {
            this.font = font;
        }
        public readonly AGS.Types.Font font;
        public abstract IEnumerable<AgsFontChar> Chars { get; }
        internal class Wfn : AgsFont
        {
            WfnFont wfn;
            internal Wfn(AGS.Types.Font font, string path)
                : base(font)
            {
                wfn = new WfnFont(path);
                foreach (WfnChar wfnChar in wfn.Characters)
                {
                    Bitmap bmp = wfnChar.GetBitmap();
                    chars.Add(new AgsFontChar(bmp, bmp == null ? 0 : bmp.Width, 0, 0));
                }
            }
            private List<AgsFontChar> chars = new List<AgsFontChar>();
            public override IEnumerable<AgsFontChar> Chars
            {
                get { return chars; }
            }
        }
        internal class Ttf : AgsFont
        {
            static FreeTypeLibrary FreeType = new FreeTypeLibrary();
            FreeTypeFace face;
            internal Ttf(AGS.Types.Font font, string path)
                : base(font)
            {
                face = FreeType.NewFace(path, 0);
                face.SetCharSize(font.PointSize * 64, font.PointSize * 64, 72, 69);
                for (int i = 0; i < face.NumGlyphs; i++)
                {
                    using (FreeTypeGlyphSlot slot = face.LockGlyph((uint)i, FT_LoadFlags.LoadMonochrome))
                    using (FreeTypeGlyph glyph = slot.GetGlyph())
                    {
                        glyph.ToBitmap(FT_RenderMode.Mono, true);
                        Bitmap bmp = glyph.GetBitmap();
                        chars.Add(new AgsFontChar(
                            bmp,
                            (int)Math.Ceiling(slot.Metrics_HorizontalLayout_Advance / 64.0),
                            (int)Math.Ceiling(slot.Metrics_HorizontalLayout_Left / 64.0),
                            (int)Math.Ceiling(slot.Metrics_HorizontalLayout_Top / 64.0)));
                    }
                }
            }
            private List<AgsFontChar> chars = new List<AgsFontChar>();
            public override IEnumerable<AgsFontChar> Chars
            {
                get
                {
                    return chars;
                }
            }
        }
    }
}
