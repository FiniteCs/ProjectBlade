using Blade.CodeAnalysis.Binding;
using Blade.CodeAnalysis.Syntax;

namespace Blade.CodeAnalysis.Symbols
{
    public sealed class ClassSymbol : MemberSymbol
    {
        internal ClassSymbol(string name, ImmutableArray<MemberSymbol> members, BoundScope scope, ClassDeclarationSyntax declaration = null) 
            : base(name)
        {
            Members = members;
            Scope = scope;
            Declaration = declaration;
        }

        public override SymbolKind Kind => SymbolKind.Class;
        public override TypeSymbol Type => new(Name);
        public ImmutableArray<MemberSymbol> Members { get; }
        internal BoundScope Scope { get; }
        public ClassDeclarationSyntax Declaration { get; }
    }
}
