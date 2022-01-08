namespace Blade.CodeAnalysis.Syntax
{
    public sealed class AdvanceToMemberExpressionSyntax : ExpressionSyntax
    {
        public AdvanceToMemberExpressionSyntax(SyntaxToken dotToken, ExpressionSyntax memberExpression)
        {
            DotToken = dotToken;
            MemberExpression = memberExpression;
        }

        public override SyntaxKind Kind => SyntaxKind.AdvanceToMemberExpression;

        public SyntaxToken DotToken { get; }
        public ExpressionSyntax MemberExpression { get; }
    }
}
