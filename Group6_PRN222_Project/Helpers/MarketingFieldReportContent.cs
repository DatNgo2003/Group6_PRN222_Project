using System.Text.Json;
using System.Text.Json.Serialization;

namespace Project_PRN222.Helpers;

public class MarketingFieldReportPayload
{
    [JsonPropertyName("v")]
    public int V { get; set; } = 1;

    /// <summary>Thay đổi / cập nhật so với kỳ trước (tuỳ nội dung báo cáo).</summary>
    [JsonPropertyName("changes")]
    public string? Changes { get; set; }

    [JsonPropertyName("reportBody")]
    public string? ReportBody { get; set; }

    [JsonPropertyName("taskEstimates")]
    public List<MarketingTaskEstimateRow> TaskEstimates { get; set; } = new();
}

public class MarketingTaskEstimateRow
{
    [JsonPropertyName("taskId")]
    public int TaskId { get; set; }

    [JsonPropertyName("amount")]
    public decimal? Amount { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }
}

public static class MarketingFieldReportContent
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static string Serialize(MarketingFieldReportPayload payload)
    {
        payload.V = 1;
        return JsonSerializer.Serialize(payload, JsonOpts);
    }

    /// <summary>
    /// Nếu Content không phải JSON (báo cáo cũ), coi toàn bộ chuỗi là nội dung tự do.
    /// </summary>
    public static MarketingFieldReportPayload Parse(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return new MarketingFieldReportPayload();

        var trimmed = content.TrimStart();
        if (!trimmed.StartsWith('{'))
            return new MarketingFieldReportPayload { ReportBody = content };

        try
        {
            var p = JsonSerializer.Deserialize<MarketingFieldReportPayload>(content, JsonOpts);
            return p ?? new MarketingFieldReportPayload { ReportBody = content };
        }
        catch
        {
            return new MarketingFieldReportPayload { ReportBody = content };
        }
    }

    public static decimal SumEstimates(MarketingFieldReportPayload payload) =>
        payload.TaskEstimates.Sum(t => t.Amount ?? 0m);

    public static string? PreviewPlainText(string? content)
    {
        var p = Parse(content);
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(p.Changes))
            parts.Add(p.Changes.Trim());
        if (!string.IsNullOrWhiteSpace(p.ReportBody))
            parts.Add(p.ReportBody.Trim());
        if (parts.Count == 0)
            return null;
        var s = string.Join(" · ", parts);
        return s.Length > 200 ? s[..200] + "…" : s;
    }
}
