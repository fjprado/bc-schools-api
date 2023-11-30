using AutoMapper;
using bc_schools_api.Domain.Enums;
using bc_schools_api.Domain.Models.Entities;
using bc_schools_api.Domain.Models.Request;
using bc_schools_api.Domain.Models.Response;
using bc_schools_api.Infra.Interfaces;
using bc_schools_api.Repository;
using bc_schools_api.Repository.DatabaseModels;
using bc_schools_api.Services.Interfaces;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using RestSharp;
using System.Linq.Expressions;
using System.Text;

namespace bc_schools_api.Services
{
    public class SchoolService : ISchoolService
    {
        private readonly ISettings _settings;
        private readonly IMapper _mapper;
        private readonly DatabaseContext _dbContext;
        private readonly IAddressService _addressService;

        public SchoolService(ISettings settings, IMapper mapper, DatabaseContext dbContext, IAddressService addressService)
        {
            _settings = settings;
            _mapper = mapper;
            _dbContext = dbContext;
            _addressService = addressService;
        }

        public async Task<List<School>> GetSchoolsList(GetSchoolRequest requestModel)
        {
            try
            {
                var filters = FilteredSchools(requestModel);

                var schoolsDb = _dbContext.School
                        .Include(x => x.SchoolCategory)
                        .Include(x => x.SchoolType)
                        .Where(filters);

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
            double defaultRangeCoordinate = 0.385;

            var predicate = PredicateBuilder.New<DbSchool>();
            predicate = predicate.And(school => (school.Latitude > (requestModel.Coordinate.Latitude - defaultRangeCoordinate) && school.Latitude < (requestModel.Coordinate.Latitude + defaultRangeCoordinate))
                                                            && (school.Longitude > (requestModel.Coordinate.Longitude - defaultRangeCoordinate) && school.Longitude < (requestModel.Coordinate.Longitude + defaultRangeCoordinate)));
            if (requestModel.Filters?.Any() ?? false)
            {
                foreach (var item in requestModel.Filters)
                {
                    switch (item.FilterType)
                    {
                        case FilterEnum.SchoolType:
                            predicate = predicate.And(school => item.FilterValues.Contains(school.SchoolTypeId));
                            break;
                        case FilterEnum.SchoolCategory:
                            predicate = predicate.And(school => item.FilterValues.Contains(school.SchoolCategoryId));
                            break;
                        case FilterEnum.District:
                            predicate = predicate.And(school => item.FilterValues.Contains(school.DistrictNumber));
                            break;
                    }
                }
            }

            return predicate;
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
            if (requestModel.Filters?.Any(x => x.FilterType == FilterEnum.LimitRange) ?? false)
            {
                return schools.Where(s => s.TravelDistance <= (requestModel.Filters.First(x => x.FilterType == FilterEnum.LimitRange).FilterValues[0] * 1000)).ToList();
            }

            return schools = schools.Where(s => s.TravelDistance <= 60 * 1000).ToList();
        }

        public async Task UpdateSchoolList(IFormFile file)
        {
            var worksheetData = await GetWorksheetData(file);
            if (!worksheetData?.Any() ?? true)
            {
                return;
            }

            var (districts, schoolCategories, schoolTypes) = GetUtilData();

            for (int pointer = 0; pointer < worksheetData?.Count(); pointer += 100)
            {
                var data = worksheetData.Skip(pointer).Take(100);
                //var schoolsToInsert = GetSchoolsToInsert(data);
                var schoolsToInsert = data.ToList();

                if (!schoolsToInsert?.Any() ?? true)
                    continue;

                var isDataUpdated = InsertMissingUtilData(districts, schoolCategories, schoolTypes, schoolsToInsert);

                if (isDataUpdated)
                {
                    var result = GetUtilData();
                    districts = result.districts;
                    schoolCategories = result.schoolCategories;
                    schoolTypes = result.schoolTypes;
                }

                var dataMapped = schoolsToInsert.Select(x =>
                    new DbSchool()
                    {
                        Address = x.GetValueOrDefault("Address") ?? "",
                        City = x.GetValueOrDefault("City") ?? "",
                        Code = x.GetValueOrDefault("School Code") ?? "",
                        DistrictNumber = int.Parse(x.GetValueOrDefault("District Number") ?? "0"),
                        Fax = x.GetValueOrDefault("Fax") ?? "",
                        GradeRange = x.GetValueOrDefault("Grade Range") ?? "",
                        Latitude = double.Parse(x.GetValueOrDefault("Latitude") ?? "0"),
                        Longitude = double.Parse(x.GetValueOrDefault("Longitude") ?? "0"),
                        Name = x.GetValueOrDefault("School Name") ?? "",
                        Phone = x.GetValueOrDefault("Phone") ?? "",
                        PostalCode = x.GetValueOrDefault("Postal Code") ?? "",
                        Province = x.GetValueOrDefault("Province") ?? "",
                        SchoolCategoryId = schoolCategories.First(cat => cat.Description == (x.GetValueOrDefault("School Category") ?? "")).Id,
                        SchoolTypeId = schoolTypes.First(type => type.Description == (x.GetValueOrDefault("Type") ?? "")).Id,
                    });

                _dbContext.School.AddRange(dataMapped);
                _dbContext.SaveChanges();
            }
        }

        private IEnumerable<Dictionary<string, string>> GetSchoolsToInsert(IEnumerable<Dictionary<string, string>> data)
        {
            var schoolsCodesToInsert = data.Select(x => x["School Code"]).Except(_dbContext.School.Select(entity => entity.Code));
            return data.Where(x => schoolsCodesToInsert.Any(y => y == x["School Code"]));
        }

        private bool InsertMissingUtilData(List<DbDistrict> districts, List<DbSchoolCategory> schoolCategories, List<DbSchoolType> schoolTypes, List<Dictionary<string, string>> dataToInsert)
        {
            bool dataUpdated = false;
            var districtsNotFound = dataToInsert.Select(x => new { DistrictNumber = int.Parse(x["District Number"]), DistrictName = x["District Name"] }).Where(dist => districts.All(y => y.Number != dist.DistrictNumber));
            if (districtsNotFound.Any())
            {
                _dbContext.District.AddRange(districtsNotFound.Select(x => new DbDistrict() { Number = x.DistrictNumber, Name = x.DistrictName }));
                dataUpdated = true;
            }

            var schoolCategoriesNotFound = dataToInsert.Select(x => x["School Category"]).Where(cat => schoolCategories.All(y => y.Description != cat));
            if (schoolCategoriesNotFound.Any())
            {
                _dbContext.SchoolCategory.AddRange(schoolCategoriesNotFound.Select(category => new DbSchoolCategory() { Description = category }));
                dataUpdated = true;
            }

            var schoolTypesNotFound = dataToInsert.Select(x => x["Type"]).Where(type => schoolTypes.All(y => y.Description != type));
            if (schoolTypesNotFound.Any())
            {
                _dbContext.SchoolType.AddRange(schoolTypesNotFound.Select(type => new DbSchoolType() { Description = type }));
                dataUpdated = true;
            }

            _dbContext.SaveChanges();

            var modifiedData = dataToInsert.Select(x =>
            {
                var coordinate = _addressService.GetAddressCoordinate($"{x["Address"]}, {x["City"]}").Result;
                x.Add("Latitude", coordinate.Latitude.ToString());
                x.Add("Longitude", coordinate.Longitude.ToString());
                return x;
            }).ToList();

            dataToInsert.Clear();
            dataToInsert.AddRange(modifiedData);

            return dataUpdated;
        }

        private (List<DbDistrict> districts, List<DbSchoolCategory> schoolCategories, List<DbSchoolType> schoolTypes) GetUtilData()
        {
            var districts = _dbContext.District.ToList();
            var schoolCategories = _dbContext.SchoolCategory.ToList();
            var schoolTypes = _dbContext.SchoolType.ToList();

            return (districts, schoolCategories, schoolTypes);
        }

        private async Task<IEnumerable<Dictionary<string, string>>> GetWorksheetData(IFormFile file)
        {
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream).ConfigureAwait(false);
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(memoryStream))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension?.Rows;
                    var colCount = worksheet.Dimension?.Columns;

                    var sb = new StringBuilder();
                    for (int row = 1; row <= rowCount.Value; row++)
                    {
                        for (int col = 1; col <= colCount.Value; col++)
                        {
                            sb.AppendFormat("{0}\t", worksheet.Cells[row, col].Value);
                        }
                        sb.Append(Environment.NewLine);
                    }

                    var arr = sb.ToString().Trim().Split("\r\n").Select(row =>
                    {
                        return row.Split("\t");
                    });

                    var headers = arr.First();

                    return arr.Skip(1)
                              .Select(row => headers.Zip(row, (header, cell) => new { Header = header, Cell = cell })
                              .ToDictionary(pair => pair.Header, pair => pair.Cell.Trim()));
                }
            }
        }
    }
}

