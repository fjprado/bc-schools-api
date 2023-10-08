using System.Data;

namespace bc_schools_api.Repository.DatabaseModels
{
    public class DbDistrict
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public virtual ICollection<DbSchool> Schools { get; set; }
    }
}
