namespace AetherisOne.Models;

public class Quote
{
    public string Id { get; set; } = "";

    public string Category { get; set; } = "";

    public List<string> Types { get; set; } = [];

    public string Difficulty { get; set; } = "";

    public string Text { get; set; } = "";

    public string Author { get; set; } = "";

    public string Reflection { get; set; } = "";
}