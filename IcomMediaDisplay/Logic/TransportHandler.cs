using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Exiled.API.Features;
using MEC;

namespace IcomMediaDisplay.Logic
{
    public class TransportHandler
    {
        private CancellationTokenSource pipeCts;
        private Task pipeTask;
        private NamedPipeServerStream namedPipeServerStream;

        public TransportHandler() => Start();

        public void Start(string pipeName = "xt_framedump")
        {
            Stop();

            pipeCts = new CancellationTokenSource();
            pipeTask = Task.Run(() => PipeLoop(pipeName, pipeCts.Token));

            Log.Debug($"TransportHandler started (pipe: {pipeName})");

            namedPipeServerStream = new(
                pipeName,
                PipeDirection.In,
                1,
                PipeTransmissionMode.Message,
                PipeOptions.Asynchronous
            );
        }

        public void Stop()
        {
            pipeCts?.Cancel();
            pipeCts = null;
        }

        // =======================
        // Pipe Reader (authoritative clock)
        // =======================

        private async Task PipeLoop(string pipeName, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    Intercom.IntercomDisplay.Network_overrideText = "Waiting for pipe connection...";
                    await namedPipeServerStream.WaitForConnectionAsync(token);
                    Intercom.IntercomDisplay.Network_overrideText = "Pipe connected.";

                    using var reader = new StreamReader(namedPipeServerStream, Encoding.UTF8);

                    while (!token.IsCancellationRequested && namedPipeServerStream.IsConnected)
                    {
                        string frame = await reader.ReadLineAsync();

                        if (string.IsNullOrEmpty(frame))
                            continue;

                        Timing.CallDelayed(0.1f, () =>
                        {
                            Intercom.IntercomDisplay.Network_overrideText = frame;
                        });
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Log.Error($"Pipe error: {ex}");
                    await Task.Delay(500, token);
                }
            }
        }
    }
}
