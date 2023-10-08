namespace bc_schools_api.Domain.Models.Response
{
    public class SchoolsResponse
    {
        public Result Result { get; set; }
    }

    public class Result
    {
        public List<SchoolResponse> Records { get; set; }
    }

    public class SchoolResponse
    {
        public string Nome { get; set; }
        public string Telefone { get; set; }
        public string Email { get; set; }
        public string Url_Website { get; set; }
        public string Logradouro { get; set; }
        public int Numero { get; set; }
        public string Bairro { get; set; }
        public string CEP { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal? DistanciaOrigem { get; set; }
    }
}
