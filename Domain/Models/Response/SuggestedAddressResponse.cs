namespace bc_schools_api.Domain.Models.Response
{
    public class SuggestedAddressResponse
    {
        public ResourceSetsAddresses[] ResourceSets { get; set; }
    }

    public class ResourceSetsAddresses
    {
        public ResourcesAddresses[] Resources { get; set; }
    }

    public class ResourcesAddresses
    {
        public Addresses[] Value { get; set; }
    }

    public class Addresses
    {
        public Address Address { get; set; }
    }

    public class Address
    {
        public string FormattedAddress { get; set; }
    }
}
