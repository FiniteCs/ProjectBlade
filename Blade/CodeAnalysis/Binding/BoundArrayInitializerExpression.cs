using Blade.CodeAnalysis.Symbols;

namespace Blade.CodeAnalysis.Binding
{
    internal sealed class BoundArrayInitializerExpression : BoundExpression
    {
        public BoundArrayInitializerExpression(ArraySymbol arraySymbol, ImmutableArray<BoundExpression> expressions)
        {
            ArraySymbol = arraySymbol;
            Expressions = expressions;
        }

        public override TypeSymbol Type => ArraySymbol.Type;

        public override BoundNodeKind Kind => BoundNodeKind.ArrayInitializerExpression;

        public ArraySymbol ArraySymbol { get; }
        public ImmutableArray<BoundExpression> Expressions { get; }
    }
}
