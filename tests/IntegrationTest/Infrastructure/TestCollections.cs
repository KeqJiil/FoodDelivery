namespace FoodDelivery.IntegrationTest.Infrastructure;

[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<MsSqlContainerFixture>;