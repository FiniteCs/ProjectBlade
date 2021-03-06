namespace Blade.CodeAnalysis.Syntax
{
    public enum SyntaxKind
    {
        // Tokens
        BadToken,
        EndOfFileToken,
        WhitespaceToken,
        NumberToken,
        StringToken,
        PlusToken,
        MinusToken,
        StarToken,
        SlashToken,
        BangToken,
        EqualsToken,
        TildeToken,
        HatToken,
        AmpersandToken,
        AmpersandAmpersandToken,
        PipeToken,
        PipePipeToken,
        EqualsEqualsToken,
        BangEqualsToken,
        LessToken,
        LessOrEqualsToken,
        GreaterToken,
        GreaterOrEqualsToken,
        OpenParenthesisToken,
        CloseParenthesisToken,
        OpenBracketToken,
        CloseBracketToken,
        OpenBraceToken,
        CloseBraceToken,
        ColonToken,
        CommaToken,
        DotToken,
        IdentifierToken,

        // Keywords
        ElseKeyword,
        FalseKeyword,
        ForKeyword,
        FunctionKeyword,
        IfKeyword,
        LetKeyword,
        ToKeyword,
        TrueKeyword,
        VarKeyword,
        WhileKeyword,
        DoKeyword,
        ClassKeyword,

        // Nodes
        CompilationUnit,
        FunctionDeclaration,
        GlobalStatement,
        Parameter,
        TypeClause,
        ElseClause,
        ArrayElement,
        TypeSyntax,
        ClassBody,

        // Statements
        BlockSyntax,
        VariableDeclaration,
        IfStatement,
        WhileStatement,
        DoWhileStatement,
        ForStatement,
        ExpressionStatement,
        ClassDeclarationSyntax,

        // Expressions
        LiteralExpression,
        NameExpression,
        UnaryExpression,
        BinaryExpression,
        ParenthesizedExpression,
        AssignmentExpression,
        CallExpression,
        ArrayInitializerExpression,
        ElementAccessExpression,
        MemberAccessExpression,
        AdvanceToMemberExpression,
    }
}
