using Grpc.Core;
using Temporalio.Api.Command.V1;
using Temporalio.Api.WorkflowService.V1;
using Temporalio.Client;

namespace Proxy;

internal partial class WorkflowServiceProxy(ITemporalClient client) : Temporalio.Api.WorkflowService.V1.WorkflowService.WorkflowServiceBase
{
	internal static List<SignalWorkflowExecutionRequest> PendingSignals { get; } = new();

	public override async Task<GetSystemInfoResponse> GetSystemInfo(GetSystemInfoRequest request, ServerCallContext context)
	{
		GetSystemInfoResponse response = await client.WorkflowService.GetSystemInfoAsync(request, context.ToRpcOptions()).ConfigureAwait(false);
		return response;
	}

	public override Task<PollWorkflowTaskQueueResponse> PollWorkflowTaskQueue(PollWorkflowTaskQueueRequest request, ServerCallContext context)
	{
		return client.WorkflowService.PollWorkflowTaskQueueAsync(request, context.ToRpcOptions());
	}

	public override Task<StartWorkflowExecutionResponse> StartWorkflowExecution(StartWorkflowExecutionRequest request, ServerCallContext context)
	{
		return client.WorkflowService.StartWorkflowExecutionAsync(request, context.ToRpcOptions());
	}

	public override Task<SignalWorkflowExecutionResponse> SignalWorkflowExecution(SignalWorkflowExecutionRequest request, ServerCallContext context)
	{
		PendingSignals.Add(request);
		// return client.WorkflowService.SignalWorkflowExecutionAsync(request, context.ToRpcOptions());
		return base.SignalWorkflowExecution(request, context);
	}

	public override Task<QueryWorkflowResponse> QueryWorkflow(QueryWorkflowRequest request, ServerCallContext context)
	{
		return client.WorkflowService.QueryWorkflowAsync(request, context.ToRpcOptions());
	}

	public override async Task<RespondWorkflowTaskCompletedResponse> RespondWorkflowTaskCompleted(RespondWorkflowTaskCompletedRequest request, ServerCallContext context)
	{
		if (request.Commands.Any(command => command.AttributesCase == Command.AttributesOneofCase.ContinueAsNewWorkflowExecutionCommandAttributes))
		{
			foreach (SignalWorkflowExecutionRequest signal in PendingSignals)
			{
				await client.WorkflowService.SignalWorkflowExecutionAsync(signal, context.ToRpcOptions());
			}

			PendingSignals.Clear();
		}

		return await client.WorkflowService.RespondWorkflowTaskCompletedAsync(request, context.ToRpcOptions());
	}

	public override Task<GetWorkflowExecutionHistoryResponse> GetWorkflowExecutionHistory(GetWorkflowExecutionHistoryRequest request, ServerCallContext context)
	{
		return client.WorkflowService.GetWorkflowExecutionHistoryAsync(request, context.ToRpcOptions());
	}

	public override Task<GetWorkflowExecutionHistoryReverseResponse> GetWorkflowExecutionHistoryReverse(GetWorkflowExecutionHistoryReverseRequest request, ServerCallContext context)
	{
		return client.WorkflowService.GetWorkflowExecutionHistoryReverseAsync(request, context.ToRpcOptions());
	}

	public override Task<ShutdownWorkerResponse> ShutdownWorker(ShutdownWorkerRequest request, ServerCallContext context)
	{
		return client.WorkflowService.ShutdownWorkerAsync(request, context.ToRpcOptions());
	}
}