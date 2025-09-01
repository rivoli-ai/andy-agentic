public class ChatHistorySummaryDto
{
    public int TotalMessages { get; set; }
    public int TotalSessions { get; set; }
    public int TotalTokens { get; set; }
    public DateTime OldestMessage { get; set; }
    public DateTime NewestMessage { get; set; }
    public Dictionary<string?, int> MessagesByAgent { get; set; } = new();
    public Dictionary<string, int> MessagesByRole { get; set; } = new();
    public Dictionary<DateTime, int> MessagesByDate { get; set; } = new();
}
