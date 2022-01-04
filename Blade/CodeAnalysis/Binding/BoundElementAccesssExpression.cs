using Blade.CodeAnalysis.Symbols;

namespace Blade.CodeAnalysis.Binding
{
    internal sealed class BoundElementAccesssExpression : BoundExpression
    {
        public BoundElementAccesssExpression(ArraySymbol array, BoundExpression indexer)
        {
            Array = array;
            Indexer = indexer;
        }

        public override TypeSymbol Type => Array.Type.ElementType;

        public override BoundNodeKind Kind => BoundNodeKind.ElementAccesssExpression;

        public ArraySymbol Array { get; }
        public BoundExpression Indexer { get; }
    }
}
