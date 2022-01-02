using Blade.CodeAnalysis.Syntax;
using Xunit;

namespace Blade.Tests.CodeAnalysis.Syntax
{
    public class ParserTests
    {
        [Theory]
        [MemberData(nameof(GetBinaryOperatorPairsData))]
        public void Parser_BinaryExpression_HonorsPrecedences(SyntaxKind op1, SyntaxKind op2)
        {
            int op1Precedence = SyntaxFacts.GetBinaryOperatorPrecedence(op1);
            int op2Precedence = SyntaxFacts.GetBinaryOperatorPrecedence(op2);
            string op1Text = SyntaxFacts.GetText(op1);
            string op2Text = SyntaxFacts.GetText(op2);
            string text = $"a {op1Text} b {op2Text} c";
            ExpressionSyntax expression = ParseExpression(text);

            if (op1Precedence >= op2Precedence)
            {
                using AssertingEnumerator e = new(expression);
                e.AssetNode(SyntaxKind.BinaryExpression);

                e.AssetNode(SyntaxKind.BinaryExpression);
                e.AssetNode(SyntaxKind.NameExpression);
                e.AssetToken(SyntaxKind.IdentifierToken, "a");
                e.AssetToken(op1, op1Text);
                e.AssetNode(SyntaxKind.NameExpression);
                e.AssetToken(SyntaxKind.IdentifierToken, "b");
                e.AssetToken(op2, op2Text);
                e.AssetNode(SyntaxKind.NameExpression);
                e.AssetToken(SyntaxKind.IdentifierToken, "c");
            }
            else
            {
                using AssertingEnumerator e = new(expression);
                e.AssetNode(SyntaxKind.BinaryExpression);
                e.AssetNode(SyntaxKind.NameExpression);
                e.AssetToken(SyntaxKind.IdentifierToken, "a");
                e.AssetToken(op1, op1Text);
                e.AssetNode(SyntaxKind.BinaryExpression);
                e.AssetNode(SyntaxKind.NameExpression);
                e.AssetToken(SyntaxKind.IdentifierToken, "b");
                e.AssetToken(op2, op2Text);
                e.AssetNode(SyntaxKind.NameExpression);
                e.AssetToken(SyntaxKind.IdentifierToken, "c");
            }
        }

        [Theory]
        [MemberData(nameof(GetUnaryOperatorPairsData))]
        public void Parser_UnaryExpression_HonorsPrecedences(SyntaxKind unary, SyntaxKind binary)
        {
            int unaryPrecedence = SyntaxFacts.GetUnaryOperatorPrecedence(unary);
            int binaryPrecedence = SyntaxFacts.GetBinaryOperatorPrecedence(binary);
            string unaryText = SyntaxFacts.GetText(unary);
            string binaryText = SyntaxFacts.GetText(binary);
            string text = $"{unaryText} a {binaryText} b";
            ExpressionSyntax expression = ParseExpression(text);

            if (unaryPrecedence >= binaryPrecedence)
            {
                using AssertingEnumerator e = new(expression);
                e.AssetNode(SyntaxKind.BinaryExpression);
                e.AssetNode(SyntaxKind.UnaryExpression);
                e.AssetToken(unary, unaryText);
                e.AssetNode(SyntaxKind.NameExpression);
                e.AssetToken(SyntaxKind.IdentifierToken, "a");
                e.AssetToken(binary, binaryText);
                e.AssetNode(SyntaxKind.NameExpression);
                e.AssetToken(SyntaxKind.IdentifierToken, "b");
            }
            else
            {
                using AssertingEnumerator e = new(expression);
                e.AssetNode(SyntaxKind.UnaryExpression);
                e.AssetToken(unary, unaryText);
                e.AssetNode(SyntaxKind.BinaryExpression);
                e.AssetNode(SyntaxKind.NameExpression);
                e.AssetToken(SyntaxKind.IdentifierToken, "a");
                e.AssetToken(binary, binaryText);
                e.AssetNode(SyntaxKind.NameExpression);
                e.AssetToken(SyntaxKind.IdentifierToken, "b");
            }
        }

        private static ExpressionSyntax ParseExpression(string text)
        {
            SyntaxTree syntaxTree = SyntaxTree.Parse(text);
            CompilationUnitSyntax root = syntaxTree.Root;
            MemberSyntax member = Assert.Single(root.Members);
            GlobalStatementSyntax globalStatement = Assert.IsType<GlobalStatementSyntax>(member);
            return Assert.IsType<ExpressionStatementSyntax>(globalStatement.Statement).Expression;
        }

        public static IEnumerable<object[]> GetBinaryOperatorPairsData()
        {
            foreach (SyntaxKind op1 in SyntaxFacts.GetBinaryOperatorKinds())
            {
                foreach (SyntaxKind op2 in SyntaxFacts.GetBinaryOperatorKinds())
                {
                    yield return new object[] { op1, op2 };
                }
            }
        }

        public static IEnumerable<object[]> GetUnaryOperatorPairsData()
        {
            foreach (SyntaxKind unary in SyntaxFacts.GetUnaryOperatorKinds())
            {
                foreach (SyntaxKind binary in SyntaxFacts.GetBinaryOperatorKinds())
                {
                    yield return new object[] { unary, binary };
                }
            }
        }
    }
}