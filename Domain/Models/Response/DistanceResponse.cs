namespace bc_schools_api.Domain.Models.Response
{
    public class DistanceResponse
    {
        public DistanceResourceSets[] ResourceSets { get; set; }
    }

    public class DistanceResourceSets
    {
        public DistanceResources[] Resources { get; set; }
    }

    public class DistanceResources
    {
        public DistanceResults[] Results { get; set; }
    }

    public class DistanceResults
    {
        public int DestinationIndex { get; set; }
        public int OriginIndex { get; set; }
        public double TravelDistance { get; set; }
        public double TravelDuration { get; set; }
    }
}
