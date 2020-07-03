namespace PetFinderAlerts.Library.Models
{
    public class PetFinderAuthorization
    {
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string access_token { get; set; }
    }
}