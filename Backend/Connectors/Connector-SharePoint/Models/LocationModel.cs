using SmintIo.Portals.SDK.Core.Models.Other;

namespace SmintIo.Portals.Connector.SharePoint.Models
{
    public class LocationModel
    {
        public const string Key = "Location";

        public AddressModel Address { get; set; }

        public GeoLocationModel Coordinates { get; set; }

        public string DispName { get; set; }

        public string LocationUri { get; set; }

        public string UniqueId { get; set; }
    }

    public class AddressModel
    {
        public const string Key = "Address";

        public string City { get; set; }

        public string CountryOrRegion { get; set; }

        public string State { get; set; }

        public string Street { get; set; }

        public string PostalCode { get; set; }
    }
}
