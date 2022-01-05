using Blade.CodeAnalysis.Symbols;

namespace Blade.CodeAnalysis.Binding
{
    internal sealed class BoundMemberAccessExpression : BoundExpression
    {
        public BoundMemberAccessExpression(MemberSymbol member, ClassSymbol @class, BoundExpression expression)
        {
            Member = member;
            Class = @class;
            Expression = expression;
        }

        public override TypeSymbol Type => Member.Type;
        public override BoundNodeKind Kind => BoundNodeKind.MemberAccessExpression;
        public MemberSymbol Member { get; }
        public ClassSymbol Class { get; }
        public BoundExpression Expression { get; }
    }
}
