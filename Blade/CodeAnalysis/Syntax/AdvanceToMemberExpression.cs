namespace Blade.CodeAnalysis.Syntax
{
    public sealed class AdvanceToMemberExpression : ExpressionSyntax
    {
        public AdvanceToMemberExpression(SyntaxToken dotToken, ExpressionSyntax memberExpression)
        {
            DotToken = dotToken;
            MemberExpression = memberExpression;
        }

        public override SyntaxKind Kind => SyntaxKind.AdvanceToMemberExpression;

        public SyntaxToken DotToken { get; }
        public ExpressionSyntax MemberExpression { get; }
    }
}
