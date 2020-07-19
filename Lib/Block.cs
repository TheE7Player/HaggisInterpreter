using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaggisInterpreter2
{
    /// <summary>
    /// An Expression Simplifier ~ Used to make expressions more robust (Logic will be simplified)
    /// </summary>
    /// 
    public enum BlockType
    {

        Null,

        Variable,

        Literal,

        Expression,

        BinOp,

        Text,

        Function,

        Record,

        Class
    }

    public interface IBlock
    {
        /// <summary>
        /// Holds the output of the block (If any)
        /// </summary>
        public Value Value { get;  set; }

        /// <summary>
        /// Importance level - Used if doing calcuations with brackets (BOMDAS/PEMDAS)
        /// (0 is the lowest level available)
        /// </summary>
        public int OrderLevel { get; set; }

        /// <summary>
        /// If the block carries an operation from the right hand side (If any)
        /// </summary>
        public string BinaryOp { get; set; }
        /// <summary>
        /// The Blocks type
        /// </summary>
        public BlockType blockType { get; set; }
    }

    public class Block : IBlock
    {
        public BlockType blockType { get; set; }

        public int OrderLevel { get; set; }

        public Value Value { get; set; }

        public string BinaryOp { get; set; }
    }

    public class ConditionBlock : IBlock
    {
        public BlockType blockType { get; set; }
        public string BinaryOp { get; set; }

        public string CompareOp { get; set; }

        public Value Value { get; set; }

        /// <summary>
        /// Left side of the comparsion (Comparing)
        /// </summary>
        public Value Left { get; set; }

        /// <summary>
        /// Right side of the comparsion (Comparing To)
        /// </summary>
        public Value Right { get; set; }
        public int OrderLevel { get; set; }
    }

    public class FuncBlock : IBlock
    {
        public BlockType blockType { get; set; }
        public string BinaryOp { get; set; }
        public Value Value { get; set; }
        public string[] Args { get; set; }
        public string FunctionName { get; set; }
        public int OrderLevel { get; set; }
    }

    public static class BlockParser
    {
        private static bool CanLookAhead(int current, int len) => ((current + 1) < len) ? true : false;
        public static List<IBlock> GenerateBlocks(string[] expr, Dictionary<string, Value> vals)
        {
            // Holds the current order level (used for calculations or expressions)
            int orderLevel = 0;

            if(expr.Length == 1)
            {
                var s_list = new List<IBlock>(1);
                if (vals.ContainsKey(expr[0]))
                {
                    s_list.Add(new Block { BinaryOp = null, blockType = BlockType.Variable, Value = vals[expr[0]], OrderLevel = orderLevel });
                    return s_list;
                }

                s_list.Add(new Block { BinaryOp = null, blockType = BlockType.Literal, Value = new Value(expr[0], true), OrderLevel = orderLevel });
                return s_list;
            }

            var _list = new List<IBlock>(2);
            int max_len = expr.Length;
            
            //string express = String.Join(" ", expr);

            for (int i = 0; i < max_len; i++)
            {
                // Look ahead for a function block?
                if (CanLookAhead(i, max_len))
                {
                    if(expr[i + 1] == "(")
                    {
                        var func_name = expr[i];
                        i++;
                        // Check if the function carries over any params
                        if (CanLookAhead(i + 1, max_len))
                        {
                            if( expr[i + 1] == ")" )
                            {
                                i += 2;
                                _list.Add(new FuncBlock { FunctionName = func_name, Value = Value.Zero, Args = null, OrderLevel = orderLevel } );
                                orderLevel++;
                                continue;
                            }

                            i++;
                            List<string> args = new List<string>();
                            while (i < max_len)
                            {
                                if (expr[i] == ")")
                                    break;

                                if (expr[i] == ",")
                                {
                                    i++; continue;
                                }

                                args.Add(expr[i]);
                                i++;
                            }
                            
      
                            i += 2;
                            _list.Add(new FuncBlock { FunctionName = func_name, Value = Value.Zero, Args = args.ToArray(), OrderLevel = orderLevel });
                            orderLevel++;
                            continue;
                        }
                    }
                }

                // The second expression prevents ')' from being excluded if it was a string ('&') concat...
                if(expr[i] == "(" || (expr[i] == ")" && expr[i - 1] != "&"))
                {
                    orderLevel = (expr[i] == "(") ? orderLevel += 1 : orderLevel -= 1;
                    continue;
                }

                // Get the value type
                Value _val = new Value(expr[i], true);
                string op = string.Empty;
                // Get the binary operator (if any)
                if (CanLookAhead(i + 1, max_len))
                    if (Expression.validOperations.Contains(expr[i + 1][0]))
                    {
                        if (expr[i + 1] != ")")
                        { 
                            i++;
                            op = expr[i];
                        }
                    }

                // Amend the information now
                BlockType bType = BlockType.Literal;

                if(vals.ContainsKey((op != String.Empty) ? expr[i-1] : expr[i]))
                    bType = BlockType.Variable;
                else
                {
                    if(_val.Type == ValueType.STRING)
                        bType = BlockType.Text;

                    if (expr[i].Length == 1 && _val.Type == ValueType.CHARACTER)
                        if (Expression.validOperations.Contains(_val.CHARACTER))
                            bType = BlockType.BinOp;
                }


                if (Expression.validComparisons.Contains(expr[i]))
                { 
                    _list.Add(new ConditionBlock { blockType = BlockType.Expression, Value = Value.Zero, CompareOp = op, Left = _val, Right = new Value(expr[i + 1], true), OrderLevel = orderLevel });
                    i += 1;
                    if (CanLookAhead(i + 1, max_len))
                    {
                        if (Expression.validComparisons.Contains(expr[i + 1]))
                        {
                            _list[_list.Count - 1].BinaryOp = expr[i + 1];
                            i++;
                            continue;
                        }

                        i++;
                    }
                }
                else
                    _list.Add(new Block { blockType = bType, Value = _val, BinaryOp = op, OrderLevel = orderLevel });  
            }

            return _list;
        }

        /// <summary>
        /// Reorders the blocks based on Order Level (Based on BOMDAS/PEMDAS)
        /// </summary>
        /// <param name="blocks">The list of the blocks to sort by</param>
        /// <returns></returns>
        public static Dictionary<int, List<IBlock>> SortByOrder(List<IBlock> blocks)
        {
            SortedDictionary<int, List<IBlock>> sortLevel = new SortedDictionary<int, List<IBlock>>();

            int indexLevel = 0;
            foreach (IBlock b in blocks)
            {
                indexLevel = b.OrderLevel;
                
                if(!sortLevel.ContainsKey(indexLevel))
                {
                    sortLevel.Add(indexLevel, new List<IBlock>(1));
                }

                sortLevel[indexLevel].Add(b);
            }

            // Then we reverse the sorted keys and return it back into a normal dictionary
            return sortLevel.OrderByDescending(kvp => kvp.Key).ToDictionary(x => x.Key, x => x.Value);
        }

        public static int GetHighestOrder(List<IBlock> blocks)
        {
            int HighestIndex = -1;
            for (int i = 0; i < blocks.Count; i++)
            {
                if (blocks[i].OrderLevel > HighestIndex)
                    HighestIndex = blocks[i].OrderLevel;
            }
            return HighestIndex;
        }
    }
}
