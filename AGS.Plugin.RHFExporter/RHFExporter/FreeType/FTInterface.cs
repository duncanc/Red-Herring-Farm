using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using FT_Library = System.IntPtr;
using FT_Stream = System.IntPtr;
using FT_Face = System.IntPtr;
using FT_Module = System.IntPtr;
using FT_Glyph = System.IntPtr;
using FT_GlyphSlot = System.IntPtr;
using FT_Error = System.Int32;
using FT_Int = System.Int32;
using FT_UInt = System.UInt32;
using FT_Byte = System.Byte;
using FT_Long = System.Int32;
using FT_ULong = System.UInt32;
using FT_String = System.String;
using FT_Pos = System.Int32;
using FT_Short = System.Int16;
using FT_UShort = System.UInt16;
using FT_FWord = System.Int16;
using FT_UFWord = System.UInt16;
using FT_F2Dot14 = System.Int16;
using FT_F26Dot6 = System.Int32;
using FT_Render_Mode = System.Int32;
using FT_Bool = System.Byte;
using FT_Size = System.IntPtr;
using FT_CharMap = System.IntPtr;
using FT_Driver = System.IntPtr;
using FT_Memory = System.IntPtr;
using FT_ListRec = System.IntPtr;
using FT_Fixed = System.Int32;
using FT_SubGlyph = System.IntPtr;
using FT_Int32 = System.Int32;

namespace FreeType
{
    [Flags]
    public enum FT_FaceFlags : int
    {
        Scalable = 1,
        FixedSizes = 2,
        FixedWidth = 4,
        SFNT = 8,
        Horizontal = 16,
        Vertical = 32,
        Kerning = 64,
        FastGlyphs = 128,
        MultipleMasters = 256,
        GlyphNames = 512,
        ExternalStream = 1024,
        Hinter = 2048,
        CIDKeyed = 4096,
        Tricky = 8192
    }

    [Flags]
    public enum FT_StyleFlags : int
    {
        Italic = 1,
        Bold = 2
    }

    [Flags]
    public enum FT_OpenFlags : uint
    {
        Memory = 0x1,
        Stream = 0x2,
        PathName = 0x4,
        Driver = 0x8,
        Params = 0x10
    }

    public enum FT_PixelMode : byte
    {
        None = 0,
        Mono = 1,
        Gray = 2,
        Gray2 = 3,
        Gray4 = 4,
        LCD = 5,
        LCD_V = 6
    }

    public enum FT_RenderMode : int
    {
        Normal = 0,
        Light = 1,
        Mono = 2,
        LCD = 3,
        LCD_V = 4
    }

    [Flags]
    public enum FT_LoadFlags : int
    {
        Default = 0x0,
        NoScale = 0x1,
        NoHinting = 0x2,
        Render = 0x4,
        NoBitmap = 0x8,
        VerticalLayout = 0x10,
        ForceAutoHint = 0x20,
        CropBitmap = 0x40,
        Pedantic = 0x80,
        IgnoreGlobalAdvanceWidth = 0x200,
        NoRecurse = 0x400,
        IgnoreTransform = 0x800,
        LoadMonochrome = 0x1000,
        LinearDesign = 0x2000,
        NoAutoHint = 0x8000
    }

    public enum FT_Encoding : uint
    {
        None = 0,
        MicrosoftSymbol = ('s' << 24) | ('y' << 16) | ('m' << 8) | 'b',
        Unicode = ('u' << 24) | ('n' << 16) | ('i' << 8) | 'c',
        SJIS = ('s' << 24) | ('j' << 16) | ('i' << 8) | 's',
        GB2312 = ('g' << 24) | ('b' << 16) | (' ' << 8) | ' ',
        BIG5 = ('b' << 24) | ('i' << 16) | ('g' << 8) | '5',
        Wansung = ('w' << 24) | ('a' << 16) | ('n' << 8) | 's',
        Johab = ('j' << 24) | ('o' << 16) | ('h' << 8) | 'a',
        AdobeStandard = ('A' << 24) | ('D' << 16) | ('O' << 8) | 'B',
        AdobeExpert = ('A' << 24) | ('D' << 16) | ('D' << 8) | 'E',
        AdobeCustom = ('A' << 24) | ('D' << 16) | ('D' << 8) | 'C',
        AdobeLatin1 = ('l' << 24) | ('a' << 16) | ('t' << 8) | '1',
        OldLatin2 = ('l' << 24) | ('t' << 16) | ('t' << 8) | '2',
        AppleRoman = ('a' << 24) | ('r' << 16) | ('m' << 8) | 'n'
    }

    public enum FT_GlyphFormat : uint
    {
        None = 0,
        Composite = ('c' << 24) | ('o' << 16) | ('m' << 8) | 'p',
        Bitmap = ('b' << 24) | ('i' << 16) | ('t' << 8) | 's',
        Outline = ('o' << 24) | ('u' << 16) | ('t' << 8) | 'l',
        Plotter = ('p' << 24) | ('l' << 16) | ('o' << 8) | 't'
    }

    internal static class FTInterface
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct BitmapSize
        {
            public FT_Short  height;
            public FT_Short  width;

            public FT_Pos    size;

            public FT_Pos    x_ppem;
            public FT_Pos    y_ppem;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FaceRec
        {
            public FT_Long num_faces;
            public FT_Long face_index;

            public FT_FaceFlags face_flags;
            public FT_StyleFlags style_flags;

            public FT_Long num_glyphs;

            [MarshalAs(UnmanagedType.LPStr)]
            public FT_String family_name;
            [MarshalAs(UnmanagedType.LPStr)]
            public FT_String style_name;

            public FT_Int num_fixed_sizes;
            public IntPtr available_sizes;

            public FT_Int num_charmaps;
            public IntPtr charmaps;

            public IntPtr generic_data;
            public IntPtr generic_callback;

            public FT_Long bbox_xMin, bbox_yMin, bbox_xMax, bbox_yMax;

            public FT_UShort units_per_EM;
            public FT_Short ascender;
            public FT_Short descender;
            public FT_Short height;

            public FT_Short max_advance_width;
            public FT_Short max_advance_height;

            public FT_Short underline_position;
            public FT_Short underline_thickness;

            public FT_GlyphSlot glyph;
            public FT_Size size;
            public FT_CharMap charmap;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GlyphRec
        {
            public FT_Library library;
            private IntPtr clazz;
            public FT_GlyphFormat format;
            public FT_Pos advance_x;
            public FT_Pos advance_y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BitmapGlyphRec
        {
            public FT_Library root_library;
            private IntPtr root_clazz;
            public FT_GlyphFormat root_format;
            public FT_Pos root_advance_x;
            public FT_Pos root_advance_y;
            public FT_Int left;
            public FT_Int top;
            public int bitmap_rows;
            public int bitmap_width;
            public int bitmap_pitch;
            public IntPtr bitmap_buffer;
            public short bitmap_num_grays;
            public FT_PixelMode bitmap_pixel_mode;
            public byte bitmap_palette_mode;
            public IntPtr bitmap_palette;
            public FT_Int bitmap_left;
            public FT_Int bitmap_top;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GlyphSlotRec
        {
            public FT_Library library;
            public FT_Face face;
            public FT_GlyphSlot next;
            public FT_UInt reserved;
            public IntPtr generic_data;
            public IntPtr generic_callback;
            public FT_Pos metrics_width;
            public FT_Pos metrics_height;
            public FT_Pos metrics_horiBearingX;
            public FT_Pos metrics_horiBearingY;
            public FT_Pos metrics_horiAdvance;
            public FT_Pos metrics_vertBearingX;
            public FT_Pos metrics_vertBearingY;
            public FT_Pos metrics_vertAdvance;
            public FT_Fixed linearHoriAdvance;
            public FT_Fixed linearVertAdvance;
            public FT_Pos advance_x;
            public FT_Pos advance_y;
            public FT_GlyphFormat format;
            public int bitmap_rows;
            public int bitmap_width;
            public int bitmap_pitch;
            public IntPtr bitmap_buffer;
            public short bitmap_num_grays;
            public byte bitmap_pixel_mode;
            public byte bitmap_palette_mode;
            public IntPtr bitmap_palette;
            public FT_Int bitmap_left;
            public FT_Int bitmap_top;
            public short outline_n_contours;
            public short outline_n_points;
            public IntPtr outline_points;
            public IntPtr outline_tags;
            public IntPtr outline_contours;
            public int outline_flags;
            public FT_UInt num_subglyphs;
            public FT_SubGlyph subglyphs;
            public IntPtr control_data;
            public int control_len;
            public FT_Pos lsb_delta;
            public FT_Pos rsb_delta;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SizeRec
        {
            public FT_Face face;
            public IntPtr generic_data;
            public IntPtr generic_callback;
            public FT_UShort size_metrics_x_ppem;
            public FT_UShort size_metrics_y_ppem;
            public FT_Fixed size_metrics_x_scale;
            public FT_Fixed size_metrics_y_scale;
            public FT_Pos size_metrics_ascender;
            public FT_Pos size_metrics_descender;
            public FT_Pos size_metrics_height;
            public FT_Pos size_metrics_max_advance;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CharMapRec
        {
            public FT_Face face;
            public FT_Encoding encoding;
            public FT_UShort platform_id;
            public FT_UShort encoding_id;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct OpenArgs
        {
            public FT_OpenFlags flags;
            public IntPtr memory_base;
            public FT_Long memory_size;
            public FT_String pathname;
            public FT_Stream stream;
            public FT_Module driver;
            public FT_Int num_params;
            public IntPtr params_;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Vector
        {
            FT_Pos x;
            FT_Pos y;
        }

        [DllImport("freetype6.dll", EntryPoint="FT_Init_FreeType")]
        public static extern FT_Error Init_FreeType(out FT_Library lib);

        [DllImport("freetype6.dll", EntryPoint = "FT_Done_FreeType")]
        public static extern FT_Error Done_FreeType(FT_Library lib);

        [DllImport("freetype6.dll", EntryPoint = "FT_Open_Face")]
        public static extern FT_Error Open_Face(
            FT_Library lib,
            OpenArgs args,
            FT_Long face_index,
            out FT_Face face);

        [DllImport("freetype6.dll", EntryPoint = "FT_New_Memory_Face")]
        public static extern FT_Error New_Memory_Face(
            FT_Library lib,
            IntPtr file_base,
            FT_Long file_size,
            FT_Long face_index,
            out FT_Face aface);

        [DllImport("freetype6.dll", EntryPoint = "FT_New_Face")]
        public static extern FT_Error New_Face(
            FT_Library lib,
            FT_String filename,
            FT_Long face_index, 
            out FT_Face face);

        [DllImport("freetype6.dll", EntryPoint = "FT_Done_Face")]
        public static extern FT_Error Done_Face(FT_Face face);

        [DllImport("freetype6.dll", EntryPoint = "FT_Set_Char_Size")]
        public static extern FT_Error Set_Char_Size(
            FT_Face face,
            FT_F26Dot6 char_width,
            FT_F26Dot6 char_height,
            FT_UInt horz_resolution,
            FT_UInt vert_resolution);

        [DllImport("freetype6.dll", EntryPoint = "FT_Load_Glyph")]
        public static extern FT_Error Load_Glyph(FT_Face face, FT_UInt glyph_index, FT_Int load_flags);

        [DllImport("freetype6.dll", EntryPoint = "FT_Load_Char")]
        public static extern FT_Error Load_Char(FT_Face face, FT_ULong char_code, FT_Int load_flags);

        [DllImport("freetype6.dll", EntryPoint = "FT_Get_Glyph")]
        public static extern FT_Error Get_Glyph(FT_GlyphSlot slot, out FT_Glyph glyph);

        [DllImport("freetype6.dll", EntryPoint = "FT_Glyph_To_Bitmap")]
        public static extern FT_Error Glyph_To_Bitmap(
            ref FT_Glyph glyph,
            FT_RenderMode render_mode,
            IntPtr origin,
            [MarshalAs(UnmanagedType.U1)] bool destroy);

        [DllImport("freetype6.dll", EntryPoint = "FT_Set_Pixel_Sizes")]
        public static extern FT_Error Set_Pixel_Sizes(FT_Face face, FT_UInt pixel_width, FT_UInt pixel_height);

        [DllImport("freetype6.dll", EntryPoint = "FT_Get_Char_Index")]
        public static extern FT_UInt Get_Char_Index(FT_Face face, FT_ULong charcode);

        [DllImport("freetype6.dll", EntryPoint = "FT_Load_Glyph")]
        public static extern FT_Error Load_Glyph(FT_Face face, FT_UInt glyph_index, FT_LoadFlags load_flags);

        [DllImport("freetype6.dll", EntryPoint = "FT_Render_Glyph")]
        public static extern FT_Error Render_Glyph(FT_GlyphSlot slot, FT_Render_Mode render_mode);

        [DllImport("freetype6.dll", EntryPoint = "FT_Done_Glyph")]
        public static extern FT_Error Done_Glyph(FT_Glyph glyph);

        // Helper
        [DebuggerHidden]
        public static void assert(FT_Error error)
        {
            string errorString;
            switch (error)
            {
                case 0x00: return;
                case 0x01: errorString = "cannot open resource"; break;
                case 0x02: errorString = "unknown file format"; break;
                case 0x03: errorString = "broken file"; break;
                case 0x04: errorString = "invalid FreeType version"; break;
                case 0x05: errorString = "module version is too low"; break;
                case 0x06: errorString = "invalid argument"; break;
                case 0x07: errorString = "unimplemented feature"; break;
                case 0x08: errorString = "broken table"; break;
                case 0x09: errorString = "broken offset within table"; break;
                case 0x0A: errorString = "array allocation size too large"; break;

                // glyph/character errorStrings
                case 0x10: errorString = "invalid glyph index"; break;
                case 0x11: errorString = "invalid character code"; break;
                case 0x12: errorString = "unsupported glyph image format"; break;
                case 0x13: errorString = "cannot render this glyph format"; break;
                case 0x14: errorString = "invalid outline"; break;
                case 0x15: errorString = "invalid composite glyph"; break;
                case 0x16: errorString = "too many hints"; break;
                case 0x17: errorString = "invalid pixel size"; break;

                // handle errorStrings
                case 0x20: errorString = "invalid object handle"; break;
                case 0x21: errorString = "invalid library handle"; break;
                case 0x22: errorString = "invalid module handle"; break;
                case 0x23: errorString = "invalid face handle"; break;
                case 0x24: errorString = "invalid size handle"; break;
                case 0x25: errorString = "invalid glyph slot handle"; break;
                case 0x26: errorString = "invalid charmap handle"; break;
                case 0x27: errorString = "invalid cache manager handle"; break;
                case 0x28: errorString = "invalid stream handle"; break;

                // driver errorStrings
                case 0x30: errorString = "too many modules"; break;
                case 0x31: errorString = "too many extensions"; break;

                // memory errorStrings
                case 0x40: errorString = "out of memory"; break;
                case 0x41: errorString = "unlisted object"; break;

                // stream errorStrings
                case 0x51: errorString = "cannot open stream"; break;
                case 0x52: errorString = "invalid stream seek"; break;
                case 0x53: errorString = "invalid stream skip"; break;
                case 0x54: errorString = "invalid stream read"; break;
                case 0x55: errorString = "invalid stream operation"; break;
                case 0x56: errorString = "invalid frame operation"; break;
                case 0x57: errorString = "nested frame access"; break;
                case 0x58: errorString = "invalid frame read"; break;

                // raster errorStrings
                case 0x60: errorString = "raster uninitialized"; break;
                case 0x61: errorString = "raster corrupted"; break;
                case 0x62: errorString = "raster overflow"; break;
                case 0x63: errorString = "negative height while rastering"; break;

                // cache errorStrings
                case 0x70: errorString = "too many registered caches"; break;

                // TrueType and SFNT errorStrings */
                case 0x80: errorString = "invalid opcode"; break;
                case 0x81: errorString = "too few arguments"; break;
                case 0x82: errorString = "stack overflow"; break;
                case 0x83: errorString = "code overflow"; break;
                case 0x84: errorString = "bad argument"; break;
                case 0x85: errorString = "division by zero"; break;
                case 0x86: errorString = "invalid reference"; break;
                case 0x87: errorString = "found debug opcode"; break;
                case 0x88: errorString = "found ENDF opcode in execution stream"; break;
                case 0x89: errorString = "nested DEFS"; break;
                case 0x8A: errorString = "invalid code range"; break;
                case 0x8B: errorString = "execution context too long"; break;
                case 0x8C: errorString = "too many function definitions"; break;
                case 0x8D: errorString = "too many instruction definitions"; break;
                case 0x8E: errorString = "SFNT font table missing"; break;
                case 0x8F: errorString = "horizontal header (hhea) table missing"; break;
                case 0x90: errorString = "locations (loca) table missing"; break;
                case 0x91: errorString = "name table missing"; break;
                case 0x92: errorString = "character map (cmap) table missing"; break;
                case 0x93: errorString = "horizontal metrics (hmtx) table missing"; break;
                case 0x94: errorString = "PostScript (post) table missing"; break;
                case 0x95: errorString = "invalid horizontal metrics"; break;
                case 0x96: errorString = "invalid character map (cmap) format"; break;
                case 0x97: errorString = "invalid ppem value"; break;
                case 0x98: errorString = "invalid vertical metrics"; break;
                case 0x99: errorString = "could not find context"; break;
                case 0x9A: errorString = "invalid PostScript (post) table format"; break;
                case 0x9B: errorString = "invalid PostScript (post) table"; break;

                // CFF, CID, and Type 1 errorStrings */
                case 0xA0: errorString = "opcode syntax errorString"; break;
                case 0xA1: errorString = "argument stack underflow"; break;
                case 0xA2: errorString = "ignore"; break;

                // BDF errorStrings
                case 0xB0: errorString = "'STARTFONT' field missing"; break;
                case 0xB1: errorString = "'FONT' field missing"; break;
                case 0xB2: errorString = "'SIZE' field missing"; break;
                case 0xB3: errorString = "'CHARS' field missing"; break;
                case 0xB4: errorString = "'STARTCHAR' field missing"; break;
                case 0xB5: errorString = "'ENCODING' field missing"; break;
                case 0xB6: errorString = "'BBX' field missing"; break;
                case 0xB7: errorString = "'BBX' too big"; break;
                case 0xB8: errorString = "Font header corrupted or missing fields"; break;
                case 0xB9: errorString = "Font glyphs corrupted or missing fields"; break;

                default: errorString = "Error #" + error; break;
            }
            throw new Exception(errorString);
        }
    }
}
