using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using FT = FreeType.FTInterface;

namespace FreeType
{
    public class FreeTypeSize
    {
        internal FreeTypeSize(IntPtr size)
        {
            this.size = size;
        }
        private IntPtr size;
        private FT.SizeRec size_rec;
        private void updateRec()
        {
            size_rec = (FT.SizeRec)Marshal.PtrToStructure(size, typeof(FT.SizeRec));
        }
        public ushort SizeMetricsXPPEM
        {
            get
            {
                updateRec();
                return size_rec.size_metrics_x_ppem;
            }
        }
        public ushort SizeMetricsYPPEM
        {
            get
            {
                updateRec();
                return size_rec.size_metrics_y_ppem;
            }
        }
        public int SizeMetricsXScale
        {
            get
            {
                updateRec();
                return size_rec.size_metrics_x_scale;
            }
        }
        public int SizeMetricsYScale
        {
            get
            {
                updateRec();
                return size_rec.size_metrics_y_scale;
            }
        }
        public int SizeMetricsAscender
        {
            get
            {
                updateRec();
                return size_rec.size_metrics_ascender;
            }
        }
        public int SizeMetricsDescender
        {
            get
            {
                updateRec();
                return size_rec.size_metrics_descender;
            }
        }
        public int SizeMetricsHeight
        {
            get
            {
                updateRec();
                return size_rec.size_metrics_height;
            }
        }
        public int SizeMetricsMaxAdvance
        {
            get
            {
                updateRec();
                return size_rec.size_metrics_max_advance;
            }
        }
    }
}
