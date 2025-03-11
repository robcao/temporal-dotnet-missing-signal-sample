using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Proxy;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddGrpc(options =>
{
	// Remove the size limit on inbound messages.
	options.MaxReceiveMessageSize = null;
	options.EnableDetailedErrors = true;
	options.Interceptors.Add<GrpcStatusCodeRewritingInterceptor>();
});

builder.Services.AddGrpcReflection();

builder.Services.AddGrpcHealthChecks(grpc => grpc.Services.Map("temporal.api.workflowservice.v1.WorkflowService", _ => true))
	.AddCheck("default", _ => HealthCheckResult.Healthy());

builder.Services.AddTemporalClient(options => options.TargetHost = "localhost:7233");

builder.WebHost.ConfigureKestrel((_, options) =>
{
	options.AllowAlternateSchemes = true;
	options.ListenAnyIP(9233, opt => opt.Protocols = HttpProtocols.Http2);
});

WebApplication app = builder.Build();

app.MapGrpcReflectionService();
app.MapGrpcHealthChecksService();
app.MapGrpcService<WorkflowServiceProxy>();

await app.RunAsync();
