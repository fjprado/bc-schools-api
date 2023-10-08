namespace bc_schools_api.Infra.Interfaces
{
    public interface ISettings
    {
        public string UrlRoutesApi { get; set; }
        public string UrlSuggestAddressApi { get; set; }
        public string UrlLocationsApi { get; set; }
        public string LocalizationApiKey { get; set; }
        public double DefaultLatitude { get; set; }
        public double DefaultLongitude { get; set; }
        public string SchoolDbConnectionString { get; set; }
    }
}
