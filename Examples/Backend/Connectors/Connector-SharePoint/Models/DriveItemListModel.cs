using System.Collections.Generic;
using Microsoft.Graph.Models;

namespace SmintIo.Portals.Connector.SharePoint.Models
{
    public class DriveItemListModel
    {
        private ICollection<DriveItem> _driveItems;

        public ICollection<DriveItem> DriveItems
        {
            get
            {
                if (_driveItems == null)
                {
                    _driveItems = new List<DriveItem>();
                }

                return _driveItems;
            }
            set
            {
                _driveItems = value;
            }
        }

        public string ContinuationUuid { get; set; }

        public bool ContinuationTooOld { get; set; }

        public bool HasContinuationUuid => !string.IsNullOrEmpty(ContinuationUuid);
    }
}
