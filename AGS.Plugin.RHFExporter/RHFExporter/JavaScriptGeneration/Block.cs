using System;
using System.Collections.Generic;
using System.Text;

namespace RedHerringFarm.JavaScriptGeneration
{
    public class Block : List<Statement>
    {
        public void Add(Expression expr)
        {
            this.Add((Statement)expr);
        }
        protected virtual void WriteBlockContents(Writer writer)
        {
            bool first = true;
            foreach (Statement statement in this)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    writer.WriteLineThenIndent();
                }
                statement.WriteTo(writer);
                if (statement.RequireSemicolon)
                {
                    writer.Write(";");
                }
            }
        }
        public virtual void WriteTo(Writer writer)
        {
            if (this.Count == 0)
            {
                writer.Write("{ }");
                return;
            }
            writer.Write("{");
            using (writer.HoldIndent())
            {
                writer.WriteLineThenIndent();
                WriteBlockContents(writer);
            }
            writer.WriteLineThenIndent();
            writer.Write("}");
        }
    }
}
