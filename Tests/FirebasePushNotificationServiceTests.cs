using Infrastructure.Data;
using Infrastructure.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Tests;

public sealed class FirebasePushNotificationServiceTests
{
    [Fact]
    public async Task SendToUserAsync_WhenDisabled_DoesNotRequireFirebaseCredentials()
    {
        await using var db = new DiabetesDbContext(
            new DbContextOptionsBuilder<DiabetesDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Firebase:Enabled"] = "false"
            })
            .Build();

        var service = new FirebasePushNotificationService(
            db,
            configuration,
            NullLogger<FirebasePushNotificationService>.Instance);

        var sent = await service.SendToUserAsync(
            Guid.NewGuid(),
            "اختبار",
            "رسالة اختبار",
            cancellationToken: CancellationToken.None);

        Assert.Equal(0, sent);
    }
}
