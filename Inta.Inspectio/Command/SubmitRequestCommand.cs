using Inta.Inspectio.Enums;
namespace Inta.Inspectio.Command
{
    public class SubmitRequestCommand
    {
        public string CompanyName { get; set; }
        public string CompanyCountry { get; set; }
        public string CompanyCode { get; set; }
        public string Description { get; set; }
        public RequestType RequestType { get; set; }

    }
}
