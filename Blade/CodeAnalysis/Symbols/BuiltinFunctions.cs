using System.Reflection;

namespace Blade.CodeAnalysis.Symbols
{
    internal static class BuiltinFunctions
    {
        public static readonly FunctionSymbol Print = new(
            "print", ParameterListCreator(
                new ParameterSymbol("text", TypeSymbol.String)
            ),
            TypeSymbol.Void
            );

        public static readonly FunctionSymbol Input = new(
            "input",
            ParameterListCreator(
                ),
            TypeSymbol.String
            );

        public static readonly FunctionSymbol Rnd = new(
            "rnd", 
            ParameterListCreator(
                new ParameterSymbol("max", TypeSymbol.Int)
                ),
            TypeSymbol.Int
            );

        public static readonly FunctionSymbol Strlen = new(
            "strlen",
            ParameterListCreator(
                new ParameterSymbol("str", TypeSymbol.String)
                ),
            TypeSymbol.Int
            );
        public static readonly FunctionSymbol Len = new(
            "len",
            ParameterListCreator(
                new ParameterSymbol("array", TypeSymbol.Array)
                ),
            TypeSymbol.Int
            );

        internal static IEnumerable<FunctionSymbol> GetAll()
            => typeof(BuiltinFunctions).GetFields(BindingFlags.Public | BindingFlags.Static)
                                       .Where(f => f.FieldType == typeof(FunctionSymbol))
                                       .Select(f => (FunctionSymbol)f.GetValue(null));

        private static ImmutableArray<ParameterSymbol> ParameterListCreator(params ParameterSymbol[] parameters)
        {
            if (parameters.Length == 0)
                return ImmutableArray<ParameterSymbol>.Empty;

            return parameters.ToImmutableArray();
        }
    }
}
