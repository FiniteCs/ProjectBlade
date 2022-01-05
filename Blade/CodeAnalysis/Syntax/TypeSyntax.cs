namespace Blade.CodeAnalysis.Syntax
{
    public sealed class TypeSyntax : SyntaxNode
    {
        public TypeSyntax(SyntaxToken typeIdentifier, SyntaxToken openBracket, SyntaxToken closeBracket)
        {
            TypeIdentifier = typeIdentifier;
            OpenBracket = openBracket;
            CloseBracket = closeBracket;
        }

        public override SyntaxKind Kind => SyntaxKind.TypeSyntax;

        public SyntaxToken TypeIdentifier { get; }
        public SyntaxToken OpenBracket { get; }
        public SyntaxToken CloseBracket { get; }
    }
}
