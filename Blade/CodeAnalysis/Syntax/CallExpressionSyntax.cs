namespace Blade.CodeAnalysis.Syntax
{
    public sealed class CallExpressionSyntax : ExpressionSyntax
    {
        public CallExpressionSyntax(TypeSyntax typeSyntax, SyntaxToken openParenthesisToken, SeparatedSyntaxList<ExpressionSyntax> arguments, SyntaxToken closeParenthesisToken)
        {
            TypeSyntax = typeSyntax;
            OpenParenthesisToken = openParenthesisToken;
            Arguments = arguments;
            CloseParenthesisToken = closeParenthesisToken;
        }

        public override SyntaxKind Kind => SyntaxKind.CallExpression;
        public TypeSyntax TypeSyntax { get; }
        public SyntaxToken OpenParenthesisToken { get; }
        public SeparatedSyntaxList<ExpressionSyntax> Arguments { get; }
        public SyntaxToken CloseParenthesisToken { get; }
    }
}
