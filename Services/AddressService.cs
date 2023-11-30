using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using bc_schools_api.Services.Interfaces;
using RestSharp;
using AutoMapper;
using bc_schools_api.Infra.Interfaces;
using bc_schools_api.Domain.Models.Entities;
using bc_schools_api.Domain.Models.Response;

namespace bc_schools_api.Services
{
    public class AddressService : IAddressService
    {
        private readonly ISettings _settings;
        public AddressService(ISettings settings)
        {
            _settings = settings;
        }

        public async Task<List<string>> GetSuggestedAddressList(string address)
        {
            try
            {
                var client = new RestRequest(_settings.UrlSuggestAddressApi, Method.Get)
                {
                    RequestFormat = DataFormat.Json
                };
                client.AddHeader("Content-type", "application/json");
                client.AddParameter("query", address, ParameterType.QueryString);
                client.AddParameter("userLocation", "-30.08,-51.21", ParameterType.QueryString);
                client.AddParameter("includeEntityTypes", "Address", ParameterType.QueryString);
                client.AddParameter("key", _settings.LocalizationApiKey, ParameterType.QueryString);

                RestClient _rest = new();
                var response = await _rest.ExecuteAsync<JObject>(client);

                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                var result = JsonConvert.DeserializeObject<SuggestedAddressResponse>(response.Content, jsonSerializerSettings);

                var suggestedAddress = result.ResourceSets.SelectMany(rs => rs.Resources.SelectMany(r => r.Value.Select(v => v.Address))).ToList();

                return suggestedAddress.Select(x => x.FormattedAddress).ToList();
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
                var client = new RestRequest(_settings.UrlLocationsApi, Method.Get)
                {
                    RequestFormat = DataFormat.Json
                };
                client.AddHeader("Content-type", "application/json");
                client.AddParameter("countryRegion", "CA", ParameterType.QueryString);
                client.AddParameter("addressLine", address, ParameterType.QueryString);
                client.AddParameter("key", _settings.LocalizationApiKey, ParameterType.QueryString);

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
