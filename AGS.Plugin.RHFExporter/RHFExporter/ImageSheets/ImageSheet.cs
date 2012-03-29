using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using RedHerringFarm.TaskManaging;

namespace RedHerringFarm.ImageSheets
{
    public class ImageSheet : IEnumerable<ImageSheetEntry>
    {
        internal class SheetSnapshot
        {
            internal SheetSnapshot(ImageSheet sheet)
            {
                Packer = new ArevaloRectanglePacker(sheet.testPacker);
                Entries = new List<ImageSheetEntry>(sheet.Entries);
                PixelArea = sheet.pixelArea;
            }
            internal void Apply(ImageSheet sheet)
            {
                sheet.testPacker = new ArevaloRectanglePacker(Packer);
                sheet.Entries = new List<ImageSheetEntry>(Entries);
                sheet.entriesByKey.Clear();
                foreach (ImageSheetEntry entry in sheet.Entries)
                {
                    string key = entry.UniqueKey;
                    if (key != null)
                    {
                        sheet.entriesByKey.Add(key, entry);
                    }
                }
                sheet.pixelArea = PixelArea;
            }
            internal ArevaloRectanglePacker Packer;
            internal List<ImageSheetEntry> Entries;
            internal int PixelArea;
        }
        public ImageSheet(int maxWidth, int maxHeight, int betweenRectPadding, int margin, PixelFormat pixelFormat)
        {
            this.MaxWidth = maxWidth;
            this.MaxHeight = maxHeight;
            this.BetweenRectPadding = betweenRectPadding;
            this.Margin = margin;
            this.pixelFormat = pixelFormat;
        }
        private PixelFormat pixelFormat;
        public ImageSheet(int maxWidth, int maxHeight, int betweenRectPadding, int margin)
            : this(maxWidth, maxHeight, betweenRectPadding, margin, PixelFormat.Format32bppPArgb)
        {
            testPacker = new ArevaloRectanglePacker(maxWidth, maxHeight);
        }
        private readonly int MaxWidth;
        private readonly int MaxHeight;
        private readonly int BetweenRectPadding;
        private readonly int Margin;

        public bool MakeTransparent = false;

        private List<ImageSheetEntry> Entries = new List<ImageSheetEntry>();
        private Dictionary<string,ImageSheetEntry> entriesByKey = new Dictionary<string,ImageSheetEntry>();
        private int pixelArea = 0;

        private ArevaloRectanglePacker testPacker;

        public object Snapshot()
        {
            return new SheetSnapshot(this);
        }

        public void RestoreSnapshot(object o)
        {
            ((SheetSnapshot)o).Apply(this);
        }

        public bool AddEntry(ImageSheetEntry entry)
        {
            if (entry.Width == 0 || entry.Height == 0)
            {
                return true;
            }

            entry.OwningSheet = this;
            packed = false;

            string key = entry.UniqueKey;
            if (key != null)
            {
                ImageSheetEntry duplicate;
                if (entriesByKey.TryGetValue(key, out duplicate))
                {
                    entry.EntryNumber = duplicate.EntryNumber;
                    return true;
                }
            }

            Point placement;
            if (!testPacker.TryPack(entry.Width, entry.Height, out placement))
            {
                return false;
            }

            entry.EntryNumber = Entries.Count;
            Entries.Add(entry);

            pixelArea += (entry.Width * entry.Height);

            if (key != null)
            {
                entriesByKey.Add(key, entry);
            }
            return true;
        }

        private int finalWidth;
        private int finalHeight;

        private bool packed;
        public bool IsPacked
        {
            get { return packed; }
        }

        public bool Pack()
        {
            if (Entries.Count == 0)
            {
                throw new Exception("Cannot pack an empty image sheet");
            }

            List<ImageSheetEntry> sorted = SortUtil.Merge<ImageSheetEntry>(Entries,
                delegate(ImageSheetEntry entry1, ImageSheetEntry entry2)
                {
                    return -Math.Max(entry1.Width, entry1.Height).CompareTo(Math.Max(entry2.Width, entry2.Height));
                });

            TaskManager.StatusUpdate("Attempting to pack " + sorted.Count + " images...");

            int smallestWidth = int.MaxValue;
            int smallestHeight = int.MaxValue;

            foreach (ImageSheetEntry entry in Entries)
            {
                smallestWidth = Math.Min(smallestWidth, entry.Width);
                smallestHeight = Math.Min(smallestHeight, entry.Height);
            }

            Dictionary<ImageSheetEntry, Point> testImagePlacement = new Dictionary<ImageSheetEntry, Point>();

            int outputWidth = MaxWidth - (Margin * 2) + BetweenRectPadding;
            int outputHeight = MaxHeight - (Margin * 2) + BetweenRectPadding;

            int testWidth = outputWidth;
            int testHeight = outputHeight;

            // 1-2 rep: 2-Dimensional Shrinkage
            int maximumWidth = outputWidth;
            int maximumHeight = outputHeight;

            int minimumWidth = (int)Math.Floor(Math.Sqrt(pixelArea)) - 1;
            int minimumHeight = minimumWidth;

            testWidth = minimumWidth + (maximumWidth - minimumWidth) / 2;
            testHeight = minimumHeight + (maximumHeight - minimumHeight) / 2;

            while (true)
            {
                testImagePlacement.Clear();

                TaskManager.StatusUpdate("Trying " + ((Margin * 2) + testWidth - BetweenRectPadding) + "x" + ((Margin * 2) + testHeight - BetweenRectPadding) + "...");

                ArevaloRectanglePacker packer = new ArevaloRectanglePacker(testWidth, testHeight);
                bool failed = false;
                foreach (ImageSheetEntry entry in sorted)
                {
                    Point pos;
                    if (!packer.TryPack(
                        entry.Width + BetweenRectPadding,
                        entry.Height + BetweenRectPadding,
                        out pos))
                    {
                        failed = true;
                        break;
                    }
                    testImagePlacement[entry] = pos;
                }
                if (failed)
                {
                    if (maximumWidth <= testWidth + 1 || maximumHeight <= testHeight + 1)
                    {
                        // go to the third loop
                        break;
                    }
                    minimumWidth = testWidth;
                    minimumHeight = testHeight;
                    testWidth = minimumWidth + (maximumWidth - minimumWidth) / 2;
                    testHeight = minimumHeight + (maximumHeight - minimumHeight) / 2;
                }
                else
                {
                    testWidth = testHeight = 0;
                    foreach (KeyValuePair<ImageSheetEntry, Point> pair in testImagePlacement)
                    {
                        pair.Key.X = Margin + pair.Value.X;
                        pair.Key.Y = Margin + pair.Value.Y;
                        testWidth = Math.Max(testWidth, pair.Value.X + pair.Key.Width + BetweenRectPadding);
                        testHeight = Math.Max(testHeight, pair.Value.Y + pair.Key.Height + BetweenRectPadding);
                    }

                    outputWidth = testWidth;
                    outputHeight = testHeight;

                    finalWidth = (Margin * 2) + outputWidth - BetweenRectPadding;
                    finalHeight = (Margin * 2) + outputHeight - BetweenRectPadding;

                    if (minimumWidth >= testWidth - 1 || minimumHeight >= testHeight - 1)
                    {
                        // go to the third loop
                        break;
                    }
                    maximumWidth = testWidth;
                    maximumHeight = testHeight;

                    testWidth = minimumWidth + (maximumWidth - minimumWidth) / 2;
                    testHeight = minimumHeight + (maximumHeight - minimumHeight) / 2;
                }
            }

            // 3: 1-Dimensional Shrink

            bool shrinkVertical = outputWidth <= outputHeight;

            if (shrinkVertical)
            {
                minimumWidth = maximumWidth = testWidth = outputWidth;
                maximumHeight = outputHeight;
                minimumHeight = (pixelArea / outputWidth) - 1;
                testHeight = minimumHeight + (maximumHeight - minimumHeight) / 2;
            }
            else
            {
                minimumHeight = maximumHeight = testHeight = outputHeight;
                maximumWidth = outputWidth;
                minimumWidth = (pixelArea / outputHeight) - 1;
                testWidth = minimumWidth + (maximumWidth - minimumWidth) / 2;
            }

            while (true)
            {
                testImagePlacement.Clear();

                TaskManager.StatusUpdate("Trying " + ((Margin * 2) + testWidth - BetweenRectPadding) + "x" + ((Margin * 2) + testHeight - BetweenRectPadding) + "...");

                ArevaloRectanglePacker packer = new ArevaloRectanglePacker(testWidth, testHeight);
                bool failed = false;
                foreach (ImageSheetEntry entry in sorted)
                {
                    Point pos;
                    if (!packer.TryPack(
                        entry.Width + BetweenRectPadding,
                        entry.Height + BetweenRectPadding,
                        out pos))
                    {
                        failed = true;
                        break;
                    }
                    testImagePlacement[entry] = pos;
                }
                if (failed)
                {
                    if (maximumWidth <= testWidth + 1 && maximumHeight <= testHeight + 1)
                    {
                        // go to the third loop
                        break;
                    }
                    minimumWidth = testWidth;
                    minimumHeight = testHeight;
                    testWidth = minimumWidth + (maximumWidth - minimumWidth) / 2;
                    testHeight = minimumHeight + (maximumHeight - minimumHeight) / 2;
                }
                else
                {
                    testWidth = testHeight = 0;
                    foreach (KeyValuePair<ImageSheetEntry, Point> pair in testImagePlacement)
                    {
                        pair.Key.X = Margin + pair.Value.X;
                        pair.Key.Y = Margin + pair.Value.Y;
                        testWidth = Math.Max(testWidth, pair.Value.X + pair.Key.Width + BetweenRectPadding);
                        testHeight = Math.Max(testHeight, pair.Value.Y + pair.Key.Height + BetweenRectPadding);
                    }

                    outputWidth = testWidth;
                    outputHeight = testHeight;

                    finalWidth = (Margin * 2) + outputWidth - BetweenRectPadding;
                    finalHeight = (Margin * 2) + outputHeight - BetweenRectPadding;

                    if (minimumWidth >= testWidth - 1 && minimumHeight >= testHeight - 1)
                    {
                        // go to the third loop
                        break;
                    }
                    maximumWidth = testWidth;
                    maximumHeight = testHeight;

                    testWidth = minimumWidth + (maximumWidth - minimumWidth) / 2;
                    testHeight = minimumHeight + (maximumHeight - minimumHeight) / 2;
                }
            }

            finalWidth = (Margin * 2) + outputWidth - BetweenRectPadding;
            finalHeight = (Margin * 2) + outputHeight - BetweenRectPadding;

            TaskManager.StatusUpdate("Final size: " + finalWidth + "x" + finalHeight);
            return packed = true;
        }
        public bool IsEmpty
        {
            get { return (Entries.Count == 0); }
        }
        public Color ClearColor = Color.Transparent;
        public Bitmap GetBitmap()
        {
            if (!packed)
            {
                throw new Exception("Image sheet has not been successfully packed");
            }
            Bitmap bmp = new Bitmap(finalWidth, finalHeight, pixelFormat);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(ClearColor);
            }
            if (pixelFormat == PixelFormat.Format1bppIndexed)
            {
                foreach (ImageSheetEntry entry in Entries)
                {
                    BitmapData locked = bmp.LockBits(
                        new Rectangle(entry.X, entry.Y, entry.Width, entry.Height),
                        ImageLockMode.WriteOnly,
                        pixelFormat);
                    entry.Draw(locked);
                    bmp.UnlockBits(locked);
                }
            }
            else
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    foreach (ImageSheetEntry entry in Entries)
                    {
                        entry.Draw(g);
                    }
                }
            }
            Bitmap pal;
            TaskManager.StatusUpdate("Attempting to palettize...");
            if (BitmapUtil.TryMakePaletted(bmp, out pal))
            {
                bmp = pal;
                TaskManager.StatusUpdate("Palettize successful: bmp is now " + bmp.PixelFormat);
            }
            else
            {
                TaskManager.StatusUpdate("Palettize failed.");
            }
            if (MakeTransparent)
            {
                BitmapUtil.MakeTransparent(bmp, ClearColor);
            }
            TaskManager.StatusUpdate("Final status: " + bmp.PixelFormat);
            return bmp;
        }
        public void WriteJson(JsonWriter output, string key)
        {
            if (!packed)
            {
                throw new Exception("Image sheet has not been successfully packed");
            }
            using (output.BeginObject(key))
            {
                using (output.BeginArray("images"))
                {
                    foreach (ImageSheetEntry entry in Entries)
                    {
                        using (output.BeginObject())
                        {
                            output.WriteValue("x", entry.X);
                            output.WriteValue("y", entry.Y);
                            output.WriteValue("w", entry.Width);
                            output.WriteValue("h", entry.Height);
                        }
                    }
                }
            }
        }

        public IEnumerator<ImageSheetEntry> GetEnumerator()
        {
            return Entries.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Entries).GetEnumerator();
        }
    }
}
