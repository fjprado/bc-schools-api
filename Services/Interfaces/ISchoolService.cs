using bc_schools_api.Domain.Models.Entities;

namespace bc_schools_api.Services.Interfaces
{
    public interface ISchoolService
    {
        Task<List<School>> GetSchoolsList(Coordinate coordenadaOrigem, int limitRange);
    }
}
