using System.IO;
using System.Runtime.CompilerServices;
using GcbTelemetryPresenter.AppLayer;
using GcbTelemetryPresenter.Domain;

namespace GcbTelemetryPresenter.Infra
{
    internal class TelemetryRepository
    {
        public TelemetryRepository(IAppSettings appSettings)
        {
            _telemetryFolder = appSettings.TelemetryFolder;
        }
        
        public async IAsyncEnumerable<DataMessage> ReadTelemetryAsync(string filename, [EnumeratorCancellation] CancellationToken token)
        {
            var lines = await File.ReadAllLinesAsync(filename, token);

            for (var i = lines.Length - 1; i > 0; --i)
            {
                yield return TelemetrySerializer.Deserialize(lines[i]);
            }
        }

        public async IAsyncEnumerable<DataMessage> ReadTelemetryAsync([EnumeratorCancellation] CancellationToken token, int startFileIndex = 0, int fileCount = 1)
        {
            foreach (var filename in GetTelemetryFilenames()
                         .Skip(startFileIndex)
                         .Take(fileCount))
            {
                await foreach(var message in ReadTelemetryAsync(filename, token))
                {
                    yield return message;
                }
            }
        }

        public IOrderedEnumerable<string> GetTelemetryFilenames()
        {
            if (!Directory.Exists(_telemetryFolder))
                throw new FileNotFoundException("Folder not found! Set correct TelemetryFolder in appsettings.json");

            return Directory.GetFiles(_telemetryFolder)
                .OrderDescending();
        }
        
        private readonly string _telemetryFolder;
    }
}
