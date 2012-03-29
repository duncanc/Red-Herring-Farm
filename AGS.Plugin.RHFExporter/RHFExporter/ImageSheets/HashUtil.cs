using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.IO;

namespace RedHerringFarm.ImageSheets
{
    public static class HashUtil
    {
        private static readonly char[] lookup = "0123456789abcdef".ToCharArray();
        public static string ToHex(byte[] data)
        {
            int len = data.Length;
            char[] output = new char[len * 2];
            for (int i = 0; i < len; i++)
            {
                output[i*2] = lookup[data[i] >> 4];
                output[i*2 + 1] = lookup[data[i] & 0x0F];
            }
            return new string(output);
        }
        public static IEnumerable<byte[]> YieldBitmapData(Bitmap bitmap)
        {
            int bitsPerPixel = Image.GetPixelFormatSize(bitmap.PixelFormat);
            int bitsPerRow = bitsPerPixel * bitmap.Width;
            int bytesPerRow = bitsPerRow / 8;
            int remainderBits = bitsPerRow % 8;
            byte finalByteMask;
            if (remainderBits != 0)
            {
                bytesPerRow++;
                finalByteMask = 1 << 7;
                for (int i = 1; i < remainderBits; i++)
                {
                    finalByteMask |= (byte)(finalByteMask >> 1);
                }
            }
            else
            {
                finalByteMask = 0xff;
            }
            byte[] row = new byte[bytesPerRow];
            for (int y = 0; y < bitmap.Height; y++)
            {
                BitmapData bdata = bitmap.LockBits(
                    new Rectangle(0, y, bitmap.Width, 1),
                    ImageLockMode.ReadOnly,
                    bitmap.PixelFormat);

                Marshal.Copy(bdata.Scan0, row, 0, row.Length);
                bitmap.UnlockBits(bdata);
                row[row.Length - 1] &= finalByteMask;

                yield return row;
            }
        }
        public static string HashBitmap(Bitmap bitmap, HashAlgorithm algorithm)
        {
            DelegatedStream customStream = new DelegatedStream();
            customStream.read = DelegatedStream.CreateByteYielder(YieldBitmapData(bitmap));
            return ToHex(algorithm.ComputeHash(customStream));
        }
        public static string MD5Bitmap(Bitmap bmp)
        {
            return HashBitmap(bmp, new MD5CryptoServiceProvider());
        }
        public static string HashFile(string path, HashAlgorithm algorithm)
        {
            return ToHex(algorithm.ComputeHash(File.Open(path, FileMode.Open, FileAccess.Read)));
        }
        public static string MD5File(string path)
        {
            return HashFile(path, new MD5CryptoServiceProvider());
        }
        public static string HashBytes(byte[] bytes, HashAlgorithm algorithm)
        {
            return ToHex(algorithm.ComputeHash(bytes));
        }
        public static string MD5Bytes(byte[] bytes)
        {
            return HashBytes(bytes, new MD5CryptoServiceProvider());
        }
        public static string HashText(string text, HashAlgorithm algorithm)
        {
            return ToHex(algorithm.ComputeHash(Encoding.UTF8.GetBytes(text)));
        }
    }
}
