#region MIT License

/*
 * Copyright (c) 2009-2010 Nick Gravelyn (nick@gravelyn.com), Markus Ewald (cygon@nuclex.org)
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the Software 
 * is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all 
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
 * PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
 * 
 */

#endregion

using System;
using System.Collections.Generic;
using System.Drawing;

namespace RedHerringFarm.ImageSheets
{
    public class ArevaloRectanglePacker
    {
        private class AnchorRankComparer : IComparer<Point>
        {
            public static readonly AnchorRankComparer Default = new AnchorRankComparer();

            #region IComparer<Point> Members

            /// <summary>Compares the rank of two anchors against each other</summary>
            /// <param name="left">Left anchor point that will be compared</param>
            /// <param name="right">Right anchor point that will be compared</param>
            /// <returns>The relation of the two anchor point's ranks to each other</returns>
            public int Compare(Point left, Point right)
            {
                //return Math.Min(left.X, left.Y) - Math.Min(right.X, right.Y);
                return (left.X + left.Y) - (right.X + right.Y);
            }

            #endregion
        }

        private int actualPackingAreaHeight = 1;

        private int actualPackingAreaWidth = 1;

        private readonly List<Point> anchors = new List<Point> { new Point(0, 0) };

        private readonly List<Rectangle> packedRectangles = new List<Rectangle>();

        private int PackingAreaWidth, PackingAreaHeight;

        public ArevaloRectanglePacker(ArevaloRectanglePacker copyFrom)
        {
            actualPackingAreaWidth = copyFrom.actualPackingAreaWidth;
            actualPackingAreaHeight = copyFrom.actualPackingAreaHeight;
            anchors = new List<Point>(copyFrom.anchors);
            packedRectangles = new List<Rectangle>(copyFrom.packedRectangles);
            PackingAreaWidth = copyFrom.PackingAreaWidth;
            PackingAreaHeight = copyFrom.PackingAreaHeight;
        }

        public ArevaloRectanglePacker(int packingAreaWidth, int packingAreaHeight)
        {
            PackingAreaWidth = packingAreaWidth;
            PackingAreaHeight = packingAreaHeight;
        }

        public bool TryPack(int rectangleWidth, int rectangleHeight, out Point placement)
        {
            int anchorIndex = SelectAnchorRecursive(rectangleWidth, rectangleHeight, actualPackingAreaWidth, actualPackingAreaHeight);

            if (anchorIndex == -1)
            {
                placement = new Point();
                return false;
            }

            placement = anchors[anchorIndex];

            OptimizePlacement(ref placement, rectangleWidth, rectangleHeight);

            bool blocksAnchor =
                ((placement.X + rectangleWidth) > anchors[anchorIndex].X) &&
                ((placement.Y + rectangleHeight) > anchors[anchorIndex].Y);

            if (blocksAnchor)
                anchors.RemoveAt(anchorIndex);

            InsertAnchor(new Point(placement.X + rectangleWidth, placement.Y));
            InsertAnchor(new Point(placement.X, placement.Y + rectangleHeight));

            packedRectangles.Add(new Rectangle(placement.X, placement.Y, rectangleWidth, rectangleHeight));

            return true;
        }

        private void OptimizePlacement(ref Point placement, int rectangleWidth, int rectangleHeight)
        {
            var rectangle = new Rectangle(placement.X, placement.Y, rectangleWidth, rectangleHeight);

            int leftMost = placement.X;
            while (IsFree(ref rectangle, PackingAreaWidth, PackingAreaHeight))
            {
                leftMost = rectangle.X;
                --rectangle.X;
            }

            rectangle.X = placement.X;

            int topMost = placement.Y;
            while (IsFree(ref rectangle, PackingAreaWidth, PackingAreaHeight))
            {
                topMost = rectangle.Y;
                --rectangle.Y;
            }

            if ((placement.X - leftMost) > (placement.Y - topMost))
                placement.X = leftMost;
            else
                placement.Y = topMost;
        }

        private int SelectAnchorRecursive(int rectangleWidth, int rectangleHeight, int testedPackingAreaWidth, int testedPackingAreaHeight)
        {
            int freeAnchorIndex = FindFirstFreeAnchor(rectangleWidth, rectangleHeight, testedPackingAreaWidth, testedPackingAreaHeight);

            if (freeAnchorIndex != -1)
            {
                actualPackingAreaWidth = testedPackingAreaWidth;
                actualPackingAreaHeight = testedPackingAreaHeight;

                return freeAnchorIndex;
            }

            bool canEnlargeWidth = (testedPackingAreaWidth < PackingAreaWidth);
            bool canEnlargeHeight = (testedPackingAreaHeight < PackingAreaHeight);
            bool shouldEnlargeHeight = (!canEnlargeWidth) || (testedPackingAreaHeight < testedPackingAreaWidth);

            if (canEnlargeHeight && shouldEnlargeHeight)
            {
                return SelectAnchorRecursive(rectangleWidth, rectangleHeight, testedPackingAreaWidth, Math.Min(testedPackingAreaHeight * 2, PackingAreaHeight));
            }
            if (canEnlargeWidth)
            {
                return SelectAnchorRecursive(rectangleWidth, rectangleHeight, Math.Min(testedPackingAreaWidth * 2, PackingAreaWidth), testedPackingAreaHeight);
            }

            return -1;
        }

        private int FindFirstFreeAnchor(int rectangleWidth, int rectangleHeight, int testedPackingAreaWidth, int testedPackingAreaHeight)
        {
            var potentialLocation = new Rectangle(0, 0, rectangleWidth, rectangleHeight);

            for (int index = 0; index < anchors.Count; ++index)
            {
                potentialLocation.X = anchors[index].X;
                potentialLocation.Y = anchors[index].Y;

                if (IsFree(ref potentialLocation, testedPackingAreaWidth, testedPackingAreaHeight))
                    return index;
            }

            return -1;
        }

        private bool IsFree(ref Rectangle rectangle, int testedPackingAreaWidth, int testedPackingAreaHeight)
        {
            bool leavesPackingArea = (rectangle.X < 0) || (rectangle.Y < 0) || (rectangle.Right > testedPackingAreaWidth) || (rectangle.Bottom > testedPackingAreaHeight);

            if (leavesPackingArea)
                return false;

            for (int index = 0; index < packedRectangles.Count; ++index)
            {
                if (packedRectangles[index].IntersectsWith(rectangle))
                    return false;
            }

            return true;
        }

        private void InsertAnchor(Point anchor)
        {
            int insertIndex = anchors.BinarySearch(anchor, AnchorRankComparer.Default);
            if (insertIndex < 0)
                insertIndex = ~insertIndex;

            anchors.Insert(insertIndex, anchor);
        }

    }
}