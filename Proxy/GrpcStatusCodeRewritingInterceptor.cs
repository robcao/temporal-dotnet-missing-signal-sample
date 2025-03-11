using Grpc.Core;
using Grpc.Core.Interceptors;
using Temporalio.Exceptions;

namespace Proxy;

/// <summary>
/// <see cref="Interceptor"/> that maps exceptions raised from Temporal's Rust client into protocol-compliant gRPC exceptions that the caller will recognize.
/// </summary>
internal class GrpcStatusCodeRewritingInterceptor : Interceptor
{
	public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
		TRequest request,
		ServerCallContext context,
		UnaryServerMethod<TRequest, TResponse> continuation)
	{
		try
		{
			return await continuation(request, context).ConfigureAwait(false);
		}

		catch (NotImplementedException ex)
		{
			Google.Rpc.Status status = new()
			{
				Code = (int)StatusCode.Unimplemented,
				Message = ex.Message,
			};

			throw status.ToRpcException();
		}

		catch (Temporalio.Exceptions.RpcException ex)
		{
			throw ex.ToGrpcException();
		}
	}
}