namespace Blade.CodeAnalysis.Syntax
{
    public sealed class ArrayElementSyntax : SyntaxNode
    {
        public ArrayElementSyntax(ExpressionSyntax expression)
        {
            Expression = expression;
        }

        public override SyntaxKind Kind => SyntaxKind.ArrayElement;
        public ExpressionSyntax Expression { get; }
    }
}
