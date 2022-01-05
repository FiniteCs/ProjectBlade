using Blade.CodeAnalysis.Binding;
using Blade.CodeAnalysis.Symbols;
using Blade.CodeAnalysis.Syntax;

namespace Blade.CodeAnalysis.Lowering
{
    internal sealed class Lowerer : BoundTreeRewriter
    {
        private int _labelCount;

        private Lowerer()
        {
        }

        private BoundLabel GenerateLabel()
        {
            string name = $"Label{++_labelCount}";
            return new BoundLabel(name);
        }

        public static BoundBlockStatement<TBlockMember> Lower<TBlockMember>(TBlockMember blockMember)
            where TBlockMember : BoundStatement
        {
            Lowerer lowerer = new();
            TBlockMember result = (TBlockMember)lowerer.RewriteStatement(blockMember);
            return Flatten(result);
        }

        private static BoundBlockStatement<TBlockMember> Flatten<TBlockMember>(TBlockMember blockMember)
        {
            ImmutableArray<TBlockMember>.Builder builder = ImmutableArray.CreateBuilder<TBlockMember>();
            Stack<TBlockMember> stack = new();
            stack.Push(blockMember);
            while (stack.Count > 0)
            {
                TBlockMember current = stack.Pop();
                if (current is BoundBlockStatement<TBlockMember> block)
                {
                    foreach (TBlockMember member in block.Statements.Reverse())
                        stack.Push(member);
                }
                else
                {
                    builder.Add(current);
                }
            }
            return new BoundBlockStatement<TBlockMember>(builder.ToImmutable());
        }

        protected override BoundStatement RewriteIfStatement(BoundIfStatement node)
        {
            if (node.ElseStatement == null)
            {
                // if <condition>
                //      <then>
                //
                // ---->
                //
                // gotoFalse <condition> end
                // <then>
                // end:
                BoundLabel endLabel = GenerateLabel();
                BoundConditionalGotoStatement gotoFalse = new(endLabel, node.Condition, false);
                BoundLabelStatement endLabelStatement = new(endLabel);
                BoundBlockStatement<BoundStatement> result = new(ImmutableArray.Create(gotoFalse, node.ThenStatement, endLabelStatement));
                return RewriteStatement(result);
            }
            else
            {
                // if <condition>
                //      <then>
                // else
                //      <else>
                //
                // ---->
                //
                // gotoFalse <condition> else
                // <then>
                // goto end
                // else:
                // <else>
                // end:
                BoundLabel elseLabel = GenerateLabel();
                BoundLabel endLabel = GenerateLabel();
                BoundConditionalGotoStatement gotoFalse = new(elseLabel, node.Condition, false);
                BoundGotoStatement gotoEndStatement = new(endLabel);
                BoundLabelStatement elseLabelStatement = new(elseLabel);
                BoundLabelStatement endLabelStatement = new(endLabel);
                BoundBlockStatement<BoundStatement> result = new(ImmutableArray.Create(
                    gotoFalse,
                    node.ThenStatement,
                    gotoEndStatement,
                    elseLabelStatement,
                    node.ElseStatement,
                    endLabelStatement
                ));
                return RewriteStatement(result);
            }
        }

        protected override BoundStatement RewriteWhileStatement(BoundWhileStatement node)
        {
            // while <condition>
            //      <body>
            //
            // ----->
            //
            // goto check
            // continue:
            // <body>
            // check:
            // gotoTrue <condition> continue
            //
            BoundLabel continueLabel = GenerateLabel();
            BoundLabel checkLabel = GenerateLabel();
            BoundGotoStatement gotoCheck = new(checkLabel);
            BoundLabelStatement continueLabelStatement = new(continueLabel);
            BoundLabelStatement checkLabelStatement = new(checkLabel);
            BoundConditionalGotoStatement gotoTrue = new(continueLabel, node.Condition);
            BoundBlockStatement<BoundStatement> result = new(ImmutableArray.Create<BoundStatement>(
                gotoCheck,
                continueLabelStatement,
                node.Body,
                checkLabelStatement,
                gotoTrue
            ));
            return RewriteStatement(result);
        }

        protected override BoundStatement RewriteDoWhileStatement(BoundDoWhileStatement node)
        {
            // do
            //      <body>
            // while <condition>
            //
            // ----->
            //
            // continue:
            // <body>
            // gotoTrue <condition> continue
            //
            BoundLabel continueLabel = GenerateLabel();
            BoundLabelStatement continueLabelStatement = new(continueLabel);
            BoundConditionalGotoStatement gotoTrue = new(continueLabel, node.Condition);
            BoundBlockStatement<BoundStatement> result = new(ImmutableArray.Create(
                continueLabelStatement,
                node.Body,
                gotoTrue
            ));
            return RewriteStatement(result);
        }

        protected override BoundStatement RewriteForStatement(BoundForStatement node)
        {
            // for <var> = <lower> to <upper>
            //      <body>
            //
            // ---->
            //
            // {
            //      var <var> = <lower>
            //      let upperBound = <upper>
            //      while (<var> <= upperBound)
            //      {
            //          <body>
            //          <var> = <var> + 1
            //      }
            // }

            BoundVariableDeclaration variableDeclaration = new(node.Variable, node.LowerBound);
            BoundVariableExpression variableExpression = new(node.Variable);
            LocalVariableSymbol upperBoundSymbol = new("upperBound", true, TypeSymbol.Int);
            BoundVariableDeclaration upperBoundDeclaration = new(upperBoundSymbol, node.UpperBound);
            BoundBinaryExpression condition = new(
                variableExpression,
                BoundBinaryOperator.Bind(SyntaxKind.LessOrEqualsToken, TypeSymbol.Int, TypeSymbol.Int),
                new BoundVariableExpression(upperBoundSymbol)
            );
            BoundExpressionStatement increment = new(
                new BoundAssignmentExpression(
                    node.Variable,
                    new BoundBinaryExpression(
                        variableExpression,
                        BoundBinaryOperator.Bind(SyntaxKind.PlusToken, TypeSymbol.Int, TypeSymbol.Int),
                        new BoundLiteralExpression(1)
                    )
                )
            );
            BoundBlockStatement<BoundStatement> whileBody = new(ImmutableArray.Create(node.Body, increment));
            BoundWhileStatement whileStatement = new(condition, whileBody);
            BoundBlockStatement<BoundStatement> result = new(ImmutableArray.Create<BoundStatement>(
                variableDeclaration,
                upperBoundDeclaration,
                whileStatement
            ));
            return RewriteStatement(result);
        }
    }
}
