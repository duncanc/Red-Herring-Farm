using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using FT = FreeType.FTInterface;

namespace FreeType
{
    public class FreeTypeGlyphSlot : IDisposable
    {
        internal FreeTypeGlyphSlot(FreeTypeFace face, IntPtr slot)
        {
            this.face = face;
            this.slot = slot;
        }
        FreeTypeFace face;
        public void Dispose()
        {
            face.UnlockGlyph();
            slot = IntPtr.Zero;
        }
        IntPtr slot;
        FT.GlyphSlotRec glyphslot_rec;
        internal void updateRec()
        {
            glyphslot_rec = (FT.GlyphSlotRec)Marshal.PtrToStructure(slot, typeof(FT.GlyphSlotRec));
        }
        public int Metrics_Width
        {
            get
            {
                updateRec();
                return glyphslot_rec.metrics_width;
            }
        }
        public int Metrics_Height
        {
            get
            {
                updateRec();
                return glyphslot_rec.metrics_height;
            }
        }
        public int Metrics_HorizontalLayout_Left
        {
            get
            {
                updateRec();
                return glyphslot_rec.metrics_horiBearingX;
            }
        }
        public int Metrics_HorizontalLayout_Top
        {
            get
            {
                updateRec();
                return glyphslot_rec.metrics_horiBearingY;
            }
        }
        public int Metrics_HorizontalLayout_Advance
        {
            get
            {
                updateRec();
                return glyphslot_rec.metrics_horiAdvance;
            }
        }
        public int Metrics_VerticalLayout_Left
        {
            get
            {
                updateRec();
                return glyphslot_rec.metrics_vertBearingX;
            }
        }
        public int Metrics_VerticalLayout_Top
        {
            get
            {
                updateRec();
                return glyphslot_rec.metrics_vertBearingY;
            }
        }
        public int Metrics_VerticalLayout_Advance
        {
            get
            {
                updateRec();
                return glyphslot_rec.metrics_vertAdvance;
            }
        }
        public int LinearHorizontalAdvance
        {
            get
            {
                updateRec();
                return glyphslot_rec.linearHoriAdvance;
            }
        }
        public int LinearVerticalAdvance
        {
            get
            {
                updateRec();
                return glyphslot_rec.linearVertAdvance;
            }
        }
        public int AdvanceX
        {
            get
            {
                updateRec();
                return glyphslot_rec.advance_x;
            }
        }
        public int AdvanceY
        {
            get
            {
                updateRec();
                return glyphslot_rec.advance_y;
            }
        }
        public FT_GlyphFormat Format
        {
            get
            {
                updateRec();
                return glyphslot_rec.format;
            }
        }
        public FreeTypeGlyph GetGlyph()
        {
            IntPtr glyph;
            FT.assert(FT.Get_Glyph(slot, out glyph));
            return new FreeTypeGlyph(glyph);
        }
    }
}
