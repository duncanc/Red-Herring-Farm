using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace RedHerringFarm
{
    public class WfnChar
    {
        public readonly int Width;
        public readonly int Height;
        public readonly int Stride;
        public readonly byte[] Data;
        internal WfnChar(BinaryReader br)
        {
            Width = br.ReadUInt16();
            Height = br.ReadUInt16();
            Stride = (int)Math.Ceiling(Width / 8.0);
            Data = br.ReadBytes(Stride * Height);
        }
        public void SetBitmapData(BitmapData bdata)
        {
            if (bdata.PixelFormat != PixelFormat.Format1bppIndexed)
            {
                throw new Exception(
                    "BitmapData is " + bdata.PixelFormat
                    + " - must be " + PixelFormat.Format1bppIndexed);
            }
            if (bdata.Width != Width || bdata.Height != Height)
            {
                throw new Exception(
                    "BitmapData is " + bdata.Width + "x" + bdata.Height
                    + " - must be " + Width + "x" + Height);
            }
            byte[] data;
            if (bdata.Stride == Stride)
            {
                data = Data;
            }
            else
            {
                data = new byte[Height * bdata.Stride];
                for (int i = 0; i < Height; i++)
                {
                    Buffer.BlockCopy(Data, Stride * i, data, bdata.Stride * i, Stride);
                }
            }
            Marshal.Copy(data, 0, bdata.Scan0, data.Length);
        }
        public Bitmap GetBitmap()
        {
            Bitmap bitmap = new Bitmap(Width, Height, PixelFormat.Format1bppIndexed);
            BitmapData bdata = bitmap.LockBits(
                new Rectangle(0,0,Width,Height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format1bppIndexed);
            SetBitmapData(bdata);
            bitmap.UnlockBits(bdata);
            return bitmap;
        }
    }
    public class WfnFont
    {
        public WfnFont(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            using (FileStream stream = File.OpenRead(path))
            {
                ReadStream(stream);
            }
        }
        public WfnFont(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            ReadStream(stream);
        }
        private const string HEADER = "WGT Font File  ";
        public WfnChar[] Characters { get { return _chars; } }
        private WfnChar[] _chars;
        public int MaxWidth = 0;
        public int MaxHeight = 0;
        private void ReadStream(Stream input)
        {
            using (BinaryReader br = new BinaryReader(input, Encoding.ASCII))
            {
                if (new String(br.ReadChars(HEADER.Length)) != HEADER)
                {
                    throw new Exception("unrecognised font file format!");
                }
                ushort addressesAddress = br.ReadUInt16();
                if (addressesAddress > br.BaseStream.Length)
                {
                    throw new Exception("corrupt or truncated font data");
                }
                br.BaseStream.Seek(addressesAddress, SeekOrigin.Begin);
                long numberOfCharacters = (br.BaseStream.Length - br.BaseStream.Position) / 2;
                ushort[] offsets = new ushort[numberOfCharacters];
                for (ushort i = 0; i < numberOfCharacters; i++)
                {
                    offsets[i] = br.ReadUInt16();
                }
                _chars = new WfnChar[numberOfCharacters];
                for (ushort i = 0; i < numberOfCharacters; i++)
                {
                    br.BaseStream.Seek(offsets[i], SeekOrigin.Begin);
                    WfnChar c = new WfnChar(br);
                    _chars[i] = c;
                    MaxWidth = Math.Max(MaxWidth, c.Width);
                    MaxHeight = Math.Max(MaxHeight, c.Height);
                }
            }
        }
    }
}
