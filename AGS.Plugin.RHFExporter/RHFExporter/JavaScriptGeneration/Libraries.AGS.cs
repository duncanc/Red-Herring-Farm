using System;
using System.Collections.Generic;
using System.Text;

namespace RedHerringFarm.JavaScriptGeneration
{
    public delegate Expression TransformCall(Expression.Calling originalCall);
    public delegate bool CallTransform(List<Expression> parameters, out Expression Transformed);

    public static class AgsLibraries
    {
        public static Expression Engine = new Expression.Custom("engine");
        public static Expression Game = new Expression.Custom("game");

        public static Dictionary<string, Expression> Functions;

        static AgsLibraries()
        {
            Functions = new Dictionary<string,Expression>();

            Add("FloatToInt", PossibleValueTypes.Int32, FloatToInt);
            Add("IntToFloat", PossibleValueTypes.Number, IntToFloat);
            Add("String$$get_Length", PossibleValueTypes.Int32, String__get_Length);
            Add("String$$geti_Chars", PossibleValueTypes.Int32, String__geti_Chars);
            Add("String$$IsNullOrEmpty", PossibleValueTypes.Int32, String__IsNullOrEmpty);
            Add("String$$LowerCase", PossibleValueTypes.Number, String__LowerCase);
            Add("String$$UpperCase", PossibleValueTypes.Number, String__UpperCase);
            Add("String$$Append", PossibleValueTypes.Number, String__Append);
            Add("String$$AppendChar", PossibleValueTypes.Number, String__AppendChar);
            Add("String$$Copy", PossibleValueTypes.Number, String__Copy);
            Add("String$$Substring", PossibleValueTypes.Number, String__Substring);

            Add("Maths$$get_Pi", PossibleValueTypes.Number, Maths__get_Pi);
            Add("Maths$$Sin", PossibleValueTypes.Number, Maths__Sin);

        }

        static void Add(string name, PossibleValueTypes VT, TransformCall transform)
        {
            LibraryFunction func = new LibraryFunction(Engine, name, VT);
            func.CallTransform = transform;
            Functions[name] = func;
        }

        public static Expression String__get_Length(Expression.Calling originalCall)
        {
            if (originalCall.Parameters.Count == 1)
            {
                return originalCall.Parameters[0].Index("length", PossibleValueTypes.Int32);
            }
            return originalCall;
        }

        public static Expression String__LowerCase(Expression.Calling originalCall)
        {
            return originalCall;
        }

        public static Expression Maths__get_Pi(Expression.Calling originalCall)
        {
            return StandardLibraries.Math.PI;
        }

        public static Expression Maths__Sin(Expression.Calling originalCall)
        {
            return StandardLibraries.Math.sin.Call(originalCall.Parameters);
        }

        public static Expression String__UpperCase(Expression.Calling originalCall)
        {
            return originalCall;
        }

        public static Expression String__Append(Expression.Calling originalCall)
        {
            if (originalCall.Parameters.Count == 2)
            {
                return originalCall.Parameters[0].BinOp(Infix.Add, originalCall.Parameters[1]);
            }
            return originalCall;
        }

        public static Expression String__AppendChar(Expression.Calling originalCall)
        {
            if (originalCall.Parameters.Count == 2)
            {
                return originalCall.Parameters[0].BinOp(
                    Infix.Add,
                    StandardLibraries.String.fromCharCode.Call(originalCall.Parameters.GetRange(1,1)));
            }
            return originalCall;
        }

        public static Expression String__Copy(Expression.Calling originalCall)
        {
            if (originalCall.Parameters.Count == 1)
            {
                return originalCall.Parameters[0];
            }
            return originalCall;
        }

        public static Expression String__geti_Chars(Expression.Calling originalCall)
        {
            if (originalCall.Parameters.Count == 2)
            {
                return StandardLibraries.String.charCodeAt(originalCall.Parameters[0], originalCall.Parameters[1]);
            }
            return originalCall;
        }

        public static Expression String__IsNullOrEmpty(Expression.Calling originalCall)
        {
            if (originalCall.Parameters.Count == 1)
            {
                return originalCall.Parameters[0].LogicallyNegate().Cast(PossibleValueTypes.UInt8);
            }
            return originalCall;
        }

        public static Expression String__Substring(Expression.Calling originalCall)
        {
            if (originalCall.Parameters.Count == 3)
            {
                return StandardLibraries.String.substr(
                    originalCall.Parameters[0],
                    originalCall.Parameters[1],
                    originalCall.Parameters[2]);
            }
            return originalCall;
        }

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
                            return originalCall.Parameters[0].BinOp(Infix.BitwiseOr, (Expression)0);
                        case 1:
                            return StandardLibraries.Math.round.Call(originalCall.Parameters.GetRange(0,1)).BinOp(Infix.BitwiseOr, (Expression)0);
                        case 2:
                            return StandardLibraries.Math.ceil.Call(originalCall.Parameters.GetRange(0, 1)).BinOp(Infix.BitwiseOr, (Expression)0);
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
    }
}
