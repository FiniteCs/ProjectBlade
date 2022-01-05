using Blade.CodeAnalysis.Symbols;

namespace Blade.CodeAnalysis.Binding
{
    internal sealed class BoundProgram
    {
        public BoundProgram(ImmutableArray<Diagnostic> diagnostics, ImmutableDictionary<FunctionSymbol, BoundBlockStatement<BoundStatement>> functions, BoundBlockStatement<BoundStatement> statement, ImmutableDictionary<ClassSymbol, Class> classes)
        {
            Diagnostics = diagnostics;
            Functions = functions;
            Statement = statement;
            Classes = classes;
        }

        public ImmutableArray<Diagnostic> Diagnostics { get; }
        public ImmutableDictionary<FunctionSymbol, BoundBlockStatement<BoundStatement>> Functions { get; }
        public ImmutableDictionary<ClassSymbol, Class> Classes { get; }
        public BoundBlockStatement<BoundStatement> Statement { get; }
    }
}
