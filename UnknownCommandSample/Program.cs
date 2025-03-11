using Microsoft.Extensions.Logging;
using Temporalio.Client;
using Temporalio.Worker;
using UnknownCommandSample;

using ILoggerFactory loggerFactory = LoggerFactory.Create(logging => logging.AddConsole());

TemporalClient client = await TemporalClient.ConnectAsync(new TemporalClientConnectOptions("localhost:9233")
{
	LoggerFactory = loggerFactory
});

string workflowId = $"workflow-{Guid.NewGuid()}";
string taskQueue = $"tq-{Guid.NewGuid()}";

TemporalWorkerOptions options = new(taskQueue);
options.DebugMode = true;
options.AddWorkflow<SleepThenReturn>();

using TemporalWorker worker = new(client, options);

await worker.ExecuteAsync(async () =>
{
	WorkflowOptions opt = new(workflowId, taskQueue)
	{
		TaskTimeout = TimeSpan.FromMinutes(10)
	};

	WorkflowHandle<SleepThenReturn, string[]> handle = await client.StartWorkflowAsync<SleepThenReturn, string[]>(wf => wf.RunAsync(Array.Empty<string>()), opt);

	try
	{
		await handle.SignalAsync(wf => wf.SendSignal("signal"));
	}

	// Proxy throws exception so we catch
	catch
	{
	}

	try
	{
		await handle.SignalAsync(wf => wf.SendSignal("signal2"));
	}

	// Proxy throws exception so we catch
	catch
	{
	}

	string[] result = await handle.GetResultAsync();

	// We expect this to be 2, but it's actually 1.
	ILogger<Program> log = loggerFactory.CreateLogger<Program>();

	log.LogInformation("Expected 2 signals to be found in result, but actual number was {count}.", result.Length);
});
