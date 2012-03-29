using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using FT = FreeType.FTInterface;

namespace FreeType
{
    public class FreeTypeCharMap
    {
        internal FreeTypeCharMap(IntPtr charmap)
        {
            this.charmap = charmap;
        }
        IntPtr charmap;
        FT.CharMapRec charmap_rec;
        private void updateRec()
        {
            charmap_rec = (FT.CharMapRec)Marshal.PtrToStructure(charmap, typeof(FT.CharMapRec));
        }
        public FT_Encoding Encoding
        {
            get
            {
                updateRec();
                return charmap_rec.encoding;
            }
        }
        public ushort PlatformID
        {
            get
            {
                updateRec();
                return charmap_rec.platform_id;
            }
        }
        public ushort EncodingID
        {
            get
            {
                updateRec();
                return charmap_rec.encoding_id;
            }
        }
    }
}
