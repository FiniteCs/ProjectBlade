using Blade.CodeAnalysis.Lowering;
using Blade.CodeAnalysis.Symbols;
using Blade.CodeAnalysis.Syntax;
using Blade.CodeAnalysis.Text;

namespace Blade.CodeAnalysis.Binding
{
    internal sealed class Binder
    {
        private readonly DiagnosticBag _diagnostics = new();
        private readonly FunctionSymbol _function;
        private BoundScope _scope;
        private string _curentVariableName;

        public Binder(BoundScope parent, FunctionSymbol function = null, ClassSymbol @class = null)
        {
            _scope = new BoundScope(parent);
            _function = function;

            if (function != null)
            {
                foreach (ParameterSymbol p in function.Parameters)
                    _scope.TryDeclareVariable(p);
            }

            if (@class != null)
            {
                foreach (FunctionSymbol functionSymbol in @class.Members)
                    _scope.TryDeclareFunction(functionSymbol);
            }
        }

        public DiagnosticBag Diagnostics => _diagnostics;

        #region Scope Binding

        public static BoundGlobalScope BindGlobalScope(BoundGlobalScope previous, CompilationUnitSyntax syntax)
        {
            BoundScope parentScope = CreateParentScope(previous);
            Binder binder = new(parentScope, default, default);

            foreach (MemberSyntax member in syntax.Members)
                binder.BindMembers(member, binder._scope);

            ImmutableArray<BoundStatement>.Builder statements = ImmutableArray.CreateBuilder<BoundStatement>();

            foreach (GlobalStatementSyntax globalStatement in syntax.Members.OfType<GlobalStatementSyntax>())
            {
                BoundStatement statement = binder.BindStatement(globalStatement.Statement);
                statements.Add(statement);
            }

            ImmutableArray<FunctionSymbol> functions = binder._scope.GetDeclaredFunctions();
            ImmutableArray<VariableSymbol> variables = binder._scope.GetDeclaredVariables();
            ImmutableArray<ClassSymbol> classes = binder._scope.GetDeclaredClasses();
            ImmutableArray<Diagnostic> diagnostics = binder.Diagnostics.ToImmutableArray();

            if (previous != null)
                diagnostics = diagnostics.InsertRange(0, previous.Diagnostics);

            BoundGlobalScope globalScope = new(previous, diagnostics, functions, variables, classes, statements.ToImmutable());
            return globalScope;
        }

        public static BoundProgram BindProgram(BoundGlobalScope globalScope)
        {
            BoundScope parentScope = CreateParentScope(globalScope);

            ImmutableDictionary<FunctionSymbol, BoundBlockStatement<BoundStatement>>.Builder functionBodies = ImmutableDictionary.CreateBuilder<FunctionSymbol, BoundBlockStatement<BoundStatement>>();
            ImmutableDictionary<ClassSymbol, Class>.Builder classes = ImmutableDictionary.CreateBuilder<ClassSymbol, Class>();

            ImmutableArray<Diagnostic>.Builder diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

            BoundGlobalScope scope = globalScope;

            while (scope != null)
            {
                foreach (FunctionSymbol function in scope.Functions)
                {
                    Binder binder = new(parentScope, function);
                    BoundStatement body = binder.BindStatement(function.Declaration.Body);
                    BoundBlockStatement<BoundStatement> loweredBody = Lowerer.Lower(body);
                    functionBodies.Add(function, loweredBody);

                    diagnostics.AddRange(binder.Diagnostics);
                }

                foreach (ClassSymbol @class in scope.Classes)
                {
                    ImmutableDictionary<FunctionSymbol, BoundBlockStatement<BoundStatement>>.Builder classFunctionBodies
                        = ImmutableDictionary.CreateBuilder<FunctionSymbol, BoundBlockStatement<BoundStatement>>();
                    Binder binder = new(parentScope, default, @class);
                    foreach (FunctionSymbol function in @class.Members)
                    {
                        binder.BindFunctionDeclaration(function.Declaration, parentScope);
                        classFunctionBodies.Add(function, binder.BindBlockStatement(function.Declaration.Body, binder.BindStatement));
                    }

                    diagnostics.AddRange(binder.Diagnostics);
                    classes.Add(@class, new Class(classFunctionBodies.ToImmutable()));
                }

                scope = scope.Previous;
            }

            BoundBlockStatement<BoundStatement> statement = Lowerer.Lower<BoundStatement>(new BoundBlockStatement<BoundStatement>(globalScope.Statements));

            return new BoundProgram(diagnostics.ToImmutable(), functionBodies.ToImmutable(), statement, classes.ToImmutable());
        }

        private static BoundScope CreateParentScope(BoundGlobalScope previous)
        {
            Stack<BoundGlobalScope> stack = new();
            while (previous != null)
            {
                stack.Push(previous);
                previous = previous.Previous;
            }

            BoundScope parent = CreateRootScope();
            while (stack.Count > 0)
            {
                previous = stack.Pop();
                BoundScope scope = new(parent);

                foreach (FunctionSymbol f in previous.Functions)
                    scope.TryDeclareFunction(f);

                foreach (VariableSymbol v in previous.Variables)
                    scope.TryDeclareVariable(v);

                parent = scope;
            }
            return parent;
        }

        private static BoundScope CreateRootScope()
        {
            BoundScope result = new(null);
            foreach (FunctionSymbol f in BuiltinFunctions.GetAll())
                result.TryDeclareFunction(f);
            return result;
        }

        private void BindFunctionDeclaration(FunctionDeclarationSyntax syntax, BoundScope scope)
        {
            ImmutableArray<ParameterSymbol>.Builder parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();

            HashSet<string> seenParameterNames = new();

            foreach (ParameterSyntax parameterSyntax in syntax.Parameters)
            {
                string parameterName = parameterSyntax.Identifier.Text;
                TypeSymbol parameterType = BindTypeClause(parameterSyntax.Type);
                if (!seenParameterNames.Add(parameterName))
                {
                    _diagnostics.ReportParameterAlreadyDeclared(parameterSyntax.Span, parameterName);
                }
                else
                {
                    ParameterSymbol parameter = new(parameterName, parameterType);
                    parameters.Add(parameter);
                }
            }

            TypeSymbol type = BindTypeClause(syntax.Type) ?? TypeSymbol.Void;

            if (type != TypeSymbol.Void)
                _diagnostics.XXX_ReportFunctionsAreUnsupported(syntax.Type.Span);

            FunctionSymbol function = new(syntax.Identifier.Text, parameters.ToImmutable(), type, syntax);
            if (!scope.TryDeclareFunction(function))
                _diagnostics.ReportSymbolAlreadyDeclared(syntax.Identifier.Span, function.Name);
        }

        private void BindClassDeclaration(ClassDeclarationSyntax syntax, BoundScope scope)
        {
            BoundScope classScope = new(scope);
            foreach (MemberSyntax member in syntax.ClassBody.BlockMembers)
                BindMembers(member, classScope);

            ClassSymbol @class = new(syntax.IdentifierToken.Text, classScope.GetMembers(), classScope, syntax);
            if (!scope.TryDeclareClass(@class))
                _diagnostics.ReportSymbolAlreadyDeclared(syntax.IdentifierToken.Span, syntax.IdentifierToken.Text);
        }

        private void BindMembers(MemberSyntax member, BoundScope scope)
        {
            if (member is FunctionDeclarationSyntax function)
                BindFunctionDeclaration(function, scope);

            if (member is ClassDeclarationSyntax @class)
            {
                BindClassDeclaration(@class, scope);
            }
        }

        #endregion

        #region Bound Statement Binding

        private BoundStatement BindStatement(StatementSyntax syntax)
        {
            switch (syntax.Kind)
            {
                case SyntaxKind.BlockSyntax:
                    return BindBlockStatement((BlockSyntax<StatementSyntax>)syntax, BindStatement);
                case SyntaxKind.VariableDeclaration:
                    return BindVariableDeclaration((VariableDeclarationSyntax)syntax);
                case SyntaxKind.IfStatement:
                    return BindIfStatement((IfStatementSyntax)syntax);
                case SyntaxKind.WhileStatement:
                    return BindWhileStatement((WhileStatementSyntax)syntax);
                case SyntaxKind.DoWhileStatement:
                    return BindDoWhileStatement((DoWhileStatementSyntax)syntax);
                case SyntaxKind.ForStatement:
                    return BindForStatement((ForStatementSyntax)syntax);
                case SyntaxKind.ExpressionStatement:
                    return BindExpressionStatement((ExpressionStatementSyntax)syntax);
                default:
                    throw new Exception($"Unexpected syntax {syntax.Kind}");
            }
        }

        private BoundBlockStatement<TBoundBlockMemberType> BindBlockStatement<TBlockSyntaxType, TBoundBlockMemberType>
            (BlockSyntax<TBlockSyntaxType> syntax, Func<TBlockSyntaxType, TBoundBlockMemberType> func)
            where TBlockSyntaxType : SyntaxNode
        {
            ImmutableArray<TBoundBlockMemberType>.Builder statements = ImmutableArray.CreateBuilder<TBoundBlockMemberType>();
            _scope = new BoundScope(_scope);
            foreach (TBlockSyntaxType syntaxType in syntax.BlockMembers)
            {
                TBoundBlockMemberType statement = func.Invoke(syntaxType);
                statements.Add(statement);
            }

            _scope = _scope.Parent;
            return new BoundBlockStatement<TBoundBlockMemberType>(statements.ToImmutable());
        }

        private BoundStatement BindVariableDeclaration(VariableDeclarationSyntax syntax)
        {
            _curentVariableName = syntax.Identifier.Text;
            bool isReadOnly = syntax.Keyword.Kind == SyntaxKind.LetKeyword;
            TypeSymbol type = BindTypeClause(syntax.TypeClause);
            BoundExpression initializer = BindExpression(syntax.Initializer);
            TypeSymbol variableType = type ?? initializer.Type;
            VariableSymbol variable = BindVariable(syntax.Identifier, isReadOnly, variableType);
            BoundExpression convertedInitializer = BindConversion(syntax.Initializer.Span, initializer, variableType);

            return new BoundVariableDeclaration(variable, convertedInitializer);
        }

        private BoundStatement BindIfStatement(IfStatementSyntax syntax)
        {
            BoundExpression condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
            BoundStatement thenStatement = BindStatement(syntax.ThenStatement);
            BoundStatement elseStatement = syntax.ElseClause == null ? null : BindStatement(syntax.ElseClause.ElseStatement);
            return new BoundIfStatement(condition, thenStatement, elseStatement);
        }

        private BoundStatement BindWhileStatement(WhileStatementSyntax syntax)
        {
            BoundExpression condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
            BoundStatement body = BindStatement(syntax.Body);
            return new BoundWhileStatement(condition, body);
        }

        private BoundStatement BindDoWhileStatement(DoWhileStatementSyntax syntax)
        {
            BoundStatement body = BindStatement(syntax.Body);
            BoundExpression condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
            return new BoundDoWhileStatement(body, condition);
        }

        private BoundStatement BindForStatement(ForStatementSyntax syntax)
        {
            BoundExpression lowerBound = BindExpression(syntax.LowerBound, TypeSymbol.Int);
            BoundExpression upperBound = BindExpression(syntax.UpperBound, TypeSymbol.Int);
            _scope = new BoundScope(_scope);
            VariableSymbol variable = BindVariable(syntax.Identifier, isReadOnly: true, TypeSymbol.Int);
            BoundStatement body = BindStatement(syntax.Body);
            _scope = _scope.Parent;
            return new BoundForStatement(variable, lowerBound, upperBound, body);
        }

        private BoundStatement BindExpressionStatement(ExpressionStatementSyntax syntax)
        {
            BoundExpression expression = BindExpression(syntax.Expression);
            return new BoundExpressionStatement(expression);
        }

        #endregion

        #region Bound Expression Binding

        private BoundExpression BindExpression(ExpressionSyntax syntax)
        {
            BoundExpression result = BindExpressionInternal(syntax);
            return result;
        }

        private BoundExpression BindExpression(ExpressionSyntax syntax, TypeSymbol targetType)
        {
            return BindConversion(syntax, targetType);
        }

        private BoundExpression BindExpressionInternal(ExpressionSyntax syntax)
        {
            switch (syntax.Kind)
            {
                case SyntaxKind.ParenthesizedExpression:
                    return BindParenthesizedExpression((ParenthesizedExpressionSyntax)syntax);
                case SyntaxKind.LiteralExpression:
                    return BindLiteralExpression((LiteralExpressionSyntax)syntax);
                case SyntaxKind.NameExpression:
                    return BindNameExpression((NameExpressionSyntax)syntax);
                case SyntaxKind.AssignmentExpression:
                    return BindAssignmentExpression((AssignmentExpressionSyntax)syntax);
                case SyntaxKind.UnaryExpression:
                    return BindUnaryExpression((UnaryExpressionSyntax)syntax);
                case SyntaxKind.BinaryExpression:
                    return BindBinaryExpression((BinaryExpressionSyntax)syntax);
                case SyntaxKind.CallExpression:
                    return BindCallExpression((CallExpressionSyntax)syntax);
                case SyntaxKind.ArrayInitializerExpression:
                    return BindArrayInitializerExpression((ArrayInitializerExpression)syntax);
                case SyntaxKind.ElementAccessExpression:
                    return BindElementAccessExpression((ElementAccessExpression)syntax);
                case SyntaxKind.MemberAccessExpression:
                    return BindMemberAccessExpression((MemberAccessExpression)syntax);
                default:
                    throw new Exception($"Unexpected syntax {syntax.Kind}");
            }
        }

        private BoundExpression BindParenthesizedExpression(ParenthesizedExpressionSyntax syntax)
        {
            return BindExpression(syntax.Expression);
        }

        private BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax)
        {
            object value = syntax.Value ?? 0;
            return new BoundLiteralExpression(value);
        }

        private BoundExpression BindNameExpression(NameExpressionSyntax syntax)
        {
            string name = syntax.IdentifierToken.Text;
            if (syntax.IdentifierToken.IsMissing)
            {
                // This means the token was inserted by the parser. We already
                // reported error so we can just return an error expression.
                return new BoundErrorExpression();
            }
            if (!_scope.TryLookupVariable(name, out VariableSymbol variable))
            {
                _diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
                return new BoundErrorExpression();
            }
            return new BoundVariableExpression(variable);
        }

        private BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax syntax)
        {
            string name = syntax.IdentifierToken.Text;
            BoundExpression boundExpression = BindExpression(syntax.Expression);
            if (!_scope.TryLookupVariable(name, out VariableSymbol variable))
            {
                _diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
                return boundExpression;
            }
            if (variable.IsReadOnly)
                _diagnostics.ReportCannotAssign(syntax.EqualsToken.Span, name);

            BoundExpression convertedExpression = BindConversion(syntax.Expression.Span, boundExpression, variable.Type);

            return new BoundAssignmentExpression(variable, convertedExpression);
        }

        private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
        {
            BoundExpression boundOperand = BindExpression(syntax.Operand);
            if (boundOperand.Type == TypeSymbol.Error)
                return new BoundErrorExpression();
            BoundUnaryOperator boundOperator = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, boundOperand.Type);
            if (boundOperator == null)
            {
                _diagnostics.ReportUndefinedUnaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundOperand.Type);
                return new BoundErrorExpression();
            }
            return new BoundUnaryExpression(boundOperator, boundOperand);
        }

        private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
        {
            BoundExpression boundLeft = BindExpression(syntax.Left);
            BoundExpression boundRight = BindExpression(syntax.Right);
            if (boundLeft.Type == TypeSymbol.Error || boundRight.Type == TypeSymbol.Error)
                return new BoundErrorExpression();
            BoundBinaryOperator boundOperator = BoundBinaryOperator.Bind(syntax.OperatorToken.Kind, boundLeft.Type, boundRight.Type);
            if (boundOperator == null)
            {
                _diagnostics.ReportUndefinedBinaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundLeft.Type, boundRight.Type);
                return new BoundErrorExpression();
            }
            return new BoundBinaryExpression(boundLeft, boundOperator, boundRight);
        }

        private BoundExpression BindCallExpression(CallExpressionSyntax syntax, BoundScope scope = null)
        {
            if (scope is null)
                scope = _scope;

            if (syntax.Arguments.Count == 1 && LookupType(syntax.TypeSyntax) is TypeSymbol type)
                return BindConversion(syntax.Arguments[0], type, allowExplicit: true);

            ImmutableArray<BoundExpression>.Builder boundArguments = ImmutableArray.CreateBuilder<BoundExpression>();

            foreach (ExpressionSyntax argument in syntax.Arguments)
            {
                BoundExpression boundArgument = BindExpression(argument);
                boundArguments.Add(boundArgument);
            }
            if (!scope.TryLookupFunction(syntax.TypeSyntax.TypeIdentifier.Text, out FunctionSymbol function))
            {
                _diagnostics.ReportUndefinedFunction(syntax.TypeSyntax.TypeIdentifier.Span, syntax.TypeSyntax.TypeIdentifier.Text);
                return new BoundErrorExpression();
            }
            if (syntax.Arguments.Count != function.Parameters.Length)
            {
                _diagnostics.ReportWrongArgumentCount(syntax.Span, function.Name, function.Parameters.Length, syntax.Arguments.Count);
                return new BoundErrorExpression();
            }
            for (int i = 0; i < syntax.Arguments.Count; i++)
            {
                BoundExpression argument = boundArguments[i];
                ParameterSymbol parameter = function.Parameters[i];

                if (argument.Type != parameter.Type)
                {
                    _diagnostics.ReportWrongArgumentType(syntax.Arguments[i].Span, parameter.Name, parameter.Type, argument.Type);
                    return new BoundErrorExpression();
                }
            }

            return new BoundCallExpression(function, boundArguments.ToImmutable());
        }

        private BoundExpression BindArrayInitializerExpression(ArrayInitializerExpression syntax)
        {
            TypeSymbol type = BindTypeClause(syntax.TypeClauseSyntax);
            ImmutableArray<BoundExpression> expressions = ImmutableArray.Create<BoundExpression>();
            foreach (ArrayElementSyntax element in syntax.ArrayElements)
                expressions = expressions.Add(BindExpression(element.Expression));

            ImmutableArray<ArrayElementSymbol> arrayElements = ImmutableArray.Create<ArrayElementSymbol>();
            for (int i = 0; i < expressions.Length; i++)
            {
                BoundExpression expression = expressions[i];
                arrayElements = arrayElements.Add(new ArrayElementSymbol($"_element{i}_", expression.Type));
                if (expression.Type != type)
                    _diagnostics.ReportCannotConvert(syntax.ArrayElements[i].Span, expression.Type, type);
            }

            ArrayTypeSymbol arrayType = new(type);
            ArraySymbol arraySymbol = new(_curentVariableName, arrayElements, arrayType);
            _scope.TryDeclareArray(arraySymbol);

            return new BoundArrayInitializerExpression(arraySymbol, expressions);
        }

        private BoundExpression BindElementAccessExpression(ElementAccessExpression syntax)
        {
            if (!_scope.TryLookupArray(syntax.IdentifierToken.Text, out ArraySymbol array))
                _diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, syntax.IdentifierToken.Text);

            BoundExpression indexer = BindExpression(syntax.IndexerExpression);
            if (indexer.Type != TypeSymbol.Int)
                _diagnostics.ReportCannotConvert(syntax.IndexerExpression.Span, indexer.Type, TypeSymbol.Int);

            return new BoundElementAccesssExpression(array, indexer);
        }

        private BoundExpression BindMemberAccessExpression(MemberAccessExpression syntax)
        {
            ClassSymbol classSymbol = null;
            BoundScope scope = _scope;
            while (scope != null)
            {
                foreach (ClassSymbol @class in scope.GetDeclaredClasses())
                {
                    if (@class.Name == syntax.TypeIdentifier.Text)
                    {
                        classSymbol = @class;
                        goto EndOfLoop;
                    }
                }

                scope = scope.Parent;
            }

        EndOfLoop:

            if (classSymbol == null)
            {
                _diagnostics.ReportUndefinedType(syntax.TypeIdentifier.Span, syntax.TypeIdentifier.Text);
                return new BoundErrorExpression();
            }

            string memberName = syntax.MemberExpression is CallExpressionSyntax callExpression
                                                        ? callExpression.TypeSyntax.TypeIdentifier.Text
                                                        : string.Empty;
            BoundExpression boundExpression = BindCallExpression((CallExpressionSyntax)syntax.MemberExpression, classSymbol.Scope);
            MemberSymbol memberSymbol = null;
            foreach (MemberSymbol member in classSymbol.Members)
            {
                if (member.Name == memberName)
                    memberSymbol = member;
            }

            if (memberSymbol == null)
            {
                _diagnostics.ReportUndefinedMember(syntax.MemberExpression.Span, memberName);
                return new BoundErrorExpression();
            }

            return new BoundMemberAccessExpression(memberSymbol, classSymbol, boundExpression);
        }

        private BoundExpression BindConversion(ExpressionSyntax syntax, TypeSymbol type, bool allowExplicit = false)
        {
            BoundExpression expression = BindExpression(syntax);
            return BindConversion(syntax.Span, expression, type, allowExplicit);
        }

        private BoundExpression BindConversion(TextSpan diagnosticSpan, BoundExpression expression, TypeSymbol type, bool allowExplicit = false)
        {
            Conversion conversion = Conversion.Classify(expression.Type, type);

            if (!conversion.Exists)
            {
                if (expression.Type != TypeSymbol.Error && type != TypeSymbol.Error)
                    _diagnostics.ReportCannotConvert(diagnosticSpan, expression.Type, type);

                return new BoundErrorExpression();
            }

            if (!allowExplicit && conversion.IsExplicit)
            {
                _diagnostics.ReportCannotConvertImplicitly(diagnosticSpan, expression.Type, type);
            }

            if (conversion.IsIdentity)
                return expression;

            return new BoundConversionExpression(type, expression);
        }

        #endregion

        #region Miscellaneous Binding

        private VariableSymbol BindVariable(SyntaxToken identifier, bool isReadOnly, TypeSymbol type)
        {
            string name = identifier.Text ?? "?";
            bool declare = !identifier.IsMissing;
            VariableSymbol variable = _function == null
                                ? new GlobalVariableSymbol(name, isReadOnly, type)
                                : new LocalVariableSymbol(name, isReadOnly, type);

            if (declare && !_scope.TryDeclareVariable(variable))
                _diagnostics.ReportSymbolAlreadyDeclared(identifier.Span, name);

            return variable;
        }

        private TypeSymbol BindTypeClause(TypeClauseSyntax syntax)
        {
            if (syntax == null)
                return null;

            TypeSymbol type = LookupType(syntax.TypeSyntax);
            if (type == null)
                _diagnostics.ReportUndefinedType(syntax.TypeSyntax.TypeIdentifier.Span, syntax.TypeSyntax.TypeIdentifier.Text);

            return type;
        }

        private static TypeSymbol LookupTypeSymbol(string name)
        {
            switch (name)
            {
                case "bool":
                    return TypeSymbol.Bool;
                case "int":
                    return TypeSymbol.Int;
                case "string":
                    return TypeSymbol.String;
                default:
                    return null;
            }
        }

        private static TypeSymbol LookupType(TypeSyntax typeSyntax)
        {
            TypeSymbol type = LookupTypeSymbol(typeSyntax.TypeIdentifier.Text);
            if (typeSyntax.OpenBracket != null)
                type = new ArrayTypeSymbol(LookupTypeSymbol(typeSyntax.TypeIdentifier.Text));

            return type;
        }

        #endregion
    }
}
