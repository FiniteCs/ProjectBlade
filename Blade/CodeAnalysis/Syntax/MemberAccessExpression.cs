namespace Blade.CodeAnalysis.Syntax
{
    public sealed class MemberAccessExpression : ExpressionSyntax
    {
        public MemberAccessExpression(SyntaxToken typeIdentifier, ImmutableArray<AdvanceToMemberExpression> advanceToMembers)
        {
            TypeIdentifier = typeIdentifier;
            AdvanceToMembers = advanceToMembers;
        }

        public override SyntaxKind Kind => SyntaxKind.MemberAccessExpression;
        public SyntaxToken TypeIdentifier { get; }
        public ImmutableArray<AdvanceToMemberExpression> AdvanceToMembers { get; }
    }
}
