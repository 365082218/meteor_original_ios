using System;
using System.CodeDom;
using System.Linq;
using System.Reflection;

namespace Starlite.Raven.Compiler {

    public static class Dom {

        public static CodeExpression @value {
            get {
                return new CodeSnippetExpression("value");
            }
        }

        public static CodeExpression @this {
            get {
                return new CodeThisReferenceExpression();
            }
        }

        public static CodeStatement @return {
            get {
                return new CodeMethodReturnStatement();
            }
        }

        private static void Comment(this CodeTypeMember member, bool doc, string comment, params object[] args) {
            member.Comments.Add(new CodeCommentStatement(string.Format(comment ?? "", args), doc));
        }

        public static void Comment(this CodeTypeMember member, string comment, params object[] args) {
            Comment(member, false, comment, args);
        }

        public static void CommentDoc(this CodeTypeMember member, string comment, params object[] args) {
            Comment(member, true, comment, args);
        }

        public static void Comment(this CodeStatementCollection statements, string comment, params object[] args) {
            statements.Add(new CodeCommentStatement(string.Format(comment, args)));
        }

        public static void Call(this CodeStatementCollection stmts, CodeExpression expr, string method, params CodeExpression[] args) {
            stmts.Add(new CodeMethodInvokeExpression(expr, method, args));
        }

        public static CodeExpression Call(this CodeExpression expr, string method, params CodeExpression[] args) {
            return new CodeMethodInvokeExpression(expr, method, args);
        }

        public static void CommentSummary(this CodeTypeMember member, Action<CodeTypeMember> commenter) {
            Comment(member, true, "<summary>");
            commenter(member);
            Comment(member, true, "</summary>");
        }

        public static void DeclareObsolete(this CodeTypeMember member, string message) {
            CodeAttributeArgument argument = new CodeAttributeArgument(new CodeSnippetExpression('"' + message + '"'));
            member.CustomAttributes.Add(new CodeAttributeDeclaration("System.Obsolete", argument));
        }

        public static void IfDef(this CodeStatementCollection stmts, string symbol, Action<CodeStatementCollection> ifdef) {
            stmts.Add(new CodeSnippetStatement("#if " + symbol));
            ifdef(stmts);
            stmts.Add(new CodeSnippetStatement("#endif"));
        }

        public static CodeExpression Field(this CodeExpression expr, string field) {
            return new CodeFieldReferenceExpression(expr, field);
        }

        public static CodeMemberMethod DeclareMethod(this CodeTypeDeclaration type, Type returnType, string methodName, Action<CodeMemberMethod> body) {
            return DeclareMethod(type, returnType.ToString(), methodName, body);
        }

        public static CodeMemberMethod DeclareMethod(this CodeTypeDeclaration type, string returnType, string methodName, Action<CodeMemberMethod> body) {
            CodeMemberMethod method;

            method = new CodeMemberMethod();
            method.Name = methodName;
            method.ReturnType = new CodeTypeReference(returnType);
            method.Attributes = MemberAttributes.Public | MemberAttributes.Final;

            if (body != null) {
                body(method);

                if (type.IsInterface) {
                    method.Statements.Clear();
                }
            }

            type.Members.Add(method);
            return method;
        }

        public static void DeclareMember(this CodeTypeDeclaration type, string memberSource, params object[] args) {
            type.Members.Add(new CodeSnippetTypeMember(string.Format(memberSource, args)));
        }

        public static CodeMemberProperty DeclareProperty(this CodeTypeDeclaration type, string propertyType, string propertyName, Action<CodeStatementCollection> getter) {
            return DeclareProperty(type, propertyType, propertyName, getter, null);
        }

        public static CodeMemberProperty DeclareProperty(this CodeTypeDeclaration type, string propertyType, string propertyName, string getter) {
            return DeclareProperty(type, propertyType, propertyName, get => get.Expr(getter), null);
        }

        public static CodeMemberProperty DeclareProperty(this CodeTypeDeclaration type, string propertyType, string propertyName, string getter, string setter) {
            if (setter != null) {
                return DeclareProperty(type, propertyType, propertyName, get => get.Expr(getter), set => set.Expr(setter));
            } else {
                return DeclareProperty(type, propertyType, propertyName, get => get.Expr(getter), null);
            }
        }

        public static CodeMemberProperty DeclareProperty(this CodeTypeDeclaration type, string propertyType, string propertyName, Action<CodeStatementCollection> getter, Action<CodeStatementCollection> setter) {
            CodeMemberProperty prop;

            prop = new CodeMemberProperty();
            prop.Name = propertyName;
            prop.Type = new CodeTypeReference(propertyType);
            prop.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            prop.HasGet = (getter != null);
            prop.HasSet = (setter != null);

            if (prop.HasGet) {
                getter(prop.GetStatements);
            }

            if (prop.HasSet) {
                setter(prop.SetStatements);
            }

            if (type.IsInterface) {
                prop.GetStatements.Clear();
                prop.SetStatements.Clear();
            }

            type.Members.Add(prop);
            return prop;
        }

        public static CodeTypeConstructor DeclareConstructorStatic(this CodeTypeDeclaration type, Action<CodeTypeConstructor> ctor) {
            CodeTypeConstructor method = new CodeTypeConstructor();
            type.Members.Add(method);

            ctor(method);

            return method;
        }

        public static CodeConstructor DeclareConstructor(this CodeTypeDeclaration type, Action<CodeConstructor> ctor) {
            CodeConstructor method;

            method = new CodeConstructor();
            method.Attributes = MemberAttributes.Public;
            type.Members.Add(method);

            ctor(method);

            return method;
        }

        public static CodeMemberField DeclareField(this CodeTypeDeclaration type, string fieldType, string fieldName) {
            CodeMemberField field;

            field = new CodeMemberField(fieldType, fieldName);
            field.Attributes = MemberAttributes.Assembly;

            type.Members.Add(field);

            return field;
        }

        public static void For(this CodeStatementCollection stmts, string variableName, CodeExpression testExpression, Action<CodeStatementCollection> body) {
            CodeIterationStatement it;

            it = new CodeIterationStatement();
            it.InitStatement = ("int " + variableName + " = 0").Stmt();
            it.TestExpression = testExpression;
            it.IncrementStatement = ("++" + variableName).Stmt();

            body(it.Statements);

            stmts.Add(it);
        }

        public static void For(this CodeStatementCollection stmts, string variableName, string testExpression, Action<CodeStatementCollection> body) {
            stmts.For(variableName, testExpression.Expr(), body);
        }

        public static CodeVariableReferenceExpression DeclareParameter(this CodeMemberProperty method, string paramType, string paramName) {
            method.Parameters.Add(new CodeParameterDeclarationExpression(paramType, paramName));
            return new CodeVariableReferenceExpression(paramName);
        }

        public static CodeVariableReferenceExpression DeclareParameter(this CodeMemberMethod method, string paramType, string paramName) {
            method.Parameters.Add(new CodeParameterDeclarationExpression(paramType, paramName));
            return new CodeVariableReferenceExpression(paramName);
        }

        public static void Assign(this CodeStatementCollection stmts, CodeExpression left, CodeExpression right) {
            stmts.Add(new CodeAssignStatement(left, right));
        }

        public static CodeVariableReferenceExpression Var(this CodeStatementCollection stmts, string type, string name) {
            stmts.Expr("{0} {1}", type, name);
            return new CodeVariableReferenceExpression(name);
        }

        public static CodeExpression New(this string type, params CodeExpression[] arguments) {
            return new CodeTypeReference(type).New(arguments);
        }

        public static CodeExpression New(this CodeTypeReference type, params CodeExpression[] arguments) {
            return new CodeObjectCreateExpression(type, arguments);
        }

        public static void Expr(this CodeStatementCollection stmts, string text, params object[] args) {
            stmts.Add(text.Expr(args));
        }

        public static void Stmt(this CodeStatementCollection stmts, string text, params object[] args) {
            stmts.Add(new CodeSnippetStatement(string.Format(text, args)));
        }

        public static void If(this CodeStatementCollection stmts, CodeExpression condition, Action<CodeStatementCollection> body) {
            CodeStatementCollection ifTrue = new CodeStatementCollection();

            body(ifTrue);

            stmts.Add(new CodeConditionStatement(condition, ifTrue.Cast<CodeStatement>().ToArray()));
        }

        public static void IfElse(this CodeStatementCollection stmts, CodeExpression condition, Action<CodeStatementCollection> bodyTrue, Action<CodeStatementCollection> bodyFalse) {
            CodeStatementCollection ifTrue = new CodeStatementCollection();
            CodeStatementCollection ifFalse = new CodeStatementCollection();

            bodyTrue(ifTrue);
            bodyFalse(ifFalse);

            stmts.Add(new CodeConditionStatement(condition, ifTrue.Cast<CodeStatement>().ToArray(), ifFalse.Cast<CodeStatement>().ToArray()));
        }

        public static CodeExpression Index(this CodeExpression expr, params CodeExpression[] indices) {
            return new CodeIndexerExpression(expr, indices);
        }

        public static CodeStatement Assign(this CodeExpression left, CodeExpression right) {
            return new CodeAssignStatement(left, right);
        }

        public static CodeExpression Expr(this string text) {
            return new CodeSnippetExpression(text);
        }

        public static CodeExpression Expr(this string text, params object[] args) {
            return new CodeSnippetExpression(string.Format(text, args));
        }

        public static CodeExpression Literal(this string text) {
            return new CodeSnippetExpression('"' + text + '"');
        }

        public static CodeExpression Literal(this bool value) {
            return new CodeSnippetExpression(value.ToString().ToLowerInvariant());
        }

        public static CodeExpression Literal(this float value) {
            return new CodeSnippetExpression(value.ToString() + "f");
        }

        public static CodeExpression Literal(this int integer) {
            return new CodeSnippetExpression(integer.ToString());
        }

        public static CodeSnippetStatement Stmt(this string text, params object[] args) {
            return new CodeSnippetStatement(string.Format(text, args));
        }

        public static CodeTypeDeclaration DeclareInterface(this CodeNamespace ns, string name, params string[] inherits) {
            CodeTypeDeclaration td;

            td = new CodeTypeDeclaration(name);
            td.IsInterface = true;
            td.BaseTypes.AddRange(inherits.Select(x => new CodeTypeReference(x)).ToArray());
            td.TypeAttributes = TypeAttributes.Public | TypeAttributes.Interface;

            ns.Types.Add(td);

            return td;
        }

        public static CodeTypeDeclaration DeclareStruct(this CodeNamespace ns, string name) {
            CodeTypeDeclaration td;

            td = new CodeTypeDeclaration(name);
            td.IsStruct = true;
            td.TypeAttributes = TypeAttributes.Public;

            ns.Types.Add(td);

            return td;
        }

        public static CodeTypeDeclaration DeclareClass(this CodeNamespace ns, string name) {
            CodeTypeDeclaration td;

            td = new CodeTypeDeclaration(name);
            td.IsClass = true;
            td.TypeAttributes = TypeAttributes.Public;

            ns.Types.Add(td);

            return td;
        }
    }
}