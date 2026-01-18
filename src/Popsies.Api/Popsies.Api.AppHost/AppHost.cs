var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Gateway>("gateway");

builder.Build().Run();
