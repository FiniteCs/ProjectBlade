using Blade.CodeAnalysis.Symbols;
using Blade.CodeAnalysis.Syntax;
using Blade.CodeAnalysis.Text;
using System.Collections;

namespace Blade.CodeAnalysis
{
    internal sealed class DiagnosticBag : IEnumerable<Diagnostic>
    {
        private readonly List<Diagnostic> _diagnostics = new();

        public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void AddRange(DiagnosticBag diagnostics)
        {
            _diagnostics.AddRange(diagnostics._diagnostics);
        }

        private void Report(TextSpan span, string message)
        {
            Diagnostic daignostic = new(span, message);
            _diagnostics.Add(daignostic);
        }

        public void ReportInvalidNumber(TextSpan span, string text, TypeSymbol type)
        {
            string message = $"The number {text} isn't valid {type}.";
            Report(span, message);
        }
        public void ReportBadCharacter(int position, char character)
        {
            TextSpan span = new(position, 1);
            string message = $"Bad character input: '{character}'.";
            Report(span, message);
        }
        public void ReportUnterminatedString(TextSpan span)
        {
            string message = "Unterminated string literal.";
            Report(span, message);
        }
        public void ReportUnexpectedToken(TextSpan span, SyntaxKind actualKind, SyntaxKind expectedKind)
        {
            string message = $"Unexpected token <{actualKind}>, expected <{expectedKind}>.";
            Report(span, message);
        }
        public void ReportUndefinedUnaryOperator(TextSpan span, string operatorText, TypeSymbol operandType)
        {
            string message = $"Unary operator '{operatorText}' is not defined for type '{operandType}'.";
            Report(span, message);
        }
        public void ReportUndefinedBinaryOperator(TextSpan span, string operatorText, TypeSymbol leftType, TypeSymbol rightType)
        {
            string message = $"Binary operator '{operatorText}' is not defined for types '{leftType}' and '{rightType}'.";
            Report(span, message);
        }

        public void ReportParameterAlreadyDeclared(TextSpan span, string parameterName)
        {
            string message = $"A parameter with the name '{parameterName}' already exists.";
            Report(span, message);
        }

        public void ReportUndefinedName(TextSpan span, string name)
        {
            string message = $"Variable '{name}' doesn't exist.";
            Report(span, message);
        }

        public void ReportUndefinedType(TextSpan span, string name)
        {
            string message = $"Type '{name}' doesn't exist.";
            Report(span, message);
        }

        public void ReportCannotConvert(TextSpan span, TypeSymbol fromType, TypeSymbol toType)
        {
            string message = $"Cannot convert type '{fromType}' to '{toType}'.";
            Report(span, message);
        }

        public void ReportCannotConvertImplicitly(TextSpan span, TypeSymbol fromType, TypeSymbol toType)
        {
            string message = $"Cannot convert type '{fromType}' to '{toType}'. An explicit conversion exists (are you missing a cast?)";
            Report(span, message);
        }

        public void ReportSymbolAlreadyDeclared(TextSpan span, string name)
        {
            string message = $"'{name}' is already declared.";
            Report(span, message);
        }
        public void ReportCannotAssign(TextSpan span, string name)
        {
            string message = $"Variable '{name}' is read-only and cannot be assigned to.";
            Report(span, message);
        }
        public void ReportUndefinedFunction(TextSpan span, string name)
        {
            string message = $"Function '{name}' doesn't exist.";
            Report(span, message);
        }
        public void ReportWrongArgumentCount(TextSpan span, string name, int expectedCount, int actualCount)
        {
            string message = $"Function '{name}' requires {expectedCount} arguments but was given {actualCount}.";
            Report(span, message);
        }
        public void ReportWrongArgumentType(TextSpan span, string name, TypeSymbol expectedType, TypeSymbol actualType)
        {
            string message = $"Parameter '{name}' requires a value of type '{expectedType}' but was given a value of type '{actualType}'.";
            Report(span, message);
        }
        public void ReportExpressionMustHaveValue(TextSpan span)
        {
            string message = "Expression must have a value.";
            Report(span, message);
        }

        public void XXX_ReportFunctionsAreUnsupported(TextSpan span)
        {
            string message = "Functions with return values are unsupported.";
            Report(span, message);
        }
    }
}
