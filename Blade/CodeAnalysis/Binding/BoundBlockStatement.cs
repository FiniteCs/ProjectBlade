namespace Blade.CodeAnalysis.Binding
{
    internal sealed class BoundBlockStatement<T> : BoundStatement
    {
        public BoundBlockStatement(ImmutableArray<T> statements)
        {
            Statements = statements;
        }

        public ImmutableArray<T> Statements { get; }

        public override BoundNodeKind Kind => BoundNodeKind.BlockStatement;
    }
}
