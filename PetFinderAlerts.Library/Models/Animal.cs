using System;
using System.Collections.Generic;

namespace PetFinderAlerts.Library.Models
{
    public class Animal
    {
        public long id { get; set; }
        public string organization_id { get; set; }
        public string url { get; set; }
        public string type { get; set; }
        public string species { get; set; }
        public BreedDetails breeds { get; set; }
        public ColorDetails colors { get; set; }
        public string age { get; set; }
        public string gender { get; set; }
        public string size { get; set; }
        public string coat { get; set; }
        public AttributeDetails attributes { get; set; }
        public EnvironmentDetails environment { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string organization_animal_id { get; set; }
        public List<PhotoUrls> photos { get; set; }
        public PhotoUrls primary_photo_cropped { get; set; }
        public string status { get; set; }
        public DateTime status_changed_at { get; set; }
        public DateTime published_at { get; set; }
        public decimal distance { get; set; }
        public ContactDetails contact { get; set; }
    }
}