namespace Blade.CodeAnalysis.Binding
{
    internal enum BoundNodeKind
    {
        // Declarations
        Class,

        // Statements
        BlockStatement,
        VariableDeclaration,
        IfStatement,
        WhileStatement,
        DoWhileStatement,
        ForStatement,
        LabelStatement,
        GotoStatement,
        ConditionalGotoStatement,
        ExpressionStatement,

        // Expressions
        LiteralExpression,
        VariableExpression,
        AssignmentExpression,
        UnaryExpression,
        BinaryExpression,
        ErrorExpression,
        CallExpression,
        ArrayInitializerExpression,
        ConversionExpression,
        ElementAccesssExpression,
        MemberAccessExpression,
    }
}
