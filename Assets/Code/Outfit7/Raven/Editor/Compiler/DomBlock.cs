using System.CodeDom;

namespace Starlite.Raven.Compiler {

    public class DomBlock {
        private int tmpVar = 0;
        private string prefix = "";
        private CodeStatementCollection stmts = null;

        public CodeStatementCollection Stmts {
            get {
                return stmts;
            }
        }

        public void Add(CodeExpression expression) {
            Stmts.Add(expression);
        }

        public void Add(CodeStatement statement) {
            Stmts.Add(statement);
        }

        public DomBlock(CodeStatementCollection stmts, string prefix) {
            this.stmts = stmts;
            this.prefix = prefix;
        }

        public DomBlock(CodeStatementCollection stmts)
          : this(stmts, "") {
        }

        public string TempVar() {
            return prefix + "tmp" + (tmpVar++);
        }
    }
}