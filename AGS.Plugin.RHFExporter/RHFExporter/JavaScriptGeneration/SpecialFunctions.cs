using System;
using System.Collections.Generic;
using System.Text;

namespace RedHerringFarm.JavaScriptGeneration
{
    public static class SpecialFunctions
    {
        public static Expression FloatToInt(Expression.Calling originalCall)
        {
            if (originalCall.Parameters.Count == 2)
            {
                int mode;
                if (originalCall.Parameters[1].TryGetIntValue(out mode))
                {
                    switch (mode)
                    {
                        case 0:
                            return originalCall.Parameters[0];
                    }
                }
            }
            return originalCall;
        }
        public static Expression IntToFloat(Expression.Calling originalCall)
        {
            if (originalCall.Parameters.Count == 1)
            {
                return originalCall.Parameters[0];
            }
            return originalCall;
        }
        public static Expression String__get_Length(Expression.Calling originalCall)
        {
            if (originalCall.Parameters.Count == 1)
            {
                return originalCall.Parameters[0].Index("length", PossibleValueTypes.Int32);
            }
            return originalCall;
        }
        public static Expression String__IsNullOrEmpty(Expression.Calling originalCall)
        {
            if (originalCall.Parameters.Count == 1)
            {
                return originalCall.Parameters[0].LogicallyNegate();
            }
            return originalCall;
        }
        public static readonly Dictionary<string, TransformCall> All;
        static SpecialFunctions()
        {
            All = new Dictionary<string,TransformCall>();
            All["FloatToInt"] = FloatToInt;
            All["IntToFloat"] = IntToFloat;
            All["String::get_Length"] = String__get_Length;
        }
    }
}
