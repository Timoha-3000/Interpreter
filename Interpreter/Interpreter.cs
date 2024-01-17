using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    public static class Interpreter
    {
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();
        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();
        public static List<string> ReservedWords => new List<string>()
        {
            "or", "and", "+", "-", "*", "/", "<", ">", "==", "!=", "=", "print", "scan", "for", "if", "to", "else", "{", "}"
        };

        private static Dictionary<string, string> valuesTable = new Dictionary<string, string>();

        public static void Program(List<string> code, ref int id) { Statement(code, ref id); }

        public static string Statement(List<string> code, ref int id, bool isLoop = false)
        {
            switch (code[id])
            {
                case "print":
                    id++;
                    Print(code, ref id);
                    break;
                case "scan":
                    id++;
                    Scan(code, ref id);
                    break;
                case "for":
                    id++;
                    For(code, ref id);
                    break;
                case "if":                    
                    id++;
                    If(code, ref id);
                    break;
                case "else":
                    Else(code, ref id);
                    break;
                default:
                    if (IsIdentifier(code[id]))
                    {
                        Assign(code, ref id);
                    }
                    break;
            }

            if (id < code.Count && code[id] == "}")
            return null;

            if (id < code.Count - 1)
                Statement(code, ref id);

            return null;
        }

        public static string Print(List<string> code, ref int id)
        {
            Console.WriteLine(PrintEnd(code, ref id));
            id++;

            return null;
        }

        public static string Else(List<string> code, ref int id)
        {
            id = id + 1; // scip {
            if (id < code.Count && code[id] != "{")
                throw new Exception($"Not know operation {code[id - 1]} {code[id]}");

            id++;
            if (id < code.Count)
                Statement(code, ref id, true);

            if (id < code.Count && code[id] != "}")
                throw new Exception($"Not know operation {code[id - 1]} {code[id]}");

            return null;
        }

        public static string If(List<string> code, ref int id)
        {
            //var excpression = BoolExcpression(code, ref id);
            var hardExcpression = HardBoolExpression(code, ref id);
            if (bool.Parse(hardExcpression))
            {
                id = id + 1; // scip {
                if (id < code.Count && code[id] != "{")
                    throw new Exception($"Not know operation {code[id - 1]} {code[id]}");

                id++;
                if (id < code.Count)
                    Statement(code, ref id, true);

                if (id < code.Count && code[id] != "}")
                    throw new Exception($"Not know operation {code[id - 1]} {code[id]}");

                if (id + 1 < code.Count && code[id + 1] == "else")
                {
                    id = id + 1; // scip else
                    id = id + 1; // scip {

                    if (id < code.Count && code[id] != "{")
                        throw new Exception($"Not know operation {code[id - 1]} {code[id]}");

                    int count = 1;
                    while (count != 0)
                    {
                        id++;
                        if (code[id] == "{")
                            count++;
                        if (code[id] == "}")
                            count--;
                    }
                }

            }
            else
            {
                id = id + 1; // scip {

                if (id < code.Count && code[id] != "{")
                    throw new Exception($"Not know operation {code[id - 1]} {code[id]}");
                
                int count = 1;
                while(count != 0)
                {
                    id++;
                    if(code[id] == "{")
                        count++;
                    if (code[id] == "}")
                        count--;
                }
                id = id + 1;
                if (id < code.Count && code[id] == "else")
                    Else(code, ref id);
            }
            id++;
            return null;
        }

        public static string BoolExcpression(List<string> code, ref int id)
        {
            var left = Expression(code, ref id);
            string relop = "";
            string right = "";

            id++;
            if (id < code.Count - 1)
                relop = code[id];

            id++;
            if (id < code.Count - 1)
                right = Expression(code, ref id);

            bool expression = false;
            switch (relop)
            {
                case "<":
                    expression = int.Parse(left) < int.Parse(right);
                    break;

                case ">":
                    expression = int.Parse(left) > int.Parse(right);
                    break;

                case "==":
                    expression = int.Parse(left) == int.Parse(right);
                    break;

                case "!=":
                    expression = int.Parse(left) != int.Parse(right);
                    break;
            }

            return expression.ToString();
        }

        public static string HardBoolExpression(List<string> code, ref int id)
        {

            if (code[id+3] != "and" && code[id + 3] != "or")
            {
                return BoolExcpression(code, ref id);
            }

            var left = BoolExcpression(code, ref id);

            string relop = "";
            string right = "";
            id++;

            if (id < code.Count - 1)
                relop = code[id];

            id++;
            if (id < code.Count - 1)
                right = BoolExcpression(code, ref id);

            bool expression = false;
            switch (relop)
            {
                case "and":
                    expression = bool.Parse(left) && bool.Parse(right);
                    break;

                case "or":
                    expression = bool.Parse(left) || bool.Parse(right);
                    break;
                default:
                    expression = bool.Parse(left);
                    break;
            }
            return expression.ToString();
        }

        public static string For(List<string> code, ref int id)
        {
            bool isUp = true;
            var key = code[id];
            string key1 = "", key2 ="";
            id = id + 1; // scip =
            if (id < code.Count && code[id] != "=")
            {
                throw new Exception($"Not know operation {code[id - 1]} {code[id]}");
            }
            if (id + 1 < code.Count)
            {
                id++;
                key1 = Expression(code, ref id);
            }

            id = id + 1; // scip to
            if (id < code.Count && !(code[id] == "to" || code[id] == "downto"))
            {
                throw new Exception($"Not know operation {code[id - 1]}{code[id]}");
            }

            if(code[id] == "to")
            {
                isUp = true;
            }
            if (code[id] == "downto")
            {
                isUp = false;
            }

            if (id + 1 < code.Count)
            {
                id++;
                key2 = Expression(code, ref id);
            }

            id = id + 1; // scip {
            if (id < code.Count && code[id] != "{")
            {
                throw new Exception($"Not know operation {code[id - 1]} {code[id]}");
            }

            int index = id + 1;
            if(index < code.Count)
            {
                if (isUp)
                {
                    for (int i = int.Parse(key1); i <= int.Parse(key2); i++)
                    {
                        id = index;
                        if (valuesTable.ContainsKey(key))
                            valuesTable[key] = i.ToString();
                        else
                            valuesTable.Add(key, i.ToString());
                        Statement(code, ref id, true);
                    }
                }
                else
                {
                    for (int i = int.Parse(key1); i >= int.Parse(key2); i--)
                    {
                        id = index;
                        if (valuesTable.ContainsKey(key))
                            valuesTable[key] = i.ToString();
                        else
                            valuesTable.Add(key, i.ToString());
                        Statement(code, ref id, true);
                    }
                }

            }

            if (id < code.Count && code[id] != "}")
            {
                throw new Exception($"Not know operation {code[id - 1]} {code[id]}");
            }

            id++;
            return null;
        }

        public static string Assign(List<string> code, ref int id)
        {
            var key = code[id];
            id = id + 1; // scip =
            if (id < code.Count && code[id] != "=")
            {
                throw new Exception($"Not know operation {code[id - 1]} {code[id]}");
            }
            if (id + 1 < code.Count)
            {
                id++;
                var val = Expression(code, ref id);
                if (valuesTable.ContainsKey(key))
                    valuesTable[key] = val;
                else
                    valuesTable.Add(key, val);
            }
            id++;
            return null;
        }

        public static string Scan(List<string> code, ref int id)
        {
            var val = Console.ReadLine();
            var succes = int.TryParse(val, out int _);
            if (succes)
            {
                if (id < code.Count)
                {
                    var key = code[id];
                    if (valuesTable.ContainsKey(key))
                        valuesTable[key] = val;
                    else
                        valuesTable.Add(key, val);
                }
                id++;
            }
            else
            {
                throw new Exception("Not number!");
            }
            return null;
        }

        public static string PrintEnd(List<string> code, ref int id)
        {
            string rtn;
            if (code[id].First() == '\"')
            {
                if (code[id].Last() == '\"')
                    rtn = String(code, ref id);
                else
                    throw new Exception($"Bad string format {code[id]}");
            }
            else
            {
                rtn = Expression(code, ref id);
            }

            if (id + 1 < code.Count && code[id + 1] == ",")
            {
                id += 1; // scip ,

                id++;
                rtn += PrintEnd(code, ref id);
            }

            return rtn;
        }

        public static string String(List<string> code, ref int id)
        {
            return code[id].Substring(1, code[id].Length - 2);
        }

        public static string Expression(List<string> code, ref int id)
        {
            var rtn = Term(code, ref id);

            if (id + 1 < code.Count && code[id + 1] == "+")
            {
                id += 1;

                id++;
                rtn = (int.Parse(rtn) + int.Parse(Expression(code, ref id))).ToString();
            }

            if (id + 1 < code.Count && code[id + 1] == "-")
            {
                id += 1;

                id++;
                rtn = (int.Parse(rtn) - int.Parse(Expression(code, ref id))).ToString();
            }

            return rtn;
        }

        public static string Term(List<string> code, ref int id)
        {
            var rtn = Factor(code, ref id);

            if (id + 1 < code.Count && code[id + 1] == "*")
            {
                id += 1; // scip *

                id++;
                rtn = (int.Parse(rtn) * int.Parse(Term(code, ref id))).ToString();
            }

            if (id + 1 < code.Count && code[id + 1] == "/")
            {
                id += 1; // scip /

                id++;
                rtn = (int.Parse(rtn) / int.Parse(Term(code, ref id))).ToString();
            }

            return rtn;
        }

        public static string Factor(List<string> code, ref int id)
        {
            if (char.IsDigit(code[id].First()))
                return Number(code, ref id);

            if (code[id].First() == '(')
            {
                if (code[id].Last() == ')')
                    return Expression(code, ref id);
                else
                    throw new Exception($"Bad inner expression format {code[id]}");
            }

            return Identifier(code, ref id);
        }

        public static string Identifier(List<string> code, ref int id)
        {
            if (IsIdentifier(code[id]))
            {
                if (ReservedWords.Contains(code[id]))
                    throw new Exception($"Identifier cant be reserved word {code[id]}");
                if (valuesTable.ContainsKey(code[id]))
                    return valuesTable[code[id]];
                else
                    throw new Exception($"Not identified value {code[id]}");
            }
            else
            {
                throw new Exception($"Bad identifier format {code[id]}");
            }
        }

        public static string Number(List<string> code, ref int id)
        {
            if(int.TryParse(code[id], out _))
                return code[id];

            throw new Exception($"Bad number format {code[id]}");
        }

        private static bool IsIdentifier(string str)
        {
            var check = true;

            foreach(var c in str)
                check &= Characters.Contains(c);

            return check;
        }

        private static string Characters => "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_";
    }
}
