using System;
using System.Collections.Generic;
using System.Text;

namespace RedHerringFarm.ImageSheets
{
    public static class SortUtil
    {
        public static List<ItemT> Merge<ItemT>(List<ItemT> list, Comparison<ItemT> compare)
        {
            if (list.Count <= 1) return list;
            int middle = list.Count / 2;
            List<ItemT> left = Merge<ItemT>(list.GetRange(0, middle), compare);
            List<ItemT> right = Merge<ItemT>(list.GetRange(middle, list.Count - middle), compare);
            int leftptr = 0;
            int rightptr = 0;
            List<ItemT> newList = new List<ItemT>(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                if (leftptr == left.Count)
                {
                    newList.Add(right[rightptr]);
                    rightptr++;
                }
                else if (rightptr == right.Count)
                {
                    newList.Add(left[leftptr]);
                    leftptr++;
                }
                else if (compare(left[leftptr], right[rightptr]) < 0)
                {
                    newList.Add(left[leftptr]);
                    leftptr++;
                }
                else
                {
                    newList.Add(right[rightptr]);
                    rightptr++;
                }
            }
            return newList;
        }
    }
}
