namespace bc_schools_api.Domain.Models.Entities
{
    public class School
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string? GradeRange { get; set; }
        public string? Phone { get; set; }
        public string? Fax { get; set; }
        public int SchoolTypeId { get; set; }
        public string? SchoolTypeDesc { get; set; }
        public int SchoolCategoryId { get; set; }
        public string? SchoolCategoryDesc { get; set; }
        public double? TravelDistance { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}