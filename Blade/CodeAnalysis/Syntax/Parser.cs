using Blade.CodeAnalysis.Text;

namespace Blade.CodeAnalysis.Syntax
{
    internal sealed class Parser
    {
        private readonly DiagnosticBag _diagnostics = new();
        private readonly ImmutableArray<SyntaxToken> _tokens;
        private int _position;

        public Parser(SourceText text)
        {
            List<SyntaxToken> tokens = new();
            Lexer lexer = new(text);
            SyntaxToken token;
            do
            {
                token = lexer.Lex();
                if (token.Kind != SyntaxKind.WhitespaceToken &&
                    token.Kind != SyntaxKind.BadToken)
                {
                    tokens.Add(token);
                }
            } while (token.Kind != SyntaxKind.EndOfFileToken);
            _tokens = tokens.ToImmutableArray();
            _diagnostics.AddRange(lexer.Diagnostics);
        }

        public DiagnosticBag Diagnostics => _diagnostics;

        #region Token Modifiers

        private SyntaxToken Peek(int offset)
        {
            int index = _position + offset;
            if (index >= _tokens.Length)
                return _tokens[^1];
            return _tokens[index];
        }

        private SyntaxToken Current => Peek(0);

        private SyntaxToken NextToken()
        {
            SyntaxToken current = Current;
            _position++;
            return current;
        }

        private SyntaxToken MatchToken(SyntaxKind kind)
        {
            if (Current.Kind == kind)
                return NextToken();
            _diagnostics.ReportUnexpectedToken(Current.Span, Current.Kind, kind);
            return new SyntaxToken(kind, Current.Position, null, null);
        }

        #endregion

        #region Miscellaneous

        public CompilationUnitSyntax ParseCompilationUnit()
        {
            ImmutableArray<MemberSyntax> members = ParseMembers();
            SyntaxToken endOfFileToken = MatchToken(SyntaxKind.EndOfFileToken);
            return new CompilationUnitSyntax(members, endOfFileToken);
        }

        private ImmutableArray<MemberSyntax> ParseMembers()
        {
            ImmutableArray<MemberSyntax>.Builder members = ImmutableArray.CreateBuilder<MemberSyntax>();

            while (Current.Kind != SyntaxKind.EndOfFileToken)
            {
                SyntaxToken startToken = Current;

                MemberSyntax member = ParseMember();
                members.Add(member);

                // If ParseMember() did not consume any tokens,
                // we need to skip the current token and continue
                // in order to avoid an infinite loop.
                //
                // We don't need to report an error, because we'll
                // already tried to parse an expression statement
                // and reported one.
                if (Current == startToken)
                    NextToken();
            }

            return members.ToImmutable();
        }

        private MemberSyntax ParseMember()
        {
            if (Current.Kind == SyntaxKind.FunctionKeyword)
                return ParseFunctionDeclaration();
            else if (Current.Kind == SyntaxKind.ClassKeyword)
                return ParseClass();

            return ParseGlobalStatement();
        }

        private MemberSyntax ParseFunctionDeclaration()
        {
            SyntaxToken functionKeyword = MatchToken(SyntaxKind.FunctionKeyword);
            SyntaxToken identifier = MatchToken(SyntaxKind.IdentifierToken);
            SyntaxToken openParenthesisToken = MatchToken(SyntaxKind.OpenParenthesisToken);
            SeparatedSyntaxList<ParameterSyntax> parameters = ParseParameterList();
            SyntaxToken closeParenthesisToken = MatchToken(SyntaxKind.CloseParenthesisToken);
            TypeClauseSyntax type = ParseOptionalTypeClause();
            BlockSyntax<StatementSyntax> body = ParseBlockSyntax(ParseStatement);
            return new FunctionDeclarationSyntax(functionKeyword, identifier, openParenthesisToken, parameters, closeParenthesisToken, type, body);
        }

        private MemberSyntax ParseGlobalStatement()
        {
            StatementSyntax statement = ParseStatement();
            return new GlobalStatementSyntax(statement);
        }

        private MemberSyntax ParseClass()
        {
            SyntaxToken classKeyword = MatchToken(SyntaxKind.ClassKeyword);
            SyntaxToken identifier = MatchToken(SyntaxKind.IdentifierToken);
            BlockSyntax<MemberSyntax> classBlock = ParseBlockSyntax(ParseMember);
            return new ClassDeclarationSyntax(classKeyword, identifier, classBlock);
        }

        private SeparatedSyntaxList<ParameterSyntax> ParseParameterList()
        {
            ImmutableArray<SyntaxNode>.Builder nodesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();

            while (Current.Kind != SyntaxKind.CloseParenthesisToken &&
                   Current.Kind != SyntaxKind.EndOfFileToken)
            {
                ParameterSyntax parameter = ParseParameter();
                nodesAndSeparators.Add(parameter);

                if (Current.Kind != SyntaxKind.CloseParenthesisToken)
                {
                    SyntaxToken comma = MatchToken(SyntaxKind.CommaToken);
                    nodesAndSeparators.Add(comma);
                }
            }

            return new SeparatedSyntaxList<ParameterSyntax>(nodesAndSeparators.ToImmutable());
        }

        private ParameterSyntax ParseParameter()
        {
            SyntaxToken identifier = MatchToken(SyntaxKind.IdentifierToken);
            TypeClauseSyntax type = ParseTypeClause();
            return new ParameterSyntax(identifier, type);
        }

        private TypeClauseSyntax ParseOptionalTypeClause()
        {
            if (Current.Kind != SyntaxKind.ColonToken)
                return null;

            return ParseTypeClause();
        }

        private TypeClauseSyntax ParseTypeClause()
        {
            SyntaxToken colonToken = MatchToken(SyntaxKind.ColonToken);
            TypeSyntax typeSyntax = ParseTypeSyntax();
            return new TypeClauseSyntax(colonToken, typeSyntax);
        }

        private SeparatedSyntaxList<ArrayElementSyntax> ParseArrayElements()
        {
            ImmutableArray<SyntaxNode>.Builder nodesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();

            while (Current.Kind != SyntaxKind.CloseBracketToken &&
                   Current.Kind != SyntaxKind.EndOfFileToken)
            {
                ArrayElementSyntax arrayElement = ParseArrayElement(out bool badExpressionStart);
                nodesAndSeparators.Add(arrayElement);

                if (Current.Kind != SyntaxKind.CloseBracketToken)
                {
                    SyntaxToken comma = MatchToken(SyntaxKind.CommaToken);
                    nodesAndSeparators.Add(comma);
                }

                if (badExpressionStart)
                    NextToken();
            }

            return new SeparatedSyntaxList<ArrayElementSyntax>(nodesAndSeparators.ToImmutable());
        }

        private SeparatedSyntaxList<ExpressionSyntax> ParseArguments()
        {
            ImmutableArray<SyntaxNode>.Builder nodesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();
            while (Current.Kind != SyntaxKind.CloseParenthesisToken &&
                   Current.Kind != SyntaxKind.EndOfFileToken)
            {
                ExpressionSyntax expression = ParseExpression();
                nodesAndSeparators.Add(expression);
                if (Current.Kind != SyntaxKind.CloseParenthesisToken)
                {
                    SyntaxToken comma = MatchToken(SyntaxKind.CommaToken);
                    nodesAndSeparators.Add(comma);
                }
            }
            return new SeparatedSyntaxList<ExpressionSyntax>(nodesAndSeparators.ToImmutable());
        }

        private TypeSyntax ParseTypeSyntax()
        {
            SyntaxToken identifier = MatchToken(SyntaxKind.IdentifierToken);
            SyntaxToken openBracket = null;
            SyntaxToken closeBracket = null;
            if (Current.Kind == SyntaxKind.OpenBracketToken &&
                Peek(1).Kind == SyntaxKind.CloseBracketToken)
            {
                openBracket = NextToken();
                closeBracket = NextToken();
            }

            return new TypeSyntax(identifier, openBracket, closeBracket);
        }

        #endregion

        #region Statement Parsing

        private StatementSyntax ParseStatement()
        {
            switch (Current.Kind)
            {
                case SyntaxKind.OpenBraceToken:
                    return ParseBlockSyntax(ParseStatement);
                case SyntaxKind.LetKeyword:
                case SyntaxKind.VarKeyword:
                    return ParseVariableDeclaration();
                case SyntaxKind.IfKeyword:
                    return ParseIfStatement();
                case SyntaxKind.WhileKeyword:
                    return ParseWhileStatement();
                case SyntaxKind.DoKeyword:
                    return ParseDoWhileStatement();
                case SyntaxKind.ForKeyword:
                    return ParseForStatement();
                default:
                    return ParseExpressionStatement();
            }
        }

        private StatementSyntax ParseVariableDeclaration()
        {
            SyntaxKind expected = Current.Kind == SyntaxKind.LetKeyword ? SyntaxKind.LetKeyword : SyntaxKind.VarKeyword;
            SyntaxToken keyword = MatchToken(expected);
            SyntaxToken identifier = MatchToken(SyntaxKind.IdentifierToken);
            TypeClauseSyntax typeClause = ParseOptionalTypeClause();
            SyntaxToken equals = MatchToken(SyntaxKind.EqualsToken);
            ExpressionSyntax initializer = ParseExpression();
            return new VariableDeclarationSyntax(keyword, identifier, typeClause, equals, initializer);
        }

        private StatementSyntax ParseIfStatement()
        {
            SyntaxToken keyword = MatchToken(SyntaxKind.IfKeyword);
            ExpressionSyntax condition = ParseExpression();
            StatementSyntax statement = ParseStatement();
            ElseClauseSyntax elseClause = ParseElseClause();
            return new IfStatementSyntax(keyword, condition, statement, elseClause);
        }

        private ElseClauseSyntax ParseElseClause()
        {
            if (Current.Kind != SyntaxKind.ElseKeyword)
                return null;
            SyntaxToken keyword = NextToken();
            StatementSyntax statement = ParseStatement();
            return new ElseClauseSyntax(keyword, statement);
        }

        private StatementSyntax ParseWhileStatement()
        {
            SyntaxToken keyword = MatchToken(SyntaxKind.WhileKeyword);
            ExpressionSyntax condition = ParseExpression();
            StatementSyntax body = ParseStatement();
            return new WhileStatementSyntax(keyword, condition, body);
        }

        private StatementSyntax ParseDoWhileStatement()
        {
            SyntaxToken doKeyword = MatchToken(SyntaxKind.DoKeyword);
            StatementSyntax body = ParseStatement();
            SyntaxToken whileKeyword = MatchToken(SyntaxKind.WhileKeyword);
            ExpressionSyntax condition = ParseExpression();
            return new DoWhileStatementSyntax(doKeyword, body, whileKeyword, condition);
        }

        private StatementSyntax ParseForStatement()
        {
            SyntaxToken keyword = MatchToken(SyntaxKind.ForKeyword);
            SyntaxToken identifier = MatchToken(SyntaxKind.IdentifierToken);
            SyntaxToken equalsToken = MatchToken(SyntaxKind.EqualsToken);
            ExpressionSyntax lowerBound = ParseExpression();
            SyntaxToken toKeyword = MatchToken(SyntaxKind.ToKeyword);
            ExpressionSyntax upperBound = ParseExpression();
            StatementSyntax body = ParseStatement();
            return new ForStatementSyntax(keyword, identifier, equalsToken, lowerBound, toKeyword, upperBound, body);
        }

        private BlockSyntax<TBlockMember> ParseBlockSyntax<TBlockMember>(Func<TBlockMember> func)
            where TBlockMember : SyntaxNode
        {
            ImmutableArray<TBlockMember>.Builder bodyObjects = ImmutableArray.CreateBuilder<TBlockMember>();
            SyntaxToken openBraceToken = MatchToken(SyntaxKind.OpenBraceToken);
            while (Current.Kind != SyntaxKind.EndOfFileToken &&
                   Current.Kind != SyntaxKind.CloseBraceToken)
            {
                SyntaxToken startToken = Current;
                TBlockMember bodyObject = func.Invoke();
                bodyObjects.Add(bodyObject);

                // If func.Invoke() did not consume any tokens,
                // we need to skip the current token and continue
                // in order to avoid an infinite loop.
                //
                // We don't need to report an error, because we'll
                // already tried to parse an expression statement
                // and reported one.
                if (Current == startToken)
                    NextToken();
            }

            SyntaxToken closeBraceToken = MatchToken(SyntaxKind.CloseBraceToken);
            return new BlockSyntax<TBlockMember>(openBraceToken, bodyObjects.ToImmutable(), closeBraceToken);
        }

        private ExpressionStatementSyntax ParseExpressionStatement()
        {
            ExpressionSyntax expression = ParseExpression();
            if (expression is not AssignmentExpressionSyntax &&
                expression is not CallExpressionSyntax &&
                (expression is MemberAccessExpression m && m.MemberExpression is not CallExpressionSyntax))
                _diagnostics.ReportInvalidExpressionStatement(expression.Span);

            return new ExpressionStatementSyntax(expression);
        }

        #endregion

        #region Expression Parsing

        private ExpressionSyntax ParseExpression()
        {
            return ParseArrayInitializer();
        }

        private ExpressionSyntax ParseArrayInitializer()
        {
            if (Current.Kind == SyntaxKind.OpenBracketToken)
            {
                SyntaxToken openBracket = MatchToken(SyntaxKind.OpenBracketToken);
                SeparatedSyntaxList<ArrayElementSyntax> arrayElements = ParseArrayElements();
                SyntaxToken close = MatchToken(SyntaxKind.CloseBracketToken);
                TypeClauseSyntax typeClause = ParseTypeClause();
                return new ArrayInitializerExpression(openBracket, arrayElements, close, typeClause);
            }

            return ParseAssignmentExpression();
        }

        private ArrayElementSyntax ParseArrayElement(out bool badExpressionStart)
        {
            badExpressionStart = false;
            if (Current.Kind.ToString().EndsWith("Keyword"))
                badExpressionStart = true;

            ExpressionSyntax expression = ParseExpression();
            return new ArrayElementSyntax(expression);
        }

        private ExpressionSyntax ParseAssignmentExpression()
        {
            if (Peek(0).Kind == SyntaxKind.IdentifierToken &&
                Peek(1).Kind == SyntaxKind.EqualsToken)
            {
                SyntaxToken identifierToken = NextToken();
                SyntaxToken operatorToken = NextToken();
                ExpressionSyntax right = ParseAssignmentExpression();
                return new AssignmentExpressionSyntax(identifierToken, operatorToken, right);
            }

            return ParseBinaryExpression();
        }

        private ExpressionSyntax ParseBinaryExpression(int parentPrecedence = 0)
        {
            ExpressionSyntax left;
            int unaryOperatorPrecedence = Current.Kind.GetUnaryOperatorPrecedence();
            if (unaryOperatorPrecedence != 0 && unaryOperatorPrecedence >= parentPrecedence)
            {
                SyntaxToken operatorToken = NextToken();
                ExpressionSyntax operand = ParseBinaryExpression(unaryOperatorPrecedence);
                left = new UnaryExpressionSyntax(operatorToken, operand);
            }
            else
            {
                left = ParsePrimaryExpression();
            }
            while (true)
            {
                int precedence = Current.Kind.GetBinaryOperatorPrecedence();
                if (precedence == 0 || precedence <= parentPrecedence)
                    break;
                SyntaxToken operatorToken = NextToken();
                ExpressionSyntax right = ParseBinaryExpression(precedence);
                left = new BinaryExpressionSyntax(left, operatorToken, right);
            }

            return left;
        }

        private ExpressionSyntax ParsePrimaryExpression()
        {
            switch (Current.Kind)
            {
                case SyntaxKind.OpenParenthesisToken:
                    return ParseParenthesizedExpression();
                case SyntaxKind.FalseKeyword:
                case SyntaxKind.TrueKeyword:
                    return ParseBooleanLiteral();
                case SyntaxKind.NumberToken:
                    return ParseNumberLiteral();
                case SyntaxKind.StringToken:
                    return ParseStringLiteral();
                case SyntaxKind.IdentifierToken:
                default:
                    return ParseIdentifierExpression();
            }
        }

        private ExpressionSyntax ParseParenthesizedExpression()
        {
            SyntaxToken left = MatchToken(SyntaxKind.OpenParenthesisToken);
            ExpressionSyntax expression = ParseExpression();
            SyntaxToken right = MatchToken(SyntaxKind.CloseParenthesisToken);
            return new ParenthesizedExpressionSyntax(left, expression, right);
        }

        private ExpressionSyntax ParseBooleanLiteral()
        {
            bool isTrue = Current.Kind == SyntaxKind.TrueKeyword;
            SyntaxToken keywordToken = isTrue ? MatchToken(SyntaxKind.TrueKeyword) : MatchToken(SyntaxKind.FalseKeyword);
            return new LiteralExpressionSyntax(keywordToken, isTrue);
        }

        private ExpressionSyntax ParseNumberLiteral()
        {
            SyntaxToken numberToken = MatchToken(SyntaxKind.NumberToken);
            return new LiteralExpressionSyntax(numberToken);
        }

        private ExpressionSyntax ParseStringLiteral()
        {
            SyntaxToken stringToken = MatchToken(SyntaxKind.StringToken);
            return new LiteralExpressionSyntax(stringToken);
        }

        private ExpressionSyntax ParseIdentifierExpression()
        {
            if (Peek(0).Kind == SyntaxKind.IdentifierToken)
            {
                if (Peek(1).Kind == SyntaxKind.OpenParenthesisToken)
                    return ParseCallExpression();
                else if (Peek(1).Kind == SyntaxKind.OpenBracketToken)
                    return ParseElementAccessExpression();
                else if (Peek(1).Kind == SyntaxKind.DotToken)
                    return ParseMemberAccessExpression();
            }

            return ParseNameExpression();
        }

        private ExpressionSyntax ParseCallExpression()
        {
            TypeSyntax typeSyntax = ParseTypeSyntax();
            SyntaxToken openParenthesisToken = MatchToken(SyntaxKind.OpenParenthesisToken);
            SeparatedSyntaxList<ExpressionSyntax> arguments = ParseArguments();
            SyntaxToken closeParenthesisToken = MatchToken(SyntaxKind.CloseParenthesisToken);
            return new CallExpressionSyntax(typeSyntax, openParenthesisToken, arguments, closeParenthesisToken);
        }

        private ExpressionSyntax ParseElementAccessExpression()
        {
            SyntaxToken identifier = MatchToken(SyntaxKind.IdentifierToken);
            SyntaxToken openBracket = MatchToken(SyntaxKind.OpenBracketToken);
            ExpressionSyntax expression = ParseExpression();
            SyntaxToken closeBracket = MatchToken(SyntaxKind.CloseBracketToken);
            return new ElementAccessExpression(identifier, openBracket, expression, closeBracket);
        }

        private ExpressionSyntax ParseMemberAccessExpression()
        {
            SyntaxToken typeIdentifier = MatchToken(SyntaxKind.IdentifierToken);
            SyntaxToken dotToken = MatchToken(SyntaxKind.DotToken);
            int pos = _position;

            // Parse the identifier to check for open parenthesis
            MatchToken(SyntaxKind.IdentifierToken);

            ExpressionSyntax memberExpression = null;
            if (Current.Kind == SyntaxKind.OpenParenthesisToken)
            {
                // reset position
                _position = pos;
                memberExpression = ParseCallExpression();
            }

            return new MemberAccessExpression(typeIdentifier, dotToken, memberExpression);
        }

        private ExpressionSyntax ParseNameExpression()
        {
            SyntaxToken identifierToken = MatchToken(SyntaxKind.IdentifierToken);
            return new NameExpressionSyntax(identifierToken);
        }

        #endregion
    }
}
