using Blade.CodeAnalysis.Symbols;

namespace Blade.CodeAnalysis.Binding
{
    internal sealed class BoundMemberAccessExpression : BoundExpression
    {
        public BoundMemberAccessExpression(MemberSymbol member, Stack<ClassSymbol> classes, BoundExpression expression)
        {
            Member = member;
            Classes = classes;
            Expression = expression;
        }

        public override TypeSymbol Type => Member.Type;
        public override BoundNodeKind Kind => BoundNodeKind.MemberAccessExpression;
        public MemberSymbol Member { get; }
        public Stack<ClassSymbol> Classes { get; }
        public BoundExpression Expression { get; }
    }
}
