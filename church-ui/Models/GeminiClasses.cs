namespace Controllers;

public class GeminiRequest
{
    public GeminiContent[] contents { get; set; }
}

public class GeminiContent
{
    public GeminiPart[] parts { get; set; }
}

public class GeminiPart
{
    public GeminiInlineData inlineData { get; set; }
    public string text { get; set; }
}

public class GeminiInlineData
{
    public string mimeType { get; set; }
    public string data { get; set; }
}