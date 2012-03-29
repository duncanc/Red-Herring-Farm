using System;
using System.Collections.Generic;
using System.Text;

namespace RedHerringFarm.JavaScriptGeneration
{
    public class LibraryFunction : Expression.Indexing
    {
        public LibraryFunction(Expression library, string name, PossibleValueTypes returnVT)
            : base(library, new StringLiteral(name))
        {
            ReturnValueType = returnVT;
        }
        PossibleValueTypes ReturnValueType;
        public TransformCall CallTransform;
        protected override PossibleValueTypes GetReturnTypes(IEnumerable<Expression> parameters)
        {
            return ReturnValueType;
        }
        public override Expression Call(IEnumerable<Expression> parameters)
        {
            Expression.Calling call = new Calling(this, GetReturnTypes(parameters));
            call.Parameters.AddRange(parameters);
            if (CallTransform != null)
            {
                return CallTransform(call);
            }
            return call;
        }
    }
    public class LibraryConstant : Expression.Indexing
    {
        public LibraryConstant(Expression library, string name, PossibleValueTypes valueType)
            : base(library, new Expression.StringLiteral(name), valueType)
        {
        }
    }

}