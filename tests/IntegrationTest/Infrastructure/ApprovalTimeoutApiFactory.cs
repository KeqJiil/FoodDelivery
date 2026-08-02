using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace FoodDelivery.IntegrationTest.Infrastructure;

public class ApprovalTimeoutApiFactory : FoodDeliveryApiFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Saga:TimeoutApprovement"] = "1"
            });
        });
    }
}
