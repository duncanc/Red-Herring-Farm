using System;
using System.Collections.Generic;
using System.Text;

namespace RedHerringFarm.JavaScriptGeneration
{
    public class Variable : Expression
    {
        public string Name;
        public PossibleValueTypes VariableType;
        public override PossibleValueTypes ValueTypes
        {
            get { return VariableType; }
        }
        public Variable(string name)
            : this(name, PossibleValueTypes.Any)
        {
        }
        public Variable(string name, PossibleValueTypes valueTypes)
        {
            Name = name;
            this.VariableType = valueTypes;
        }
        public override void WriteTo(Writer writer)
        {
            writer.Write(Name);
        }
        public Expression InitialValue;
    }
}
