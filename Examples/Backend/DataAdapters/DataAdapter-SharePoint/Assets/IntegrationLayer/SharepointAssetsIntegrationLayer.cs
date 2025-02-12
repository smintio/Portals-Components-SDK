using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Graph;
using SmintIo.Portals.Connector.SharePoint.Extensions;
using SmintIo.Portals.Connector.SharePoint.Models;
using SmintIo.Portals.DataAdapter.SharePoint.Assets.Common;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Impl;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Models;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Parameters;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Results;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Data;
using SmintIo.Portals.SDK.Core.Rest.Prefab.Exceptions;
using ChangeType = SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.Assets.Models.ChangeType;

namespace SmintIo.Portals.DataAdapter.SharePoint.Assets
{
    public partial class SharepointAssetsDataAdapter : AssetsDataAdapterBaseImpl, IAssetsIntegrationLayerApiProvider
    {
        public async Task<GetFolderContentsResult> GetFolderContentsForIntegrationLayerAsync(GetFolderContentsParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var pageSize = parameters.PageSize ?? 50;

            var driveItemList = await _sharepointClient.GetFolderDriveItemsAsync(
                parameters.FolderId?.UnscopedId,
                skipToken: parameters.ResultSetUuid,
                pageSize).ConfigureAwait(false);

            if (driveItemList == null)
            {
                return new GetFolderContentsResult
                {
                    Details = new GetFolderContentsDetailsModel
                    {
                        TotalResults = 0,
                        HasMoreResults = false,
                        IgnoreTopLevelFolders = !_sharepointClient.IsRootFolderSync
                    },
                    AssetDataObjects = new AssetDataObject[0],
                    FolderDataObjects = new FolderDataObject[0]
                };
            }

            var parentFolderIdsByAssetId = await _sharepointClient.GetParentFolderIdsByAssetIdAsync(driveItemList.DriveItems);

            var converter = new SharepointContentConverter(_logger, Context, _sharepointClient, _entityModelProvider, parentFolderIdsByAssetId);

            var assetDataObjectTasks = driveItemList.DriveItems
                .Where(di => di.IsAsset())
                .Select(di => converter.GetAssetDataObjectAsync(di))
                .ToArray();

            var folderDataObjectTasks = driveItemList.DriveItems
                .Where(di => di.IsFolder())
                .Select(di => converter.GetFolderDataObjectAsync(di))
                .ToArray();

            var assetDataObjects = await Task.WhenAll(assetDataObjectTasks).ConfigureAwait(false);
            var folderDataObjects = await Task.WhenAll(folderDataObjectTasks).ConfigureAwait(false);

            var folderContentsResult = new GetFolderContentsResult()
            {
                Details = new GetFolderContentsDetailsModel
                {
                    ResultSetId = driveItemList.ContinuationUuid,
                    CurrentItemsPerPage = pageSize,
                    HasMoreResults = driveItemList.HasContinuationUuid,
                    IgnoreTopLevelFolders = !_sharepointClient.IsRootFolderSync
                },
                AssetDataObjects = assetDataObjects,
                FolderDataObjects = folderDataObjects
            };

            return folderContentsResult;
        }

        public async Task<GetChangesResult> GetChangesAsync(GetChangesParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            DriveItemChangesListModel driveItemsChangesList;

            try
            {
                driveItemsChangesList = await _sharepointClient.GetDriveItemChangesListAsync(deltaLink: parameters.LastContinuationId).ConfigureAwait(false);
            }
            catch (ExternalDependencyException ex)
            when (ex.ErrorCode == ExternalDependencyStatusEnum.ContinuationUuidTooOld)
            {
                driveItemsChangesList = new DriveItemChangesListModel
                {
                    ContinuationTooOld = true
                };
            }

            var changes = await GetChangesModelsAsync(driveItemsChangesList).ConfigureAwait(false);

            var changesResult = new GetChangesResult()
            {
                Changes = changes,
                Details = new GetChangesDetailsModel
                {
                    ContinuationUuid = driveItemsChangesList?.ContinuationUuid
                },
                ContinuationUuidTooOld = driveItemsChangesList?.ContinuationTooOld ?? false
            };

            return changesResult;
        }

        private static ChangeModel GetFolderChangeModel(FolderDataObject folderDataObject, ChangeType changeType, bool recursionIsHandledByDataAdapter)
        {
            return new ChangeModel
            {
                UnscopedAssetIdentifier = folderDataObject.Id,
                FolderDataObject = folderDataObject,
                Type = changeType,
                RecursionIsHandledByDataAdapter = recursionIsHandledByDataAdapter
            };
        }

        private static ChangeModel GetFolderChangeModel(string unscopedAssetIdentifier, ChangeType changeType, bool recursionIsHandledByDataAdapter)
        {
            return new ChangeModel
            {
                UnscopedAssetIdentifier = unscopedAssetIdentifier,
                Type = changeType,
                RecursionIsHandledByDataAdapter = recursionIsHandledByDataAdapter
            };
        }

        private static ChangeModel GetAssetChangeModel(AssetDataObject assetDataObject, ChangeType changeType)
        {
            return new ChangeModel
            {
                UnscopedAssetIdentifier = assetDataObject.Id,
                AssetDataObject = assetDataObject,
                Type = changeType
            };
        }

        private static ChangeModel GetAssetChangeModel(string unscopedAssetIdentifier, ChangeType changeType)
        {
            return new ChangeModel
            {
                UnscopedAssetIdentifier = unscopedAssetIdentifier,
                Type = changeType
            };
        }

        private async Task<ChangeModel[]> GetChangesModelsAsync(DriveItemChangesListModel driveItemsChangesList)
        {
            if (driveItemsChangesList == null || (!driveItemsChangesList.DriveItems.Any() && !driveItemsChangesList.FolderDriveItemsToDelete.Any()))
            {
                return Array.Empty<ChangeModel>();
            }

            var parentFolderIdsByAssetId = await _sharepointClient.GetParentFolderIdsByAssetIdAsync(driveItemsChangesList.DriveItems).ConfigureAwait(false);

            var converter = new SharepointContentConverter(_logger, Context, _sharepointClient, _entityModelProvider, parentFolderIdsByAssetId);

            var folderDataObjectsToUpdate = await GetFolderDriveItemsToUpdateAsync(driveItemsChangesList, converter).ConfigureAwait(false);

            var foldersToDeleteRecursivelyUnscopedAssetIds = driveItemsChangesList.FolderDriveItemsToDelete
                .Select(di => di.GetAssetId())
                .ToArray();

            var foldersToDeleteUnscopedAssetIds = driveItemsChangesList.DriveItems
                .Where(di => di.IsFolder() && di.CanBeDeleted())
                .Select(di => di.GetAssetId())
                .ToArray();

            var assetDataObjectsToUpdate = await GetAssetDataObjectsToUpdateAsync(driveItemsChangesList, converter).ConfigureAwait(false);

            var assetsToDeleteUnscopedAssetIds = driveItemsChangesList.DriveItems
                .Where(di => di.IsAsset() && di.CanBeDeleted())
                .Select(di => di.GetAssetId())
                .ToArray();

            var folderUpdateChangeModels = folderDataObjectsToUpdate.Select(fdo => GetFolderChangeModel(fdo, ChangeType.FolderUpdate, recursionIsHandledByDataAdapter: true));

            var folderDeletionRecursivelyChangeModels = foldersToDeleteRecursivelyUnscopedAssetIds
                .Select(unscopedAssetId => GetFolderChangeModel(unscopedAssetId, ChangeType.FolderDeletion, recursionIsHandledByDataAdapter: false));

            var folderDeletionChangeModels = foldersToDeleteUnscopedAssetIds
                .Select(unscopedAssetId => GetFolderChangeModel(unscopedAssetId, ChangeType.FolderDeletion, recursionIsHandledByDataAdapter: true));

            var assetUpdateChangeModels = assetDataObjectsToUpdate.Select(ado => GetAssetChangeModel(ado, ChangeType.AssetUpdate));
            var assetDeletionChangeModels = assetsToDeleteUnscopedAssetIds.Select(unscopedAssetId => GetAssetChangeModel(unscopedAssetId, ChangeType.AssetDeletion));

            var changeModels =
                folderUpdateChangeModels
                .Concat(folderDeletionRecursivelyChangeModels)
                .Concat(folderDeletionChangeModels)
                .Concat(assetUpdateChangeModels)
                .Concat(assetDeletionChangeModels)
                .ToArray();

            return changeModels;
        }

        private static async Task<List<FolderDataObject>> GetFolderDriveItemsToUpdateAsync(DriveItemChangesListModel driveItemsChangesList, SharepointContentConverter converter)
        {
            var folderDriveItemsToUpdate = driveItemsChangesList.DriveItems
                .Where(di => di.IsFolder() && di.CanBeUpdated())
                .ToArray();

            var folderDataObjectsToUpdate = new List<FolderDataObject>();

            foreach (var driveItem in folderDriveItemsToUpdate)
            {
                var assetDataObject = await converter.GetFolderDataObjectAsync(driveItem).ConfigureAwait(false);

                folderDataObjectsToUpdate.Add(assetDataObject);
            }

            return folderDataObjectsToUpdate;
        }

        private async Task<List<AssetDataObject>> GetAssetDataObjectsToUpdateAsync(DriveItemChangesListModel driveItemsChangesList, SharepointContentConverter converter)
        {
            var assetsToFetch = driveItemsChangesList.DriveItems
                .Where(di => di.IsAsset() && di.CanBeUpdated())
                .Select(di => di.GetAssetId())
                .Distinct()
                .ToList();

            var driveItemsToUpdate = await _sharepointClient.GetDriveItemsBatchAsync(assetsToFetch).ConfigureAwait(false);

            var assetDataObjectsToUpdate = new List<AssetDataObject>();

            foreach (var driveItem in driveItemsToUpdate)
            {
                var assetDataObject = await converter.GetAssetDataObjectAsync(driveItem).ConfigureAwait(false);

                assetDataObjectsToUpdate.Add(assetDataObject);
            }

            return assetDataObjectsToUpdate;
        }

        public async Task<GetAssetChangeResult> GetAssetChangeAsync(GetAssetChangeParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var unscopedId = parameters.AssetId?.UnscopedId;

            if (string.IsNullOrEmpty(unscopedId))
            {
                return new GetAssetChangeResult();
            }

            DriveItem driveItem = null;

            try
            {
                driveItem = await _sharepointClient.GetDriveItemAsync(unscopedId).ConfigureAwait(false);
            }
            catch (ExternalDependencyException e)
            when (e.IsNotFound())
            {
                // no permissions or so...
            }

            if (driveItem == null || driveItem.IsFolder())
            {
                return new GetAssetChangeResult
                {
                    Change = new ChangeModel
                    {
                        UnscopedAssetIdentifier = unscopedId,
                        Type = ChangeType.AssetDeletion
                    }
                };
            }

            var driveItems = new List<DriveItem>()
            {
                driveItem
            };

            var parentFolderIdsByAssetId = await _sharepointClient.GetParentFolderIdsByAssetIdAsync(driveItems).ConfigureAwait(false);

            var converter = new SharepointContentConverter(_logger, Context, _sharepointClient, _entityModelProvider, parentFolderIdsByAssetId);

            var assetDataObject = await converter.GetAssetDataObjectAsync(driveItem).ConfigureAwait(false);

            var assetChangeResult = new GetAssetChangeResult()
            {
                Change = new ChangeModel
                {
                    UnscopedAssetIdentifier = unscopedId,
                    Type = ChangeType.AssetUpdate,
                    AssetDataObject = assetDataObject
                }
            };

            return assetChangeResult;
        }
    }
}