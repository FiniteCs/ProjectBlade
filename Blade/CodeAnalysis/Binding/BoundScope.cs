using Blade.CodeAnalysis.Symbols;
using System.Linq;

namespace Blade.CodeAnalysis.Binding
{
    internal sealed class BoundScope
    {
        private Dictionary<string, VariableSymbol> _variables = new();
        private Dictionary<string, FunctionSymbol> _functions = new();
        private Dictionary<string, ClassSymbol> _classes = new();
        private Dictionary<string, ArraySymbol> _arrays = new();
        private ImmutableArray<MemberSymbol> _members
        {
            get
            {
                List<MemberSymbol> members = new();
                members.AddRange(from FunctionSymbol function in _functions.Values
                                 select function);
                members.AddRange(from ClassSymbol classSymbol in _classes.Values
                                 select classSymbol);

                return members.ToImmutableArray();
            }
        }

        public BoundScope(BoundScope parent)
        {
            Parent = parent;
        }

        public BoundScope Parent { get; }

        public bool TryDeclareVariable(VariableSymbol variable)
        {
            if (_variables == null)
                _variables = new();

            if (_variables.ContainsKey(variable.Name))
                return false;

            _variables.Add(variable.Name, variable);
            return true;
        }

        public bool TryLookupVariable(string name, out VariableSymbol variable)
        {
            variable = null;
            if (_variables != null && _variables.TryGetValue(name, out variable))
                return true;

            if (Parent == null)
                return false;

            return Parent.TryLookupVariable(name, out variable);
        }

        public bool TryDeclareFunction(FunctionSymbol function)
        {
            if (_functions == null)
                _functions = new();

            if (_functions.ContainsKey(function.Name))
                return false;

            _functions.Add(function.Name, function);
            return true;
        }

        public bool TryLookupFunction(string name, out FunctionSymbol function)
        {
            function = null;
            if (_functions != null && _functions.TryGetValue(name, out function))
                return true;

            if (Parent == null)
                return false;

            return Parent.TryLookupFunction(name, out function);
        }

        public bool TryDeclareArray(ArraySymbol array)
        {
            if (_arrays == null)
                _arrays = new();

            if (_arrays.ContainsKey(array.Name))
                return false;

            _arrays.Add(array.Name, array);
            return true;
        }

        public bool TryLookupArray(string name, out ArraySymbol array)
        {
            array = null;
            if (_arrays != null && _arrays.TryGetValue(name, out array))
                return true;

            if (Parent == null)
                return false;

            return Parent.TryLookupArray(name, out array);
        }

        public bool TryDeclareClass(ClassSymbol @class)
        {
            if (_classes == null)
                _classes = new();

            if (_classes.ContainsKey(@class.Name))
                return false;

            _classes.Add(@class.Name, @class);
            return true;
        }

        public bool TryLookupClass(string name, out ClassSymbol @class)
        {
            @class = null;
            if (_classes != null && _classes.TryGetValue(name, out @class))
                return true;

            if (Parent == null)
                return false;

            return Parent.TryLookupClass(name, out @class);
        }

        public ImmutableArray<VariableSymbol> GetDeclaredVariables()
        {
            if (_variables == null)
                return ImmutableArray<VariableSymbol>.Empty;

            return _variables.Values.ToImmutableArray();
        }

        public ImmutableArray<FunctionSymbol> GetDeclaredFunctions()
        {
            if (_functions == null)
                return ImmutableArray<FunctionSymbol>.Empty;

            return _functions.Values.ToImmutableArray();
        }

        public ImmutableArray<ClassSymbol> GetDeclaredClasses()
        {
            if (_classes == null)
                return ImmutableArray<ClassSymbol>.Empty;

            return _classes.Values.ToImmutableArray();
        }

        public ImmutableArray<MemberSymbol> GetMembers()
        {
            if (_members == null)
                return ImmutableArray<MemberSymbol>.Empty;

            return _members;
        }
    }
}
