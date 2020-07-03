using Newtonsoft.Json;
using PetFinderAlerts.Library;
using PetFinderAlerts.Library.Models;
using System;
using System.IO;

namespace PetFinderAlerts.Shell
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // todo: move to config - might be stored in different places depending on the type of shell/UI
                string clientId = "wBQyBvGi1nFj8b6KjN6mkXR0UDpGGprqv3rudsPVkBDqjh45EU";
                string clientSecret = "E57QB3uj2WcxAp9A1HSf9yTnH33PIhw6loh0GtpK";

                // Load the search parameters from an external JSON file.
                string jsonSearchParameters = File.ReadAllText("../../SearchFilters.json");
                var searchParameters = JsonConvert.DeserializeObject<AnimalSearchParameters>(jsonSearchParameters);

                ApiProxy proxy = new ApiProxy(clientId, clientSecret);
                var matchingAnimals = proxy.GetMatchingAnimals(searchParameters);

                foreach (var animal in matchingAnimals)
                {
                    Console.WriteLine($"{animal.name}:\t{animal.url}");
                    Console.WriteLine();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.ReadLine();
        }
    }
}