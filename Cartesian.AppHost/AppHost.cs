var builder = DistributedApplication.CreateBuilder(args);

// builder.AddDockerComposeEnvironment("cartesian-compose");

var postgres = builder.AddPostgres("postgres")
    .WithImage("postgis/postgis:18-3.6-alpine")
    .WithDataVolume();

var cartesianDb = postgres.AddDatabase("database", "cartesian");

var services = builder.AddProject<Projects.Cartesian_Services>("services")
    .WithReference(cartesianDb);

builder.AddNpmApp("frontend", "../Cartesian.Frontend", "dev")
    .WithReference(services)
    .WithHttpEndpoint(env: "VITE_PORT", targetPort: 5173, isProxied: false)
    .WithExternalHttpEndpoints();

builder.Build().Run();
