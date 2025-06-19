using Rich.Lexer;
using Rich.Parser.SyntaxNodes;

namespace Rich.Semantics;

public static class LanguageDefinedTypes
{
    public static readonly TypeDefinitionSyntax NothingType =
        new(new IdentifierSyntax(new SpanMeta("nothing", string.Empty, 0, 0, 0)));
    public static readonly TypeDefinitionSyntax IntegerType =
        new(new IdentifierSyntax(new SpanMeta("int", string.Empty, 0, 0, 0)));
    public static readonly TypeDefinitionSyntax DecimalType =
        new(new IdentifierSyntax(new SpanMeta("decimal", string.Empty, 0, 0, 0)));
    public static readonly TypeDefinitionSyntax BooleanType =
        new(new IdentifierSyntax(new SpanMeta("bool", string.Empty, 0, 0, 0)));
    public static readonly TypeDefinitionSyntax StringType =
        new(new IdentifierSyntax(new SpanMeta("str", string.Empty, 0, 0, 0)));
    public static readonly TypeDefinitionSyntax ByteType =
        new(new IdentifierSyntax(new SpanMeta("byte", string.Empty, 0, 0, 0)));
}