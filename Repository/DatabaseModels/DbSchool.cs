namespace bc_schools_api.Repository.DatabaseModels
{
    public class DbSchool
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string PostalCode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string GradeRange { get; set; } = "";
        public string? Phone { get; set; }
        public string? Fax { get; set; }
        public int DistrictNumber { get; set; }
        public int SchoolTypeId { get; set; }
        public int SchoolCategoryId { get; set; }

        public virtual DbDistrict District { get; set; }
        public virtual DbSchoolType SchoolType { get; set; }
        public virtual DbSchoolCategory SchoolCategory { get; set; }
    }
}
