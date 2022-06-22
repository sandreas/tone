using System.Collections.Generic;

namespace tone.Api.JavaScript;

public class FetchData
{
    public string Method { get; set; } = "get";
    public Dictionary<string, string> Headers { get; set; } = new();
    public string Body { get; set; } = "";
}