using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace PetFinderAlerts.Shell
{
    class Program
    {
        static HttpClient client = new HttpClient();

        static void Main(string[] args)
        {
            try
            {
                var authorization = GetAuthorization();

                // todo: get search parameters dynamically from UI, config, or a file
                var searchParameters = new AnimalSearchParameters
                {
                    Type = "dog",
                    Location = "55806",
                    Distance = 200
                };

                var matchingAnimals = GetMatchingAnimals(authorization.access_token, searchParameters);
                ;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.ReadLine();
        }

        static PetFinderAuthorization GetAuthorization()
        {
            string clientId = "wBQyBvGi1nFj8b6KjN6mkXR0UDpGGprqv3rudsPVkBDqjh45EU"; // todo: move to config
            string clientSecret = "E57QB3uj2WcxAp9A1HSf9yTnH33PIhw6loh0GtpK"; // todo: move to config

            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", $"grant_type=client_credentials&client_id={clientId}&client_secret={clientSecret}", ParameterType.RequestBody);

            var client = new RestClient("https://api.petfinder.com/v2/oauth2/token");
            IRestResponse response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var token = JsonConvert.DeserializeObject<PetFinderAuthorization>(response.Content);
                return token;
            }
            else
            {
                throw new Exception($"response.StatusCode was not OK ({response.StatusCode})");
            }
        }

        static List<Animal> GetMatchingAnimals(string accessToken, AnimalSearchParameters searchParameters)
        {
            var animals = new List<Animal>();

            // Create the initial request, passing in the desired search parameters.
            var request = GetNewRequest(accessToken, searchParameters);

            var client = new RestClient("https://api.petfinder.com/v2/animals");
            IRestResponse response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var results = JsonConvert.DeserializeObject<Animals>(response.Content);
                animals.AddRange(results.animals);

                // Since the API limits results to 100, continue calling the API until we don't have a "next" page.
                while (!String.IsNullOrWhiteSpace(results.pagination?.links?.next?.href))
                {
                    // Update the client to point to the next page of data.
                    client.BaseUrl = new Uri("https://api.petfinder.com" + results.pagination.links.next.href);

                    // Create a new request, but don't pass in any search parameters since those are included via pagination.
                    request = GetNewRequest(accessToken, null);

                    response = client.Execute(request);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        results = JsonConvert.DeserializeObject<Animals>(response.Content);
                        animals.AddRange(results.animals);
                    }
                    else
                    {
                        throw new Exception($"response.StatusCode was not OK ({response.StatusCode})");
                    }
                }
            }
            else
            {
                throw new Exception($"response.StatusCode was not OK ({response.StatusCode})");
            }

            return animals;
        }

        private static RestRequest GetNewRequest(string accessToken, AnimalSearchParameters searchParameters)
        {
            var request = new RestRequest(Method.GET);

            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddHeader("authorization", $"Bearer {accessToken}");

            if (searchParameters != null)
            {
                var requestParameters = GetRequestParameters(searchParameters);
                request.Parameters.AddRange(requestParameters);
            }

            return request;
        }

        private static IEnumerable<Parameter> GetRequestParameters(AnimalSearchParameters searchParameters)
        {
            // Always return the maximum number of records.
            yield return new Parameter("limit", "100", ParameterType.QueryString);

            if (!String.IsNullOrWhiteSpace(searchParameters.Type))
            {
                yield return new Parameter("type", searchParameters.Type, ParameterType.QueryString);
            }

            if (!String.IsNullOrWhiteSpace(searchParameters.Location))
            {
                yield return new Parameter("location", searchParameters.Location, ParameterType.QueryString);
            }

            if (searchParameters.Distance.HasValue)
            {
                yield return new Parameter("distance", searchParameters.Distance.Value, ParameterType.QueryString);
            }
        }
    }

    public class PetFinderAuthorization
    {
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string access_token { get; set; }
    }

    public class Animals
    {
        public List<Animal> animals { get; set; }
        public PaginationDetails pagination { get; set; }
    }

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

    public class BreedDetails
    {
        public string primary { get; set; }
        public string secondary { get; set; }
        public bool? mixed { get; set; }
        public bool? unknown { get; set; }
    }

    public class ColorDetails
    {
        public string primary { get; set; }
        public string secondary { get; set; }
        public string tertiary { get; set; }
    }

    public class AttributeDetails
    {
        public bool? spayed_neutered { get; set; }
        public bool? house_trained { get; set; }
        public bool? declawed { get; set; }
        public bool? special_needs { get; set; }
        public bool? shots_current { get; set; }
    }

    public class EnvironmentDetails
    {
        public bool? children { get; set; }
        public bool? dogs { get; set; }
        public bool? cats { get; set; }
    }

    public class PhotoUrls
    {
        public string small { get; set; }
        public string medium { get; set; }
        public string large { get; set; }
        public string full { get; set; }
    }

    public class ContactDetails
    {
        public string email { get; set; }
        public string phone { get; set; }
        public AddressDetails address { get; set; }
    }

    public class AddressDetails
    {
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string postcode { get; set; }
        public string country { get; set; }
    }

    public class PaginationDetails
    {
        public int count_per_page { get; set; }
        public int total_count { get; set; }
        public int current_page { get; set; }
        public int total_pages { get; set; }
        [JsonProperty("_links")] public PaginationLinks links { get; set; }
    }

    public class PaginationLinks
    {
        public PaginationLink next { get; set; }
    }

    public class PaginationLink
    {
        public string href { get; set; }
    }

    public class AnimalSearchParameters
    {
        public string Type { get; set; }
        public string Location { get; set; }
        public int? Distance { get; set; }
    }
}