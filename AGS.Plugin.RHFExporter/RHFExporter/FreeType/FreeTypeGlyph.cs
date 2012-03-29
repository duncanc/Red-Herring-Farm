using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using FT = FreeType.FTInterface;

namespace FreeType
{
    public class FreeTypeGlyph : IDisposable
    {
        internal FreeTypeGlyph(IntPtr glyph)
        {
            this.glyph = glyph;
        }

        protected IntPtr glyph;
        private FT.GlyphRec glyph_rec;

        public void Dispose()
        {
            FT.Done_Glyph(glyph);
            glyph = IntPtr.Zero;
        }

        ~FreeTypeGlyph()
        {
            if (glyph != IntPtr.Zero)
            {
                FT.Done_Glyph(glyph);
            }
        }

        private void updateRec()
        {
            glyph_rec = (FT.GlyphRec)Marshal.PtrToStructure(glyph, typeof(FT.GlyphRec));
        }

        public int AdvanceX
        {
            get
            {
                updateRec();
                return glyph_rec.advance_x;
            }
        }

        public int AdvanceY
        {
            get
            {
                updateRec();
                return glyph_rec.advance_y;
            }
        }
        public FT_GlyphFormat Format
        {
            get
            {
                updateRec();
                return glyph_rec.format;
            }
        }

        private FT.BitmapGlyphRec bitmap_glyph_rec;

        private void updateBitmapRec()
        {
            bitmap_glyph_rec = (FT.BitmapGlyphRec)Marshal.PtrToStructure(glyph, typeof(FT.BitmapGlyphRec));
        }

        public Bitmap GetBitmap()
        {
            updateBitmapRec();
            if (bitmap_glyph_rec.bitmap_width == 0 || bitmap_glyph_rec.bitmap_rows == 0)
            {
                return null;
            }
            byte[] data = new byte[bitmap_glyph_rec.bitmap_pitch * bitmap_glyph_rec.bitmap_rows];
            Marshal.Copy(bitmap_glyph_rec.bitmap_buffer, data, 0, data.Length);
            PixelFormat pixelFormat;
            switch (bitmap_glyph_rec.bitmap_pixel_mode)
            {
                case FT_PixelMode.Mono:
                    pixelFormat = PixelFormat.Format1bppIndexed;
                    break;
                case FT_PixelMode.Gray:
                    pixelFormat = PixelFormat.Format8bppIndexed;
                    break;
                default:
                    throw new Exception("unsupported pixel mode: " + bitmap_glyph_rec.bitmap_pixel_mode);
            }
            Bitmap bitmap = new Bitmap(
                bitmap_glyph_rec.bitmap_width,
                bitmap_glyph_rec.bitmap_rows,
                pixelFormat);
            ColorPalette pal = bitmap.Palette;
            for (int i = 0; i < bitmap.Palette.Entries.Length; i++)
            {
                int j = (int)(255.0 * ((double)i / (double)(bitmap.Palette.Entries.Length - 1)));
                pal.Entries[i] = Color.FromArgb(j, j, j);
            }
            bitmap.Palette = pal;
            BitmapData locked = bitmap.LockBits(
                new Rectangle(0, 0, bitmap_glyph_rec.bitmap_width, bitmap_glyph_rec.bitmap_rows),
                ImageLockMode.WriteOnly,
                pixelFormat);
            if (locked.Stride != bitmap_glyph_rec.bitmap_pitch)
            {
                byte[] newData = new byte[locked.Stride * bitmap_glyph_rec.bitmap_rows];
                for (int i = 0; i < bitmap_glyph_rec.bitmap_rows; i++)
                {
                    Buffer.BlockCopy(data, i * bitmap_glyph_rec.bitmap_pitch, newData, i * locked.Stride, Math.Min(bitmap_glyph_rec.bitmap_pitch, locked.Stride));
                }
                data = newData;
            }
            Marshal.Copy(
                data,
                0,
                locked.Scan0,
                data.Length);
            bitmap.UnlockBits(locked);
            return bitmap;
        }

        public void ToBitmap(FT_RenderMode renderMode, bool destroy)
        {
            if (Format != FT_GlyphFormat.Bitmap)
            {
                FT.assert(FT.Glyph_To_Bitmap(ref glyph, renderMode, IntPtr.Zero, destroy));
            }
        }
        public override int GetHashCode()
        {
            return glyph.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return (obj is FreeTypeGlyph) && (((FreeTypeGlyph)obj).glyph == this.glyph);
        }
        public static bool operator ==(FreeTypeGlyph a, FreeTypeGlyph b)
        {
            return a.glyph == b.glyph;
        }
        public static bool operator !=(FreeTypeGlyph a, FreeTypeGlyph b)
        {
            return a.glyph != b.glyph;
        }
    }
}
