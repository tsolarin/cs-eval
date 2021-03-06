using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Eval.Csharp
{
    class ExpressionTransformer
    {
        private List<ParameterExpression> _exprVariables;
        private List<Expression> _expressions;
        private ExpressionStatementSyntax _expr;
        private object _context;
        private List<Type> _exportedTypes;

        public ExpressionTransformer(ExpressionStatementSyntax expr, Dictionary<string, object> variables, object context, List<Type> types)
        {
            _expr = expr;
            _context = context;
            _exportedTypes = types;
            _expressions = new List<Expression>();
            _exprVariables = new List<ParameterExpression>();

            var varsList = variables.ToList();
            for (int i = 0; i < varsList.Count; i++)
            {
                _exprVariables.Add(Expression.Variable(varsList[i].Value.GetType(), varsList[i].Key));
                _expressions.Add(Expression.Assign(_exprVariables[i], Expression.Constant(varsList[i].Value, varsList[i].Value.GetType())));
            }
        }

        private ConstantExpression TransformLiteralExpression(LiteralExpressionSyntax node)
        {
            string literal = node.GetText().ToString();
            switch (node.Kind())
            {
                case SyntaxKind.NumericLiteralExpression:
                    return Expression.Constant(int.Parse(literal), typeof(int));
                case SyntaxKind.StringLiteralExpression:
                    return Expression.Constant(literal.TrimStart('"').TrimEnd('"'), typeof(string));
                case SyntaxKind.CharacterLiteralExpression:
                    return Expression.Constant(literal.TrimStart('\'').TrimEnd('\'')[0], typeof(char));
                case SyntaxKind.TrueLiteralExpression:
                    return Expression.Constant(true, typeof(bool));
                case SyntaxKind.FalseLiteralExpression:
                    return Expression.Constant(true, typeof(bool));
                case SyntaxKind.NullLiteralExpression:
                    return Expression.Constant(null);
                default:
                    return null;
            }
        }

        private BinaryExpression TransformBinaryExpressionSyntax(BinaryExpressionSyntax node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.AddExpression:
                    return Expression.Add(TransformExpressionSyntax(node.Left), TransformExpressionSyntax(node.Right));
                case SyntaxKind.SubtractExpression:
                    return Expression.Subtract(TransformExpressionSyntax(node.Left), TransformExpressionSyntax(node.Right));
                case SyntaxKind.MultiplyExpression:
                    return Expression.Multiply(TransformExpressionSyntax(node.Left), TransformExpressionSyntax(node.Right));
                case SyntaxKind.DivideExpression:
                    return Expression.Divide(TransformExpressionSyntax(node.Left), TransformExpressionSyntax(node.Right));
                case SyntaxKind.ModuloExpression:
                    return Expression.Modulo(TransformExpressionSyntax(node.Left), TransformExpressionSyntax(node.Right));
                case SyntaxKind.EqualsExpression:
                    return Expression.Equal(TransformExpressionSyntax(node.Left), TransformExpressionSyntax(node.Right));
                case SyntaxKind.NotEqualsExpression:
                    return Expression.NotEqual(TransformExpressionSyntax(node.Left), TransformExpressionSyntax(node.Right));
                case SyntaxKind.LessThanExpression:
                    return Expression.LessThan(TransformExpressionSyntax(node.Left), TransformExpressionSyntax(node.Right));
                case SyntaxKind.LessThanOrEqualExpression:
                    return Expression.LessThanOrEqual(TransformExpressionSyntax(node.Left), TransformExpressionSyntax(node.Right));
                case SyntaxKind.GreaterThanExpression:
                    return Expression.GreaterThan(TransformExpressionSyntax(node.Left), TransformExpressionSyntax(node.Right));
                case SyntaxKind.GreaterThanOrEqualExpression:
                    return Expression.GreaterThanOrEqual(TransformExpressionSyntax(node.Left), TransformExpressionSyntax(node.Right));
                case SyntaxKind.LogicalAndExpression:
                    return Expression.And(TransformExpressionSyntax(node.Left), TransformExpressionSyntax(node.Right));
                case SyntaxKind.LogicalOrExpression:
                    return Expression.Or(TransformExpressionSyntax(node.Left), TransformExpressionSyntax(node.Right));
                case SyntaxKind.CoalesceExpression:
                    return Expression.Coalesce(TransformExpressionSyntax(node.Left), TransformExpressionSyntax(node.Right));
                default:
                    return null;
            }
        }

        private BinaryExpression TransformAssignmentExpressionSyntax(AssignmentExpressionSyntax node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.AddAssignmentExpression:
                    return Expression.AddAssign(TransformExpressionSyntax(node.Left), TransformExpressionSyntax(node.Right));
                case SyntaxKind.SubtractAssignmentExpression:
                    return Expression.SubtractAssign(TransformExpressionSyntax(node.Left), TransformExpressionSyntax(node.Right));
                case SyntaxKind.MultiplyAssignmentExpression:
                    return Expression.MultiplyAssign(TransformExpressionSyntax(node.Left), TransformExpressionSyntax(node.Right));
                case SyntaxKind.DivideAssignmentExpression:
                    return Expression.DivideAssign(TransformExpressionSyntax(node.Left), TransformExpressionSyntax(node.Right));
                case SyntaxKind.ModuloAssignmentExpression:
                    return Expression.ModuloAssign(TransformExpressionSyntax(node.Left), TransformExpressionSyntax(node.Right));
                default:
                    return null;
            }
        }

        private UnaryExpression TransformPrefixUnaryExpressionSyntax(PrefixUnaryExpressionSyntax node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.PreIncrementExpression:
                    return Expression.PreIncrementAssign(TransformExpressionSyntax(node.Operand));
                case SyntaxKind.PreDecrementExpression:
                    return Expression.PreDecrementAssign(TransformExpressionSyntax(node.Operand));
                case SyntaxKind.LogicalNotExpression:
                    return Expression.Negate(TransformExpressionSyntax(node.Operand));
                default:
                    return null;
            }
        }

        private UnaryExpression TransformPostfixUnaryExpressionSyntax(PostfixUnaryExpressionSyntax node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.PostIncrementExpression:
                    return Expression.PostIncrementAssign(TransformExpressionSyntax(node.Operand));
                case SyntaxKind.PostDecrementExpression:
                    return Expression.PostDecrementAssign(TransformExpressionSyntax(node.Operand));
                default:
                    return null;
            }
        }

        private ParameterExpression TransformIdentifierNameSyntax(IdentifierNameSyntax node)
        {
            string identifier = node.Identifier.ValueText;
            var param = _exprVariables.FirstOrDefault(v => v.Name == identifier);

            if (param == null)
            {
                BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                FieldInfo fieldInfo = _context.GetType().GetField(identifier, bindingFlags);
                PropertyInfo propInfo = _context.GetType().GetProperty(identifier, bindingFlags);

                if (fieldInfo == null && propInfo == null)
                    throw new Exception(string.Format("CS0103: The name '{0}' does not exist in the current context", identifier));

                var value = fieldInfo?.GetValue(null) ?? propInfo?.GetValue(null);
                var variable = Expression.Variable(value.GetType(), identifier);
                _exprVariables.Add(variable);
                _expressions.Add(Expression.Assign(variable, Expression.Constant(value, value.GetType())));
                param = variable;
            }

            return param;
        }

        private ConstantExpression TransformThisExpressionSyntax(ThisExpressionSyntax node)
        {
            return Expression.Constant(this._context, this._context.GetType());
        }

        private NewExpression TransformObjectCreationExpressionSyntax(ObjectCreationExpressionSyntax node)
        {
            var suppliedArgs = node.ArgumentList.Arguments;
            int argsLength = suppliedArgs.Count();
            Expression[] arguments = new Expression[argsLength];

            for (int i = 0; i < argsLength; i++)
                arguments[i] = TransformExpressionSyntax(suppliedArgs[i].Expression);

            string typeName = node.Type.ToString();
            Type type = _exportedTypes.FirstOrDefault(t => t.Name == typeName || t.FullName == typeName);

            if (type == null)
                throw new Exception(string.Format("CS0246: The type or namespace name '{0}' could not be found (are you missing a using directive or an assembly reference?)", typeName));

            var constructors = type.GetTypeInfo().DeclaredConstructors;
            foreach (ConstructorInfo constructor in constructors)
            {
                try
                {
                    return Expression.New(constructor, arguments);
                }
                catch (System.Exception) { }
            }
            throw new Exception(string.Format("Constructor with specified arguments could not be found on type '{0}'", typeName));
        }

        private MemberExpression TransformMemberAccessExpressionSyntax(MemberAccessExpressionSyntax node)
        {
            return Expression.PropertyOrField(TransformExpressionSyntax(node.Expression), node.Name.Identifier.ValueText);
        }

        private MethodCallExpression TransformInvocationExpressionSyntax(InvocationExpressionSyntax node)
        {
            var suppliedArgs = node.ArgumentList.Arguments;
            int argsLength = suppliedArgs.Count();
            Expression[] arguments = new Expression[argsLength];

            for (int i = 0; i < argsLength; i++)
                arguments[i] = TransformExpressionSyntax(suppliedArgs[i].Expression);

            ExpressionSyntax invoker = node.Expression;
            if (invoker.Kind() == SyntaxKind.SimpleMemberAccessExpression)
            {
                string methodName = ((MemberAccessExpressionSyntax)invoker).Name.Identifier.ValueText;
                var instance = TransformExpressionSyntax(((MemberAccessExpressionSyntax)invoker).Expression);
                return Expression.Call(instance, methodName, null, arguments);
            }
            else if (invoker.Kind() == SyntaxKind.IdentifierName)
            {
                string methodName = ((IdentifierNameSyntax)invoker).Identifier.ValueText;

                if (this._context == null)
                    throw new Exception(string.Format("CS0103: The name '{0}' does not exist in the current context", methodName));

                return Expression.Call(this._context.GetType(), methodName, null, arguments);
            }

            throw new Exception("Unsupported Expression");
        }

        private Expression TransformExpressionSyntax(ExpressionSyntax node)
        {
            switch (node.GetType().ToString())
            {
                case "Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax":
                    return TransformLiteralExpression((LiteralExpressionSyntax)node);
                case "Microsoft.CodeAnalysis.CSharp.Syntax.BinaryExpressionSyntax":
                    return TransformBinaryExpressionSyntax((BinaryExpressionSyntax)node);
                case "Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax":
                    return TransformIdentifierNameSyntax((IdentifierNameSyntax)node);
                case "Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax":
                    return TransformMemberAccessExpressionSyntax((MemberAccessExpressionSyntax)node);
                case "Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax":
                    return TransformInvocationExpressionSyntax((InvocationExpressionSyntax)node);
                case "Microsoft.CodeAnalysis.CSharp.Syntax.ThisExpressionSyntax":
                    return TransformThisExpressionSyntax((ThisExpressionSyntax)node);
                case "Microsoft.CodeAnalysis.CSharp.Syntax.AssignmentExpressionSyntax":
                    return TransformAssignmentExpressionSyntax((AssignmentExpressionSyntax)node);
                case "Microsoft.CodeAnalysis.CSharp.Syntax.PrefixUnaryExpressionSyntax":
                    return TransformPrefixUnaryExpressionSyntax((PrefixUnaryExpressionSyntax)node);
                case "Microsoft.CodeAnalysis.CSharp.Syntax.PostfixUnaryExpressionSyntax":
                    return TransformPostfixUnaryExpressionSyntax((PostfixUnaryExpressionSyntax)node);
                case "Microsoft.CodeAnalysis.CSharp.Syntax.ObjectCreationExpressionSyntax":
                    return TransformObjectCreationExpressionSyntax((ObjectCreationExpressionSyntax)node);
                default:
                    throw new Exception("Unsupported Expression");
            }
        }

        public Expression Transform()
        {
            _expressions.Add(
                TransformExpressionSyntax(_expr.ChildNodes().OfType<ExpressionSyntax>().First())
            );
            return Expression.Block(
                _exprVariables,
                _expressions
            );
        }
    }
}
