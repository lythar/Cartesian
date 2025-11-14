using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// builder.AddDockerComposeEnvironment("cartesian-compose");

var postgres = builder.AddPostgres("postgres")
    .WithImage("postgis/postgis:18-3.6-alpine")
    .WithHostPort(41021)
    .WithDataVolume();

var cartesianDb = postgres.AddDatabase("database", "cartesian");

var cartesianStorage = builder.AddMinioContainer("cartesian-storage")
    .WithHostPort(41022)
    .WithDataVolume();

var services = builder.AddProject<Cartesian_Services>("services")
    .WithReference(cartesianDb)
    .WithReference(cartesianStorage);

builder.AddNodeApp("frontend", "../Cartesian.Frontend", "dev")
    .WithPnpm()
    .WithRunScript("dev")
    .WithReference(services)
    .WithHttpEndpoint(env: "PORT", targetPort: 5173, isProxied: false)
    .WithExternalHttpEndpoints();

builder.Build().Run();
