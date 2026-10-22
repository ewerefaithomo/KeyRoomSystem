namespace KeyRoomSystem.Models
{
    public class SearchViewModel
    {
        // =========================================================
        // Search Text
        // =========================================================

        public string Query { get; set; } =
            string.Empty;


        // =========================================================
        // Search Type
        // =========================================================

        public string SearchType { get; set; } =
            "all";


        // =========================================================
        // Staff Search Results
        // =========================================================

        public List<Staff> StaffResults { get; set; } =
            new List<Staff>();


        // =========================================================
        // Key Search Results
        // =========================================================

        public List<KeyItem> KeyResults { get; set; } =
            new List<KeyItem>();


        // =========================================================
        // Transaction Search Results
        // =========================================================

        public List<KeyTransaction> Results { get; set; } =
            new List<KeyTransaction>();


        // =========================================================
        // Total Results
        // =========================================================

        public int TotalResults { get; set; }
    }
}