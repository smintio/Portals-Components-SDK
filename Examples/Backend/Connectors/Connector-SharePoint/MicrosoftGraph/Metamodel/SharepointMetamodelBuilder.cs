using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmintIo.Portals.Connector.SharePoint.Client;
using SmintIo.Portals.Connector.SharePoint.Models;
using SmintIo.Portals.Connector.SharePoint.Resources;
using SmintIo.Portals.ConnectorSDK.Metamodel;
using SmintIo.Portals.ConnectorSDK.Parsers;
using SmintIo.Portals.SDK.Core.Extensions;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Model;
using SmintIo.Portals.SDK.Core.Models.Strings;
using EntityType = SmintIo.Portals.SDK.Core.Models.Metamodel.Model.EntityType;

namespace SmintIo.Portals.Connector.SharePoint.MicrosoftGraph.Metamodel
{
    public class SharepointMetamodelBuilder : IMetamodelBuilder
    {
        public const string RootEntityKey = "SharepointFile";

        private const string GeoLoc = "GeoLoc";

        private static readonly ICollection<string> _locationProperties = new HashSet<string>
        {
            nameof(LocationModel.DispName),
            nameof(LocationModel.LocationUri),
            nameof(LocationModel.UniqueId)
        };

        private static readonly ICollection<string> _addressProperties = new HashSet<string>
        {
            nameof(AddressModel.City),
            nameof(AddressModel.CountryOrRegion),
            nameof(AddressModel.State),
            nameof(AddressModel.Street),
            nameof(AddressModel.PostalCode)
        };

        private readonly ILogger _logger;
        private readonly ISharepointClient _sharepointClient;
        private readonly string _siteId;

        private readonly ConnectorMetamodel _metamodel;

        public SharepointMetamodelBuilder(ILogger logger, ISharepointClient sharepointClient, string siteId, string siteDriveId, string siteListId, IEnumerable<string> siteFolderIds)
        {
            _logger = logger;
            _sharepointClient = sharepointClient;
            _siteId = siteId;

            var folderIds = string.Join(", ", siteFolderIds);

            var foldersHash = GetMD5EncodeHash(folderIds);

            _metamodel = new ConnectorMetamodel(
                $"{SharepointConnectorStartup.SharepointConnector}-{siteId}-{siteDriveId}-{siteListId}-{foldersHash}",
                isRandomAccessSupported: true,
                isFullTextSearchProposalsSupported: false,
                isFolderNavigationSupported: false);
        }

        private static string GetMD5EncodeHash(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            using (var md5 = MD5.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(value);
                var challengeBytes = md5.ComputeHash(bytes);

                var encodedValue = string.Concat(Array.ConvertAll(challengeBytes, h => h.ToString("X2")));

                return encodedValue;
            }
        }

        /// <summary>
        /// Builds the metamodel from the configured Sharepoint instance. Sharepoint only knows one entity type, which
        /// is a so-called "driveItem".
        /// For example, assume there is a column called "Mood", that indicates which mood is prevalent in an image, then there
        /// would be a <see cref="ColumnDefinitionResponse"/> called "Mood" of type <see cref="ColumnType.Choice"/> for which the
        /// <see cref="ConnectorMetamodel"/> would have a separate <see cref="EntityModel"/>. Also, the root entity would have a
        /// property referencing that entity model.
        /// </summary>
        /// <returns>A <see cref="ConnectorMetamodel"/> representing all properties a file can have in sharepoint</returns>
        public async Task<ConnectorMetamodel> BuildAsync()
        {
            var columnDefinitionResponses = await _sharepointClient.GetSiteMetadataAsync(_siteId).ConfigureAwait(false);

            if (columnDefinitionResponses != null)
            {
                AddEntities(columnDefinitionResponses);
            }

            return _metamodel;
        }

        /// <summary>
        /// Sharepoint can have internal columns which cannot be modified by the user.
        /// For objects such as Link - it's Url property will always be sent as Url (no translation available)
        /// However since Link itself can be localized. Its properties will hold the translation values.
        /// E.g. The url value can be English or German one.
        private static IEnumerable<KeyValuePair<string, string>> GetStaticColumnLabels(EntityModel entityModel, string columnName)
        {
            return entityModel.Labels.Select(l => new KeyValuePair<string, string>(l.Key, columnName));
        }

        private void AddEntities(IEnumerable<ColumnDefinitionResponse> columnDefinitionResponses)
        {
            var rootEntityModel = AddRootEntityModel();

            foreach (var columnDefinitionResponse in columnDefinitionResponses)
            {
                var key = LocalizedStringColumnParser.GetKey(columnDefinitionResponse.Name);

                if (_locationProperties.Contains(key)
                    || _addressProperties.Contains(key)
                    || key.Equals(GeoLoc))
                {
                    // We skip duplicate data. Location data object will contain everything.
                    continue;
                }

                var entityLabel = columnDefinitionResponse.DisplayName.Localize();

                AddEntity(columnDefinitionResponse, entityLabel, columnDefinitionResponse.Name);

                var propertyLabel = columnDefinitionResponse.DisplayName.Localize();

                AddProperty(
                    rootEntityModel,
                    columnDefinitionResponse,
                    columnDefinitionResponse.Name,
                    propertyLabel);
            }

            TryAddLocationProperties(columnDefinitionResponses);
            TryAddImageProperties(columnDefinitionResponses);
            TryAddUrlProperties(columnDefinitionResponses);
            TryAddTaxonomyProperties(columnDefinitionResponses);
            TryAddLookupProperties(columnDefinitionResponses);

            var translationLinker = new SharepointTranslationLinker();

            translationLinker.Link(_metamodel);
        }

        private EntityModel AddRootEntityModel()
        {
            var rootEntityLabels = new ResourceLocalizedStringsModel(nameof(MetamodelMessages.c_sharepoint_root_entity));

            var rootEntityModel = CreateEntityModel(RootEntityKey, rootEntityLabels);

            rootEntityModel.Type = EntityType.TopLevelObject;

            _metamodel.AddEntity(rootEntityModel);

            return rootEntityModel;
        }

        private void AddEntity(ColumnDefinitionResponse columnDefinitionResponse, LocalizedStringsModel localizedStringsModel, string entityKey)
        {
            var columnType = columnDefinitionResponse.GetColumnType();

            if (!columnType.IsComplexType())
            {
                return;
            }

            if (_metamodel.HasEntity(entityKey))
            {
                return;
            }

            _logger.LogDebug($"Creating {nameof(EntityModel)} for key '{columnDefinitionResponse.DisplayName}'");

            var isEnum = columnType.IsEnum();

            var entityModel = CreateEntityModel(columnDefinitionResponse.Name, localizedStringsModel, isEnum);

            _metamodel.AddEntity(entityModel);
        }

        private EntityModel CreateEntityModel(string entityModelKey, LocalizedStringsModel localizedStringsModel = null, bool isEnum = false)
        {
            var entity = isEnum
                ? EnumEntityModel.CreateEntity(entityModelKey, parentEntityModelKey: null, localizedStringsModel)
                : new EntityModel(entityModelKey, localizedStringsModel)
                {
                    Type = EntityType.Fieldset
                };

            return entity;
        }

        private void AddProperty(
            EntityModel entityModel,
            ColumnDefinitionResponse columnDefinitionResponse,
            string propertyName,
            LocalizedStringsModel localizedStringsModel)
        {
            var columnKey = columnDefinitionResponse.GetKey();
            var columnType = columnDefinitionResponse.GetColumnType();

            if (columnType == ColumnType.Lookup)
            {
                // We don't support lookups.
                return;
            }

            var dataType = columnType.GetDataType();

            var customEntityModel = _metamodel.GetEntity(columnKey);

            var propertyModel = customEntityModel != null
                ? entityModel.AddProperty(propertyName, dataType, customEntityModel.Key, localizedStringsModel)
                : entityModel.AddProperty(propertyName, dataType, localizedStringsModel);

            SetCurrencySymbol(columnDefinitionResponse, dataType, propertyModel);

            if (columnDefinitionResponse.FieldType == SharepointFieldType.Html)
            {
                // propertyModel.SemanticType = SemanticType.Html;
            }
        }

        private void SetCurrencySymbol(ColumnDefinitionResponse columnDefinitionResponse, DataType dataType, PropertyModel propertyModel)
        {
            if (dataType != DataType.CurrencyModel)
            {
                return;
            }

            if (string.IsNullOrEmpty(columnDefinitionResponse.CurrencyLocale))
            {
                return;
            }

            try
            {
                var cultureInfo = CultureInfo.GetCultureInfo(columnDefinitionResponse.CurrencyLocale);

                propertyModel.SemanticHint = cultureInfo.NumberFormat.CurrencySymbol;
            }
            catch (CultureNotFoundException ex)
            {
                // Highly unlikely, but we never know.
                _logger.LogError(ex, $"Unable to parse cultureInfo {columnDefinitionResponse.CurrencyLocale}");
            }
        }

        private void TryAddLocationProperties(IEnumerable<ColumnDefinitionResponse> columnDefinitionResponses)
        {
            var locationColumnDefinitions = columnDefinitionResponses
                .Where(cdr => cdr.FieldType == SharepointFieldType.Location)
                .ToArray();

            if (locationColumnDefinitions.Length == 0)
            {
                return;
            }

            foreach (var locationColumnDefinition in locationColumnDefinitions)
            {
                var locationEntityModel = _metamodel.Entities.SingleOrDefault(e => e.Key.Equals(locationColumnDefinition.Name, StringComparison.OrdinalIgnoreCase));

                if (locationEntityModel == null)
                {
                    continue;
                }

                var addressEntityModelKey = $"{locationColumnDefinition.Name}_{AddressModel.Key}";

                var (culture, _, _) = LocalizedStringColumnParser.GetCultureAndTranslation(locationColumnDefinition.DisplayName, trimParent: false);

                var addressEntityLabel = locationColumnDefinition.DisplayName.LocalizeByCulture(culture);

                var addressEntityModel = CreateEntityModel(addressEntityModelKey, addressEntityLabel);

                foreach (var columnDefinitionResponse in columnDefinitionResponses)
                {
                    EntityModel entityModel = null;

                    // We do this by name instead of DisplayName, because we expect Sharepoint internal columns.
                    var columnDefinitionKey = LocalizedStringColumnParser.GetKey(columnDefinitionResponse.Name);

                    if (_locationProperties.Contains(columnDefinitionKey) || GeoLoc.Equals(columnDefinitionKey, StringComparison.OrdinalIgnoreCase))
                    {
                        entityModel = locationEntityModel;
                    }
                    else if (_addressProperties.Contains(columnDefinitionKey))
                    {
                        entityModel = addressEntityModel;
                    }

                    if (entityModel == null)
                    {
                        continue;
                    }

                    var propertyName = columnDefinitionKey;
                    var propertyLabel = columnDefinitionResponse.DisplayName.LocalizeByCulture(culture);

                    // Sharepoint is inconsistent, the json object has a property of DisplayName instead of DispName.
                    if (propertyName.Equals(nameof(LocationModel.DispName), StringComparison.OrdinalIgnoreCase))
                    {
                        propertyName = "DisplayName";
                    }
                    else if (propertyName.Equals(GeoLoc, StringComparison.OrdinalIgnoreCase))
                    {
                        propertyName = nameof(LocationModel.Coordinates);
                    }

                    if (entityModel.GetProperty(propertyName, entityModelProvider: null) != null)
                    {
                        continue;
                    }

                    AddProperty(entityModel, columnDefinitionResponse, propertyName, propertyLabel);
                }

                var addressEntity = _metamodel.AddEntity(addressEntityModel);

                locationEntityModel.AddProperty(AddressModel.Key, DataType.DataObject, addressEntity.Key, addressEntityLabel);
            }
        }

        private void TryAddImageProperties(IEnumerable<ColumnDefinitionResponse> columnDefinitionResponses)
        {
            var imageColumnDefinitions = columnDefinitionResponses
                .Where(cdr => cdr.FieldType == SharepointFieldType.Image)
                .ToArray();

            if (imageColumnDefinitions.Length == 0)
            {
                return;
            }

            foreach (var imageColumnDefinition in imageColumnDefinitions)
            {
                // The image object name cannot be modified by the user.
                // It is an internal Sharepoint Model
                var imageEntityModel = _metamodel.Entities.SingleOrDefault(e => e.Key.Equals(imageColumnDefinition.Name, StringComparison.OrdinalIgnoreCase));

                if (imageEntityModel == null)
                {
                    continue;
                }

                imageEntityModel.AddProperty(nameof(ImageModel.FileName), DataType.String, nameof(ImageModel.FileName).Localize());
                imageEntityModel.AddProperty(nameof(ImageModel.ServerRelativeUrl), DataType.String, nameof(ImageModel.ServerRelativeUrl).Localize());
                imageEntityModel.AddProperty(nameof(ImageModel.Id), DataType.String, nameof(ImageModel.Id).Localize());
                imageEntityModel.AddProperty(nameof(ImageModel.ServerUrl), DataType.String, nameof(ImageModel.ServerUrl).Localize());
                imageEntityModel.AddProperty(nameof(ImageModel.Type), DataType.String, nameof(ImageModel.Type).Localize());
                imageEntityModel.AddProperty(nameof(ImageModel.FieldName), DataType.String, nameof(ImageModel.FieldName).Localize());
            }
        }

        private void TryAddUrlProperties(IEnumerable<ColumnDefinitionResponse> columnDefinitionResponses)
        {
            var urlColumnDefinitions = columnDefinitionResponses
                .Where(cdr => cdr.FieldType == SharepointFieldType.Url)
                .ToArray();

            if (urlColumnDefinitions.Length == 0)
            {
                return;
            }

            foreach (var urlColumnDefinition in urlColumnDefinitions)
            {
                var urlEntityModel = _metamodel.Entities.SingleOrDefault(e => e.Key.Equals(urlColumnDefinition.Name, StringComparison.OrdinalIgnoreCase));

                if (urlEntityModel == null)
                {
                    continue;
                }

                var urlLabels = GetStaticColumnLabels(urlEntityModel, nameof(UrlModel.Url));
                var urlLocalizedStringsModel = new LocalizedStringsModel(urlLabels);

                AddProperty(urlEntityModel, new ColumnDefinitionResponse(), nameof(UrlModel.Url), urlLocalizedStringsModel);

                var descriptionLabels = GetStaticColumnLabels(urlEntityModel, nameof(UrlModel.Description));
                var descriptionLocalizedStringsModel = new LocalizedStringsModel(descriptionLabels);

                AddProperty(urlEntityModel, new ColumnDefinitionResponse(), nameof(UrlModel.Description), descriptionLocalizedStringsModel);
            }
        }

        private void TryAddTaxonomyProperties(IEnumerable<ColumnDefinitionResponse> columnDefinitionResponses)
        {
            var taxonomyColumnDefinitions = columnDefinitionResponses
                .Where(cdr => cdr.FieldType == SharepointFieldType.Taxonomy || cdr.FieldType == SharepointFieldType.TaxonomyMulti)
                .ToArray();

            if (taxonomyColumnDefinitions.Length == 0)
            {
                return;
            }

            foreach (var taxonomyColumnDefinition in taxonomyColumnDefinitions)
            {
                var taxonomyEntityModel = _metamodel.Entities.SingleOrDefault(e => e.Key.Equals(taxonomyColumnDefinition.Name, StringComparison.OrdinalIgnoreCase));

                if (taxonomyEntityModel == null)
                {
                    continue;
                }

                var labels = GetStaticColumnLabels(taxonomyEntityModel, nameof(TaxonomyModel.Label));
                var labelLocalizedStringsModel = new LocalizedStringsModel(labels);

                AddProperty(taxonomyEntityModel, new ColumnDefinitionResponse(), nameof(TaxonomyModel.Label), labelLocalizedStringsModel);

                var termGuidLabels = GetStaticColumnLabels(taxonomyEntityModel, nameof(TaxonomyModel.TermGuid));
                var termGuidLocalizedStringsModel = new LocalizedStringsModel(termGuidLabels);

                AddProperty(taxonomyEntityModel, new ColumnDefinitionResponse(), nameof(TaxonomyModel.TermGuid), termGuidLocalizedStringsModel);

                var wssIdLabels = GetStaticColumnLabels(taxonomyEntityModel, nameof(TaxonomyModel.WssId));
                var wssIdLocalizedStringsModel = new LocalizedStringsModel(wssIdLabels);
                var wssIdColumnDefinitionResponse = new ColumnDefinitionResponse
                {
                    FieldType = SharepointFieldType.Integer
                };

                AddProperty(taxonomyEntityModel, wssIdColumnDefinitionResponse, nameof(TaxonomyModel.WssId), wssIdLocalizedStringsModel);
            }
        }

        private void TryAddLookupProperties(IEnumerable<ColumnDefinitionResponse> columnDefinitionResponses)
        {
            var userMultiColumnDefinitions = columnDefinitionResponses
                .Where(cdr => cdr.FieldType == SharepointFieldType.UserMulti)
                .ToArray();

            if (userMultiColumnDefinitions.Length == 0)
            {
                return;
            }

            foreach (var userMultiColumnDefinition in userMultiColumnDefinitions)
            {
                var lookupEntityModel = _metamodel.Entities.SingleOrDefault(e => e.Key.Equals(userMultiColumnDefinition.Name, StringComparison.OrdinalIgnoreCase));

                if (lookupEntityModel == null)
                {
                    continue;
                }

                var emailLabels = GetStaticColumnLabels(lookupEntityModel, nameof(LookupModel.Email));
                var emailLocalizedStringsModel = new LocalizedStringsModel(emailLabels);

                AddProperty(lookupEntityModel, new ColumnDefinitionResponse(), nameof(LookupModel.Email), emailLocalizedStringsModel);
            }
        }
    }
}