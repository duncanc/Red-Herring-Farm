using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace RedHerringFarm.JavaScriptGeneration
{
    public static class Util
    {
        private static Regex JsonStringEscapables = new Regex(@"[\x00-\x1F\x7F""/\\]", RegexOptions.Compiled);

        private static string JsonStringEscape(Match match)
        {
            char c;
            switch (c = match.Value[0])
            {
                case '\r': return "\\r";
                case '\n': return "\\n";
                case '\t': return "\\t";
                case '\\':
                case '"':
                case '/': return "\\" + c;
                default: return String.Format("\\u{0:X4}", (uint)c);
            }
        }

        private static string JavaScriptStringEscape(Match match)
        {
            char c;
            switch (c = match.Value[0])
            {
                case '\r': return "\\r";
                case '\n': return "\\n";
                case '\t': return "\\t";
                case '\\':
                case '"': return "\\" + c;
                case '/': return "" + c;
                default:
                    if (c < 256)
                    {
                        return String.Format("\\x{0:X2}", (uint)c);
                    }
                    return String.Format("\\u{0:X4}", (uint)c);
            }
        }

        public static string ObfuscateString(string str)
        {
            if (str == null) return null;
            char[] chars = new char[str.Length];
            for (int i = 0; i < str.Length; i++)
            {
                chars[i] = (char)(str[i] ^ ((i % 15) + 1));
            }
            return new String(chars);
        }

        public static string EncodeString(string str, bool json)
        {
            if (json)
            {
                return String.Format("\"{0}\"", JsonStringEscapables.Replace(str, JsonStringEscape));
            }
            else
            {
                return String.Format("\"{0}\"", JsonStringEscapables.Replace(str, JavaScriptStringEscape));
            }
        }

        private static Dictionary<string,bool> ReservedWords;
        private static string reservedWords = @"
            break else new var case finally return void catch for switch while continue
            function this with default if throw delete in try do instanceof typeof
        ";
        private static string futureReservedWords = @"
            abstract enum int short boolean export interface static byte extends long
            super char final native synchronized class float package throws const goto
            private transient debugger implements protected volatile double import public
        ";
        public static bool IsReservedWord(string word)
        {
            return ReservedWords.ContainsKey(word);
        }
        public static IEnumerable<string> GetReservedWords()
        {
            return ReservedWords.Keys;
        }
        public static bool IsIdentifier(string str)
        {
            return (str != null)
                && Regex.IsMatch(str, @"^[_\$a-zA-Z][_\$a-zA-Z0-9]*$")
                && !IsReservedWord(str);
        }
        static Util()
        {
            ReservedWords = new Dictionary<string,bool>();
            foreach (Match match in Regex.Matches(reservedWords, @"\S+"))
            {
                ReservedWords[match.Value] = true;
            }
            foreach (Match match in Regex.Matches(futureReservedWords, @"\S+"))
            {
                ReservedWords[match.Value] = true;
            }
        }

        public const int PRECEDENCE_FUNCTION_CALL = 150;
        public const int PRECEDENCE_POSTFIX = 150;
        public const int PRECEDENCE_PREFIX = 130;
        public const int PRECEDENCE_MULTIPLY_DIVIDE_MODULUS = 120;
        public const int PRECEDENCE_ADD_SUBTRACT = 110;
        public const int PRECEDENCE_BITWISE_SHIFT = 100;
        public const int PRECEDENCE_LESS_GREATER = 90;
        public const int PRECEDENCE_EQUAL_NOT_EQUAL = 80;
        public const int PRECEDENCE_BITWISE_AND = 70;
        public const int PRECEDENCE_BITWISE_XOR = 60;
        public const int PRECEDENCE_BITWISE_OR = 50;
        public const int PRECEDENCE_LOGICAL_AND = 40;
        public const int PRECEDENCE_LOGICAL_OR = 30;
        public const int PRECEDENCE_TERNARY = 20;
        public const int PRECEDENCE_ASSIGN_MUTATE = 10;
        public const int PRECEDENCE_COMMA = 5;

        public static bool OperatorIsRightToLeft(Infix infix)
        {
            switch (infix)
            {
                case Infix.Assign:
                case Infix.AddAssign:
                case Infix.SubtractAssign:
                case Infix.MultiplyAssign:
                case Infix.ModulusAssign:
                case Infix.BitwiseAndAssign:
                case Infix.BitwiseLeftShiftAssign:
                case Infix.BitwiseSignedRightShiftAssign:
                case Infix.BitwiseOrAssign:
                case Infix.BitwiseUnsignedRightShiftAssign:
                case Infix.BitwiseXorAssign:
                case Infix.DivideAssign:
                    return true;
                default:
                    return false;
            }
        }

        public static int GetOperatorPrecedence(Infix infix)
        {
            switch (infix)
            {
                case Infix.Multiply:
                case Infix.Divide:
                case Infix.Modulus:
                    return Util.PRECEDENCE_MULTIPLY_DIVIDE_MODULUS;

                case Infix.Add:
                case Infix.Subtract:
                    return Util.PRECEDENCE_ADD_SUBTRACT;

                case Infix.BitwiseLeftShift:
                case Infix.BitwiseSignedRightShift:
                case Infix.BitwiseUnsignedRightShift:
                    return Util.PRECEDENCE_BITWISE_SHIFT;

                case Infix.IsLessThan:
                case Infix.IsLessThanOrEqualTo:
                case Infix.IsGreaterThan:
                case Infix.IsGreaterThanOrEqualTo:
                    return Util.PRECEDENCE_LESS_GREATER;

                case Infix.IsEqualTo:
                case Infix.IsKindaEqualTo:
                case Infix.IsNotEqualTo:
                case Infix.IsKindaNotEqualTo:
                    return Util.PRECEDENCE_EQUAL_NOT_EQUAL;

                case Infix.BitwiseAnd:
                    return Util.PRECEDENCE_BITWISE_AND;

                case Infix.BitwiseXor:
                    return Util.PRECEDENCE_BITWISE_XOR;

                case Infix.BitwiseOr:
                    return Util.PRECEDENCE_BITWISE_OR;

                case Infix.LogicalAnd:
                    return Util.PRECEDENCE_LOGICAL_AND;

                case Infix.LogicalOr:
                    return Util.PRECEDENCE_LOGICAL_OR;

                case Infix.Assign:
                case Infix.AddAssign:
                case Infix.SubtractAssign:
                case Infix.MultiplyAssign:
                case Infix.DivideAssign:
                case Infix.ModulusAssign:
                case Infix.BitwiseXorAssign:
                case Infix.BitwiseAndAssign:
                case Infix.BitwiseOrAssign:
                case Infix.BitwiseSignedRightShiftAssign:
                case Infix.BitwiseUnsignedRightShiftAssign:
                case Infix.BitwiseLeftShiftAssign:
                    return Util.PRECEDENCE_ASSIGN_MUTATE;

                default:
                    throw new Exception("Unknown binop: " + infix);
            }
        }

        public static string GetOperatorSymbol(Infix infix)
        {
            switch (infix)
            {
                case Infix.Add:
                    return "+";
                case Infix.Assign:
                    return "=";
                case Infix.BitwiseAnd:
                    return "&";
                case Infix.BitwiseOr:
                    return "|";
                case Infix.BitwiseXor:
                    return "^";
                case Infix.BitwiseAndAssign:
                    return "&=";
                case Infix.BitwiseSignedRightShiftAssign:
                    return ">>=";
                case Infix.BitwiseLeftShift:
                    return "<<";
                case Infix.BitwiseLeftShiftAssign:
                    return "<<=";
                case Infix.BitwiseUnsignedRightShiftAssign:
                    return ">>>=";
                case Infix.BitwiseOrAssign:
                    return "|=";
                case Infix.BitwiseSignedRightShift:
                    return ">>";
                case Infix.BitwiseUnsignedRightShift:
                    return ">>>";
                case Infix.BitwiseXorAssign:
                    return "^=";
                case Infix.Divide:
                    return "/";
                case Infix.DivideAssign:
                    return "/=";
                case Infix.In:
                    return "in";
                case Infix.InstanceOf:
                    return "instanceof";
                case Infix.IsEqualTo:
                    return "===";
                case Infix.IsGreaterThan:
                    return ">";
                case Infix.IsGreaterThanOrEqualTo:
                    return ">=";
                case Infix.IsKindaEqualTo:
                    return "==";
                case Infix.IsLessThan:
                    return "<";
                case Infix.IsLessThanOrEqualTo:
                    return "<=";
                case Infix.IsNotEqualTo:
                    return "!==";
                case Infix.IsKindaNotEqualTo:
                    return "!=";
                case Infix.LogicalAnd:
                    return "&&";
                case Infix.LogicalOr:
                    return "||";
                case Infix.Modulus:
                    return "%";
                case Infix.ModulusAssign:
                    return "%=";
                case Infix.Multiply:
                    return "*";
                case Infix.MultiplyAssign:
                    return "*=";
                case Infix.AddAssign:
                    return "+=";
                case Infix.Subtract:
                    return "-";
                case Infix.SubtractAssign:
                    return "-=";
                default:
                    throw new Exception("Unknown operator: " + infix);
            }
        }

        public static string RegexEscape(String str)
        {
            return Regex.Replace(str, @"([\[\]\\\*\+\.\?\(\)\^\$\{\}])", @"\$1");
        }

        public static string EscapedRegexLiteral(string str, string flags)
        {
            return "/" + RegexEscape(str).Replace(@"/", @"\/") + "/" + flags;
        }

    }
}
