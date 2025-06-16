namespace Rich.Lexer;

public class SpanMeta
{
    public string? Text { get; set; }
    public string FilePath { get; set; }
    public int CharacterPosition { get; set; }
    public int Line { get; set; }
    public int Column { get; set; }

    private SpanMeta()
    {
        FilePath = string.Empty;
    }

    public SpanMeta(string? text, string filePath, int characterPosition, int line, int column)
    {
        Text = text;
        FilePath = filePath;
        CharacterPosition = characterPosition;
        Line = line;
        Column = column;
    }
    
    public SpanMeta Combine(SpanMeta span)
    {
        if (span.FilePath != FilePath) throw new Exception("File paths are not equal so spans cannot be combined.");
        
        var spanMeta = new SpanMeta
        {
            FilePath = FilePath,
            Text = Text + span.Text,
            CharacterPosition = span.CharacterPosition,
            Line = Line,
            Column = span.Column
        };

        return spanMeta;
    }

    public override string ToString()
    {
        return $"[ Text: |{Text}|, Line: {Line}, Column: {Column}, File Path: {FilePath} ]";
    }
}