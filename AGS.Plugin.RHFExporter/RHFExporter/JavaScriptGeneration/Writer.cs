using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace RedHerringFarm.JavaScriptGeneration
{
    public class Writer : IDisposable
    {
        private TextWriter writer;
        private bool ownWriter;
        public void Write(string text)
        {
            writer.Write(text);
        }
        public void WriteLine(string line)
        {
            writer.WriteLine(line);
        }
        public void WriteLine()
        {
            writer.WriteLine();
        }
        private int indent;
        public void WriteLineThenIndent()
        {
            writer.WriteLine();
            for (int i = 0; i < indent; i++)
            {
                writer.Write("  ");
            }
        }
        private Stack<ScopeContext> scopeStack = new Stack<ScopeContext>();
        public ScopeContext HoldScopeContext(ScopedBlock block)
        {
            return new ScopeContext(this, block);
        }
        public class ScopeContext : IDisposable
        {
            private Writer writer;
            public readonly ScopedBlock Block;
            public ScopeContext(Writer writer, ScopedBlock block)
            {
                this.writer = writer;
                Block = block;
                writer.scopeStack.Push(this);
            }
            public void Dispose()
            {
                if (writer.scopeStack.Count == 0 || writer.scopeStack.Peek() != this)
                {
                    throw new Exception("Scope context error");
                }
                writer.scopeStack.Pop();
            }
        }
        public IndentationBlock HoldIndent()
        {
            return new IndentationBlock(this);
        }
        public class IndentationBlock : IDisposable
        {
            Writer writer;
            internal IndentationBlock(Writer writer)
            {
                this.writer = writer;
                writer.indent++;
            }
            public void Dispose()
            {
                writer.indent--;
            }
        }
        private Writer(TextWriter writer, bool ownWriter)
        {
            this.writer = writer;
            this.ownWriter = ownWriter;
        }
        public void Dispose()
        {
            if (ownWriter)
            {
                writer.Dispose();
            }
        }
        public static Writer Create(string path)
        {
            return new Writer(new StreamWriter(path), true);
        }
    }
}
