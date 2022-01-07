using Blade.CodeAnalysis.Symbols;

namespace Blade.CodeAnalysis.Binding
{
    internal sealed class Class
    {
        public Class(ImmutableDictionary<FunctionSymbol, BoundBlockStatement<BoundStatement>> functions, ImmutableDictionary<ClassSymbol, Class> classes)
        {
            Functions = functions;
            Classes = classes;
        }

        public ImmutableDictionary<FunctionSymbol, BoundBlockStatement<BoundStatement>> Functions { get; }
        public ImmutableDictionary<ClassSymbol, Class> Classes { get; }
    }
}
