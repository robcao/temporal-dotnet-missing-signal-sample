This is a minimal Temporal frontend proxy implementation that delays the sending of signals to the server until a `RespondWorkflowTaskCompleted` rpc comes in with a continue as new command.

By default, the proxy listens on localhost:9233.

This proxy only supports running a single workflow at a time.