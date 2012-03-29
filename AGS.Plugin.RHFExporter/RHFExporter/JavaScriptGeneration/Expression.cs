using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace RedHerringFarm.JavaScriptGeneration
{
    [Flags]
    public enum PossibleValueTypes
    {
        None = 0,

        UInt8 = 1,
        Int16 = 1 | 2,
        Int32 = 1 | 2 | 4,
        Number = 1 | 2 | 4 | 8,
        NonNumeric = ~Number,

        Boolean = 16,
        String = 32,
        
        Any = 0xffffff
    }
    public enum Infix
    {
        Multiply,
        Divide,
        Modulus,

        Add,
        Subtract,

        BitwiseLeftShift,
        BitwiseSignedRightShift,
        BitwiseUnsignedRightShift,

        IsLessThan,
        IsGreaterThan,
        IsLessThanOrEqualTo,
        IsGreaterThanOrEqualTo,
        InstanceOf,
        In,

        IsKindaEqualTo,
        IsKindaNotEqualTo,
        IsEqualTo,
        IsNotEqualTo,
        
        BitwiseAnd,
        BitwiseXor,
        BitwiseOr,

        LogicalAnd,
        LogicalOr,

        Assign,
        MultiplyAssign,
        DivideAssign,
        ModulusAssign,
        AddAssign,
        SubtractAssign,
        BitwiseLeftShiftAssign,
        BitwiseSignedRightShiftAssign,
        BitwiseUnsignedRightShiftAssign,
        BitwiseAndAssign,
        BitwiseXorAssign,
        BitwiseOrAssign
    }
    public enum Prefix
    {
        Delete,
        Void,
        TypeOf,
        Increment,
        Decrement,
        Positive,
        Negative,
        BinaryNot,
        LogicalNot
    }
    public enum Postfix
    {
        Increment,
        Decrement
    }
    public abstract class Expression
    {
        public abstract void WriteTo(Writer writer);

        public virtual bool TryGetIntValue(out int value)
        {
            value = 0;
            return false;
        }

        public virtual bool TryGetStringValue(out string value)
        {
            value = null;
            return false;
        }

        public virtual Expression Cast(PossibleValueTypes newValueType)
        {
            int v;
            bool use_v = TryGetIntValue(out v);
            switch (ValueTypes)
            {
                case PossibleValueTypes.Boolean:
                    if (use_v)
                    {
                        return (v == 0) ? False : True;
                    }
                    switch (newValueType)
                    {
                        case PossibleValueTypes.UInt8:
                        case PossibleValueTypes.Int16:
                        case PossibleValueTypes.Int32:
                        case PossibleValueTypes.Number:
                            return new BoolToIntCast(this);
                    }
                    break;
                case PossibleValueTypes.Number:
                    if (use_v)
                    {
                        return new LiteralNumber(v);
                    }
                    if (newValueType == PossibleValueTypes.Int32)
                    {
                        return new NumberToInt32Cast(this);
                    }
                    goto case PossibleValueTypes.Int32;
                case PossibleValueTypes.Int32:
                    if (use_v)
                    {
                        return new LiteralNumber(v);
                    }
                    if (newValueType == PossibleValueTypes.Int16)
                    {
                        return new NumberToInt16Cast(this);
                    }
                    goto case PossibleValueTypes.Int16;
                case PossibleValueTypes.Int16:
                    if (use_v)
                    {
                        return new LiteralNumber((short)v);
                    }
                    if (newValueType == PossibleValueTypes.UInt8)
                    {
                        return new NumberToUInt8Cast(this);
                    }
                    goto case PossibleValueTypes.UInt8;
                case PossibleValueTypes.UInt8:
                    if (use_v)
                    {
                        return new LiteralNumber(v & 0xff);
                    }
                    if (newValueType == PossibleValueTypes.Boolean)
                    {
                        return new IntToBoolCast(this);
                    }
                    break;
            }
            return this;
        }

        public virtual Expression LogicallyNegate()
        {
            return this.Cast(PossibleValueTypes.Boolean).UnOp(Prefix.LogicalNot);
        }

        public class NumberToInt32Cast : InfixOperation
        {
            public NumberToInt32Cast(Expression numberExpr)
                : base(numberExpr, Infix.BitwiseOr, (Expression)0)
            {
            }
            public override Expression Cast(PossibleValueTypes newValueType)
            {
                switch (newValueType)
                {
                    case PossibleValueTypes.Number:
                    case PossibleValueTypes.Int32:
                        return this;
                    case PossibleValueTypes.Int16:
                        return new NumberToInt16Cast(Left);
                    case PossibleValueTypes.UInt8:
                        return new NumberToUInt8Cast(Left);
                }
                return base.Cast(newValueType);
            }
            public override PossibleValueTypes ValueTypes
            {
                get { return PossibleValueTypes.Int32; }
            }
        }

        public class NumberToInt16Cast : InfixOperation
        {
            public Expression NumberExpression;
            public NumberToInt16Cast(Expression numberExpr)
                : base(numberExpr.BinOp(Infix.BitwiseLeftShift, (Expression)16),
                    Infix.BitwiseSignedRightShift,
                    (Expression)16)
            {
                NumberExpression = numberExpr;
            }
            public override Expression Cast(PossibleValueTypes newValueType)
            {
                switch (newValueType)
                {
                    case PossibleValueTypes.Number:
                    case PossibleValueTypes.Int16:
                    case PossibleValueTypes.Int32:
                        return this;
                    case PossibleValueTypes.UInt8:
                        return new NumberToUInt8Cast(NumberExpression);
                }
                return base.Cast(newValueType);
            }
            public override PossibleValueTypes ValueTypes
            {
                get { return PossibleValueTypes.Int16; }
            }
        }

        public class NumberToUInt8Cast : InfixOperation
        {
            public NumberToUInt8Cast(Expression numberExpression)
                : base(numberExpression, Infix.BitwiseAnd, (Expression)0xff)
            {
            }
            public override Expression Cast(PossibleValueTypes newValueType)
            {
                switch (newValueType)
                {
                    case PossibleValueTypes.Number:
                    case PossibleValueTypes.Int32:
                    case PossibleValueTypes.Int16:
                    case PossibleValueTypes.UInt8:
                        return this;
                }
                return base.Cast(newValueType);
            }
            public override PossibleValueTypes ValueTypes
            {
                get { return PossibleValueTypes.UInt8; }
            }
        }

        public class IntToBoolCast : InfixOperation
        {
            public IntToBoolCast(Expression intExpr)
                : base(intExpr, Infix.IsNotEqualTo, (Expression)0)
            {
            }
            public override Expression Cast(PossibleValueTypes newValueType)
            {
                switch (newValueType)
                {
                    case PossibleValueTypes.Int16:
                    case PossibleValueTypes.Int32:
                    case PossibleValueTypes.UInt8:
                        return Left;
                }
                return base.Cast(newValueType);
            }
        }

        public class BoolToIntCast : Expression.TernaryOperation
        {
            public BoolToIntCast(Expression boolExpr)
                : base(boolExpr, (Expression)1, (Expression)0)
            {
            }
            public override Expression Cast(PossibleValueTypes newValueType)
            {
                if (newValueType == PossibleValueTypes.Boolean)
                {
                    return IfThisIsTrue;
                }
                return base.Cast(newValueType);
            }
            public override Expression LogicallyNegate()
            {
                return new BoolToIntCast(IfThisIsTrue.LogicallyNegate());
            }
        }

        public virtual PossibleValueTypes ValueTypes
        {
            get { return PossibleValueTypes.Any; }
        }

        public static explicit operator Expression(double num)
        {
            return new LiteralNumber(num);
        }

        public class LiteralNumber : Expression
        {
            public double Value;
            public LiteralNumber(double value)
            {
                Value = value;
            }
            public override bool TryGetIntValue(out int value)
            {
                value = (int)Value;
                return (Value == (double)value);
            }
            public override void WriteTo(Writer writer)
            {
                writer.Write(Value.ToString());
            }
            public override Expression LogicallyNegate()
            {
                return (Value == 0.0) ? True : False;
            }
            public override PossibleValueTypes ValueTypes
            {
                get
                {
                    if (Value == (double)(byte)Value)
                    {
                        return PossibleValueTypes.UInt8;
                    }
                    if (Value == (double)(short)Value)
                    {
                        return PossibleValueTypes.Int16;
                    }
                    if (Value == (double)(int)Value)
                    {
                        return PossibleValueTypes.Int32;
                    }
                    return PossibleValueTypes.Number;
                }
            }
            public override Expression Cast(PossibleValueTypes newValueType)
            {
                switch (newValueType)
                {
                    case PossibleValueTypes.UInt8:
                        return new LiteralNumber((byte)Value);
                    case PossibleValueTypes.Int16:
                        return new LiteralNumber((short)Value);
                    case PossibleValueTypes.Int32:
                        return new LiteralNumber((int)Value);
                    case PossibleValueTypes.Boolean:
                        return (Value == 0) ? False : True;
                }
                return base.Cast(newValueType);
            }
        }

        public static explicit operator Expression(bool b)
        {
            return b ? True : False;
        }

        public static readonly Expression True = new Boolean(true);
        public static readonly Expression False = new Boolean(false);

        public class Boolean : Expression
        {
            public bool Value;
            internal Boolean(bool value)
            {
                Value = value;
            }
            public override Expression Cast(PossibleValueTypes newValueType)
            {
                switch(newValueType)
                {
                    case PossibleValueTypes.Int16:
                    case PossibleValueTypes.Int32:
                    case PossibleValueTypes.UInt8:
                    case PossibleValueTypes.Number:
                        return Value ? (Expression)1 : (Expression)0;
                }
                return this;
            }
            public override PossibleValueTypes ValueTypes
            {
                get { return PossibleValueTypes.Boolean; }
            }
            public override Expression LogicallyNegate()
            {
                return Value ? False : True;
            }
            public override void WriteTo(Writer writer)
            {
                writer.Write(Value ? "true" : "false");
            }
            public override bool TryGetIntValue(out int value)
            {
                value = Value ? 1 : 0;
                return true;
            }
        }

        public static Expression Null = new Custom("null");

        public Indexing Index(Expression index)
        {
            return new Indexing(this, index);
        }
        public Indexing Index(string key)
        {
            return new Indexing(this, new StringLiteral(key));
        }
        public Expression Index(params string[] keys)
        {
            Expression finalExpression = this;
            for (int i = 0; i < keys.Length; i++)
            {
                finalExpression = finalExpression.Index(keys[i]);
            }
            return finalExpression;
        }
        public Indexing Index(string key, PossibleValueTypes valTypes)
        {
            return new Indexing(this, new StringLiteral(key), valTypes);
        }

        public class Indexing : Expression
        {
            public Expression Target;
            public Expression IndexValue;
            private PossibleValueTypes valTypes;

            public override PossibleValueTypes ValueTypes
            {
                get { return valTypes; }
            }

            public Indexing(Expression target, Expression value)
                : this(target, value, PossibleValueTypes.Any)
            {
            }
            public Indexing(Expression target, Expression value,PossibleValueTypes valTypes)
            {
                Target = target;
                IndexValue = value;
                this.valTypes = valTypes;
            }

            public override void WriteTo(Writer writer)
            {
                Target.WriteTo(writer);
                if (IndexValue is StringLiteral)
                {
                    string index = ((StringLiteral)IndexValue).Value;
                    if (Util.IsIdentifier(index))
                    {
                        writer.Write("." + index);
                        return;
                    }
                }
                writer.Write("[");
                IndexValue.WriteTo(writer);
                writer.Write("]");
            }
        }

        public class StringLiteral : Expression
        {
            public string Value;
            public StringLiteral(string value)
            {
                Value = value;
            }
            public override bool TryGetStringValue(out string value)
            {
                value = Value;
                return true;
            }
            public override void WriteTo(Writer writer)
            {
                writer.Write(Util.EncodeString(Value, false));
            }
        }

        public class ObfuscatedStringLiteral : StringLiteral
        {
            public ObfuscatedStringLiteral(string value)
                : base(value)
            {
            }
            public override void WriteTo(Writer writer)
            {
                writer.Write("util.obfus(" + Util.EncodeString(Util.ObfuscateString(Value), false) + ")");
            }
        }

        public abstract class Operation : Expression
        {
            public abstract int NumOperands
            {
                get;
            }
            public abstract int Precedence
            {
                get;
            }
            public abstract bool RightToLeft
            {
                get;
            }
        }

        public class TernaryOperation : Operation
        {
            public Expression IfThisIsTrue, ThenThis, ElseThis;
            public TernaryOperation(Expression ifThisIsTrue, Expression thenThis, Expression elseThis)
            {
                IfThisIsTrue = ifThisIsTrue;
                ThenThis = thenThis;
                ElseThis = elseThis;
            }
            public override void WriteTo(Writer writer)
            {
                int testPrecedence = this.Precedence;
                if (RightToLeft)
                {
                    testPrecedence = testPrecedence - 1;
                }
                if (IfThisIsTrue is Operation && ((Operation)IfThisIsTrue).Precedence < testPrecedence)
                {
                    writer.Write("(");
                    IfThisIsTrue.WriteTo(writer);
                    writer.Write(")");
                }
                else
                {
                    IfThisIsTrue.WriteTo(writer);
                }
                writer.Write(" ? ");
                if (ThenThis is Operation && ((Operation)ThenThis).Precedence < testPrecedence)
                {
                    writer.Write("(");
                    ThenThis.WriteTo(writer);
                    writer.Write(")");
                }
                else
                {
                    ThenThis.WriteTo(writer);
                }
                writer.Write(" : ");
                if (ElseThis is Operation && ((Operation)ElseThis).Precedence < testPrecedence)
                {
                    writer.Write("(");
                    ElseThis.WriteTo(writer);
                    writer.Write(")");
                }
                else
                {
                    ElseThis.WriteTo(writer);
                }
            }
            public override PossibleValueTypes ValueTypes
            {
                get { return ThenThis.ValueTypes | ElseThis.ValueTypes; }
            }
            public override int NumOperands
            {
                get { return 3; }
            }
            public override int Precedence
            {
                get { return Util.PRECEDENCE_TERNARY; }
            }
            public override bool RightToLeft
            {
                get { return true; }
            }
        }

        public InfixOperation BinOp(Infix op, Expression right)
        {
            return new InfixOperation(this, op, right);
        }

        public UnaryPrefixOperation UnOp(Prefix op)
        {
            return new UnaryPrefixOperation(op, this);
        }

        public class UnaryPrefixOperation : Operation
        {
            public Prefix Operator;
            public Expression Operand;
            public UnaryPrefixOperation(Prefix prefix, Expression value)
            {
                Operator = prefix;
                Operand = value;
            }
            public override bool TryGetIntValue(out int value)
            {
                if (!Operand.TryGetIntValue(out value))
                {
                    return false;
                }
                switch (Operator)
                {
                    case Prefix.Negative:
                        value -= value;
                        return true;
                    case Prefix.BinaryNot:
                        value = (value == 0) ? 1 : 0;
                        return true;
                    case Prefix.Positive:
                        return true;
                    default:
                        return false;
                }
            }
            public override PossibleValueTypes ValueTypes
            {
                get
                {
                    switch (Operator)
                    {
                        case Prefix.BinaryNot:
                            return Operand.ValueTypes;
                        case Prefix.Decrement:
                            switch (Operand.ValueTypes)
                            {
                                case PossibleValueTypes.UInt8:
                                    return PossibleValueTypes.Int16;
                                case PossibleValueTypes.Int16:
                                    return PossibleValueTypes.Int32;
                                case PossibleValueTypes.Int32:
                                case PossibleValueTypes.Number:
                                    return PossibleValueTypes.Number;
                            }
                            return PossibleValueTypes.Any;
                        case Prefix.Delete:
                            return PossibleValueTypes.Any;
                        case Prefix.Increment:
                            switch (Operand.ValueTypes)
                            {
                                case PossibleValueTypes.UInt8:
                                    return PossibleValueTypes.Int16;
                                case PossibleValueTypes.Int16:
                                    return PossibleValueTypes.Int32;
                                case PossibleValueTypes.Int32:
                                case PossibleValueTypes.Number:
                                    return PossibleValueTypes.Number;
                            }
                            return PossibleValueTypes.Any;
                        case Prefix.LogicalNot:
                            return PossibleValueTypes.Boolean;
                        case Prefix.Negative:
                            switch (Operand.ValueTypes)
                            {
                                case PossibleValueTypes.UInt8:
                                    return PossibleValueTypes.Int16;
                                case PossibleValueTypes.Int16:
                                    return PossibleValueTypes.Int32;
                                case PossibleValueTypes.Int32:
                                case PossibleValueTypes.Number:
                                    return PossibleValueTypes.Number;
                            }
                            return PossibleValueTypes.Any;
                        case Prefix.Positive:
                            return Operand.ValueTypes;
                        case Prefix.TypeOf:
                            return PossibleValueTypes.String;
                    }
                    return PossibleValueTypes.Any;
                }
            }
            public override Expression LogicallyNegate()
            {
                switch (Operator)
                {
                    case Prefix.BinaryNot:
                        return Operand;
                    default:
                        return base.LogicallyNegate();
                }
            }
            public override void WriteTo(Writer writer)
            {
                switch (Operator)
                {
                    case Prefix.BinaryNot:
                        writer.Write("~");
                        break;
                    case Prefix.Decrement:
                        writer.Write("--");
                        break;
                    case Prefix.Delete:
                        writer.Write("delete ");
                        break;
                    case Prefix.Increment:
                        writer.Write("++");
                        break;
                    case Prefix.LogicalNot:
                        writer.Write("!");
                        break;
                    case Prefix.Negative:
                        // avoid -(-a) becoming --a etc.
                        if (Operand is UnaryPrefixOperation)
                        {
                            writer.Write("- ");
                        }
                        else
                        {
                            writer.Write("-");
                        }
                        break;
                    case Prefix.Positive:
                        // avoid +(+a) becoming ++a etc.
                        if (Operand is UnaryPrefixOperation)
                        {
                            writer.Write("+ ");
                        }
                        else
                        {
                            writer.Write("+");
                        }
                        break;
                    case Prefix.TypeOf:
                        writer.Write("typeof ");
                        break;
                    case Prefix.Void:
                        writer.Write("void ");
                        break;
                    default:
                        throw new Exception("Unknown prefix: " + Operator);
                }
                int testPrecedence  = Precedence;
                if (RightToLeft)
                {
                    testPrecedence = testPrecedence - 1;
                }
                if (Operand is Operation && ((Operation)Operand).Precedence < testPrecedence)
                {
                    writer.Write("(");
                    Operand.WriteTo(writer);
                    writer.Write(")");
                }
                else
                {
                    Operand.WriteTo(writer);
                }
            }
            public override int NumOperands
            {
                get { return 1; }
            }
            public override int Precedence
            {
                get { return Util.PRECEDENCE_PREFIX; }
            }
            public override bool RightToLeft
            {
                get { return true; }
            }
            public override Expression Cast(PossibleValueTypes newValueType)
            {
                switch (Operator)
                {
                    case Prefix.LogicalNot:
                        if (newValueType == PossibleValueTypes.Boolean)
                        {
                            return Operand.Cast(PossibleValueTypes.Boolean).LogicallyNegate();
                        }
                        break;
                }
                return base.Cast(newValueType);
            }
        }

        public class InfixOperation : Operation
        {
            public Expression Left;
            public Infix Operator;
            public Expression Right;
            public override PossibleValueTypes ValueTypes
            {
                get
                {
                    switch (Operator)
                    {
                        case Infix.Subtract:
                        case Infix.SubtractAssign:
                        case Infix.AddAssign:
                        case Infix.Add:
                            switch (Left.ValueTypes)
                            {
                                case PossibleValueTypes.UInt8:
                                    switch (Right.ValueTypes)
                                    {
                                        case PossibleValueTypes.UInt8:
                                            return PossibleValueTypes.Int16;
                                        case PossibleValueTypes.Int16:
                                            return PossibleValueTypes.Int32;
                                    }
                                    return PossibleValueTypes.Number;
                                case PossibleValueTypes.Int16:
                                    switch (Right.ValueTypes)
                                    {
                                        case PossibleValueTypes.UInt8:
                                        case PossibleValueTypes.Int16:
                                            return PossibleValueTypes.Int32;
                                    }
                                    return PossibleValueTypes.Number;
                                case PossibleValueTypes.Int32:
                                    return PossibleValueTypes.Number;
                                case PossibleValueTypes.Number:
                                    return PossibleValueTypes.Number;
                            }
                            return Left.ValueTypes | Right.ValueTypes;
                        case Infix.Assign:
                            return Right.ValueTypes;

                        case Infix.BitwiseAndAssign:
                        case Infix.BitwiseAnd:
                            return Left.ValueTypes & Right.ValueTypes & PossibleValueTypes.Int32;

                        case Infix.BitwiseLeftShift:
                        case Infix.BitwiseLeftShiftAssign:
                            return PossibleValueTypes.Int32;

                        case Infix.BitwiseOr:
                        case Infix.BitwiseOrAssign:
                            return (Left.ValueTypes | Right.ValueTypes) & PossibleValueTypes.Int32;

                        case Infix.BitwiseSignedRightShift:
                        case Infix.BitwiseSignedRightShiftAssign:
                            return Left.ValueTypes;

                        case Infix.BitwiseUnsignedRightShift:
                        case Infix.BitwiseUnsignedRightShiftAssign:
                            return Left.ValueTypes;

                        case Infix.BitwiseXor:
                        case Infix.BitwiseXorAssign:
                            return (Left.ValueTypes | Right.ValueTypes) & PossibleValueTypes.Int32;

                        case Infix.Divide:
                        case Infix.DivideAssign:
                            return PossibleValueTypes.Number;

                        case Infix.In:
                        case Infix.InstanceOf:
                        case Infix.IsEqualTo:
                        case Infix.IsGreaterThan:
                        case Infix.IsGreaterThanOrEqualTo:
                        case Infix.IsKindaEqualTo:
                        case Infix.IsLessThan:
                        case Infix.IsLessThanOrEqualTo:
                        case Infix.IsNotEqualTo:
                        case Infix.IsKindaNotEqualTo:
                            return PossibleValueTypes.Boolean;

                        case Infix.LogicalAnd:
                        case Infix.LogicalOr:
                            return Left.ValueTypes | Right.ValueTypes;

                        case Infix.Modulus:
                        case Infix.ModulusAssign:
                            return Right.ValueTypes;

                        case Infix.Multiply:
                        case Infix.MultiplyAssign:
                            switch (Left.ValueTypes)
                            {
                                case PossibleValueTypes.Number:
                                    return PossibleValueTypes.Number;
                                case PossibleValueTypes.UInt8:
                                    switch (Right.ValueTypes)
                                    {
                                        case PossibleValueTypes.UInt8:
                                            return PossibleValueTypes.Int16;
                                        case PossibleValueTypes.Int16:
                                            return PossibleValueTypes.Int32;
                                        case PossibleValueTypes.Int32:
                                        case PossibleValueTypes.Number:
                                            return PossibleValueTypes.Number;
                                    }
                                    break;
                                case PossibleValueTypes.Int16:
                                    switch (Right.ValueTypes)
                                    {
                                        case PossibleValueTypes.UInt8:
                                        case PossibleValueTypes.Int16:
                                            return PossibleValueTypes.Int32;
                                        case PossibleValueTypes.Int32:
                                        case PossibleValueTypes.Number:
                                            return PossibleValueTypes.Number;
                                    }
                                    break;
                                case PossibleValueTypes.Int32:
                                    switch (Right.ValueTypes)
                                    {
                                        case PossibleValueTypes.UInt8:
                                        case PossibleValueTypes.Int16:
                                        case PossibleValueTypes.Int32:
                                        case PossibleValueTypes.Number:
                                            return PossibleValueTypes.Number;
                                    }
                                    break;
                            }
                            break;
                    }
                    return PossibleValueTypes.Any;
                }
            }
            public override bool TryGetIntValue(out int value)
            {
                int left, right;
                if (!Left.TryGetIntValue(out left) || !Right.TryGetIntValue(out right))
                {
                    value = 0;
                    return false;
                }
                switch (Operator)
                {
                    case Infix.Add:
                        value = left + right;
                        return true;
                    case Infix.BitwiseAnd:
                        value = left & right;
                        return true;
                    case Infix.BitwiseLeftShift:
                        value = left << right;
                        return true;
                    case Infix.BitwiseOr:
                        value = left | right;
                        return true;
                    case Infix.BitwiseSignedRightShift:
                        value = left >> right;
                        return true;
                    case Infix.BitwiseUnsignedRightShift:
                        value = (int)(((uint)left) >> right);
                        return true;
                    case Infix.BitwiseXor:
                        value = left ^ right;
                        return true;
                    case Infix.Divide:
                        value = left / right;
                        return true;
                    case Infix.IsKindaEqualTo:
                    case Infix.IsEqualTo:
                        value = (left == right) ? 1 : 0;
                        return true;
                    case Infix.IsGreaterThan:
                        value = (left > right) ? 1 : 0;
                        return true;
                    case Infix.IsGreaterThanOrEqualTo:
                        value = (left >= right) ? 1 : 0;
                        return true;
                    case Infix.IsLessThan:
                        value = (left < right) ? 1 : 0;
                        return true;
                    case Infix.IsLessThanOrEqualTo:
                        value = (left <= right) ? 1 : 0;
                        return true;
                    case Infix.IsNotEqualTo:
                        value = (left != right) ? 1 : 0;
                        return true;
                    case Infix.LogicalAnd:
                        value = (left == 0) ? left : right;
                        return true;
                    case Infix.LogicalOr:
                        value = (left == 0) ? right : left;
                        return true;
                    case Infix.Modulus:
                        value = left % right;
                        return true;
                    case Infix.Multiply:
                        value = left * right;
                        return true;
                    case Infix.Subtract:
                        value = left - right;
                        return true;
                    default:
                        value = 0;
                        return false;
                }
            }
            public override bool TryGetStringValue(out string value)
            {
                string left, right;
                if (Operator == Infix.Add && Left.TryGetStringValue(out left) && Right.TryGetStringValue(out right))
                {
                    value = left + right;
                    return true;
                }
                value = null;
                return false;
            }
            public override Expression LogicallyNegate()
            {
                switch (Operator)
                {
                    case Infix.IsEqualTo:
                        return Left.BinOp(Infix.IsNotEqualTo, Right);
                    case Infix.IsNotEqualTo:
                        return Left.BinOp(Infix.IsEqualTo, Right);
                    case Infix.IsGreaterThan:
                        return Left.BinOp(Infix.IsLessThanOrEqualTo, Right);
                    case Infix.IsLessThan:
                        return Left.BinOp(Infix.IsGreaterThanOrEqualTo, Right);
                    case Infix.IsGreaterThanOrEqualTo:
                        return Left.BinOp(Infix.IsLessThan, Right);
                    case Infix.IsLessThanOrEqualTo:
                        return Left.BinOp(Infix.IsGreaterThan, Right);
                    default:
                        return base.LogicallyNegate();
                }
            }
            public InfixOperation(Expression left, Infix op, Expression right)
            {
                Left = left;
                Operator = op;
                Right = right;
            }
            public override void WriteTo(Writer writer)
            {
                int testPrecedence = this.Precedence;
                if (RightToLeft)
                {
                    testPrecedence = testPrecedence - 1;
                }
                if (Left is Operation && ((Operation)Left).Precedence < testPrecedence)
                {
                    writer.Write("(");
                    Left.WriteTo(writer);
                    writer.Write(")");
                }
                else
                {
                    Left.WriteTo(writer);
                }
                writer.Write(" " + Util.GetOperatorSymbol(Operator) + " ");
                if (Right is Operation && ((Operation)Right).Precedence < testPrecedence)
                {
                    writer.Write("(");
                    Right.WriteTo(writer);
                    writer.Write(")");
                }
                else
                {
                    Right.WriteTo(writer);
                }
            }
            public override int NumOperands
            {
                get { return 2; }
            }
            public override bool RightToLeft
            {
                get { return Util.OperatorIsRightToLeft(Operator); }
            }
            public override int Precedence
            {
                get { return Util.GetOperatorPrecedence(Operator); }
            }
            public override Expression Cast(PossibleValueTypes newValueType)
            {
                switch (Operator)
                {
                    case Infix.LogicalAnd:
                    case Infix.LogicalOr:
                        if (newValueType == PossibleValueTypes.Boolean)
                        {
                            return Left
                                .Cast(PossibleValueTypes.Boolean)
                                .BinOp(Operator, Right.Cast(PossibleValueTypes.Boolean));
                        }
                        break;
                    case Infix.Multiply:
                        if (newValueType == PossibleValueTypes.Int32)
                        {
                            Expression variableSide;
                            int constantSide;
                            if (Left.TryGetIntValue(out constantSide))
                            {
                                variableSide = Right;
                            }
                            else if (Right.TryGetIntValue(out constantSide))
                            {
                                variableSide = Left;
                            }
                            else
                            {
                                break;
                            }
                            if (constantSide <= 0 || (variableSide.ValueTypes & PossibleValueTypes.Number)!=0)
                            {
                                break;
                            }
                            int leftShift = 0;
                            while ((constantSide & 1) == 0)
                            {
                                leftShift++;
                                constantSide >>= 1;
                            }
                            if (constantSide != 1)
                            {
                                break;
                            }
                            return variableSide.BinOp(Infix.BitwiseLeftShift, (Expression)leftShift);
                        }
                        break;
                    case Infix.Divide:
                        if (newValueType == PossibleValueTypes.Int32)
                        {
                            Expression variableSide;
                            int constantSide;
                            if (Right.TryGetIntValue(out constantSide))
                            {
                                variableSide = Left;
                            }
                            else
                            {
                                break;
                            }
                            if (constantSide <= 0 || (variableSide.ValueTypes & PossibleValueTypes.Number) != 0)
                            {
                                break;
                            }
                            int rightShift = 0;
                            while ((constantSide & 1) == 0)
                            {
                                rightShift++;
                                constantSide >>= 1;
                            }
                            if (constantSide != 1)
                            {
                                break;
                            }
                            return variableSide.BinOp(Infix.BitwiseSignedRightShift, (Expression)rightShift);
                        }
                        break;
                }
                return base.Cast(newValueType);
            }
        }

        public class Custom : Expression
        {
            public string Value;
            private PossibleValueTypes valueType;
            public override PossibleValueTypes  ValueTypes
            {
	            get 
	            { 
                    return valueType;
	            }
            }
            public Custom(string value)
                : this(value, PossibleValueTypes.Any)
            {
            }
            public Custom(string value, PossibleValueTypes valueType)
            {
                Value = value;
                this.valueType = valueType;
            }
            public override void WriteTo(Writer writer)
            {
                writer.Write(Value);
            }
        }

        public class ObjectLiteral : Expression, IDictionary<string, Expression>
        {
            public readonly Dictionary<string, Expression> Entries
                = new Dictionary<string,Expression>();
            public override void WriteTo(Writer writer)
            {
                if (Entries.Count == 0)
                {
                    writer.Write("{ }");
                    return;
                }
                writer.Write("{");
                using (writer.HoldIndent())
                {
                    bool first = true;
                    foreach (KeyValuePair<string, Expression> entry in Entries)
                    {
                        if (first)
                        {
                            first = false;
                        }
                        else
                        {
                            writer.Write(",");
                        }
                        writer.WriteLineThenIndent();
                        if (Util.IsIdentifier(entry.Key))
                        {
                            writer.Write(entry.Key);
                        }
                        else
                        {
                            writer.Write(Util.EncodeString(entry.Key, false));
                        }
                        writer.Write(": ");
                        entry.Value.WriteTo(writer);
                    }
                }
                writer.WriteLineThenIndent();
                writer.Write("}");
            }

            #region Dictionary Proxy Methods

            public void Add(string key, Expression value)
            {
                Entries.Add(key, value);
            }

            public bool ContainsKey(string key)
            {
                return Entries.ContainsKey(key);
            }

            public ICollection<string> Keys
            {
                get { return Entries.Keys; }
            }

            public bool Remove(string key)
            {
                return Entries.Remove(key);
            }

            public bool TryGetValue(string key, out Expression value)
            {
                return Entries.TryGetValue(key, out value);
            }

            public ICollection<Expression> Values
            {
                get { return Entries.Values; }
            }

            public Expression this[string key]
            {
                get { return Entries[key]; }
                set { Entries[key] = value; }
            }

            void ICollection<KeyValuePair<string, Expression>>.Add(KeyValuePair<string, Expression> item)
            {
                ((ICollection<KeyValuePair<string, Expression>>)Entries).Add(item);
            }

            void ICollection<KeyValuePair<string, Expression>>.Clear()
            {
                ((ICollection<KeyValuePair<string, Expression>>)Entries).Clear();
            }

            bool ICollection<KeyValuePair<string, Expression>>.Contains(KeyValuePair<string, Expression> item)
            {
                return ((ICollection<KeyValuePair<string, Expression>>)Entries).Contains(item);
            }

            void ICollection<KeyValuePair<string, Expression>>.CopyTo(KeyValuePair<string, Expression>[] array, int arrayIndex)
            {
                ((ICollection<KeyValuePair<string, Expression>>)Entries).CopyTo(array, arrayIndex);
            }

            int ICollection<KeyValuePair<string, Expression>>.Count
            {
                get { return ((ICollection<KeyValuePair<string, Expression>>)Entries).Count; }
            }

            bool ICollection<KeyValuePair<string, Expression>>.IsReadOnly
            {
                get { return ((ICollection<KeyValuePair<string, Expression>>)Entries).IsReadOnly; }
            }

            bool ICollection<KeyValuePair<string, Expression>>.Remove(KeyValuePair<string, Expression> item)
            {
                return ((ICollection<KeyValuePair<string, Expression>>)Entries).Remove(item);
            }

            IEnumerator<KeyValuePair<string, Expression>> IEnumerable<KeyValuePair<string, Expression>>.GetEnumerator()
            {
                return Entries.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)Entries).GetEnumerator();
            }

            #endregion
        }

        public class ArrayLiteral : Expression, IList<Expression>
        {
            public readonly List<Expression> Entries
                 = new List<Expression>();
            public override void WriteTo(Writer writer)
            {
                writer.Write("[");
                for (int i = 0; i < Entries.Count; i++)
                {
                    if (i > 0) writer.Write(", ");
                    Entries[i].WriteTo(writer);
                }
                writer.Write("]");
            }

            #region List Proxy Methods

            public int IndexOf(Expression item)
            {
                return Entries.IndexOf(item);
            }

            public void Insert(int index, Expression item)
            {
                Entries.Insert(index, item);
            }

            public void RemoveAt(int index)
            {
                Entries.RemoveAt(index);
            }

            public Expression this[int index]
            {
                get { return Entries[index]; }
                set { Entries[index] = value; }
            }

            public void Add(Expression item)
            {
                Entries.Add(item);
            }

            public void Clear()
            {
                Entries.Clear();
            }

            public bool Contains(Expression item)
            {
                return Entries.Contains(item);
            }

            public void CopyTo(Expression[] array, int arrayIndex)
            {
                Entries.CopyTo(array, arrayIndex);
            }

            public int Count
            {
                get { return Entries.Count; }
            }

            bool ICollection<Expression>.IsReadOnly
            {
                get { return ((ICollection<Expression>)Entries).IsReadOnly; }
            }

            public bool Remove(Expression item)
            {
                return Entries.Remove(item);
            }

            public IEnumerator<Expression> GetEnumerator()
            {
                return Entries.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)Entries).GetEnumerator();
            }

            #endregion
        }

        public InfixOperation Assign(Expression value)
        {
            return this.BinOp(Infix.Assign, value);
        }

        public class New : Calling
        {
            public New(Expression constructor)
                : base(constructor, PossibleValueTypes.Any) 
            {
            }
            public override void WriteTo(Writer writer)
            {
                writer.Write("new ");
                base.WriteTo(writer);
            }
        }

        protected virtual PossibleValueTypes GetReturnTypes(IEnumerable<Expression> parameters)
        {
            return PossibleValueTypes.Any;
        }

        public Expression Call(params Expression[] parameters)
        {
            return Call((IEnumerable<Expression>)parameters);
        }
        public virtual Expression Call(IEnumerable<Expression> parameters)
        {
            PossibleValueTypes returnTypes = GetReturnTypes(parameters);
            Calling call = new Calling(this, returnTypes);
            call.Parameters.AddRange(parameters);
            return call;
        }

        public virtual Expression CallMethod(string methodName, List<Expression> parameters)
        {
            Calling call = new Calling(this.Index(methodName), PossibleValueTypes.Any);
            call.Parameters.AddRange(parameters);
            return call;
        }

        public class Calling : Expression
        {
            public Expression CallingOn;
            public List<Expression> Parameters = new List<Expression>();
            public PossibleValueTypes ReturnValueType;
            public override PossibleValueTypes ValueTypes
            {
                get { return ReturnValueType; }
            }
            public Calling(Expression callingOn, PossibleValueTypes returnValueType)
            {
                CallingOn = callingOn;
                this.ReturnValueType = returnValueType;
            }
            public override void WriteTo(Writer writer)
            {
                if (CallingOn is FunctionDefinition)
                {
                    writer.Write("(");
                    CallingOn.WriteTo(writer);
                    writer.Write(")");
                }
                else
                {
                    CallingOn.WriteTo(writer);
                }
                writer.Write("(");
                for (int i = 0; i < Parameters.Count; i++)
                {
                    if (i > 0) writer.Write(", ");
                    Parameters[i].WriteTo(writer);
                }
                writer.Write(")");
            }
        }
    }

    public class FunctionDefinition : Expression
    {
        public List<Variable> Parameters = new List<Variable>();
        public Variable This = new Variable("this");
        public Variable Arguments = new Variable("arguments");
        public Variable NewParam(string name)
        {
            Variable paramVariable = new Variable(name);
            Parameters.Add(paramVariable);
            return paramVariable;
        }
        public Variable NewVar(string name)
        {
            return NewVar(name, PossibleValueTypes.Any);
        }
        public Variable NewVar(string name, PossibleValueTypes valueType)
        {
            Variable variable = new Variable(name, valueType);
            Body.Variables.Add(name, variable);
            return variable;
        }
        public ScopedBlock Body = new ScopedBlock();
        public override void WriteTo(Writer writer)
        {
            writer.Write("function(");
            for (int i = 0; i < Parameters.Count; i++)
            {
                if (i > 0) writer.Write(", ");
                Parameters[i].WriteTo(writer);
            }
            writer.Write(") ");
            Body.WriteTo(writer);
        }
    }

}
