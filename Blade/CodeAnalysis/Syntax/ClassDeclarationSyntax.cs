namespace Blade.CodeAnalysis.Syntax
{
    public sealed class ClassDeclarationSyntax : MemberSyntax
    {
        public ClassDeclarationSyntax(SyntaxToken classKeyword, SyntaxToken identifierToken, BlockSyntax<MemberSyntax> classBody)
        {
            ClassKeyword = classKeyword;
            IdentifierToken = identifierToken;
            ClassBody = classBody;
        }

        public override SyntaxKind Kind => SyntaxKind.ClassDeclarationSyntax;
        public SyntaxToken ClassKeyword { get; }
        public SyntaxToken IdentifierToken { get; }
        public BlockSyntax<MemberSyntax> ClassBody { get; }
    }
}
