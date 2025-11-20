using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("compose");

var postgres = builder.AddPostgres("postgres")
    .WithImage("postgis/postgis:18-alpine")
    .WithHostPort(41021)
    .WithDataVolume();

var database = postgres.AddDatabase("database", "cartesian");

var storage = builder.AddMinioContainer("storage")
    .WithHostPort(41022)
    .WithDataVolume();

var services = builder.AddProject<Cartesian_Services>("services")
    .WithReference(database)
    .WithReference(storage);

builder.AddNodeApp("frontend", "../Cartesian.Frontend", "dev")
    .WithPnpm()
    .WithRunScript("dev")
    .WithReference(services)
    .WithHttpEndpoint(env: "PORT", targetPort: 5173, isProxied: false)
    .WithExternalHttpEndpoints();

builder.Build().Run();
