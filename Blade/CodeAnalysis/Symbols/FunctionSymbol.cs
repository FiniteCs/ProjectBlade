using Blade.CodeAnalysis.Syntax;

namespace Blade.CodeAnalysis.Symbols
{
    public sealed class FunctionSymbol : MemberSymbol
    {
        public FunctionSymbol(string name, ImmutableArray<ParameterSymbol> parameters, TypeSymbol type, FunctionDeclarationSyntax declaration = null)
            : base(name)
        {
            Parameters = parameters;
            Type = type;
            Declaration = declaration;
        }

        public override SymbolKind Kind => SymbolKind.Function;
        public override TypeSymbol Type { get; }
        public FunctionDeclarationSyntax Declaration { get; }
        public ImmutableArray<ParameterSymbol> Parameters { get; }
    }
}
