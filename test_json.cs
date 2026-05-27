using System;
using System.Text.Json;
using System.Collections.Generic;

public class ApiNinjasCarResponse
{
    public string Class { get; set; }
}

public class Program
{
    public static void Main()
    {
        var json = "[{\"class\": \"midsize car\"}]";
        var res = JsonSerializer.Deserialize<List<ApiNinjasCarResponse>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Console.WriteLine(res[0].Class);
    }
}
