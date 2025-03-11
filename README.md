# Missing Signal Sample

## Reproduction Steps

Start a Temporal dev server in a new terminal: `temporal server start-dev`

Start the Temporal proxy server in a new terminal:
- Navigate to the [proxy](./Proxy/) directory, and then run `dotnet run`.

Start the unknown command sample in a new terminal:
- Navigate to the [sample](./UnknownCommandSample/) directory, and then run `dotnet run`.

We use a proxy server in between the application and the Temporal server because otherwise it is difficult to consistently reproduce the UnknownCommand.

This sample sends two signals.

After running the sample, we can see that the workflow result only has the first signal. We expect there to be two.

The problem appears to be that the `SleepAndRun` workflow has a wait condition for having at least one signal, or for having the timer fire.

After receiving the UnknownCommand from the server, the workflow receives a new activation with three commands:

- SignalWorkflow
- SignalWorkflow
- FireTimer

After the first SignalWorkflow job is handled, the wait condition in the main workflow body becomes true, and then main workflow body runs to completion.

After that, the remaining commands are handled, causing us to lose the second SignalWorkflow job (it's not included in result of main workflow body).