using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.DTOs;
using NotificationService.Models;

namespace NotificationService.Services;

public interface INotificationService
{
    Task<IReadOnlyList<NotificationResponseDto>> GetForUserAsync(int recipientUserId, bool unreadOnly, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(int recipientUserId, CancellationToken ct = default);
    Task<NotificationResponseDto?> MarkAsReadAsync(int id, int recipientUserId, CancellationToken ct = default);
    Task MarkAllAsReadAsync(int recipientUserId, CancellationToken ct = default);
    Task<NotificationResponseDto> CreateAsync(int recipientUserId, string title, string message, string type, CancellationToken ct = default);
}

public class NotificationManager(NotificationDbContext db) : INotificationService
{
    public async Task<IReadOnlyList<NotificationResponseDto>> GetForUserAsync(int recipientUserId, bool unreadOnly, CancellationToken ct = default)
    {
        var query = db.Notifications.AsNoTracking().Where(x => x.RecipientUserId == recipientUserId);
        if (unreadOnly) query = query.Where(x => !x.IsRead);
        return await query.OrderByDescending(x => x.CreatedAt).Select(x => new NotificationResponseDto
        {
            Id = x.Id, Title = x.Title, Message = x.Message, Type = x.Type,
            IsRead = x.IsRead, CreatedAt = x.CreatedAt, ReadAt = x.ReadAt
        }).ToListAsync(ct);
    }

    public Task<int> GetUnreadCountAsync(int recipientUserId, CancellationToken ct = default) =>
        db.Notifications.CountAsync(x => x.RecipientUserId == recipientUserId && !x.IsRead, ct);

    public async Task<NotificationResponseDto?> MarkAsReadAsync(int id, int recipientUserId, CancellationToken ct = default)
    {
        var notification = await db.Notifications.SingleOrDefaultAsync(x => x.Id == id && x.RecipientUserId == recipientUserId, ct);
        if (notification is null) return null;
        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return Map(notification);
    }

    public async Task MarkAllAsReadAsync(int recipientUserId, CancellationToken ct = default)
    {
        var items = await db.Notifications.Where(x => x.RecipientUserId == recipientUserId && !x.IsRead).ToListAsync(ct);
        var readAt = DateTime.UtcNow;
        foreach (var item in items) { item.IsRead = true; item.ReadAt = readAt; }
        if (items.Count > 0) await db.SaveChangesAsync(ct);
    }

    public async Task<NotificationResponseDto> CreateAsync(int recipientUserId, string title, string message, string type, CancellationToken ct = default)
    {
        if (recipientUserId <= 0 || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("A recipient, title, and message are required.");
        var notification = new Notification
        {
            RecipientUserId = recipientUserId, Title = title.Trim(), Message = message.Trim(),
            Type = string.IsNullOrWhiteSpace(type) ? "General" : type.Trim(), CreatedAt = DateTime.UtcNow
        };
        db.Notifications.Add(notification);
        await db.SaveChangesAsync(ct);
        return Map(notification);
    }

    private static NotificationResponseDto Map(Notification x) => new()
    {
        Id = x.Id, Title = x.Title, Message = x.Message, Type = x.Type,
        IsRead = x.IsRead, CreatedAt = x.CreatedAt, ReadAt = x.ReadAt
    };
}
