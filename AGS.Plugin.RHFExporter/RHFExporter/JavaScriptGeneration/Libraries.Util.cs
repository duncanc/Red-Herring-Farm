using System;
using System.Collections.Generic;
using System.Text;

namespace RedHerringFarm.JavaScriptGeneration
{
    public static class OtherLibraries
    {
        public static readonly UtilLibrary Util = new UtilLibrary();
    }
    public class UtilLibrary : Expression.Custom
    {
        public UtilLibrary()
            : base("util")
        {
            imul = new LibraryFunction(this, "imul", PossibleValueTypes.Int32);
            fillArray = new LibraryFunction(this, "fillArray", PossibleValueTypes.Any);
            structArray = new LibraryFunction(this, "structArray", PossibleValueTypes.Any);
        }
        public LibraryFunction imul;
        public LibraryFunction fillArray;
        public LibraryFunction structArray;
    }
}
