var builder = DistributedApplication.CreateBuilder(args);

// Configure PostgreSQL with Docker container and persistent volume
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .AddDatabase("authorizerdb");

// Configure API with database reference
var api = builder.AddProject<Projects.Authorizer_Api>("authorizer-api")
    .WithReference(postgres);

builder.Build().Run();
