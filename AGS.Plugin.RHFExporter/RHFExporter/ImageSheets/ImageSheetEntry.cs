using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace RedHerringFarm.ImageSheets
{
    public abstract class ImageSheetEntry
    {
        protected ImageSheetEntry()
        {
        }
        public abstract int Width { get; }
        public abstract int Height { get; }
        private int x, y, number;
        private ImageSheet owningSheet;
        protected int paddingLeft, paddingRight, paddingTop, paddingBottom;
        public int LeftPadding
        {
            get { return paddingLeft; }
        }
        public int RightPadding
        {
            get { return paddingRight; }
        }
        public int TopPadding
        {
            get { return paddingTop; }
        }
        public int BottomPadding
        {
            get { return paddingBottom; }
        }
        public int X
        {
            get { return x; }
            internal set { x = value; }
        }
        public int Y
        {
            get { return y; }
            internal set { y = value; }
        }
        public ImageSheet OwningSheet
        {
            get { return owningSheet; }
            internal set { owningSheet = value; }
        }
        public int EntryNumber
        {
            get
            {
                return number;
            }
            internal set { number = value; }
        }
        public virtual string UniqueKey
        {
            get { return null; }
        }
        public abstract void Draw(Graphics g);
        public abstract void Draw(BitmapData bdata);
        public void WriteJson(JsonWriter output)
        {
            WriteJson(output, null);
        }
        public void WriteJson(JsonWriter output, string key)
        {
            using (output.BeginObject(key))
            {
                output.WriteValue("x", X);
                output.WriteValue("y", Y);
                output.WriteValue("w", Width);
                output.WriteValue("h", Height);
                if (paddingLeft != 0)
                {
                    output.WriteValue("l", paddingLeft);
                }
                if (paddingRight != 0)
                {
                    output.WriteValue("r", paddingRight);
                }
                if (paddingTop != 0)
                {
                    output.WriteValue("t", paddingTop);
                }
                if (paddingBottom != 0)
                {
                    output.WriteValue("b", paddingBottom);
                }
            }
        }
    }
}
