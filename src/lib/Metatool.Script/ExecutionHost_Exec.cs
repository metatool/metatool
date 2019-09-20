using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

namespace Metatool.Script
{
    public partial class ExecutionHost
    {
        private TextWriter? _processInputStream;
        public event Action<ResultObject> Dumped;
        public event Action<ExceptionResultObject> Error;
        public event Action                        ReadInput;
        private readonly JsonSerializer _jsonSerializer;

        private async Task RunProcess(string assemblyPath, CancellationToken cancellationToken)
        {
            using (var process = new Process
            {
                StartInfo = GetProcessStartInfo(assemblyPath)
            })
            using (cancellationToken.Register(() =>
            {
                try
                {
                    _processInputStream = null;
                    process.Kill();
                }
                catch { }
            }))
            {
                if (process.Start())
                {
                    _processInputStream = new StreamWriter(process.StandardInput.BaseStream, Encoding.UTF8);

                    await Task.WhenAll(
                        Task.Run(() => ReadObjectProcessStream(process.StandardOutput)),
                        Task.Run(() => ReadProcessStream(process.StandardError)));
                }
            }
        }
        private static Lazy<string> CurrentPid { get; } = new Lazy<string>(() => Process.GetCurrentProcess().Id.ToString());

        public async Task SendInput(string message)
        {
            var stream = _processInputStream;
            if (stream != null)
            {
                await stream.WriteLineAsync(message).ConfigureAwait(false);
                await stream.FlushAsync().ConfigureAwait(false);
            }
        }

        private ProcessStartInfo GetProcessStartInfo(string assemblyPath)
        {
            var dotnetPath = Path.Combine(Environment.GetEnvironmentVariable("ProgramW6432")!, "dotnet");
            var dotnetExe  = Path.Combine(dotnetPath, "dotnet.exe");
            return new ProcessStartInfo
            {
                FileName = dotnetExe,
                Arguments = $"\"{assemblyPath}\" --pid {CurrentPid.Value}",
                WorkingDirectory = _parameters.WorkingDirectory,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };
        }

        private async Task ReadProcessStream(StreamReader reader)
        {
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (line != null)
                {
                    Dumped?.Invoke(ResultObject.Create(line, DumpQuotas.Default));
                }
            }
        }

        private void ReadObjectProcessStream(StreamReader reader)
        {
            using (var jsonReader = new JsonTextReader(reader) { SupportMultipleContent = true })
            {
                while (jsonReader.Read())
                {
                    try
                    {
                        var result = _jsonSerializer.Deserialize<ResultObject>(jsonReader);

                        switch (result)
                        {
                            case ExceptionResultObject exceptionResult:
                                Error?.Invoke(exceptionResult);
                                break;
                            case InputReadRequest _:
                                ReadInput?.Invoke();
                                break;
                            default:
                                Dumped?.Invoke(result);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Dumped?.Invoke(ResultObject.Create("Error deserializing result: " + ex.Message, DumpQuotas.Default));
                    }
                }
            }
        }
    }
}
