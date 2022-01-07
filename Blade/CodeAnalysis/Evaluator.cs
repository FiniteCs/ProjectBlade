using Blade.CodeAnalysis.Binding;
using Blade.CodeAnalysis.Symbols;

namespace Blade.CodeAnalysis
{
    internal sealed class Evaluator
    {
        private readonly BoundProgram _program;
        private readonly Dictionary<VariableSymbol, object> _globals;
        private readonly Stack<Dictionary<VariableSymbol, object>> _locals = new();
        private readonly Dictionary<ArraySymbol, object[]> _arrays = new();
        private Random _random;

        private object _lastValue;

        public Evaluator(BoundProgram program, Dictionary<VariableSymbol, object> variables)
        {
            _program = program;
            _globals = variables;
            _locals.Push(new Dictionary<VariableSymbol, object>());
        }

        public object Evaluate()
        {
            return EvaluateStatement(_program.Statement);
        }

        private object EvaluateStatement<TBlockMember>(BoundBlockStatement<TBlockMember> body)
            where TBlockMember : BoundStatement
        {
            Dictionary<BoundLabel, int> labelToIndex = new();

            for (int i = 0; i < body.Statements.Length; i++)
            {
                if (body.Statements[i] is BoundLabelStatement l)
                    labelToIndex.Add(l.Label, i + 1);
            }

            int index = 0;

            while (index < body.Statements.Length)
            {
                TBlockMember member = body.Statements[index];

                switch (member.Kind)
                {
                    case BoundNodeKind.VariableDeclaration:
                        EvaluateVariableDeclaration((BoundVariableDeclaration)(BoundStatement)member);
                        index++;
                        break;
                    case BoundNodeKind.ExpressionStatement:
                        EvaluateExpressionStatement((BoundExpressionStatement)(BoundStatement)member);
                        index++;
                        break;
                    case BoundNodeKind.GotoStatement:
                        BoundGotoStatement gs = (BoundGotoStatement)(BoundStatement)member;
                        index = labelToIndex[gs.Label];
                        break;
                    case BoundNodeKind.ConditionalGotoStatement:
                        BoundConditionalGotoStatement cgs = (BoundConditionalGotoStatement)(BoundStatement)member;
                        bool condition = (bool)EvaluateExpression(cgs.Condition);
                        if (condition == cgs.JumpIfTrue)
                            index = labelToIndex[cgs.Label];
                        else
                            index++;
                        break;
                    case BoundNodeKind.LabelStatement:
                        index++;
                        break;
                    default:
                        throw new Exception($"Unexpected node {member.Kind}");
                }
            }
            return _lastValue;
        }

        private void EvaluateVariableDeclaration(BoundVariableDeclaration node)
        {
            object value = EvaluateExpression(node.Initializer);
            _lastValue = value;
            Assign(node.Variable, value);
        }

        private void EvaluateExpressionStatement(BoundExpressionStatement node)
        {
            _lastValue = EvaluateExpression(node.Expression);
        }

        private object EvaluateExpression(BoundExpression node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.LiteralExpression:
                    return EvaluateLiteralExpression((BoundLiteralExpression)node);
                case BoundNodeKind.VariableExpression:
                    return EvaluateVariableExpression((BoundVariableExpression)node);
                case BoundNodeKind.AssignmentExpression:
                    return EvaluateAssignmentExpression((BoundAssignmentExpression)node);
                case BoundNodeKind.UnaryExpression:
                    return EvaluateUnaryExpression((BoundUnaryExpression)node);
                case BoundNodeKind.BinaryExpression:
                    return EvaluateBinaryExpression((BoundBinaryExpression)node);
                case BoundNodeKind.CallExpression:
                    return EvaluateCallExpression((BoundCallExpression)node);
                case BoundNodeKind.ArrayInitializerExpression:
                    return EvaluateArrayInitializerExpression((BoundArrayInitializerExpression)node);
                case BoundNodeKind.ElementAccesssExpression:
                    return EvaluateElementAccessExpression((BoundElementAccesssExpression)node);
                case BoundNodeKind.MemberAccessExpression:
                    return EvaluateMemberAccessExpression((BoundMemberAccessExpression)node);
                case BoundNodeKind.ConversionExpression:
                    return EvaluateConversionExpression((BoundConversionExpression)node);
                default:
                    throw new Exception($"Unexpected node {node.Kind}");
            }
        }

        private static object EvaluateLiteralExpression(BoundLiteralExpression node)
        {
            return node.Value;
        }

        private object EvaluateVariableExpression(BoundVariableExpression node)
        {
            if (node.Variable.Kind == SymbolKind.GlobalVariable)
            {
                return _globals[node.Variable];
            }
            else
            {
                Dictionary<VariableSymbol, object> locals = _locals.Peek();
                return locals[node.Variable];
            }
        }

        private object EvaluateAssignmentExpression(BoundAssignmentExpression node)
        {
            object value = EvaluateExpression(node.Expression);
            Assign(node.Variable, value);
            return value;
        }

        private object EvaluateUnaryExpression(BoundUnaryExpression node)
        {
            object operand = EvaluateExpression(node.Operand);
            switch (node.Op.Kind)
            {
                case BoundUnaryOperatorKind.Identity:
                    return (int)operand;
                case BoundUnaryOperatorKind.Negation:
                    return -(int)operand;
                case BoundUnaryOperatorKind.LogicalNegation:
                    return !(bool)operand;
                case BoundUnaryOperatorKind.OnesComplement:
                    return ~(int)operand;
                default:
                    throw new Exception($"Unexpected unary operator {node.Op}");
            }
        }

        private object EvaluateBinaryExpression(BoundBinaryExpression node)
        {
            object left = EvaluateExpression(node.Left);
            object right = EvaluateExpression(node.Right);
            switch (node.Op.Kind)
            {
                case BoundBinaryOperatorKind.Addition:
                    if (node.Type == TypeSymbol.Int)
                        return (int)left + (int)right;
                    else
                        return (string)left + (string)right;
                case BoundBinaryOperatorKind.Subtraction:
                    return (int)left - (int)right;
                case BoundBinaryOperatorKind.Multiplication:
                    return (int)left * (int)right;
                case BoundBinaryOperatorKind.Division:
                    return (int)left / (int)right;
                case BoundBinaryOperatorKind.BitwiseAnd:
                    if (node.Type == TypeSymbol.Int)
                        return (int)left & (int)right;
                    else
                        return (bool)left & (bool)right;
                case BoundBinaryOperatorKind.BitwiseOr:
                    if (node.Type == TypeSymbol.Int)
                        return (int)left | (int)right;
                    else
                        return (bool)left | (bool)right;
                case BoundBinaryOperatorKind.BitwiseXor:
                    if (node.Type == TypeSymbol.Int)
                        return (int)left ^ (int)right;
                    else
                        return (bool)left ^ (bool)right;
                case BoundBinaryOperatorKind.LogicalAnd:
                    return (bool)left && (bool)right;
                case BoundBinaryOperatorKind.LogicalOr:
                    return (bool)left || (bool)right;
                case BoundBinaryOperatorKind.Equals:
                    return Equals(left, right);
                case BoundBinaryOperatorKind.NotEquals:
                    return !Equals(left, right);
                case BoundBinaryOperatorKind.Less:
                    return (int)left < (int)right;
                case BoundBinaryOperatorKind.LessOrEquals:
                    return (int)left <= (int)right;
                case BoundBinaryOperatorKind.Greater:
                    return (int)left > (int)right;
                case BoundBinaryOperatorKind.GreaterOrEquals:
                    return (int)left >= (int)right;
                default:
                    throw new Exception($"Unexpected binary operator {node.Op}");
            }
        }

        private object EvaluateCallExpression(BoundCallExpression node, ImmutableDictionary<FunctionSymbol, BoundBlockStatement<BoundStatement>> functions = null)
        {
            if (node.Function == BuiltinFunctions.Input)
            {
                return Console.ReadLine();
            }
            else if (node.Function == BuiltinFunctions.Print)
            {
                string message = (string)EvaluateExpression(node.Arguments[0]);
                Console.WriteLine(message);
                return null;
            }
            else if (node.Function == BuiltinFunctions.Rnd)
            {
                int max = (int)EvaluateExpression(node.Arguments[0]);
                if (_random == null)
                    _random = new Random();
                return _random.Next(max);
            }
            else
            {
                ImmutableDictionary<FunctionSymbol, BoundBlockStatement<BoundStatement>> _functions = functions ?? _program.Functions;
                Dictionary<VariableSymbol, object> locals = new();
                for (int i = 0; i < node.Arguments.Length; i++)
                {
                    ParameterSymbol parameter = node.Function.Parameters[i];
                    object value = EvaluateExpression(node.Arguments[i]);
                    locals.Add(parameter, value);
                }

                _locals.Push(locals);

                BoundBlockStatement<BoundStatement> statement = _functions[node.Function];
                object result = EvaluateStatement(statement);

                _locals.Pop();

                return result;
            }
        }

        private object EvaluateArrayInitializerExpression(BoundArrayInitializerExpression node)
        {
            object[] array = new object[node.Expressions.Length];
            int index = 0;
            foreach (BoundExpression item in node.Expressions)
            {
                array[index] = EvaluateExpression(item);
                index++;
            }

            _arrays.Add(node.ArraySymbol, array);
            return array;
        }

        private object EvaluateElementAccessExpression(BoundElementAccesssExpression node)
        {
            int index = (int)EvaluateExpression(node.Indexer);
            return _arrays[node.Array][index];
        }

        private object EvaluateMemberAccessExpression(BoundMemberAccessExpression node)
        {
            static Class GetLastClass(Class classObj, Class c = null)
            {
                foreach (var (ClassSymbol, Class) in classObj.Classes)
                {
                    if (classObj.Classes.Count == 0)
                        c = Class;
                    else
                        c = GetLastClass(Class, c);
                }

                return c ?? classObj;
            }

            object value = null;
            Class c = null;
            foreach (var (ClassSymbol, Class) in _program.Classes)
            {
                if (ClassSymbol.Name == node.Classes.First().Name)
                {
                    c = Class;
                    break;
                }
            }

            Class last = GetLastClass(c);
            foreach (FunctionSymbol function in last.Functions.Keys)
            {
                if (function.Name == node.Member.Name)
                {
                    value = EvaluateCallExpression((BoundCallExpression)node.Expression, last.Functions);
                    break;
                }
            }

            return value;
        }

        private object EvaluateConversionExpression(BoundConversionExpression node)
        {
            object value = EvaluateExpression(node.Expression);
            if (node.Type == TypeSymbol.Bool)
                return Convert.ToBoolean(value);
            else if (node.Type == TypeSymbol.Int)
                return Convert.ToInt32(value);
            else if (node.Type == TypeSymbol.String)
                return Convert.ToString(value);
            else
                throw new Exception($"Unexpected type {node.Type}");
        }

        private void Assign(VariableSymbol variable, object value)
        {
            if (variable.Kind == SymbolKind.GlobalVariable)
            {
                _globals[variable] = value;
            }
            else
            {
                Dictionary<VariableSymbol, object> locals = _locals.Peek();
                locals[variable] = value;
            }
        }
    }
}
