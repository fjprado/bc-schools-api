using AutoMapper;
using bc_schools_api.Domain.Models.Entities;
using bc_schools_api.Domain.Models.Request;
using bc_schools_api.Domain.Models.Response;
using bc_schools_api.Infra.Interfaces;
using bc_schools_api.Repository;
using bc_schools_api.Repository.DatabaseModels;
using bc_schools_api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace bc_schools_api.Services
{
    public class SchoolService : ISchoolService
    {
        private readonly ISettings _settings;
        private readonly IMapper _mapper;
        private readonly DatabaseContext _dbContext;

        public SchoolService(ISettings settings, IMapper mapper, DatabaseContext dbContext)
        {
            _settings = settings;
            _mapper = mapper;
            _dbContext = dbContext;
        }

        public async Task<List<School>> GetSchoolsList(Coordinate originCoordinate, int limitRange)
        {
            try
            {
                double defaultRangeCoordinate = 0.385;
                var schoolsDb = _dbContext.School.Where(x => (x.Latitude > (originCoordinate.Latitude - defaultRangeCoordinate) && x.Latitude < (originCoordinate.Latitude + defaultRangeCoordinate))
                                                        && (x.Longitude > (originCoordinate.Longitude - defaultRangeCoordinate) && x.Longitude < (originCoordinate.Longitude + defaultRangeCoordinate)));

                var schools = await GetSchoolsDistance(schoolsDb, originCoordinate);

                return schools.Where(s => s.TravelDistance <= (limitRange * 1000)).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<IEnumerable<School>> GetSchoolsDistance(IEnumerable<DbSchool> dbSchools, Coordinate originCoordinate)
        {
            try
            {
                var destinationCoordinates = dbSchools.Select(s => new Coordinate() { Latitude = s.Latitude, Longitude = s.Longitude});

                if (originCoordinate.Latitude == null || originCoordinate.Longitude == null)
                {
                    originCoordinate = new Coordinate()
                    {
                        Latitude = _settings.DefaultLatitude,
                        Longitude = _settings.DefaultLongitude
                    };
                }

                var serializedRequest = JsonConvert.SerializeObject(new DistanceRequest()
                {
                    Origins = new List<Coordinate>() { originCoordinate },
                    Destinations = destinationCoordinates
                });

                var client = new RestRequest(_settings.UrlRoutesApi, Method.Post);

                client.RequestFormat = DataFormat.Json;
                client.AddHeader("Content-type", "application/json");
                client.AddParameter("application/json", serializedRequest, ParameterType.RequestBody);
                client.AddParameter("key", _settings.LocalizationApiKey, ParameterType.QueryString);

                RestClient _rest = new();
                var response = await _rest.ExecuteAsync<JObject>(client);

                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                var apiResponse = JsonConvert.DeserializeObject<DistanceResponse>(response.Content, jsonSerializerSettings);

                var results = apiResponse?.ResourceSets?.SelectMany(rs => rs.Resources.SelectMany(r => r.Results));

                var schools = _mapper.Map<IEnumerable<DbSchool>, List<School>>(dbSchools);
                foreach (var school in schools)
                {
                    school.TravelDistance = results?.Where(w => w.DestinationIndex == schools?.IndexOf(school))?.FirstOrDefault()?.TravelDistance ?? 0;
                }

                return schools.OrderBy(o => o.TravelDistance);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}