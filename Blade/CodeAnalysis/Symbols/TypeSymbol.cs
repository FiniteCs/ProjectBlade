namespace Blade.CodeAnalysis.Symbols
{
    public class TypeSymbol : Symbol
    {
        public static readonly TypeSymbol Error = new("?");
        public static readonly TypeSymbol Int = new("int");
        public static readonly TypeSymbol Bool = new("bool");
        public static readonly TypeSymbol String = new("string");
        public static readonly TypeSymbol Void = new("void");
        public static readonly TypeSymbol Array = new("__array");

        internal protected TypeSymbol(string name)
            : base(name)
        {
        }

        public override SymbolKind Kind => SymbolKind.Type;

        public virtual TypeSymbol Type { get => this; }

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
        {
            if (left is null)
                return false;

            return left.Equals(right);
        }

        public static bool operator !=(TypeSymbol left, TypeSymbol right)
        {
            if (left is null)
                return false;

            return !left.Equals(right);
        }

        public bool Equals(TypeSymbol type)
        {
            if (type is null)
                return false;

            if (type.Name == Name)
                return true;

            return false;
        }
    }
}
