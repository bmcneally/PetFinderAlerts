namespace PetFinderAlerts.Library.Models
{
    public class AnimalSearchParameters
    {
        public string Type { get; set; }
        public string Location { get; set; }
        public int? Distance { get; set; }
    }
}