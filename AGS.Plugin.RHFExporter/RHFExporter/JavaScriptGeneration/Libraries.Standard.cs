using System;
using System.Collections.Generic;
using System.Text;

namespace RedHerringFarm.JavaScriptGeneration
{
    public static class StandardLibraries
    {
        public static readonly MathLibrary Math = new MathLibrary();
        public static readonly StringLibrary String = new StringLibrary();
        public static readonly RegExpLibrary RegExp = new RegExpLibrary();
        public static readonly DateLibrary Date = new DateLibrary();
    }
    public class DateLibrary : Expression.Custom
    {
        public DateLibrary()
            : base("Date")
        {
        }
        public Expression getFullYear(Expression dateExpr)
        {
            return new Expression.Calling(dateExpr.Index("getFullYear"), PossibleValueTypes.Int32);
        }
        public Expression getMonth(Expression dateExpr)
        {
            return new Expression.Calling(dateExpr.Index("getMonth"), PossibleValueTypes.UInt8);
        }
        public Expression getDate(Expression dateExpr)
        {
            return new Expression.Calling(dateExpr.Index("getDate"), PossibleValueTypes.UInt8);
        }
        public Expression getHours(Expression dateExpr)
        {
            return new Expression.Calling(dateExpr.Index("getHours"), PossibleValueTypes.UInt8);
        }
        public Expression getMinutes(Expression dateExpr)
        {
            return new Expression.Calling(dateExpr.Index("getMinutes"), PossibleValueTypes.UInt8);
        }
        public Expression getSeconds(Expression dateExpr)
        {
            return new Expression.Calling(dateExpr.Index("getSeconds"), PossibleValueTypes.UInt8);
        }
        public Expression valueOf(Expression dateExpr)
        {
            return new Expression.Calling(dateExpr.Index("valueOf"), PossibleValueTypes.Number);
        }
    }
    public class RegExpLibrary : Expression.Custom
    {
        public RegExpLibrary()
            : base("RegExp")
        {
        }
    }
    public class StringLibrary : Expression.Custom
    {
        public StringLibrary()
            : base("String")
        {
            fromCharCode = new LibraryFunction(this, "fromCharCode", PossibleValueTypes.String);
        }
        public LibraryFunction fromCharCode;
        public Expression length(Expression stringExpr)
        {
            return stringExpr.Index("length", PossibleValueTypes.Int32);
        }
        public Expression charCodeAt(Expression stringExpr, Expression i)
        {
            Expression.Calling call = new Expression.Calling(stringExpr.Index("charCodeAt"), PossibleValueTypes.Int32);
            call.Parameters.Add(i);
            return call;
        }
        public Expression substr(Expression stringExpr, Expression index, Expression length)
        {
            Expression.Calling call = new Expression.Calling(stringExpr.Index("substr"), PossibleValueTypes.String);
            call.Parameters.Add(index);
            call.Parameters.Add(length);
            return call;
        }
    }
    public class MathLibrary : Expression.Custom
    {
        public MathLibrary()
            : base("Math")
        {
            abs = new LibraryFunction(this, "abs", PossibleValueTypes.Number);
            acos = new LibraryFunction(this, "acos", PossibleValueTypes.Number);
            asin = new LibraryFunction(this, "atan", PossibleValueTypes.Number);
            atan = new LibraryFunction(this, "atan2", PossibleValueTypes.Number);
            ceil = new LibraryFunction(this, "ceil", PossibleValueTypes.Number);
            cos = new LibraryFunction(this, "cos", PossibleValueTypes.Number);
            exp = new LibraryFunction(this, "exp", PossibleValueTypes.Number);
            floor = new LibraryFunction(this, "exp", PossibleValueTypes.Number);
            log = new LibraryFunction(this, "log", PossibleValueTypes.Number);
            max = new LibraryFunction(this, "max", PossibleValueTypes.Number);
            min = new LibraryFunction(this, "min", PossibleValueTypes.Number);
            pow = new LibraryFunction(this, "pow", PossibleValueTypes.Number);
            random = new LibraryFunction(this, "random", PossibleValueTypes.Number);
            round = new LibraryFunction(this, "round", PossibleValueTypes.Number);
            sin = new LibraryFunction(this, "sin", PossibleValueTypes.Number);
            sqrt = new LibraryFunction(this, "sqrt", PossibleValueTypes.Number);
            tan = new LibraryFunction(this, "tan", PossibleValueTypes.Number);

            E = new LibraryConstant(this, "E", PossibleValueTypes.Number);
            LN10 = new LibraryConstant(this, "LN10", PossibleValueTypes.Number);
            LN2 = new LibraryConstant(this, "LN2", PossibleValueTypes.Number);
            LOG2E = new LibraryConstant(this, "LOG2E", PossibleValueTypes.Number);
            LOG10E = new LibraryConstant(this, "LOG10E", PossibleValueTypes.Number);
            PI = new LibraryConstant(this, "PI", PossibleValueTypes.Number);
            SQRT1_2 = new LibraryConstant(this, "SQRT1_2", PossibleValueTypes.Number);
            SQRT2 = new LibraryConstant(this, "SQRT2", PossibleValueTypes.Number);
        }
        public LibraryFunction
            abs, acos, asin, atan, atan2, ceil, cos, exp, floor, log,
            max, min, pow, random, round, sin, sqrt, tan;

        public LibraryConstant E, LN10, LN2, LOG2E, LOG10E, PI, SQRT1_2, SQRT2;
    }
}
