namespace Blade.CodeAnalysis.Syntax
{
    public sealed class MemberAccessExpression : ExpressionSyntax
    {
        public MemberAccessExpression(SyntaxToken typeIdentifier, SyntaxToken dotToken, ExpressionSyntax memberExpression)
        {
            TypeIdentifier = typeIdentifier;
            DotToken = dotToken;
            MemberExpression = memberExpression;
        }

        public override SyntaxKind Kind => SyntaxKind.MemberAccessExpression;
        public SyntaxToken TypeIdentifier { get; }
        public SyntaxToken DotToken { get; }
        public ExpressionSyntax MemberExpression { get; }
    }
}
