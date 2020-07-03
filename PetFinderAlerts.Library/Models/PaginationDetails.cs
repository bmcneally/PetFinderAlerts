using Newtonsoft.Json;

namespace PetFinderAlerts.Library.Models
{
    public class PaginationDetails
    {
        public int count_per_page { get; set; }
        public int total_count { get; set; }
        public int current_page { get; set; }
        public int total_pages { get; set; }
        [JsonProperty("_links")] public PaginationLinks links { get; set; }
    }
}