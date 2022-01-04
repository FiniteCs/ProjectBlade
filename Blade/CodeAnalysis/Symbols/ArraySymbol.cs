namespace Blade.CodeAnalysis.Symbols
{
    public sealed class ArraySymbol : Symbol
    {
        public ArraySymbol(string name, ImmutableArray<ArrayElementSymbol> arrayElements, ArrayTypeSymbol type) 
            : base(name)
        {
            ArrayElements = arrayElements;
            Type = type;
        }

        public override SymbolKind Kind => SymbolKind.Array;
        public ImmutableArray<ArrayElementSymbol> ArrayElements { get; }
        public ArrayTypeSymbol Type { get; }
    }
}
