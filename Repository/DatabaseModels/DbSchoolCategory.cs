namespace bc_schools_api.Repository.DatabaseModels
{
    public class DbSchoolCategory
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public virtual ICollection<DbSchool> Schools { get; set; }
    }
}
