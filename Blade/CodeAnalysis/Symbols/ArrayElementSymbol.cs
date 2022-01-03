namespace Blade.CodeAnalysis.Symbols
{
    public sealed class ArrayElementSymbol : Symbol
    {
        public ArrayElementSymbol(string name, TypeSymbol type) 
            : base(name)
        {
            Type = type;
        }

        public override SymbolKind Kind => SymbolKind.ArrayElement;
        public TypeSymbol Type { get; }
    }
}
