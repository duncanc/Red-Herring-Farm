using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using RedHerringFarm.TaskManaging;

namespace RedHerringFarm.ImageSheets
{
    public static class BitmapUtil
    {
        public static Bitmap WindowBitmap(Bitmap bmp, int windowX, int windowY, int width, int height)
        {
            Bitmap newBitmap = new Bitmap(
                width,
                height,
                bmp.PixelFormat);
            BitmapData locked = bmp.LockBits(
                new Rectangle(windowX, windowY, width, height),
                ImageLockMode.ReadOnly,
                bmp.PixelFormat);
            byte[] data = new byte[locked.Stride * locked.Height];
            Marshal.Copy(locked.Scan0, data, 0, data.Length);
            BitmapData locked2 = newBitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                newBitmap.PixelFormat);
            if (locked2.Stride != locked.Stride)
            {
                byte[] newData = new byte[locked2.Stride * locked2.Height];
                for (int y = 0; y < locked.Height; y++)
                {
                    Buffer.BlockCopy(
                        data,
                        locked.Stride * y,
                        newData,
                        locked2.Stride * y,
                        Math.Min(locked.Stride, locked2.Stride));
                }
                data = newData;
            }
            Marshal.Copy(data, 0, locked2.Scan0, data.Length);
            newBitmap.UnlockBits(locked2);
            bmp.UnlockBits(locked);
            return newBitmap;
        }
        private static bool TryMakePaletted_A8R8G8B8(
            BitmapData inbits, BitmapData outbits,
            byte[] indata, byte[] outdata,
            ColorPalette outpalette, out int colorcount)
        {
            List<int> discoveredColors = new List<int>();
            List<int> paletteIndex = new List<int>();
            List<int> paletteColors = new List<int>();
            int[] inrow = new int[inbits.Width];
            int inrowlen = inbits.Width * 4;
            for (int y = 0; y < inbits.Height; y++)
            {
                Buffer.BlockCopy(indata, y * inbits.Stride, inrow, 0, inrowlen);
                int outrowbase = outbits.Stride * y;
                for (int x = 0; x < inbits.Width; x++)
                {
                    int argb = inrow[x];
                    if ((argb & 0xff000000) != 0xff000000)
                    {
                        colorcount = 0;
                        return false;
                    }
                    int index = discoveredColors.BinarySearch(argb);
                    if (index < 0)
                    {
                        if (paletteColors.Count >= 256)
                        {
                            colorcount = 0;
                            return false;
                        }
                        index = ~index;
                        discoveredColors.Insert(index, argb);
                        paletteIndex.Insert(index, paletteColors.Count);
                        paletteColors.Add(argb);
                    }
                    outdata[outrowbase + x] = (byte)paletteIndex[index];
                }
            }
            colorcount = paletteColors.Count;
            for (int i = 0; i < colorcount; i++)
            {
                outpalette.Entries[i] = Color.FromArgb(paletteColors[i]);
            }
            return true;
        }
        public static void MakeTransparent(Bitmap bmp, Color col)
        {
            if ((bmp.PixelFormat & PixelFormat.Indexed) == PixelFormat.Indexed)
            {
                ColorPalette pal = bmp.Palette;
                for (int i = 0; i < pal.Entries.Length; i++)
                {
                    if (pal.Entries[i].R == col.R && pal.Entries[i].G == col.G && pal.Entries[i].B == col.B)
                    {
                        pal.Entries[i] = Color.FromArgb(0, col.R, col.G, col.B);
                    }
                    else
                    {
                        pal.Entries[i] = Color.FromArgb(255, pal.Entries[i].R, pal.Entries[i].G, pal.Entries[i].B);
                    }
                }
                bmp.Palette = pal;
                return;
            }
            bmp.MakeTransparent(col);
        }
        private static void ChangePaletted_8I_1I(BitmapData inbits, BitmapData outbits, byte[] indata, byte[] outdata)
        {
            int height = inbits.Height;
            for (int y = 0; y < height; y++)
            {
                int inbase = y * inbits.Stride;
                int outbase = y * outbits.Stride;
                for (int x = 0; x < inbits.Width; x++)
                {
                    if (indata[inbase + x] != 0)
                    {
                        outdata[outbase + (x/8)] |= (byte)(0x80 >> (x % 8));
                    }
                }
            }
        }
        private static void ChangePaletted_8I_4I(BitmapData inbits, BitmapData outbits, byte[] indata, byte[] outdata)
        {
            int height = inbits.Height;
            for (int y = 0; y < height; y++)
            {
                int inbase = y * inbits.Stride;
                int outbase = y * outbits.Stride;
                for (int x = 0; x < inbits.Width; x++)
                {
                    if (x % 2 == 0)
                    {
                        outdata[outbase + (x / 2)] |= (byte)(indata[inbase + x] << 4);
                    }
                    else
                    {
                        outdata[outbase + (x / 2)] |= (byte)(indata[inbase + x] & 0xf);
                    }
                }
            }
        }
        public static void ChangePaletted(Bitmap input, PixelFormat newFormat, out Bitmap output)
        {
            if ((input.PixelFormat & PixelFormat.Indexed) == 0 || (newFormat & PixelFormat.Indexed) == 0)
            {
                throw new Exception("Can only convert paletted to paletted");
            }
            output = new Bitmap(input.Width, input.Height, newFormat);
            BitmapData inbits = input.LockBits(
                new Rectangle(0, 0, input.Width, input.Height),
                ImageLockMode.ReadOnly,
                input.PixelFormat);
            BitmapData outbits = output.LockBits(
                new Rectangle(0, 0, input.Width, input.Height),
                ImageLockMode.WriteOnly,
                newFormat);

            byte[] indata = new byte[inbits.Stride * inbits.Height];
            Marshal.Copy(inbits.Scan0, indata, 0, indata.Length);
            byte[] outdata = new byte[outbits.Stride * outbits.Height];
            switch (inbits.PixelFormat)
            {
                case PixelFormat.Format8bppIndexed:
                    switch (outbits.PixelFormat)
                    {
                        case PixelFormat.Format1bppIndexed:
                            ChangePaletted_8I_1I(inbits, outbits, indata, outdata);
                            break;
                        case PixelFormat.Format4bppIndexed:
                            ChangePaletted_8I_4I(inbits, outbits, indata, outdata);
                            break;
                        default:
                            throw new Exception("Unsupported conversion: " + inbits.PixelFormat + " -> " + outbits.PixelFormat);
                    }
                    break;
                default:
                    throw new Exception("Unsupported conversion: " + inbits.PixelFormat + " -> " + outbits.PixelFormat);
            }

            Marshal.Copy(outdata, 0, outbits.Scan0, outdata.Length);
            input.UnlockBits(inbits);
            output.UnlockBits(outbits);

            ColorPalette outpalette = output.Palette;
            for (int i = 0; i < Math.Min(input.Palette.Entries.Length, output.Palette.Entries.Length); i++)
            {
                outpalette.Entries[i] = input.Palette.Entries[i];
            }
            output.Palette = outpalette;
        }
        public static bool TryMakePaletted(Bitmap input, out Bitmap output)
        {
            if ((input.PixelFormat & PixelFormat.Indexed) != 0)
            {
                output = null;
                return false;
            }
            output = new Bitmap(
                input.Width,
                input.Height,
                PixelFormat.Format8bppIndexed);
            BitmapData inbits = input.LockBits(
                new Rectangle(0, 0, input.Width, input.Height),
                ImageLockMode.ReadOnly,
                input.PixelFormat);
            BitmapData outbits = output.LockBits(
                new Rectangle(0, 0, output.Width, output.Height),
                ImageLockMode.WriteOnly,
                output.PixelFormat);

            byte[] indata = new byte[inbits.Stride * inbits.Height];
            byte[] outdata = new byte[outbits.Stride * outbits.Height];
            Marshal.Copy(inbits.Scan0, indata, 0, indata.Length);
            ColorPalette outpalette = output.Palette;
            int colorcount;
            switch (input.PixelFormat)
            {
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                    if (!TryMakePaletted_A8R8G8B8(inbits, outbits, indata, outdata, outpalette, out colorcount))
                    {
                        return false;
                    }
                    break;
                default:
                    throw new Exception("Unsupported pixel format: " + input.PixelFormat);
            }
            Marshal.Copy(outdata, 0, outbits.Scan0, outdata.Length);
            for (int i = colorcount; i < outpalette.Entries.Length; i++)
            {
                outpalette.Entries[i] = Color.FromArgb(0, 0, 0);
            }
            output.Palette = outpalette;
            input.UnlockBits(inbits);
            output.UnlockBits(outbits);
            if (colorcount <= 2)
            {
                ChangePaletted(output, PixelFormat.Format1bppIndexed, out output);
            }
            else if (colorcount <= 16)
            {
                ChangePaletted(output, PixelFormat.Format4bppIndexed, out output);
            }
            return true;
        }
    }
}
