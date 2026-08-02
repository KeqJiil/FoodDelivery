namespace FoodDelivery.IntegrationTest.Infrastructure;

[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<MsSqlContainerFixture>;

[CollectionDefinition("Api")]
public class ApiCollection : ICollectionFixture<FoodDeliveryApiFactory>;

[CollectionDefinition("ApprovalTimeout")]
public class ApprovalTimeoutCollection : ICollectionFixture<ApprovalTimeoutApiFactory>;