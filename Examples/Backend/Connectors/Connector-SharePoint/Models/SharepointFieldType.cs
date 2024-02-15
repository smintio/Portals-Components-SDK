namespace SmintIo.Portals.Connector.SharePoint.Models
{
    public enum SharepointFieldType
    {
        Invalid = 0,
        Integer = 1,
        Text = 2,
        Note = 3,
        DateTime = 4,
        Counter = 5,
        Choice = 6,
        Lookup = 7,
        Boolean = 8,
        Number = 9,
        Currency = 10,
        Url = 11,
        Computed = 12,
        MultiChoice = 15,
        Calculated = 17,
        File = 18,
        User = 20,
        Geolocation = 31,
        Location = 33,
        Image = 34,
        Taxonomy = 98,
        TaxonomyMulti = 99,
        UserMulti = 100
    }
}