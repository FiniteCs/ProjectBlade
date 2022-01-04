namespace Blade.CodeAnalysis.Symbols
{
    public class TypeSymbol : Symbol
    {
        public static readonly TypeSymbol Error = new("?");
        public static readonly TypeSymbol Int = new("int");
        public static readonly TypeSymbol Bool = new("bool");
        public static readonly TypeSymbol String = new("string");
        public static readonly TypeSymbol Void = new("void");

        private protected TypeSymbol(string name)
            : base(name)
        {
        }

        public override SymbolKind Kind => SymbolKind.Type;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is null)
            {
                return false;
            }

            return false;
        }

        public override int GetHashCode()
            => base.GetHashCode();

        public static bool operator ==(TypeSymbol left, TypeSymbol right)
            => left.Equals(right);

        public static bool operator !=(TypeSymbol left, TypeSymbol right)
            => !left.Equals(right);

        public bool Equals(TypeSymbol type)
        {
            if (type is null)
                return false;

            if (type.Name == Name)
                return true;

            return false;
        }
    }

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
