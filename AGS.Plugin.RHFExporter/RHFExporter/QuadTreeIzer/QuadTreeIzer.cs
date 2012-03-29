using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace RedHerringFarm
{
    public static class QuadTreeTools
    {
        public static int GetRootSize(int width, int height)
        {
            int rootSize = 0;
            while ((1 << rootSize) < width || (1 << rootSize) < height) rootSize++;
            return rootSize;
        }
        public static int RootSizeToPixels(int rootSize)
        {
            return 1 << rootSize;
        }
        public delegate int ZoneGetter(int x, int y);
    }
    public class QuadTreeNode
    {
        private QuadTreeTools.ZoneGetter getZone;
        public int x, y, size;
        public QuadTreeNode(QuadTreeTools.ZoneGetter getZone, int size, int x, int y)
        {
            this.getZone = getZone;
            this.size = size;
            this.x = x;
            this.y = y;
        }
        public QuadTreeNode[] children;
        public int zone;
        public void process(int width, int height)
        {
            if (size == 1)
            {
                zone = getZone(x, y);
            }
            else
            {
                QuadTreeNode tl = new QuadTreeNode(getZone, size / 2, x, y);
                QuadTreeNode tr = new QuadTreeNode(getZone, size / 2, x + size / 2, y);
                QuadTreeNode bl = new QuadTreeNode(getZone, size / 2, x, y + size / 2);
                QuadTreeNode br = new QuadTreeNode(getZone, size / 2, x + size / 2, y + size / 2);

                tl.process(width, height);
                tr.process(width, height);
                bl.process(width, height);
                br.process(width, height);

                /*
                if (tr.x >= width)
                {
                    tr.children = null;
                    br.children = null;
                    tr.zone = tl.zone;
                    br.zone = bl.zone;
                }
                if (bl.y >= height)
                {
                    bl.children = null;
                    br.children = null;
                    bl.zone = tl.zone;
                    br.zone = tr.zone;
                }
                 */

                if (tl.children == null && tr.children == null && bl.children == null && br.children == null
                    && tl.zone == tr.zone && tr.zone == bl.zone && bl.zone == br.zone)
                {
                    zone = tl.zone;
                }
                else
                {
                    children = new QuadTreeNode[] { tl, tr, bl, br };
                }
            }
        }
    }
    /*
    public abstract class ZoneGridData
    {
        public readonly int Width, Height;
        protected ZoneGridData(int width, int height)
        {
            Width = width;
            Height = height;
        }
        public abstract byte ZoneAtCoords(int x, int y);

        public class Bitmap : ZoneGridData
        {
            private System.Drawing.Bitmap bmp;
            public Bitmap(System.Drawing.Bitmap bmp)
            {

            }
        }
    }
     */
}
