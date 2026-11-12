using Application.Interfaces;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Notifications;

public sealed class FirebasePushNotificationService(
    DiabetesDbContext db,
    IConfiguration configuration,
    ILogger<FirebasePushNotificationService> logger) : IPushNotificationService
{
    private const string AppName = "DiabetesPlatform";

    public async Task<int> SendToUserAsync(
        Guid userId,
        string title,
        string body,
        IReadOnlyDictionary<string, string>? data = null,
        CancellationToken cancellationToken = default)
    {
        if (!configuration.GetValue<bool>("Firebase:Enabled"))
            return 0;

        var deviceTokens = await db.DeviceTokens
            .Where(x => x.UserId == userId)
            .Select(x => x.Token)
            .ToListAsync(cancellationToken);

        if (deviceTokens.Count == 0)
            return 0;

        var app = GetOrCreateApp();
        var messaging = FirebaseMessaging.GetMessaging(app);
        var sent = 0;

        foreach (var token in deviceTokens)
        {
            try
            {
#pragma warning disable CS0618
                var message = new Message
                {
                    Token = token,
#pragma warning restore CS0618
                    Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = title,
                        Body = body
                    },
                    Data = data is null
                        ? new Dictionary<string, string>()
                        : new Dictionary<string, string>(data)
                };

                await messaging.SendAsync(message, cancellationToken);
                sent++;

                var device = await db.DeviceTokens
                    .SingleOrDefaultAsync(x => x.Token == token, cancellationToken);

                if (device is not null)
                    device.LastUsedAt = DateTime.UtcNow;
            }
            catch (FirebaseMessagingException ex)
                when (ex.MessagingErrorCode == MessagingErrorCode.Unregistered)
            {
                await db.DeviceTokens
                    .Where(x => x.Token == token)
                    .ExecuteDeleteAsync(cancellationToken);
            }
            catch (FirebaseMessagingException ex)
            {
                logger.LogWarning(
                    ex,
                    "FCM delivery failed for user {UserId} and device token.",
                    userId);
            }
        }

        if (sent > 0)
            await db.SaveChangesAsync(cancellationToken);

        return sent;
    }

    private static FirebaseApp GetOrCreateApp()
    {
        var existing = FirebaseApp.GetInstance(AppName);
        if (existing is not null)
            return existing;

        return FirebaseApp.Create(
            new AppOptions
            {
                Credential = GoogleCredential.GetApplicationDefault()
            },
            AppName);
    }
}
