namespace Blade.CodeAnalysis.Syntax
{
    public sealed class ArrayInitializerExpressionSyntax : ExpressionSyntax
    {
        public ArrayInitializerExpressionSyntax(SyntaxToken openBracketToken, SeparatedSyntaxList<ArrayElementSyntax> arrayElements, SyntaxToken closeBracketToken, TypeClauseSyntax typeClauseSyntax)
        {
            OpenBracketToken = openBracketToken;
            ArrayElements = arrayElements;
            CloseBracketToken = closeBracketToken;
            TypeClauseSyntax = typeClauseSyntax;
        }

        public override SyntaxKind Kind => SyntaxKind.ArrayInitializerExpression;
        public SyntaxToken OpenBracketToken { get; }
        public SeparatedSyntaxList<ArrayElementSyntax> ArrayElements { get; }
        public SyntaxToken CloseBracketToken { get; }
        public TypeClauseSyntax TypeClauseSyntax { get; }
    }
}
