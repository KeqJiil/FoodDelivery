namespace FoodDelivery.IntegrationTest.Infrastructure;

public static class Eventually
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan DefaultPollInterval = TimeSpan.FromMilliseconds(100);

    public static async Task AssertAsync(Func<Task> assertion, TimeSpan? timeout = null, TimeSpan? pollInterval = null)
    {
        var deadline = DateTime.UtcNow + (timeout ?? DefaultTimeout);
        var interval = pollInterval ?? DefaultPollInterval;

        while (true)
        {
            try
            {
                await assertion();
                return;
            }
            catch (Exception) when (DateTime.UtcNow < deadline)
            {
                await Task.Delay(interval);
            }
        }
    }
}
