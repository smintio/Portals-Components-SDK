using System.Collections.Generic;
using Microsoft.Graph;

namespace SmintIo.Portals.Connector.SharePoint.Models
{
    public class DriveItemChangesListModel : DriveItemListModel
    {
        private ICollection<DriveItem> _folderDriveItems;

        public ICollection<DriveItem> FolderDriveItemsToDelete
        {
            get
            {
                if (_folderDriveItems == null)
                {
                    _folderDriveItems = new List<DriveItem>();
                }

                return _folderDriveItems;
            }
            set
            {
                _folderDriveItems = value;
            }
        }
    }
}
