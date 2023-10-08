using bc_schools_api.Domain.Models.Entities;

namespace bc_schools_api.Domain.Models.Request
{
    public class DistanceRequest
    {
        public IEnumerable<Coordinate> Origins { get; set; }
        public IEnumerable<Coordinate> Destinations { get; set; }
        public string TravelMode { get; set; } = "driving";
    }
}
