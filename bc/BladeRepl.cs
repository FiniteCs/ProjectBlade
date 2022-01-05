using Blade.CodeAnalysis;
using Blade.CodeAnalysis.Symbols;
using Blade.CodeAnalysis.Syntax;
using Blade.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Blade
{
    internal sealed class BladeRepl : Repl
    {
        private Compilation _previous;
        private bool _showTree;
        private bool _showProgram;
        private readonly Dictionary<VariableSymbol, object> _variables = new();


        protected override void RenderLine(string line)
        {
            IEnumerable<SyntaxToken> tokens = SyntaxTree.ParseTokens(line);
            foreach (SyntaxToken token in tokens)
            {
                bool isKeyword = token.Kind.ToString().EndsWith("Keyword");
                bool isNumber = token.Kind == SyntaxKind.NumberToken;
                bool isOperator = token.Kind.GetBinaryOperatorPrecedence() > 0 ||
                                  token.Kind.GetUnaryOperatorPrecedence() > 0 ||
                                  token.Kind == SyntaxKind.EqualsToken;
                bool isIdentifier = token.Kind == SyntaxKind.IdentifierToken;
                bool isString = token.Kind == SyntaxKind.StringToken;

                if (isKeyword ||
                    token.Text == "int" ||
                    token.Text == "string" ||
                    token.Text == "bool" ||
                    token.Text == "void")
                    Console.ForegroundColor = ConsoleColor.Blue;
                else if (isNumber)
                    Console.ForegroundColor = ConsoleColor.Cyan;
                else if (isOperator)
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                else if (isIdentifier)
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                else if (isString)
                    Console.ForegroundColor = ConsoleColor.Magenta;

                Console.Write(token.Text);

                Console.ResetColor();
            }
        }

        protected override void EvaluateMetaCommand(string input)
        {
            switch (input)
            {
                case "#showTree":
                    _showTree = !_showTree;
                    Console.WriteLine(_showTree ? "Showing Parse Tree." : "Not Showing Parse Tree.");
                    break;
                case "#showProgram":
                    _showProgram = !_showProgram;
                    Console.WriteLine(_showProgram ? "Showing Bound Tree." : "Not Showing Bound Tree.");
                    break;
                case "#cls":
                    Console.Clear();
                    break;
                case "#reset":
                    _previous = null;
                    _variables.Clear();
                    break;
                default:
                    base.EvaluateMetaCommand(input);
                    break;
            }
        }

        protected override void EvaluateSubmission(string text)
        {
            SyntaxTree syntaxTree = SyntaxTree.Parse(text);

            Compilation compilation = _previous == null
                                      ? new(syntaxTree)
                                      : _previous.ContinueWith(syntaxTree);

            if (_showTree)
            {
                syntaxTree.Root.WriteTo(Console.Out);
            }

            if (_showProgram)
                compilation.EmitTree(Console.Out);

            EvaluationResult result;
            result = compilation.Evaluate(_variables);
#if false
            try
            {
                result = compilation.Evaluate(_variables);
            }
            catch (Exception e)
            {
                result = new(ImmutableArray.Create<Diagnostic>(), -1);
                Console.WriteLine(e.Message);
            }
#endif

            if (!result.Diagnostics.Any())
            {
                if (result != null)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    if (result.Value is object[] array)
                    {
                        foreach (object o in array)
                            Console.Write($"{o}, ");

                        Console.WriteLine();
                        goto endOfMethod;
                    }

                    Console.WriteLine(result.Value);
                    Console.ResetColor();
                }

                _previous = compilation;
            }
            else
            {

                foreach (Diagnostic diagnostic in result.Diagnostics)
                {
                    int lineIndex = syntaxTree.Text.GetLineIndex(diagnostic.Span.Start);
                    int lineNumber = lineIndex + 1;
                    TextLine line = syntaxTree.Text.Lines[lineIndex];
                    int character = diagnostic.Span.Start - line.Start + 1;

                    Console.WriteLine();

                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write($"({lineNumber}, {character}): ");
                    Console.WriteLine(diagnostic);
                    Console.ResetColor();

                    TextSpan prefixSpan = TextSpan.FromBounds(line.Start, diagnostic.Span.Start);
                    TextSpan suffixSpan = TextSpan.FromBounds(diagnostic.Span.End, line.End);

                    string prefix = syntaxTree.Text.ToString(prefixSpan);
                    string error = syntaxTree.Text.ToString(diagnostic.Span);
                    string suffix = syntaxTree.Text.ToString(suffixSpan);

                    Console.Write("    ");
                    Console.Write(prefix);

                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write(error);
                    Console.ResetColor();

                    Console.Write(suffix);

                    Console.WriteLine();
                }

                Console.WriteLine();
            }
        endOfMethod:
            return;
        }

        protected override bool IsCompleteSubmission(string text)
        {
            if (string.IsNullOrEmpty(text))
                return true;
            bool lastTwoLinesAreBlank = text.Split(Environment.NewLine)
                                           .Reverse()
                                           .TakeWhile(s => string.IsNullOrEmpty(s))
                                           .Take(2)
                                           .Count() == 2;
            if (lastTwoLinesAreBlank)
                return true;

            SyntaxTree syntaxTree = SyntaxTree.Parse(text);

            if (syntaxTree.Root.Members.Last().GetLastToken().IsMissing)
                return false;

            return true;
        }
    }
}
