using Microsoft.Extensions.DependencyInjection;
using SmintIo.Portals.Connector.Picturepark;
using SmintIo.Portals.DataAdapterSDK.DataAdapters;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.ExternalUsers;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Permissions;
using SmintIo.Portals.SDK.Core.Configuration;
using SmintIo.Portals.SDK.Core.Models.Strings;
using System;
using System.Threading.Tasks;

namespace SmintIo.Portals.DataAdapter.Picturepark.ExternalUsers
{
    public class PictureparkExternalUsersDataAdapterStartup : IDataAdapterStartup
    {
        public const string PictureparkExternalUsersDataAdapter = "external-users";

        public string Key => PictureparkExternalUsersDataAdapter;

        public string ConnectorKey => PictureparkConnectorStartup.PictureparkConnector;

        public LocalizedStringsModel Name => new LocalizedStringsModel()
        {
            { LocalizedStringsModel.DefaultCulture, "User data access" },
            { "de", "Zugriff auf Benutzerdaten" },
        };

        public LocalizedStringsModel Description => new LocalizedStringsModel()
        {
            { LocalizedStringsModel.DefaultCulture, "Provides services to query user data from Picturepark." },
            { "de", "Stellt Dienste zum Abfragen von Benutzerdaten von Picturepark zur Verfügung." }
        };

        public string LogoUrl => null;

        public string IconUrl => null;

        public void ConfigureServices(IServiceCollection services)
        {
        }

        public Type ComponentImplementation => typeof(PictureparkExternalUsersDataAdapter);

        public Type ConfigurationImplementation => typeof(PictureparkExternalUsersDataAdapterConfiguration);

        public Type[] PublicApiInterfaces => new Type[] { typeof(IExternalUsersRead) };

        public DataAdapterPermission[] Permissions => null;

        public Task FillDefaultFormFieldValuesModelAsync(string connectorEntityModelKey, FormFieldValuesModel formFieldValuesModel)
        {
            return Task.CompletedTask;
        }
    }
}