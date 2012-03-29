using System;
using System.Collections.Generic;
using System.Text;

namespace RedHerringFarm.JavaScriptGeneration
{
    public class Script : ScopedBlock
    {
        public Dictionary<string, Expression> ExternalGlobals = new Dictionary<string,Expression>();
        public Expression GetExternalGlobal(string name)
        {
            Expression externalGlobal;
            if (!ExternalGlobals.TryGetValue(name, out externalGlobal))
            {
                externalGlobal = new Expression.Custom(name);
                ExternalGlobals.Add(name, externalGlobal);
            }
            return externalGlobal;
        }
        public override void WriteTo(Writer writer)
        {
            WriteBlockContents(writer);
            writer.WriteLineThenIndent();
        }
    }
}
