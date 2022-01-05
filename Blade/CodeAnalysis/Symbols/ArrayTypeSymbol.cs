namespace Blade.CodeAnalysis.Symbols
{
    public sealed class ArrayTypeSymbol : TypeSymbol
    {
        public ArrayTypeSymbol(TypeSymbol type)
            : base($"{type.Name}[]")
        {
            ElementType = type;
        }

        public TypeSymbol ElementType { get; }
    }
}
