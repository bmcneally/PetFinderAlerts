using Newtonsoft.Json;
using System;

namespace PetFinderAlerts.Library.Models
{
    public class PetFinderAuthorization
    {
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string access_token { get; set; }

        [JsonIgnore]
        public DateTime? expireDateTime { get; set; }

        public bool IsExpired
        {
            get
            {
                return (!expireDateTime.HasValue || expireDateTime.Value < DateTime.Now);
            }
        }
    }
}