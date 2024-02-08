using Microsoft.Extensions.DependencyInjection;
using SmintIo.Portals.Connector.Picturepark;
using SmintIo.Portals.DataAdapter.Picturepark.Resources;
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

        public LocalizedStringsModel Name { get; } = new ResourceLocalizedStringsModel(nameof(PictureparkExternalUsersConfigurationMessages.da_external_users_name));

        public LocalizedStringsModel Description { get; } = new ResourceLocalizedStringsModel(nameof(PictureparkExternalUsersConfigurationMessages.da_external_users_description));

        public string LogoUrl => null;

        public string IconUrl => null;

        public void ConfigureServices(IServiceCollection services)
        {
        }

        public Type ComponentImplementation => typeof(PictureparkExternalUsersDataAdapter);

        public Type ConfigurationImplementation => typeof(PictureparkExternalUsersDataAdapterConfiguration);

        public Type ConfigurationMessages => typeof(PictureparkExternalUsersConfigurationMessages);

        public Type MetamodelMessages => null;

        public Type[] PublicApiInterfaces { get; } = [typeof(IExternalUsersRead)];

        public DataAdapterPermission[] Permissions => null;

        public Task FillDefaultFormFieldValuesModelAsync(string connectorEntityModelKey, FormFieldValuesModel formFieldValuesModel)
        {
            return Task.CompletedTask;
        }
    }
}