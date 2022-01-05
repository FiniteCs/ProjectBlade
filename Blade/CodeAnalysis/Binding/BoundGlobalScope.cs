using Blade.CodeAnalysis.Symbols;

namespace Blade.CodeAnalysis.Binding
{
    internal sealed class BoundGlobalScope
    {
        public BoundGlobalScope(BoundGlobalScope previous, 
                                ImmutableArray<Diagnostic> diagnostics, 
                                ImmutableArray<FunctionSymbol> functions, 
                                ImmutableArray<VariableSymbol> variables, 
                                ImmutableArray<ClassSymbol> classes, 
                                ImmutableArray<BoundStatement> statements)
        {
            Previous = previous;
            Diagnostics = diagnostics;
            Functions = functions;
            Variables = variables;
            Classes = classes;
            Statements = statements;
        }

        public BoundGlobalScope Previous { get; }
        public ImmutableArray<Diagnostic> Diagnostics { get; }
        public ImmutableArray<FunctionSymbol> Functions { get; }
        public ImmutableArray<VariableSymbol> Variables { get; }
        public ImmutableArray<ClassSymbol> Classes { get; }
        public ImmutableArray<BoundStatement> Statements { get; }
    }
}
