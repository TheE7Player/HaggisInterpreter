using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace HaggisInterpreter2
{
    public partial class Interpreter
    {
        private string[] file;
        public static bool executionHandled = false;

        public Dictionary<string, Value> variables { private set; get; }
        public Stack<string> callStack { private set; get; }

        private int line;
        public static int Line;
        public static int Column;

        public int[] errorArea { private set; get; }

        private string[] validTypes = {"INTEGER", "CHAR", "BOOLEAN", "REAL", "STRING"};
        private Dictionary<string, dynamic> DefaultVal = new Dictionary<string, dynamic>{
            {"INTEGER", 0 },
            {"CHAR", ' ' },
            {"BOOLEAN", false },
            {"REAL", 0.0 },
            {"STRING", string.Empty }
        };

        #region INTERPRETER_FLAGS
        public struct FLAGS
        {
            /// <summary>
            /// INTERPRETER FLAG: Flag which prints the line in the script when it calls a print function (SEND)
            /// </summary>
            public bool DebugSendRequests { get; set; }
            /// <summary>
            ///  INTERPRETER FLAG: Value of flags to allow auto input if request (RECIEVE)
            /// </summary>
            public Dictionary<string, string> Inputs { get; set; }
        }

        public FLAGS _flags { get; private set; }
        #endregion

        public Interpreter(string[] Contents, FLAGS flags)
        {
            this.file = Contents;
            this.variables = new Dictionary<string, Value>(1);
            this.line = 0;
            Line = 0;
            //this.col = 0;
            this._flags = flags;
            this.callStack = new Stack<string>(1);
        }
   
    }
}
