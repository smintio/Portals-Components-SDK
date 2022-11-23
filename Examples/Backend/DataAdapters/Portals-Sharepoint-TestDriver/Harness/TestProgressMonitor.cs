using System.Threading.Tasks;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Progress;
// using SmintIo.Portals.DataAdapterSDK.DataAdapters.Progress;
using SmintIo.Portals.SDK.Core.Models.Strings;

namespace SmintIo.Portals.ConnectorSDK.TestDriver.Sharepoint.Test.Harness
{
    public class TestProgressMonitor  : IProgressMonitor
    {
        public double CurrentValue { get; private set; } = 0;
        public double Maximum { get; set; }

        public Task ReportProgressAsync(double units, LocalizedStringsModel displayText)
        {
            ReportProgressInvocations++;
            CurrentValue = units;
            return Task.CompletedTask;
        }

        public Task FinishedAsync(LocalizedStringsModel displayText)
        {
            FinishedInvocations++;
            return Task.CompletedTask;
        }
        public int ReportProgressInvocations { get; set; }
        public int FinishedInvocations { get; set; }
    }
}