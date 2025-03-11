using Grpc.Core;
using Temporalio.Client;

namespace Proxy;

internal static class RpcExceptionExtensions
{
	/// <summary>
	/// Maps an exception from Temporal's Rust client into a <see cref="Grpc.Core.RpcException"/> to preserve status codes for the original caller.
	/// </summary>
	/// <remarks>
	/// If we don't do this, by default all exceptions get mapped to UNKNOWN.
	/// Note that the Temporal server uses the richer gRPC error model, and encodes failure details as special trailers: https://grpc.io/docs/guides/error/#richer-error-model
	/// grpc-dotnet out of the box does not use the richer gRPC error model, so we use Grpc.StatusProto: https://github.com/grpc/grpc-dotnet/blob/master/src/Grpc.StatusProto/README.md
	/// </remarks>
	public static Grpc.Core.RpcException ToGrpcException(this Temporalio.Exceptions.RpcException ex)
	{
		Google.Rpc.Status status = new()
		{
			Code = (int)ex.Code,
			Message = ex.Message,
		};

		if (ex.GrpcStatus.Value is { } upstreamStatus)
		{
			status.Details.Add(upstreamStatus.Details);
		}

		return status.ToRpcException();
	}

	/// <summary>
	/// Converts the request <see cref="ServerCallContext"/> into an upstream <see cref="RpcOptions"/>
	/// </summary>
	public static RpcOptions ToRpcOptions(this ServerCallContext ctx)
	{
		RpcOptions options = new()
		{
			CancellationToken = ctx.CancellationToken,
		};

		return options;
	}
}
