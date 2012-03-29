using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using JS = RedHerringFarm.JavaScriptGeneration;

namespace RedHerringFarm
{
    public class JsonWriterSettings
    {
        public bool NiceFormatting = true;
    }
    public class JsonWriter : IDisposable
    {
        TextWriter output;
        bool ownOutput;
        internal JsonWriterSettings settings;
        internal int indent = 0;

        internal void WriteIndent()
        {
            if (settings.NiceFormatting)
            {
                output.WriteLine();
                for (int i = 0; i < indent; i++)
                {
                    output.Write("  ");
                }
            }
        }

        public bool ObfuscateKeys = false;
        public bool ObfuscateValues = false;

        internal JsonWriter(TextWriter output, bool ownOutput, JsonWriterSettings settings)
        {
            if (output == null)
            {
                throw new ArgumentNullException("output");
            }
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }
            this.output = output;
            this.ownOutput = ownOutput;
            this.settings = settings;
        }
        public static JsonWriter Create(Stream stream)
        {
            return new JsonWriter(new StreamWriter(stream), true, new JsonWriterSettings());
        }
        public static JsonWriter Create(String path)
        {
            return new JsonWriter(new StreamWriter(path), true, new JsonWriterSettings());
        }
        public static JsonWriter Create(String path, JsonWriterSettings settings)
        {
            return new JsonWriter(new StreamWriter(path), true, settings);
        }
        public static JsonWriter Create(TextWriter output)
        {
            return new JsonWriter(output, false, new JsonWriterSettings());
        }

        private Stack<JsonContainer> containerStack = new Stack<JsonContainer>();

        private void BeginContainer(JsonContainer container)
        {
            Advance();
            container.WriteStart(output);
            containerStack.Push(container);
        }

        public JsonContainer.Array BeginArray(string key)
        {
            ObjectKey(key);
            JsonContainer.Array arr = new JsonContainer.Array(this);
            BeginContainer(arr);
            return arr;
        }

        public JsonContainer.Object BeginObject(string key)
        {
            ObjectKey(key);
            JsonContainer.Object obj = new JsonContainer.Object(this);
            BeginContainer(obj);
            return obj;
        }

        public JsonContainer.Object BeginObject() { return BeginObject(null); }
        public JsonContainer.Array BeginArray() { return BeginArray(null); }

        public void End(JsonContainer container)
        {
            if (containerStack.Count == 0 || containerStack.Peek() != container)
            {
                throw new Exception("Malformed JSON: Attempt to close container before closing a nested container");
            }
            End();
        }

        public void End()
        {
            if (containerStack.Count == 0)
            {
                throw new MalformedJsonException("Malformed JSON: Not in an array");
            }
            containerStack.Pop().WriteEnd(output);
        }

        bool writtenRootValue = false;

        private void Advance()
        {
            if (containerStack.Count == 0)
            {
                if (writtenRootValue)
                {
                    throw new Exception("Malformed JSON: Multiple root values");
                }
                else
                {
                    writtenRootValue = true;
                }
            }
            else
            {
                containerStack.Peek().WriteBeginEntry(output);
            }
        }

        public JsonWriter ObjectKey(string key)
        {
            if (containerStack.Count == 0 || !(containerStack.Peek() is JsonContainer.Object))
            {
                if (key == null)
                {
                    return this;
                }
                throw new MalformedJsonException("Malformed JSON: Not in an object");
            }
            JsonContainer.Object jsonObject = (JsonContainer.Object)containerStack.Peek();
            jsonObject.Key = key;
            return this;
        }

        public void WriteValue(string key, string str) { ObjectKey(key); WriteValue(str); }
        public void WriteValue(string key, int v) { ObjectKey(key); WriteValue(v); }
        public void WriteValue(string key, int? v) { ObjectKey(key); WriteValue(v); }
        public void WriteValue(string key, bool b) { ObjectKey(key); WriteValue(b); }
        public void WriteValue(string key, double v) { ObjectKey(key); WriteValue(v); }

        public void WriteValue(string str)
        {
            Advance();
            if (str == null)
            {
                output.Write("null");
            }
            else
            {
                if (ObfuscateValues)
                {
                    output.Write(JS.Util.EncodeString(JS.Util.ObfuscateString(str), true));
                }
                else
                {
                    output.Write(JS.Util.EncodeString(str, true));
                }
            }
        }

        public void WriteValue(bool b)
        {
            Advance();
            output.Write(b ? "true" : "false");
        }

        public void WriteValue(int i)
        {
            Advance();
            output.Write(i);
        }

        public void WriteValue(int? i)
        {
            Advance();
            if (i == null)
            {
                output.Write("null");
            }
            else
            {
                output.Write((int)i);
            }
        }

        public void WriteValue(float v)
        {
            Advance();
            output.Write(v);
        }

        public void WriteValue(double v)
        {
            Advance();
            output.Write(v);
        }

        public void WriteValue(uint v)
        {
            Advance();
            output.Write(v);
        }

        public void WriteValue(short v)
        {
            Advance();
            output.Write(v);
        }

        public void WriteValue(ushort v)
        {
            Advance();
            output.Write(v);
        }

        public void WriteValue(byte v)
        {
            Advance();
            output.Write(v);
        }

        public void WriteValue(sbyte v)
        {
            Advance();
            output.Write(v);
        }

        public void WriteNull()
        {
            Advance();
            output.Write("null");
        }

        public void WriteNull(string key) { ObjectKey(key); WriteNull(); }

        public void Dispose()
        {
            if (containerStack.Count > 0)
            {
                throw new MalformedJsonException("Malformed JSON: Unterminated object/array");
            }
            if (ownOutput)
            {
                output.Dispose();
            }
        }
    }
    public class MalformedJsonException : Exception
    {
        public MalformedJsonException(string message)
            : base(message)
        {
        }
    }
    public abstract class JsonContainer : IDisposable
    {
        internal JsonContainer(JsonWriter output)
        {
            this.j_output = output;
        }
        internal JsonWriter j_output;
        public void Dispose()
        {
            j_output.End(this);
        }
        protected bool firstValue = true;
        internal abstract void WriteStart(TextWriter output);
        internal abstract void WriteBeginEntry(TextWriter output);
        internal abstract void WriteEnd(TextWriter output);
        public sealed class Array : JsonContainer
        {
            internal Array(JsonWriter output)
                : base(output)
            {
            }
            internal override void WriteStart(TextWriter t_output)
            {
                t_output.Write('[');
                j_output.indent++;
            }
            internal override void WriteBeginEntry(TextWriter output)
            {
                if (firstValue)
                {
                    firstValue = false;
                }
                else
                {
                    output.Write(",");
                }
                j_output.WriteIndent();
            }
            internal override void WriteEnd(TextWriter output)
            {
                j_output.indent--;
                if (!firstValue)
                {
                    j_output.WriteIndent();
                }
                output.Write(']');
            }
        }
        public sealed class Object : JsonContainer
        {
            internal Object(JsonWriter output)
                : base(output)
            {
            }
            internal override void WriteStart(TextWriter output)
            {
                output.Write('{');
                j_output.indent++;
            }
            private string key;
            public string Key
            {
                get { return key; }
                set { key = value; }
            }
            private Dictionary<string, bool> usedKeys = new Dictionary<string, bool>();
            internal override void WriteBeginEntry(TextWriter output)
            {
                if (key == null)
                {
                    throw new MalformedJsonException("Malformed JSON: No Object Key specified");
                }
                if (usedKeys.ContainsKey(key))
                {
                    throw new MalformedJsonException("Malformed JSON: Duplicate Object Key " + JS.Util.EncodeString(key, false));
                }
                usedKeys.Add(key, true);
                if (firstValue)
                {
                    firstValue = false;
                }
                else
                {
                    output.Write(",");
                }
                j_output.WriteIndent();
                if (j_output.ObfuscateKeys)
                {
                    output.Write(JS.Util.EncodeString(JS.Util.ObfuscateString(key), true));
                }
                else
                {
                    output.Write(JS.Util.EncodeString(key, true));
                }
                if (j_output.settings.NiceFormatting)
                {
                    output.Write(": ");
                }
                else
                {
                    output.Write(":");
                }
                key = null;
            }
            internal override void WriteEnd(TextWriter output)
            {
                j_output.indent--;
                if (!firstValue)
                {
                    j_output.WriteIndent();
                }
                output.Write('}');
            }
        }
    }
}
