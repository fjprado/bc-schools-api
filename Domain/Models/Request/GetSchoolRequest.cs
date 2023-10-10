using bc_schools_api.Domain.Models.Entities;

namespace bc_schools_api.Domain.Models.Request
{
    public class GetSchoolRequest
    {
        public Coordinate Coordinate { get; set; }
        public IEnumerable<Filter> Filters { get; set; }
    }
}
