using System;
using System.Collections.Generic;
using System.Text;

namespace RedHerringFarm.JavaScriptGeneration
{
    public delegate int BlockFilter(Block block, Statement statement, int index);

    public static class BlockFilters
    {
        public static int RemoveUnreachableCode(Block block, Statement statement, int index)
        {
            if (statement is Statement.If)
            {
                Statement.If _if = (Statement.If)statement;
                int v;
                if (_if.Condition.TryGetIntValue(out v))
                {
                    if (v == 0)
                    {
                        block.RemoveAt(index);
                        block.InsertRange(index, _if.ElseDoThis);
                        return index;
                    }
                    else
                    {
                        block.RemoveAt(index);
                        block.InsertRange(index, _if.ThenDoThis);
                        return index;
                    }
                }
            }
            else if (statement is Statement.While)
            {
                Statement.While loop = (Statement.While)statement;
                int v;
                if (loop.WhileThisIsTrue.TryGetIntValue(out v) && v == 0)
                {
                    block.RemoveAt(index);
                    return index;
                }
            }
            else if (statement is Statement.Return)
            {
                block.RemoveRange(index + 1, block.Count - (index + 1));
            }
            return index + 1;
        }

        public static IEnumerable<BlockFilter> All
        {
            get
            {
                yield return RemoveUnreachableCode;
            }
        }
    }
}
