namespace bc_schools_api.Domain.Models.Response
{
    public class LocalizationResponse
    {
        public LocalizationResourceSets[] ResourceSets { get; set; }
    }

    public class LocalizationResourceSets
    {
        public LocalizationResources[] Resources { get; set; }
    }

    public class LocalizationResources
    {
        public GeocodePoints[] GeocodePoints { get; set; }
    }

    public class GeocodePoints
    {
        public double[] Coordinates { get; set; }
    }
}
