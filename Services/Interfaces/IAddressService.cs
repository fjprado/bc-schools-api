using bc_schools_api.Domain.Models.Entities;

namespace bc_schools_api.Services.Interfaces
{
    public interface IAddressService
    {
        Task<List<OriginAddress>> GetSuggestedAddressList(string address);
        Task<Coordinate> GetAddressCoordinate(string address);
    }
}
