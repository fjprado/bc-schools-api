using bc_schools_api.Domain.Enums;

namespace bc_schools_api.Domain.Models.Entities
{
    public class Filter
    {
        public FilterEnum FilterType { get; set; }
        public dynamic[] FilterValues { get; set; } = Array.Empty<dynamic>();
    }
}
