using KeyRoomSystem.Models;

namespace KeyRoomSystem.Models
{
    public class ReportViewModel
    {
        public string ReportType { get; set; } =
            "not-returned-today";

        public string ReportTitle { get; set; } =
            "Details of Keys Not Returned for Today";

        public DateTime StartDate { get; set; } =
            DateTime.Today;

        public DateTime EndDate { get; set; } =
            DateTime.Today;

        public int TotalRecords { get; set; }

        public List<KeyTransaction> Transactions { get; set; } =
            new List<KeyTransaction>();
    }
}