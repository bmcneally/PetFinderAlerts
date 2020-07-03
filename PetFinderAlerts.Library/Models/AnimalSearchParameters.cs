namespace PetFinderAlerts.Library.Models
{
    public class AnimalSearchParameters
    {
        public string Type { get; set; }
        public string Breed { get; set; }
        public string Size { get; set; }
        public string Gender { get; set; }
        public string Age { get; set; }
        public string Color { get; set; }
        public string Coat { get; set; }
        public string Status { get; set; }
        public string Name { get; set; }
        public string Organization { get; set; }
        public bool? GoodWithChildren { get; set; }
        public bool? GoodWithDogs { get; set; }
        public bool? GoodWithCats { get; set; }
        public string Location { get; set; }
        public int? Distance { get; set; }
        public string Before { get; set; }
        public string After { get; set; }
        public string Sort { get; set; }
    }
}