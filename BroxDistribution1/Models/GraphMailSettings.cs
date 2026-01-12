namespace BroxDistribution.Models;

public class GraphMailSettings
{
    public string TenantId { get; set; } = "";
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string SenderUpn { get; set; } = "";      // mailbox Graph sends as
    public string AdminToEmail { get; set; } = "";   // where admin notifications go
}