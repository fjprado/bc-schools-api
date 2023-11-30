using bc_schools_api.Domain.Models.Entities;
using bc_schools_api.Domain.Models.Request;

namespace bc_schools_api.Services.Interfaces
{
    public interface ISchoolService
    {
        Task<List<School>> GetSchoolsList(GetSchoolRequest requestModel);
        Task UpdateSchoolList(IFormFile file);
    }
}
