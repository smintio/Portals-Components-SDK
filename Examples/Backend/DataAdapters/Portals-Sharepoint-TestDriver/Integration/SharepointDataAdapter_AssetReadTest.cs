using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using SmintIo.Portals.Connector.SharePoint.MicrosoftGraph.Metamodel;
using SmintIo.Portals.ConnectorSDK.TestDriver.Sharepoint.Test.Harness;
using SmintIo.Portals.DataAdapter.SharePoint.Assets;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Models;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Parameters;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Data;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Model;
using SmintIo.Portals.SDK.Core.Models.Strings;
using SmintIo.Portals.SDK.Core.Rest.Prefab.Exceptions;
using Xunit;
using Xunit.Abstractions;
// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace SmintIo.Portals.ConnectorSDK.TestDriver.Sharepoint.Test.Integration
{
    public class SharepointDataAdapterAssetReadTest : SharepointConnectorTest
    {
        private readonly SharepointAssetsDataAdapter _dataAdapter;
        private readonly ILogger<SharepointDataAdapterAssetReadTest> _logger;
        private IEnumerable<string> allFileIds;

        public SharepointDataAdapterAssetReadTest(ITestOutputHelper helper, SharepointFixture fixture) : base(helper,
            fixture)
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            _logger = loggerFactory.CreateLogger<SharepointDataAdapterAssetReadTest>();
            _dataAdapter = fixture.DataAdapter;
            allFileIds = fixture.SharepointOptions.Files.Select(f => f.Id);
        }

        [Fact]
        public async Task VerifyMetaModelAsync()
        {
            // obtain the metamodel and all assets
            var metamodel = Fixture.Metamodel;
            var allFiles = await _dataAdapter.GetAssetsAsync(new GetAssetsParameters()
            {
                AssetIds = allFileIds.Select(f => new AssetIdentifier(f)).ToArray()
            }, new TestProgressMonitor());

            metamodel.Should().NotBeNull();
            allFiles.AssetDataObjects.Should().NotBeEmpty();

            // get the entity model for a sharepoint file
            var rootEntityModel = metamodel.GetEntity($"c1___{SharepointMetamodelBuilder.RootEntityKey}");

            foreach (var asset in allFiles.AssetDataObjects)
            {
                _logger.LogInformation($"\n\n*** Inspecting asset {asset.Name[LocalizedStringsModel.DefaultCulture]} ***\n");
                // most metadata from sharepoint is store in the "RawData" field
                var additionalValues = asset.RawData[0].Values;
                additionalValues.Should().NotBeEmpty();

                foreach (var (key, val) in additionalValues)
                {
                    // get the corresponding entry in the entity model
                    var rootEntityModelProp = rootEntityModel.Properties.FirstOrDefault(p => p.Key == key);

                    if (rootEntityModelProp == null)
                    {
                        _logger.LogWarning($"!!! no property found in the root entity model for key \"{key}\" !!!");
                        continue;
                    }

                    // the actual value for one metadata field of an asset 
                    var metadataFieldValue = val.GetValue<object>().ToString();
                    if (rootEntityModelProp.DataType != DataType.DataObject)
                    {
                        // simple prop 
                        var prop = rootEntityModel.Properties.FirstOrDefault(p => p.Key == key);
                        _logger.LogInformation($"Field [{key}] has value \"{metadataFieldValue}\" and its metamodel property is \'{prop?.Key}\' ({prop?.DataType})");
                        prop.Should().NotBeNull();
                    }
                    else
                    {
                        // complex prop with its own entity model
                        var targetEntity = metamodel.GetEntity(rootEntityModelProp.TargetMetamodelEntityKey);
                        targetEntity.Should().NotBeNull();

                        // Choice fields are special:
                        if (targetEntity is EnumEntityModel eem)
                        {
                            rootEntityModelProp.DataType.Should().Be(DataType.DataObject);
                            eem.EnumValues.Should().ContainSingle(o => o.Id == metadataFieldValue);
                            eem.EnumValues.Where(edo => edo.Id == metadataFieldValue).Should().HaveCount(1);
                            _logger.LogInformation(
                                $"Field \"{key}\" is an enum, has value \"{metadataFieldValue}\" and its EnumEntityModel has values [{string.Join(" / ", eem.EnumValues.Select(edo => edo.Id))}]");
                        }
                        else
                        {
                            rootEntityModelProp.DataType.Should().Be(DataType.DataObject);
                            var props = targetEntity.Properties.Select(pm => pm.Key);
                            _logger.LogInformation(
                                $"Field \"{key}\" is a complex object, its reference is \"{targetEntity.Key}\" and its properties are [{string.Join(", ", props)}]");
                        }
                    }
                }
            }
        }

        [Fact]
        public async Task GetAssetDownloadItemMappingAsync()
        {
            var param = new GetAssetsDownloadItemMappingsParameters()
            {
                AssetIds = new[]
                {
                    new AssetIdentifier(SharepointOptions.Files[0].Id),
                    new AssetIdentifier(SharepointOptions.Files[1].Id),
                    new AssetIdentifier(SharepointOptions.Files[2].Id)
                }
            };
            var testProgressMonitor = new TestProgressMonitor();
            var result = await
                _dataAdapter.GetAssetsDownloadItemMappingsAsync(param, testProgressMonitor);

            result.Should().NotBeNull();
            result.AssetDownloadItemMappings.Should().HaveCount(3);

            // Item Mappings should be Original or Original + Preview (for images)
            result.AssetDownloadItemMappings.Should().OnlyContain(model => model.AssetDownloadItemMappings.Count >= 1);

            testProgressMonitor.CurrentValue.Should().Be(testProgressMonitor.Maximum);
            testProgressMonitor.ReportProgressInvocations.Should().Be(4);
            testProgressMonitor.FinishedInvocations.Should().Be(1);
        }

        [Fact]
        public async Task GetAsset_ShouldReturnResultAsync()
        {
            var asset = await _dataAdapter.GetAssetAsync(new GetAssetParameters
            {
                AssetId = new AssetIdentifier(SharepointOptions.Files[0].Id)
            });

            asset.AssetDataObject.Should().NotBeNull();
            asset.AssetDataObject.FileMetadata.FileName.Should().BeEquivalentTo(SharepointOptions.Files[0].Name);
            asset.AssetDataObject.Id.Should().MatchRegex(".*__.*");

            AssertRawData(asset);
        }        

        [Fact]
        public Task GetAsset_NotFound_ShouldReturnEmptyAsync()
        {
            return Assert.ThrowsAsync<ExternalDependencyException>(() => _dataAdapter.GetAssetAsync(
                new GetAssetParameters
                {
                    AssetId = new AssetIdentifier("b!vX0Z05Whf0KjPVRyFi-dIcPnqE2SrE5Amwl4PQndVufOwDIJuOBnTYT2tsl1F6Nh__not_exist")
                }));
        }

        [Fact]
        public async Task GetAsset_ThrowsExceptionWhenIdIsNullAsync()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _dataAdapter.GetAssetAsync(new GetAssetParameters
            {
                AssetId = null
            }));
        }

        [Fact]
        public async Task GetAsset_InvalidParametersAsync()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _dataAdapter.GetAssetAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _dataAdapter.GetAssetAsync(new GetAssetParameters()));
            await Assert.ThrowsAsync<ExternalDependencyException>(() => _dataAdapter.GetAssetAsync(
                new GetAssetParameters()
                {
                    AssetId = new AssetIdentifier(null)
                }));
        }

        [Fact]
        public async Task GetAssets_ShouldReturnEmptyListAsync()
        {
            var testProgressMonitor = new TestProgressMonitor();
            var allAssets = await _dataAdapter.GetAssetsAsync(new GetAssetsParameters(), testProgressMonitor);

            allAssets.Should().NotBeNull();
            allAssets.AssetDataObjects.Should().BeEmpty();
        }

        [Fact]
        public Task GetAllAssetsWithNullParams_ShouldReturnEmptyListAsync()
        {
            return Assert.ThrowsAsync<ArgumentNullException>(() => _dataAdapter.GetAssetsAsync(null, new TestProgressMonitor()));
        }

        [Fact]
        public async Task GetAllAssetsWithIds_ShouldReturnListAsync()
        {
            var testProgressMonitor = new TestProgressMonitor();
            var allAssets = await _dataAdapter.GetAssetsAsync(new GetAssetsParameters()
            {
                AssetIds = new[]
                {
                    new AssetIdentifier(SharepointOptions.Files[0].Id),
                    new AssetIdentifier(SharepointOptions.Files[1].Id)
                }
            }, testProgressMonitor);

            allAssets.AssetDataObjects.Should().HaveCount(2, "Test-account should have 2 files in it!");
            allAssets.AssetDataObjects.Should()
                .OnlyContain(ado => !string.IsNullOrEmpty(ado.Version), "All Assets should have a version!");

            allAssets.AssetDataObjects.Should()
                .OnlyContain(ado => ado.PermissionUuids.Any(), "Not using pass through. We should always have permissions.");

            testProgressMonitor.CurrentValue.Should().Be(testProgressMonitor.Maximum);
            testProgressMonitor.ReportProgressInvocations.Should().Be(3);
            testProgressMonitor.FinishedInvocations.Should().Be(1);
        }

        [Fact]
        public async Task GetAssetsByIdAsync_ShouldReturnSingleAssetAsync()
        {
            var id = SharepointOptions.Files[0].Id;
            var testProgressMonitor = new TestProgressMonitor();
            var asset = await _dataAdapter.GetAssetsAsync(new GetAssetsParameters
            {
                AssetIds = new[] { new AssetIdentifier(id) }
            }, testProgressMonitor);

            asset.AssetDataObjects.Should().HaveCount(1);
            asset.AssetDataObjects[0].FileMetadata.FileName.Should().BeEquivalentTo(SharepointOptions.Files[0].Name);
            asset.AssetDataObjects[0].Version.Should().NotBeNullOrEmpty();
            asset.AssetDataObjects[0].RawData.Should().NotBeEmpty();
            asset.AssetDataObjects[0].RawData[0].Values.Should().HaveCountGreaterOrEqualTo(25);
            asset.AssetDataObjects[0].Id.Should().MatchRegex(".*__.*");

            testProgressMonitor.CurrentValue.Should().Be(testProgressMonitor.Maximum);
            testProgressMonitor.ReportProgressInvocations.Should().Be(2);
            testProgressMonitor.FinishedInvocations.Should().Be(1);
        }

        [Fact]
        public async Task GetAssetsByIdAsync_ShouldReturnMultipleAssetsAsync()
        {
            AssetIdentifier[] ids =
            {
                new AssetIdentifier(SharepointOptions.Files[0].Id),
                new AssetIdentifier(SharepointOptions.Files[1].Id)
            };


            var testProgressMonitor = new TestProgressMonitor();
            var assets = await _dataAdapter.GetAssetsAsync(new GetAssetsParameters { AssetIds = ids },
                testProgressMonitor);

            assets.AssetDataObjects.Should().HaveCount(2);
            assets.AssetDataObjects[0].FileMetadata.FileName.Should().BeEquivalentTo(SharepointOptions.Files[0].Name);
            assets.AssetDataObjects[1].FileMetadata.FileName.Should().BeEquivalentTo(SharepointOptions.Files[1].Name);

            testProgressMonitor.CurrentValue.Should().Be(testProgressMonitor.Maximum);
            testProgressMonitor.ReportProgressInvocations.Should().Be(3);
            testProgressMonitor.FinishedInvocations.Should().Be(1);
        }

        [Fact]
        public Task GetAssetsByIdAsync_WhenNotFoundAsync()
        {
            const string id = "b!vX0Z05Whf0KjPVRyFi-dIcPnqE2SrE5Amwl4PQndVufOwDIJuOBnTYT2tsl1F6Nh__this_does_not_exist";

            return Assert.ThrowsAsync<ExternalDependencyException>(() => _dataAdapter.GetAssetsAsync(
                new GetAssetsParameters
                {
                    AssetIds = new[] { new AssetIdentifier(id) }
                }, new TestProgressMonitor()));
        }

        [Fact]
        public async Task GetAssetPermissionAsync()
        {
            var result = await _dataAdapter.GetAssetsPermissionsAsync(new GetAssetsPermissionsParameters()
            {
                AssetIds = new[] { new AssetIdentifier(SharepointOptions.Files[0].Id) }
            });
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAssetPermission_WhenNotFoundAsync()
        {
            var result = await _dataAdapter.GetAssetsPermissionsAsync(new GetAssetsPermissionsParameters()
            {
                AssetIds = new[] { new AssetIdentifier("not_exist") }
            });

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAssetPermission_WhenNoIdsGivenAsync()
        {
            var result = await _dataAdapter.GetAssetsPermissionsAsync(new GetAssetsPermissionsParameters());

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAssetThumbnailDownloadStreamAsync()
        {
            var id = new AssetIdentifier(SharepointOptions.Files[1].Id);
            var result = await _dataAdapter.GetAssetThumbnailDownloadStreamAsync(
                id,
                ContentTypeEnumDataObject.Image,
                AssetThumbnailSize.Large,
                thumbnailSpec: null);

            result.Stream.Should().NotBeNull();
            result.FileName.Should().BeEquivalentTo(SharepointOptions.Files[1].Name);
            result.MediaType.Should().BeEquivalentTo(SharepointOptions.Files[1].MediaType);
        }

        [Fact]
        public async Task GetAssetThumbnailDownloadStream_SizeNotFound_ShouldDefaultToMediumAsync()
        {
            var id = new AssetIdentifier(SharepointOptions.Files[1].Id);
            var result = await _dataAdapter.GetAssetThumbnailDownloadStreamAsync(
                id,
                ContentTypeEnumDataObject.Image,
                AssetThumbnailSize.PlaybackSmall,
                thumbnailSpec: null);

            result.Stream.Should().NotBeNull();
            result.FileName.Should().BeEquivalentTo(SharepointOptions.Files[1].Name);
            result.MediaType.Should().BeEquivalentTo(SharepointOptions.Files[1].MediaType);
            result.FileSizeInBytes.Should().BeLessThan(SharepointOptions.Files[1].Size); //now we know the byte length
        }

        [Fact]
        public async Task GetAssetThumbnailDownloadStream_ContentTypeMismatchAsync()
        {
            var id = new AssetIdentifier(SharepointOptions.Files[1].Id);
            var result = await _dataAdapter.GetAssetThumbnailDownloadStreamAsync(
                id,
                ContentTypeEnumDataObject.Image,
                AssetThumbnailSize.Small,
                thumbnailSpec: null);

            result.Stream.Should().NotBeNull();
            result.FileName.Should().BeEquivalentTo(SharepointOptions.Files[1].Name);
            result.MediaType.Should().BeEquivalentTo(SharepointOptions.Files[1].MediaType);
            result.FileSizeInBytes.Should().BeLessThan(SharepointOptions.Files[1].Size); //now we know the byte length
        }

        [Fact]
        public Task GetAssetThumbnailDownloadStream_WhenNotFoundAsync()
        {
            const string id = "b!vX0Z05Whf0KjPVRyFi-dIcPnqE2SrE5Amwl4PQndVufOwDIJuOBnTYT2tsl1F6Nh__not_exist";

            return Assert.ThrowsAsync<ExternalDependencyException>(() => _dataAdapter.GetAssetThumbnailDownloadStreamAsync(
                new AssetIdentifier(id),
                ContentTypeEnumDataObject.Image,
                AssetThumbnailSize.Large,
                thumbnailSpec: null));
        }

        [Fact]
        public async Task GetAssetDownloadStreamAsync()
        {
            var result = await _dataAdapter.GetAssetDownloadStreamAsync(
                new AssetIdentifier(SharepointOptions.Files[0].Id),
                null);

            result.Stream.Should().NotBeNull();
            result.FileName.Should().BeEquivalentTo(SharepointOptions.Files[0].Name);
            result.MediaType.Should().Be(SharepointOptions.Files[0].MediaType);
            result.FileSizeInBytes.Should().Be(SharepointOptions.Files[0].Size);
            result.FileSizeInBytes.Should().Be(result.Stream.Length);
        }

        [Fact]
        public Task GetAssetDownloadStream_WhenNotfoundAsync()
        {
            return Assert.ThrowsAsync<ExternalDependencyException>(() => _dataAdapter.GetAssetDownloadStreamAsync(
                new AssetIdentifier("b!vX0Z05Whf0KjPVRyFi-dIcPnqE2SrE5Amwl4PQndVufOwDIJuOBnTYT2tsl1F6Nh__not_exist"),
                null));
        }

        private static void AssertRawData(DataAdapterSDK.DataAdapters.Interfaces.Assets.Results.GetAssetResult asset)
        {
            var sharepointFile = asset.AssetDataObject.RawData.SingleOrDefault(rd => rd.MetamodelEntityKey == "c1___SharepointFile");

            sharepointFile.Should().NotBeNull();

            sharepointFile.Values.TryGetValue("SingleLineText", out var singleLineTextProperty).Should().BeTrue();

            var singleLineTextValue = singleLineTextProperty.Value as LocalizedStringsModel;

            singleLineTextValue.Should().NotBeNull();
            singleLineTextValue.Should().HaveCount(2);

            singleLineTextValue["de"].Should().NotBeNullOrEmpty();

            sharepointFile.Values.TryGetValue("Choice", out var choiceProperty).Should().BeTrue();

            var choiceValue = choiceProperty.Value as EnumDataObject;

            choiceValue.Should().NotBeNull();
            choiceValue.ListDisplayName.Should().HaveCount(2);

            choiceValue.ListDisplayName[LocalizedStringsModel.DefaultCulture].Should().NotBeNullOrEmpty();
            choiceValue.ListDisplayName["en"].Should().NotBeNullOrEmpty();

            sharepointFile.Values.TryGetValue("Multiselect", out var multiselectProperty).Should().BeTrue();

            var multiselectValue = multiselectProperty.Value as LocalizedStringsArrayModel;

            multiselectValue.Should().NotBeNull();
            multiselectValue.Should().HaveCount(2);

            multiselectValue[LocalizedStringsModel.DefaultCulture].Should().NotBeNullOrEmpty();
            multiselectValue["en"].Should().NotBeNullOrEmpty();

            sharepointFile.Values.TryGetValue("Description", out var descriptionProperty).Should().BeTrue();

            var descriptionValue = descriptionProperty.Value as LocalizedStringsModel;

            descriptionValue.Should().NotBeNull();
            descriptionValue.Should().HaveCount(3);

            descriptionValue[LocalizedStringsModel.DefaultCulture].Should().NotBeNullOrEmpty();
            descriptionValue["de"].Should().NotBeNullOrEmpty();
            descriptionValue["es"].Should().NotBeNullOrEmpty();

            sharepointFile.Values.TryGetValue("Location", out var locationProperty).Should().BeTrue();

            var locationValue = locationProperty.Value as DataObject;

            locationValue.Should().NotBeNull();
            locationValue.Values.Should().HaveCount(3);

            locationValue.Values.TryGetValue("DisplayName", out var displayNameProperty).Should().BeTrue();

            var displayNameValue = displayNameProperty.Value as LocalizedStringsModel;

            displayNameValue.Should().NotBeNull();
            displayNameValue.Should().HaveCount(2);

            displayNameValue[LocalizedStringsModel.DefaultCulture].Should().NotBeNullOrEmpty();
            displayNameValue["de"].Should().NotBeNullOrEmpty();

            locationValue.Values.TryGetValue("Address", out var addressProperty).Should().BeTrue();

            var addressValue = addressProperty.Value as DataObject;

            addressValue.Should().NotBeNull();
            addressValue.Values.Should().HaveCount(5);

            addressValue.Values.TryGetValue("City", out var addressCityProperty).Should().BeTrue();

            var addressCityValue = addressCityProperty.Value as LocalizedStringsModel;

            addressCityValue.Should().NotBeNull();
            addressCityValue.Should().HaveCount(2);

            addressCityValue[LocalizedStringsModel.DefaultCulture].Should().NotBeNullOrEmpty();
            addressCityValue["de"].Should().NotBeNullOrEmpty();

            sharepointFile.Values.TryGetValue("Calculated", out var calculatedProperty).Should().BeTrue();

            var calculatedValue = calculatedProperty.Value as LocalizedStringsModel;

            calculatedValue.Should().NotBeNull();
            calculatedValue.Should().HaveCount(2);

            calculatedValue[LocalizedStringsModel.DefaultCulture].Should().NotBeNullOrEmpty();
            calculatedValue["de"].Should().NotBeNullOrEmpty();

            sharepointFile.Values.TryGetValue("ChoiceAlt2", out var choiceAlt2Property).Should().BeTrue();

            var choiceAlt2Value = choiceAlt2Property.Value as EnumDataObject;

            choiceAlt2Value.Should().NotBeNull();
            choiceAlt2Value.ListDisplayName.Should().HaveCount(2);
            choiceAlt2Value.ListDisplayName[LocalizedStringsModel.DefaultCulture].Should().NotBeNullOrEmpty();
            choiceAlt2Value.ListDisplayName["de"].Should().NotBeNullOrEmpty();

            sharepointFile.Values.TryGetValue("Image", out var imageProperty).Should().BeTrue();

            var imageValue = imageProperty.Value as DataObject;

            imageValue.Should().NotBeNull();
            imageValue.Values.Should().HaveCount(6);

            imageValue.Values.TryGetValue("FileName", out var imageFileNameProperty).Should().BeTrue();

            var imageFileNameValue = imageFileNameProperty.Value as LocalizedStringsModel;

            imageFileNameValue.Should().NotBeNull();
            imageFileNameValue.Should().HaveCount(3);

            imageFileNameValue[LocalizedStringsModel.DefaultCulture].Should().NotBeNullOrEmpty();
            imageFileNameValue["de"].Should().NotBeNullOrEmpty();
            imageFileNameValue["es"].Should().NotBeNullOrEmpty();
        }
    }
}