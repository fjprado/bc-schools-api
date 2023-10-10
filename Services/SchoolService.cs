﻿using AutoMapper;
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
using System.Linq.Expressions;
using System;
using bc_schools_api.Domain.Enums;
using System.Linq;

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

        public async Task<List<School>> GetSchoolsList(GetSchoolRequest requestModel)
        {
            try
            {
                var filters = FilteredSchools(requestModel);

                var schoolsDb = _dbContext.School.Where(filters.Compile());

                var schools = await GetSchoolsDistance(schoolsDb, requestModel);

                return schools.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private Expression<Func<DbSchool, bool>> FilteredSchools(GetSchoolRequest requestModel)
        {
            var filtersDetails = new List<Expression<Func<DbSchool, bool>>>();
            double defaultRangeCoordinate = 0.385;
            Expression<Func<DbSchool, bool>> FilterDefault() => school => (school.Latitude > (requestModel.Coordinate.Latitude - defaultRangeCoordinate) && school.Latitude < (requestModel.Coordinate.Latitude + defaultRangeCoordinate))
                                                            && (school.Longitude > (requestModel.Coordinate.Longitude - defaultRangeCoordinate) && school.Longitude < (requestModel.Coordinate.Longitude + defaultRangeCoordinate));

            filtersDetails.Add(FilterDefault());

            if (requestModel.Filters.Any())
            {
                Expression<Func<DbSchool, bool>> FilterBySchoolType(dynamic[] schoolTypes) => school => schoolTypes.Contains(school.SchoolTypeId);
                Expression<Func<DbSchool, bool>> FilterBySchoolCategory(dynamic[] schoolCategories) => school => schoolCategories.Contains(school.SchoolCategoryId);
                Expression<Func<DbSchool, bool>> FilterByGradeRange(dynamic[] gradeRanges) => school => school.GradeRange.Split(",", StringSplitOptions.None).Any(x => gradeRanges.Contains(x));
                Expression<Func<DbSchool, bool>> FilterByDistrict(dynamic[] districts) => school => districts.Contains(school.DistrictNumber);

                foreach (var item in requestModel.Filters)
                {
                    switch (item.FilterType)
                    {
                        case FilterEnum.SchoolType:
                            filtersDetails.Add(FilterBySchoolType(item.FilterValues));
                            break;
                        case FilterEnum.SchoolCategory:
                            filtersDetails.Add(FilterBySchoolCategory(item.FilterValues));
                            break;
                        case FilterEnum.GradeRange:
                            filtersDetails.Add(FilterByGradeRange(item.FilterValues));
                            break;
                        case FilterEnum.District:
                            filtersDetails.Add(FilterByDistrict(item.FilterValues));
                            break;
                    }
                }
            }

            var parameter = Expression.Parameter(typeof(DbSchool), "school");
            Expression combinedPredicate = null;

            foreach (var filter in filtersDetails)
            {
                if (combinedPredicate == null)
                    combinedPredicate = filter.Body;
                else
                    combinedPredicate = Expression.AndAlso(combinedPredicate, filter.Body);
            }

            return Expression.Lambda<Func<DbSchool, bool>>(combinedPredicate, parameter);
        }

        private async Task<IEnumerable<School>> GetSchoolsDistance(IEnumerable<DbSchool> dbSchools, GetSchoolRequest requestModel)
        {
            try
            {
                var destinationCoordinates = dbSchools.Select(s => new Coordinate() { Latitude = s.Latitude, Longitude = s.Longitude });

                if (requestModel.Coordinate.Latitude == null || requestModel.Coordinate.Longitude == null)
                {
                    requestModel.Coordinate = new Coordinate()
                    {
                        Latitude = _settings.DefaultLatitude,
                        Longitude = _settings.DefaultLongitude
                    };
                }

                var serializedRequest = JsonConvert.SerializeObject(new DistanceRequest()
                {
                    Origins = new List<Coordinate>() { requestModel.Coordinate },
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

                schools = FilterDistanceRange(requestModel, schools);

                return schools.OrderBy(o => o.TravelDistance);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static List<School> FilterDistanceRange(GetSchoolRequest requestModel, List<School> schools)
        {
            if (requestModel.Filters.Any(x => x.FilterType == FilterEnum.LimitRange))
            {
                return schools.Where(s => s.TravelDistance <= (requestModel.Filters.First(x => x.FilterType == FilterEnum.LimitRange).FilterValues[0] * 1000)).ToList();
            }

            return schools = schools.Where(s => s.TravelDistance <= 30 * 1000).ToList();
        }
}
}