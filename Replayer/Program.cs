using Replayer;
using Temporalio.Common;
using Temporalio.Worker;

WorkflowReplayerOptions options = new()
{
	DebugMode = true,
};

options.AddWorkflow<SleepThenReturn>();

WorkflowReplayer replayer = new(options);

string json = File.ReadAllText(@"C:\Sourcecode\temporal-cancel-example\Replayer\2ece439e-b112-418e-bae0-385cfe77f38f_events.json");

await replayer.ReplayWorkflowAsync(WorkflowHistory.FromJson("2ece439e-b112-418e-bae0-385cfe77f38f", json)).ConfigureAwait(false);
