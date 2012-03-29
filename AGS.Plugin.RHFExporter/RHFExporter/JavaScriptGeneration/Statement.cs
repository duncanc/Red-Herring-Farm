using System;
using System.Collections.Generic;
using System.Text;

namespace RedHerringFarm.JavaScriptGeneration
{
    public abstract class Statement
    {
        public abstract void WriteTo(Writer writer);

        public static explicit operator Statement(Expression expr)
        {
            return new ExpressionStatement(expr);
        }

        public class ExpressionStatement : Statement
        {
            public Expression Expression;
            public ExpressionStatement(Expression expression)
            {
                Expression = expression;
            }
            public override void WriteTo(Writer writer)
            {
                Expression.WriteTo(writer);
            }
        }

        public virtual bool RequireSemicolon
        {
            get { return true; }
        }

        public class GenericBlock : Statement
        {
            public Block Block = new Block();
            public override bool RequireSemicolon
            {
                get { return false; }
            }
            public override void WriteTo(Writer writer)
            {
                Block.WriteTo(writer);
            }
        }

        public class While : Statement
        {
            public Expression WhileThisIsTrue;
            public Block KeepDoingThis = new Block();
            public While(Expression whileThisIsTrue)
            {
                WhileThisIsTrue = whileThisIsTrue;
            }
            public override bool RequireSemicolon
            {
                get { return false; }
            }
            public override void WriteTo(Writer writer)
            {
                writer.Write("while (");
                WhileThisIsTrue.WriteTo(writer);
                writer.Write(") ");
                KeepDoingThis.WriteTo(writer);
            }
        }

        public class If : Statement
        {
            public Expression Condition;
            public Block ThenDoThis = new Block();
            public Block ElseDoThis = new Block();

            public If(Expression condition)
            {
                Condition = condition;
            }
            public override bool RequireSemicolon
            {
                get { return false; }
            }
            public override void WriteTo(Writer writer)
            {
                writer.Write("if (");
                Condition.WriteTo(writer);
                writer.Write(") ");
                ThenDoThis.WriteTo(writer);
                if (ElseDoThis.Count != 0)
                {
                    writer.WriteLineThenIndent();
                    writer.Write("else ");
                    if (ElseDoThis.Count == 1 && ElseDoThis[0] is If)
                    {
                        ElseDoThis[0].WriteTo(writer);
                    }
                    else
                    {
                        ElseDoThis.WriteTo(writer);
                    }
                }
            }
        }

        public class InitVariables : Statement
        {
            public List<Expression> Assignments = new List<Expression>();
            public InitVariables(Expression target, Expression value)
            {
                Add(target, value);
            }
            public InitVariables()
            {
            }
            public void Add(Expression target, Expression value)
            {
                Assignments.Add(target.BinOp(Infix.Assign, value));
            }
            public override bool RequireSemicolon
            {
                get
                {
                    return Assignments.Count > 0;
                }
            }
            public override void WriteTo(Writer writer)
            {
                for (int i = 0; i < Assignments.Count; i++)
                {
                    if (i > 0)
                    {
                        writer.Write(";");
                        writer.WriteLineThenIndent();
                    }
                    Assignments[i].WriteTo(writer);
                }
            }
        }

        public class Return : Statement
        {
            public Expression Value;
            public Return()
            {
            }
            public Return(Expression value)
            {
                Value = value;
            }
            public override void WriteTo(Writer writer)
            {
                if (Value == null)
                {
                    writer.Write("return");
                }
                else
                {
                    writer.Write("return ");
                    Value.WriteTo(writer);
                }
            }
        }
    }
}
