namespace Blade.CodeAnalysis.Symbols
{
    public abstract class MemberSymbol : Symbol
    {
        private protected MemberSymbol(string name) 
            : base(name)
        {
        }

        public abstract TypeSymbol Type { get; }
    }
}
