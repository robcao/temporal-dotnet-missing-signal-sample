using Microsoft.Extensions.Logging;
using Temporalio.Workflows;

namespace UnknownCommandSample;

[Workflow]
public class SleepThenReturn
{
	internal List<string> Signals { get; init; } = new();

	[WorkflowRun]
	public async Task<string[]> RunAsync(string[] input)
	{
		if (!string.IsNullOrWhiteSpace(Workflow.Info.ContinuedRunId))
		{
			Workflow.Logger.LogInformation("Now continuing as new, there are {count} signals.", input.Length);

			return input;
		}

		await Workflow.WhenAnyAsync(Workflow.DelayAsync(TimeSpan.FromSeconds(5)), Workflow.WaitConditionAsync(() => Signals.Count > 0));

		await Workflow.WaitConditionAsync(() => Workflow.AllHandlersFinished);

		List<string> next = new();

		foreach (string signal in Signals)
		{
			next.Add(signal);
		}

		throw Workflow.CreateContinueAsNewException<SleepThenReturn>(wf => wf.RunAsync(next.ToArray()));
	}

	[WorkflowSignal]
	public Task SendSignal(string signal)
	{
		Workflow.Logger.LogInformation("Handling signal input {signal}.", signal);
		Signals.Add(signal);
		return Task.CompletedTask;
	}
}

