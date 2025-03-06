using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Threading.Tasks;

using Serilog;

namespace A.UI.Service
{
    public sealed class IpcReceiveSession : IDisposable
    {
        private readonly string _mapName;
        private readonly int _pollingMilliseconds;

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private MemoryMappedFile _mappedFile;

        public IpcReceiveSession(string mapName, int pollingMilliseconds = 100)
        {
            _mapName = mapName;
            _pollingMilliseconds = pollingMilliseconds;
        }

        public event EventHandler<SessionDataReceivedEventArgs> DataReceived;

        public async Task<bool> OpenSessionReceiveAsync()
        {
            bool opened = await OpenSessionAsync();

            if (!opened)
            {
                return false;
            }

            await ReceiveAsync();

            return true;
        }

        public async Task<bool> OpenSessionAsync()
        {
            CancellationToken token = _cancellationTokenSource.Token;

            Console.Write("Try open session {0}...  ", _mapName);
            Log.Information("[IPC] Try open session {SessionName}...", _mapName);

            while (true)
            {
                bool success;

                try
                {
                    success = OpenSession();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[IPC] Open session {SessionName} failed", _mapName);

                    throw;
                }

                if (success)
                {
                    Console.WriteLine("OK. Session {0} opened.", _mapName);
                    Log.Information("[IPC] Open session {SessionName} succeeded", _mapName);

                    return true;
                }

                if (token.IsCancellationRequested)
                {
                    break;
                }

                await Task.Delay(_pollingMilliseconds);
            }

            Console.WriteLine("Canceled.");
            Log.Information("[IPC] Open session {SessionName} canceled", _mapName);

            return false;
        }

        public async Task ReceiveAsync()
        {
            if (_mappedFile == null)
            {
                Log.Error("[IPC] Cannot receiving data because session {SessionName} is not opened", _mapName);

                throw new InvalidOperationException("Please open session first.");
            }

            CancellationToken token = _cancellationTokenSource.Token;
            string oldData = null;

            Console.WriteLine("Start receiving data...");
            Log.Information("[IPC] Start receiving data from session {SessionName}...", _mapName);

            while (true)
            {
                try
                {
                    oldData = Receive(oldData);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[IPC] Session {SessionName} is terminated because receiving data failed", _mapName);

                    throw;
                }

                if (token.IsCancellationRequested)
                {
                    break;
                }

                await Task.Delay(_pollingMilliseconds);
            }

            Console.WriteLine("End receiving data.");

            Log.Information("[IPC] Session {SessionName} is closed", _mapName);
            Log.Information("[IPC] End receiving data from {SessionName}", _mapName);
        }

        private bool OpenSession()
        {
            try
            {
                _mappedFile = MemoryMappedFile.OpenExisting(_mapName);
            }
            catch (FileNotFoundException)
            {
            }

            return _mappedFile != null;
        }

        private string Receive(string oldData)
        {
            using (StreamReader reader = new StreamReader(_mappedFile.CreateViewStream()))
            {
                string newData = reader.ReadToEnd()?.TrimEnd('\0');

                if (!string.IsNullOrWhiteSpace(newData) && newData != oldData)
                {
                    Log.Information("[IPC] Received data {Data} from session {SessionName}", newData, _mapName);

                    DataReceived?.Invoke(this, new SessionDataReceivedEventArgs(newData));
                }

                return newData;
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            Thread.Sleep(_pollingMilliseconds * 5);

            _mappedFile?.Dispose();
            _mappedFile = null;

            GC.SuppressFinalize(this);

            Log.Information("[IPC] Session {SessionName} is disposed", _mapName);
        }
    }

    public sealed class SessionDataReceivedEventArgs : EventArgs
    {
        public SessionDataReceivedEventArgs(string data)
        {
            Data = data;
        }

        public string Data { get; }
    }
}
