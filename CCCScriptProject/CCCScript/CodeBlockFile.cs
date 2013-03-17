public class CodeBlockFile
{
    public CodeBlockFile()
    {
        SourceCode = string.Empty;
        Definition = string.Empty;
        Reference = string.Empty;
        Class = string.Empty;
        Method = string.Empty;
        Field = string.Empty;
    }
    public string SourceCode { get; set; }
    public string Definition { get; set; }
    public string Reference { get; set; }
    public string Class { get; set; }
    public string Method { get; set; }
    public string Field { get; set; }
}