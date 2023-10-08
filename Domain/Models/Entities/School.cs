namespace bc_schools_api.Domain.Models.Entities
{
    public class School
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string PostalCode { get; set; }
        public string? GradeRange { get; set; }
        public string? Phone { get; set; }
        public string? Fax { get; set; }
        public string? DistrictName { get; set; }
        public string? SchoolTypeDesc { get; set; }
        public string? SchoolCategoryDesc { get; set; }
        public double? TravelDistance { get; set; }
    }
}