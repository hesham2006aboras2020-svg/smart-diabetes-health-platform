using Domain.Enums;
namespace Domain.Entities;
public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? AlertId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsSent { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public User User { get; set; } = null!;
    public Alert? Alert { get; set; }
}