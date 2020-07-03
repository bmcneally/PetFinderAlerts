using System.Collections.Generic;

namespace PetFinderAlerts.Library.Models
{
    public class Animals
    {
        public List<Animal> animals { get; set; }
        public PaginationDetails pagination { get; set; }
    }
}