namespace Blade.CodeAnalysis.Syntax
{
    public sealed class ElementAccessExpression : ExpressionSyntax
    {
        public ElementAccessExpression(SyntaxToken identifierToken, SyntaxToken openBracketToken, ExpressionSyntax indexerExpression, SyntaxToken closeBracketToken)
        {
            IdentifierToken = identifierToken;
            OpenBracketToken = openBracketToken;
            IndexerExpression = indexerExpression;
            CloseBracketToken = closeBracketToken;
        }

        public override SyntaxKind Kind => SyntaxKind.ElementAccessExpression;
        public SyntaxToken IdentifierToken { get; }
        public SyntaxToken OpenBracketToken { get; }
        public ExpressionSyntax IndexerExpression { get; }
        public SyntaxToken CloseBracketToken { get; }
    }
}
