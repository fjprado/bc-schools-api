using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using bc_schools_api.Domain.Entities;
using bc_schools_api.Services.Interfaces;
using RestSharp;
using bc_schools_api.Domain.Models;
using AutoMapper;

namespace bc_schools_api.Services
{
    public class AddressService : IAddressService
    {
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        public AddressService(IConfiguration configuration, IMapper mapper)
        {
            _configuration = configuration;
            _mapper = mapper;
        }

        public async Task<List<OriginAddress>> GetSuggestedAddressList(string address)
        {
            try
            {
                var client = new RestRequest(_configuration["ProjectSettings:UrlSuggestAddressApi"], Method.Get)
                {
                    RequestFormat = DataFormat.Json
                };
                client.AddHeader("Content-type", "application/json");
                client.AddParameter("query", address, ParameterType.QueryString);
                client.AddParameter("userLocation", "-30.08,-51.21", ParameterType.QueryString);
                client.AddParameter("includeEntityTypes", "Address", ParameterType.QueryString);
                client.AddParameter("key", _configuration["ProjectSettings:LocalizationApiKey"], ParameterType.QueryString);

                RestClient _rest = new();
                var response = await _rest.ExecuteAsync<JObject>(client);

                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                var result = JsonConvert.DeserializeObject<SuggestedAddressResponse>(response.Content, jsonSerializerSettings);

                var suggestedAddress = result.ResourceSets.SelectMany(rs => rs.Resources.SelectMany(r => r.Value.Select(v => v.Address))).ToList();

                return _mapper.Map<List<Address>, List<OriginAddress>>(suggestedAddress);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Coordinate> GetAddressCoordinate(string address)
        {
            try
            {
                var client = new RestRequest(_configuration["ProjectSettings:UrlLocationsApi"], Method.Get)
                {
                    RequestFormat = DataFormat.Json
                };
                client.AddHeader("Content-type", "application/json");
                client.AddParameter("countryRegion", "BR", ParameterType.QueryString);
                client.AddParameter("addressLine", address, ParameterType.QueryString);
                client.AddParameter("key", _configuration["ProjectSettings:LocalizationApiKey"], ParameterType.QueryString);

                RestClient _rest = new();
                var response = await _rest.ExecuteAsync<JObject>(client);

                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                var result = JsonConvert.DeserializeObject<LocalizationResponse>(response.Content, jsonSerializerSettings);

                var coordinateList = result.ResourceSets.SelectMany(rs => rs.Resources.SelectMany(r => r.GeocodePoints)).FirstOrDefault().Coordinates;
                return new Coordinate() { Latitude = coordinateList[0], Longitude = coordinateList[1] };
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
