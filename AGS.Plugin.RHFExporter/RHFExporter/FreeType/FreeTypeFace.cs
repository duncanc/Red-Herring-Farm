using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using FT = FreeType.FTInterface;

namespace FreeType
{
    public class FreeTypeFace : IDisposable
    {
        internal FreeTypeFace(IntPtr face)
        {
            this.face = face;
        }
        private IntPtr face;
        private FT.FaceRec face_rec;

        private void updateFaceRec()
        {
            face_rec = (FT.FaceRec)Marshal.PtrToStructure(face, typeof(FT.FaceRec));
        }

        public int NumFaces
        {
            get
            {
                updateFaceRec();
                return face_rec.num_faces;
            }
        }

        public int FaceIndex
        {
            get
            {
                updateFaceRec();
                return face_rec.face_index;
            }
        }

        public FT_FaceFlags FaceFlags
        {
            get
            {
                updateFaceRec();
                return face_rec.face_flags;
            }
        }

        public FT_StyleFlags StyleFlags
        {
            get
            {
                updateFaceRec();
                return face_rec.style_flags;
            }
        }

        public int NumGlyphs
        {
            get
            {
                updateFaceRec();
                return face_rec.num_glyphs;
            }
        }

        public string FamilyName
        {
            get
            {
                updateFaceRec();
                return face_rec.family_name;
            }
        }

        public string StyleName
        {
            get
            {
                updateFaceRec();
                return face_rec.style_name;
            }
        }

        public int NumFixedSizes
        {
            get
            {
                updateFaceRec();
                return face_rec.num_fixed_sizes;
            }
        }

        public int NumCharMaps
        {
            get
            {
                updateFaceRec();
                return face_rec.num_charmaps;
            }
        }

        public int BoundingBoxLeft
        {
            get
            {
                updateFaceRec();
                return face_rec.bbox_xMin;
            }
        }

        public int BoundingBoxRight
        {
            get
            {
                updateFaceRec();
                return face_rec.bbox_xMax;
            }
        }

        public int BoundingBoxTop
        {
            get
            {
                updateFaceRec();
                return face_rec.bbox_yMin;
            }
        }

        public int BoundingBoxBottom
        {
            get
            {
                updateFaceRec();
                return face_rec.bbox_yMax;
            }
        }

        public ushort UnitsPerEm
        {
            get
            {
                updateFaceRec();
                return face_rec.units_per_EM;
            }
        }

        public short Ascender
        {
            get
            {
                updateFaceRec();
                return face_rec.ascender;
            }
        }

        public short Descender
        {
            get
            {
                updateFaceRec();
                return face_rec.descender;
            }
        }

        public short Height
        {
            get
            {
                updateFaceRec();
                return face_rec.height;
            }
        }

        public short MaxAdvanceWidth
        {
            get
            {
                updateFaceRec();
                return face_rec.max_advance_width;
            }
        }

        public short MaxAdvanceHeight
        {
            get
            {
                updateFaceRec();
                return face_rec.max_advance_height;
            }
        }

        public short UnderlinePosition
        {
            get
            {
                updateFaceRec();
                return face_rec.underline_position;
            }
        }

        public short UnderlineThickness
        {
            get
            {
                updateFaceRec();
                return face_rec.underline_thickness;
            }
        }

        public FreeTypeCharMap GetCharMap(int i)
        {
            updateFaceRec();
            if (i < 0 || i >= face_rec.num_charmaps) throw new IndexOutOfRangeException();
            return new FreeTypeCharMap(Marshal.ReadIntPtr(face_rec.charmaps, i * IntPtr.Size));
        }

        public void Dispose()
        {
            FT.assert(FT.Done_Face(face));
            face = IntPtr.Zero;
        }

        ~FreeTypeFace()
        {
            if (face != IntPtr.Zero)
            {
                FT.Done_Face(face);
            }
        }

        public void SetCharSize(int charWidth, int charHeight, uint horizResolution, uint vertResolution)
        {
            FT.assert(FT.Set_Char_Size(face, charWidth, charHeight, horizResolution, vertResolution));
        }

        FreeTypeGlyphSlot lockedGlyph = null;

        public FreeTypeGlyphSlot LockGlyph(uint glyphIndex, FT_LoadFlags loadFlags)
        {
            if (lockedGlyph != null)
            {
                throw new Exception("A glyph is already locked, use Dispose() or using { }");
            }
            FT.assert(FT.Load_Glyph(face, glyphIndex, loadFlags));
            updateFaceRec();
            lockedGlyph = new FreeTypeGlyphSlot(this, face_rec.glyph);
            return lockedGlyph;
        }

        internal void UnlockGlyph()
        {
            lockedGlyph = null;
        }

        public uint GetCharIndex(uint charcode)
        {
            return FT.Get_Char_Index(face, charcode);
        }
    }
}
