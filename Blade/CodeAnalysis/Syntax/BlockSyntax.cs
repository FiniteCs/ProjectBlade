namespace Blade.CodeAnalysis.Syntax
{
    public sealed class BlockSyntax<T> : StatementSyntax
        where T : SyntaxNode
    {
        public BlockSyntax(SyntaxToken openBraceToken, ImmutableArray<T> blockMembers, SyntaxToken closeBraceToken)
        {
            OpenBraceToken = openBraceToken;
            BlockMembers = blockMembers;
            CloseBraceToken = closeBraceToken;
        }

        public override SyntaxKind Kind => SyntaxKind.BlockSyntax;
        public SyntaxToken OpenBraceToken { get; }
        public ImmutableArray<T> BlockMembers { get; }
        public SyntaxToken CloseBraceToken { get; }
    }
}
