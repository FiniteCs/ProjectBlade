namespace Blade.CodeAnalysis.Syntax
{
    public sealed class TypeClauseSyntax : SyntaxNode
    {
        public TypeClauseSyntax(SyntaxToken colonToken, TypeSyntax typeSyntax)
        {
            ColonToken = colonToken;
            TypeSyntax = typeSyntax;
        }

        public override SyntaxKind Kind => SyntaxKind.TypeClause;
        public SyntaxToken ColonToken { get; }
        public TypeSyntax TypeSyntax { get; }
    }
}
