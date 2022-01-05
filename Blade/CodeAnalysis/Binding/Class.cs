using Blade.CodeAnalysis.Symbols;

namespace Blade.CodeAnalysis.Binding
{
    internal sealed class Class
    {
        public Class(ImmutableDictionary<FunctionSymbol, BoundBlockStatement<BoundStatement>> functions)
        {
            Functions = functions;
        }

        public ImmutableDictionary<FunctionSymbol, BoundBlockStatement<BoundStatement>> Functions { get; }
    }
}
