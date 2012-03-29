using System;
using System.Runtime.InteropServices;
using FT = FreeType.FTInterface;

namespace FreeType
{
    public class FreeTypeLibrary : IDisposable
    {
        public FreeTypeLibrary()
        {
            FT.assert(FT.Init_FreeType(out lib));
        }

        private IntPtr lib = IntPtr.Zero;

        public void Dispose()
        {
            FT.assert(FT.Done_FreeType(lib));
            lib = IntPtr.Zero;
        }

        ~FreeTypeLibrary()
        {
            if (lib != IntPtr.Zero)
            {
                FT.Done_FreeType(lib);
            }
        }

        public FreeTypeFace NewFace(byte[] data, int faceIndex)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                FT.OpenArgs args = new FT.OpenArgs();
                args.flags = FT_OpenFlags.Memory;
                args.memory_base = handle.AddrOfPinnedObject();
                args.memory_size = data.Length;
                args.pathname = null;
                args.stream = IntPtr.Zero;
                args.num_params = 0;
                args.params_ = IntPtr.Zero;

                IntPtr face;
                FT.assert(FT.Open_Face(lib, args, faceIndex, out face));
                return new FreeTypeFace(face);
            }
            finally
            {
                handle.Free();
            }
        }

        public FreeTypeFace NewFace(string filename, int faceIndex)
        {
            IntPtr face;
            FT.assert(FT.New_Face(lib, filename, faceIndex, out face));
            return new FreeTypeFace(face);
        }
    }
}
