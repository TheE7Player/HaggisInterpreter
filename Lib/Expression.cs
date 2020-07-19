﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace HaggisInterpreter2
{
    internal static class Expression
    {
        public static char[] validOperations = new char[] { '+', '/', '*', '-', '(', ')', '&', '!', '=', '>', '<'};
        public static string[] validFunctions = new string[] { "Lower", "Upper", "Trim", "Title" };
        public static string[] validComparisons = new string[] {">", "<", "!=", "NOT", "AND", "OR", "<=", ">=", "<>", "!", "="};
        public enum ExpressionType
        {
            /// <summary>
            /// LITERAL - Constant number, expressed at compile time: 0, 200, "Hey"
            /// </summary>
            LITERAL,

            /// <summary>
            /// EXPRESSION - Modification of one or more values at run time (Comparsion, Concat, Addition etc)
            /// </summary>
            EXPRESSION
        }

        #region Expression Logic

        public static string GetExpression(string[] values, int exprStartIndex)
        {
            exprStartIndex++;
            var sb = new StringBuilder();

            for (int i = exprStartIndex; i < values.Length; i++)
                if (i < (values.Length - 1))
                    sb.Append($"{values[i]} ");
                else
                    sb.Append(values[i]);

            return sb.ToString();
        }

        public static ExpressionType GetExpressionType(object val)
        {
            if (val.ToString().ToCharArray().Any(x => validOperations.Contains(x)))
                return ExpressionType.EXPRESSION;

            return ExpressionType.LITERAL;
        }

        public static string[] Evaluate(string Expression)
        {
            // Clean up the string
            var iter = Expression.Trim().ToCharArray();
            var sb = new StringBuilder();
            List<string> output = new List<string>(2);

            // 34 -> "
            // 32 -> (whitespace)
            for (int i = 0; i < iter.Length; i++)
            {
                // Is current char number '32'
                if ((int)iter[i] == 32)
                {
                    if (sb.Length > 0)
                    {
                        output.Add(sb.ToString());
                        sb.Clear();
                    }
                    continue;
                }

                if ((int)iter[i] == 34)
                {
                    i++;
                    while ((int)iter[i] != 34)
                    {
                        sb.Append(iter[i]);
                        i++;
                    }
                    output.Add(sb.ToString());
                    sb.Clear();
                    continue;
                }
                else
                {
                    //40 = (, 41 == )
                    if ((int)iter[i] == 40 || (int)iter[i] == 41)
                    {
                        if (sb.Length > 0)
                        {
                            output.Add(sb.ToString());
                            sb.Clear();

                            sb.Append(iter[i]);
                            output.Add(sb.ToString());
                            sb.Clear();
                        }
                        else
                        {
                            sb.Append(iter[i]);
                            output.Add(sb.ToString());
                            sb.Clear();
                        }
                    }
                    else
                        sb.Append(iter[i]);
                }
            }

            if (sb.Length > 0)
            {
                output.Add(sb.ToString());
                sb.Clear();
            }

            iter = null;
            sb = null;

            return output.ToArray();
        }

        private static Value DoExpr(Value l, string op, Value r)
        {
            // Value l = ((Value)(Object.ReferenceEquals(forceCompare, null) ? this : forceCompare));

            if (l.Type != r.Type)
            {
                // promote one value to higher type

                // Ignore if the left hand is a string
                bool lOptIgnore = (l.Type == ValueType.STRING || l.Type == ValueType.CHARACTER) ? true : false;
                if (lOptIgnore == false)
                {
                    if (r.Type != ValueType.STRING)
                        if (l.Type > r.Type)
                            r = r.Convert(l.Type);
                        else
                            l = l.Convert(r.Type);
                }
            }

            if (op.Equals("+"))
            {
                if (l.Type == ValueType.REAL)
                    return new Value(l.REAL + r.REAL);

                if (l.Type == ValueType.INTEGER)
                    return new Value(l.INT + r.INT);

                if (l.Type == ValueType.CHARACTER || l.Type == ValueType.STRING)
                    throw new Exception("Left opp cannot use + to join text together! Use '&' instead");

                if (r.Type == ValueType.CHARACTER || r.Type == ValueType.STRING)
                    throw new Exception("Right opp cannot use + to join text together! Use '&' instead");
            }
            else if (op.Equals("&"))
            {
                if (l.Type == ValueType.STRING || r.Type == ValueType.STRING)
                    return new Value(l.ToString() + r.ToString());

                // You cannot append l char, need to convert to string (C# Does that automatically for us)
                if (l.Type == ValueType.CHARACTER || r.Type == ValueType.CHARACTER)
                    if (l.Type == ValueType.CHARACTER)
                        return new Value(l.CHARACTER + r.ToString());
                    else if (r.Type == ValueType.CHARACTER)
                        return new Value(l.ToString() + r.CHARACTER);
                    else
                        throw new Exception($"PROBLEM: Issue with managing characters on left or right hand side ({l},{r})");
            }
            else if (op.Equals("="))
            {
                switch (l.Type)
                {
                    case ValueType.REAL:
                        return new Value(l.REAL == r.REAL ? true : false);

                    case ValueType.STRING:
                        return new Value(l.STRING == r.STRING ? true : false);

                    case ValueType.BOOLEAN:
                        return new Value(l.BOOLEAN == r.BOOLEAN ? true : false);

                    case ValueType.INTEGER:
                        return new Value(l.INT == r.INT ? true : false);

                    case ValueType.CHARACTER:
                        return new Value(l.CHARACTER == r.CHARACTER ? true : false);
                }
            }
            else if (op.Equals("!="))
            {
                switch (l.Type)
                {
                    case ValueType.REAL:
                        return new Value(l.REAL == r.REAL ? false : true);

                    case ValueType.STRING:
                        return new Value(l.STRING == r.STRING ? false : true);

                    case ValueType.BOOLEAN:
                        return new Value(l.BOOLEAN == r.BOOLEAN ? false : true);

                    case ValueType.INTEGER:
                        return new Value(l.INT == r.INT ? false : true);

                    case ValueType.CHARACTER:
                        return new Value(l.CHARACTER == r.CHARACTER ? false : true);
                }
            }
            else
            {
                if (l.Type == ValueType.STRING)
                    throw new Exception("Cannot perform requested \"BinOp\" on \"STRING\"");

                if (l.Type == ValueType.CHARACTER)
                    throw new Exception("Cannot perform requested \"BinOp\" on \"CHARACTER\"");

                if (l.Type == ValueType.REAL)
                    switch (op)
                    {
                        case "-": return new Value(l.REAL - r.REAL);
                        case "*": return new Value(l.REAL * r.REAL);
                        case "/": return new Value(l.REAL / r.REAL);
                        //case Token.Caret: return new Value(Math.Pow(l.REAL, r.REAL));
                        case "<": return new Value(l.REAL < r.REAL ? true : false);
                        case ">": return new Value(l.REAL > r.REAL ? true : false);
                        case "<=": return new Value((l.REAL <= r.REAL) ? true : false);
                        case ">=": return new Value(l.REAL >= r.REAL ? true : false);
                        case "AND": return new Value((l.REAL != 0) && (r.REAL != 0) ? true : false);
                        case "OR": return new Value((l.REAL != 0) || (r.REAL != 0) ? true : false);
                        case "!=": return new Value((l.REAL != 0) != (r.REAL != 0) ? true : false);
                    }

                if (l.Type == ValueType.INTEGER)
                    switch (op)
                    {
                        case "-": return new Value(l.INT - r.INT);
                        case "*": return new Value(l.INT * r.INT);
                        case "/": return new Value(l.INT / r.INT);
                        //case Token.Caret: return new Value(Math.Pow(l.INT, r.INT));
                        case "<": return new Value(l.INT < r.INT ? true : false);
                        case ">": return new Value(l.INT > r.INT ? true : false);
                        case "<=": return new Value(l.INT <= r.INT ? true : false);
                        case ">=": return new Value(l.INT >= r.INT ? true : false);
                        case "AND": return new Value((l.INT != 0) && (r.INT != 0) ? true : false);
                        case "OR": return new Value((l.INT != 0) || (r.INT != 0) ? true : false);
                        case "!=": return new Value((l.INT != 0) != (r.INT != 0) ? true : false);
                    }

                if (l.Type == ValueType.BOOLEAN)
                    switch (op)
                    {
                        case "AND": return new Value((l.BOOLEAN != false) && (r.BOOLEAN != false) ? true : false);
                        case "OR": return new Value((l.BOOLEAN != false) || (r.BOOLEAN != false) ? true : false);
                        case "NOT": return new Value((l.BOOLEAN != false) != (r.BOOLEAN != false) ? true : false);
                    }
            }

            throw new Exception("Unknown binary operator.");
        }

        #endregion Expression Logic

        #region Expression Operations

        private static Tuple<Value, string, Value> GetOperands(ref Stack stack, ref Dictionary<string, Value> vals)
        {
            var rightOp = stack.Pop();
            if (vals.ContainsKey(rightOp.ToString()))
                rightOp = vals[rightOp.ToString()];
            else if (rightOp.GetType().ToString() != "HaggisInterpreter2.Value")
                rightOp = new Value(rightOp as string, true);

            var opOper = (string)stack.Pop();
            var leftOp = stack.Pop();
            if (vals.ContainsKey(leftOp.ToString()))
                leftOp = vals[leftOp.ToString()];
            else if (leftOp.GetType().ToString() != "HaggisInterpreter2.Value")
                leftOp = new Value(leftOp as string, true);

            return new Tuple<Value, string, Value>((Value)leftOp, opOper, (Value)rightOp);
        }

        private static Tuple<string, int> FindFunc(string[] exp)
        {
            int index = 0;

            for (int i = 0; i < exp.Length; i++)
            {
                if(validFunctions.Contains(exp[i]))
                {
                    if (index == 0)
                        return new Tuple<string, int>(exp[i], exp[i].Length + 1);
                    else
                    {
                        index += exp[i].Length + 2;
                        return new Tuple<string, int>(exp[i], index + 1);
                    }
                }

                // Add gap to suite whitespace
                index += exp[i].Length + 1;
            }

            return null;
        }

        public static Value PerformExpression(Dictionary<string, Value> vals, string expression)
        {
            var exp = Evaluate(expression.ToString());

            var blocks = BlockParser.GenerateBlocks(exp, vals);

            // If its all just text, we just ammend it
            if(!blocks.Any(x => x.BinaryOp != ""))
            {
                int allText = blocks.Where(x=>x.blockType == BlockType.Text).Distinct().Count();

                if(allText == blocks.Count)
                {
                    var sb = new StringBuilder();
                    for (int i = 0; i < blocks.Count; i++)
                    {
          
                        sb.Append(blocks[i].Value.ToString());

                        if(i < (blocks.Count - 1))
                            sb.Append(" ");
                    }
                    blocks = new List<IBlock> { new Block { Value = new Value(sb.ToString(), true) } };
                }
            }


            if(blocks.Count > 1)
            {
                var sortedLevel = BlockParser.SortByOrder(blocks);

                List<IBlock> lvlList;
                int HighestIndex = 0;
                int currentLevel = 0;
                foreach (var lvl in sortedLevel)
                {
                    lvlList = lvl.Value;
                    currentLevel = lvl.Key;
                    HighestIndex = lvlList.Count - 1;

                    if (string.IsNullOrEmpty(lvlList[HighestIndex - 1].BinaryOp))
                    {
                        if(lvlList[0].blockType != BlockType.BinOp)
                            throw new Exception($"Cannot do an operation with the given BinOP, \"{ lvlList[HighestIndex - 1].BinaryOp }\"!");

                        // We need to fix the order, The newest value (Highest index needs to be on top, at index 0)
                        var newLeft = lvlList[HighestIndex];   
                        lvlList.RemoveAt(HighestIndex);
                        lvlList.Insert(0, newLeft);

                        // We need to remove the operator!
                        lvlList[0].BinaryOp = lvlList[1].Value.ToString();
                        lvlList.RemoveAt(1);

                        // Reassign the max index as it was based on 3(ish) items and not 2
                        HighestIndex = lvlList.Count - 1;
                    }
                        

                    // Cache the valve(s) for better performance
                    Value left = Value.Zero;
                    Value right = Value.Zero;
                    Value eval = Value.Zero;
                    string op = string.Empty;

                    while (lvlList.Count > 0)
                    {

                        if (lvlList[HighestIndex].GetType().Name == "ConditionBlock")
                        {
                            ConditionBlock cb = lvlList[HighestIndex] as ConditionBlock;

                            // First check if any variables here are stored
                            if (cb.Left.Type == ValueType.STRING)
                                cb.Left = (vals.ContainsKey(cb.Left.STRING)) ? vals[cb.Left.STRING] : cb.Left;

                            if (cb.Right.Type == ValueType.STRING)
                                cb.Right = (vals.ContainsKey(cb.Right.STRING)) ? vals[cb.Right.STRING] : cb.Right;

                            var _eval = DoExpr(cb.Left, cb.CompareOp, cb.Right);

                            lvlList.RemoveAt(HighestIndex);
                            HighestIndex = ((lvlList.Count - 1) > 0) ? HighestIndex = lvlList.Count - 1 : 0;

                            if (string.IsNullOrEmpty(cb.BinaryOp))
                                lvlList.Add(new Block { Value = _eval, OrderLevel = currentLevel, BinaryOp = String.Empty, blockType = BlockType.Literal });
                            else
                                lvlList.Insert(HighestIndex, new Block { Value = _eval, OrderLevel = currentLevel, BinaryOp = cb.BinaryOp, blockType = BlockType.Literal });

                            // If we managed to get all ConditionBlocks into Blocks, we reset back to normal (As an index error will happen if we don't!)
                            bool ConversionNotComplete = lvlList.Any(x => x.GetType().Name == "ConditionBlock");

                            // Refix back to normal HighestIndex
                            if (!ConversionNotComplete)
                                HighestIndex = lvlList.Count - 1;

                            continue;
                        }
                        else if (lvlList[HighestIndex].GetType().Name == "FuncBlock")
                        {
                            FuncBlock fb = lvlList[HighestIndex] as FuncBlock;

                            //TODO: ID array of args (Only single args at the moment)
                            string args;

                            if (fb.Args.Length == 1)
                            {
                                args = fb.Args[0];
                                args = (vals.ContainsKey(args)) ? vals[args].ToString() : fb.Args[0];
                            }
                            else 
                            { 
                                args = string.Join(",", fb.Args);
                                throw new Exception("Multiple arguments aren't supported in this current build - Please wait till this gets optimised!");
                            }

                            var _eval = FuncExtensions(fb.FunctionName, args, vals);

                            lvlList.RemoveAt(HighestIndex);
                            HighestIndex = ((lvlList.Count - 1) > 0) ? HighestIndex = lvlList.Count - 1 : 0;

                            if (string.IsNullOrEmpty(fb.BinaryOp))
                                lvlList.Add(new Block { Value = _eval, OrderLevel = currentLevel, BinaryOp = String.Empty, blockType = BlockType.Literal });
                            else
                                lvlList.Insert(HighestIndex, new Block { Value = _eval, OrderLevel = currentLevel, BinaryOp = fb.BinaryOp, blockType = BlockType.Literal });

                            // If we managed to get all ConditionBlocks into Blocks, we reset back to normal (As an index error will happen if we don't!)
                            bool ConversionNotComplete = lvlList.Any(x => x.GetType().Name == "FuncBlock");

                            // Refix back to normal HighestIndex
                            if (!ConversionNotComplete)
                                HighestIndex = lvlList.Count - 1;

                            continue;
                        }
                        else
                        {
                            try
                            {
                                if (lvlList[HighestIndex - 1].blockType == BlockType.Variable)
                                    left = vals[lvlList[HighestIndex - 1].Value.ToString()];
                                else
                                    left = lvlList[HighestIndex - 1].Value;
                            }
                            catch (Exception)
                            {
                                left = lvlList[HighestIndex - 1].Value;
                            }

                            op = lvlList[HighestIndex - 1].BinaryOp;

                            try
                            {
                                if (lvlList[HighestIndex].blockType == BlockType.Variable)
                                    right = vals[lvlList[HighestIndex].Value.ToString()];
                                else
                                    right = lvlList[HighestIndex].Value;
                            }
                            catch (Exception)
                            {
                                right = lvlList[HighestIndex].Value;
                            }

                            lvlList.RemoveAt(HighestIndex);
                            lvlList.RemoveAt(HighestIndex - 1);

                        }

                        // Move up the data type to allow doubles with floats not to be assigned "0"
                        if (left.Type == ValueType.INTEGER)
                            left = left.Convert(ValueType.REAL);

                        if (right.Type == ValueType.INTEGER)
                            right = right.Convert(ValueType.REAL);

                        eval = DoExpr(left, op, right);

                        if (lvlList.Count == 0)
                        {
                            if((currentLevel - 1) == -1)
                            {
                                return eval;
                            }

                            // That means we've reach all the expressions needed for this current level
                            int newLevel = BlockParser.GetHighestOrder(sortedLevel[currentLevel - 1]);

                            sortedLevel[currentLevel - 1].Add(new Block { Value = eval, OrderLevel = newLevel, BinaryOp = String.Empty, blockType = BlockType.Literal});
                            continue;
                        }
                        
                        // Ammend it as there are still some more operations to do
                        sortedLevel[currentLevel].Add(new Block { Value = eval, OrderLevel = currentLevel, BinaryOp = String.Empty, blockType = BlockType.Literal });
                        HighestIndex = lvlList.Count - 1;
                    }
                }
            }

            if (blocks[0].GetType().Name == "ConditionBlock")
            {
                ConditionBlock cb = blocks[0] as ConditionBlock;

                // First check if any variables here are stored
                if (cb.Left.Type == ValueType.STRING)
                    cb.Left = (vals.ContainsKey(cb.Left.STRING))?vals[cb.Left.STRING]:cb.Left;

                if (cb.Right.Type == ValueType.STRING)
                    cb.Right = (vals.ContainsKey(cb.Right.STRING)) ? vals[cb.Right.STRING] : cb.Right;

                // Good, now we check if its an integer, we raise it to the next highest type (Float/Double -> REAL)
                if (cb.Left.Type == ValueType.INTEGER)
                    cb.Left = cb.Left.Convert(ValueType.REAL);

                if (cb.Right.Type == ValueType.INTEGER)
                    cb.Right = cb.Right.Convert(ValueType.REAL);

                return DoExpr(cb.Left, cb.CompareOp, cb.Right);
            }
            else if(blocks[0].GetType().Name == "FuncBlock")
            {
                FuncBlock fb = blocks[0] as FuncBlock;

                //TODO: ID array of args (Only single args at the moment)
                string args;

                if (fb.Args.Length == 1)
                {
                    args = fb.Args[0];
                    args = (vals.ContainsKey(args)) ? vals[args].ToString() : fb.Args[0];
                }
                else
                {
                    args = string.Join(",", fb.Args);
                    throw new Exception("Multiple arguments aren't supported in this current build - Please wait till this gets optimised!");
                }

                return FuncExtensions(fb.FunctionName, args, vals);
            }
            else
                return blocks[0].Value;
           
            #region OLD CODE (Stack & Queue Based)
            /*
            var stack = new Stack(exp.Length + 4);

            bool anyBrackets = exp.Any(x => x.StartsWith("("));
            bool mutlipleQuery = exp.Any(y => y == "AND" || y == "OR");


            if (anyBrackets)
            {
                //TODO: What if '(' or ')' is in the string/quote?
                int lBrackets = 0;
                int rBrackets = 0;

                foreach (var item in exp)
                {
                    if (item == "(")
                    { lBrackets++; continue; }

                    if (item == ")")
                    { rBrackets++; continue; }
                }

                if (lBrackets != rBrackets)
                    throw new Exception($"Missing or Unbalanced Brackets: [(] {lBrackets} [)] {rBrackets}");
            }

            // Populate the stack
            if (!anyBrackets)
            {
                foreach (var item in exp)
                    stack.Push(item);
            }
            else
            {
                // We need to deal with this as brackets have to be calculate before hand
                var queueOp = new Queue();
                var expr_final = expression;
                int HighestOrder = 0;
                int HighestOrderEnd = 0;
                string newExp = string.Empty;
                while (true)
                {
                    HighestOrder = expr_final.LastIndexOf("(");

                    if (HighestOrder == -1)
                        break;

                    HighestOrderEnd = expr_final.IndexOf(")", HighestOrder);

                    newExp = expr_final.Substring(HighestOrder + 1, (HighestOrderEnd - HighestOrder) - 1);
                    queueOp.Enqueue(newExp);
                    expr_final = expr_final.Replace($"({newExp})", $"`{queueOp.Count - 1}`");
                    newExp = string.Empty;
                    HighestOrder = 0;
                    HighestOrderEnd = 0;
                }

                queueOp.Enqueue(expr_final);
                expr_final = null; newExp = null;

                string expr;
                string[] _exp;
                var refIndex = new Dictionary<string, Value>(2);
                var personalStack = new Stack();
                while (queueOp.Count >= 1)
                {
                    expr = queueOp.Dequeue() as string;
                    _exp = Expression.Evaluate(expr);

                    foreach (var pStack in _exp)
                    {
                        if (pStack.Contains("`"))
                        {
                            personalStack.Push(refIndex[pStack]);
                        }
                        else
                            personalStack.Push(pStack);
                    }

                    if (personalStack.Count == 1)
                    {
                        //TODO: Evaluate if this method is robust enough?

                        if (anyBrackets && exp.Any(a => validFunctions.Contains(a)))
                        {
                            var func = FindFunc(exp);

                            var f_expr = expression.Substring(func.Item2);
                            f_expr = f_expr.Substring(0, f_expr.LastIndexOf(")"));
                            var result = FuncExtensions(func.Item1, f_expr, vals);

                            return result;
                        }


                        return new Value(personalStack.Pop() as string, true);
                    }

                    var _e = GetOperands(ref personalStack, ref vals);

                    // For the best results, convert INT to Double
                    Value lNum = _e.Item1;
                    Value rNum = _e.Item3;

                    if (lNum.Type == ValueType.INTEGER)
                        lNum = lNum.Convert(ValueType.REAL);

                    if (rNum.Type == ValueType.INTEGER)
                        rNum = rNum.Convert(ValueType.REAL);

                    Value _e_result = DoExpr(lNum, _e.Item2, rNum);

                    if (queueOp.Count != 0)
                    {
                        var isNot = queueOp.Peek() as string;

                        if (isNot.StartsWith("NOT"))
                        {
                            // Perform a switch (True becomes false, False becomes true)
                            _e_result.BOOLEAN = !_e_result.BOOLEAN;
                            queueOp.Dequeue();
                        }
                    }
                    if (queueOp.Count == 0)
                    {
                        refIndex = null; personalStack = null; _exp = null; expr = null;
                        return _e_result;
                    }

                    refIndex.Add($"`{refIndex.Count}`", _e_result);
                }
            }

            bool runCycle = false;

            while (stack.Count > 0)
            {
                if (stack.Count == 1)
                {
                    if (runCycle)
                        break;
                    else
                    {
                        var singleObject = stack.Pop();
                        if (vals.ContainsKey(singleObject.ToString()))
                            singleObject = vals[singleObject.ToString()];
                        else
                            singleObject = new Value(singleObject as string, true);

                        stack.Push(singleObject);
                        runCycle = true;
                        continue;
                    }
                }

                Value eval = new Value();

                if (mutlipleQuery)
                {
                    var operandsL = GetOperands(ref stack, ref vals);
                    var oper = stack.Pop() as string;
                    var operandsR = GetOperands(ref stack, ref vals);

                    var l = DoExpr(operandsL.Item1, operandsL.Item2, operandsL.Item3);
                    var r = DoExpr(operandsR.Item1, operandsR.Item2, operandsR.Item3);

                    eval = DoExpr(l, oper, r);
                }
                else
                {
                    var operands = GetOperands(ref stack, ref vals);
                    eval = DoExpr(operands.Item1, operands.Item2, operands.Item3);
                }

                stack.Push(eval);
                runCycle = true;
            }

            return (Value)stack.Pop();*/
            #endregion

        }

        private static Value FuncExtensions(string function, string expression, Dictionary<string, Value> vals)
        {
            //var exp = PerformExpression(vals, expression);

            if (function == "Lower")
                return new Value(expression.ToLower());
            else if (function == "Upper")
                return new Value(expression.ToUpper());
            else if (function == "Trim")
                return new Value(expression.Trim());
            else if (function == "Title")
            {
                var text = expression.ToCharArray();
                // We force the first char to be Title automatically
                text[0] = char.ToUpper(text[0]);

                // If the character before the current character is whitespace, set the current char to upper
                for (int i = 1; i < text.Length; i++)
                    if(char.IsWhiteSpace(text[i - 1]))
                        text[i] = char.ToUpper(text[i]);

                return new Value(String.Join("", text));
            }

            return new Value();
        }

        #endregion Expression Operations
    }
}