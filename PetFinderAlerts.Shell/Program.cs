using Newtonsoft.Json;
using PetFinderAlerts.Library.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;

namespace PetFinderAlerts.Shell
{
    class Program
    {
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
            // Always return the maximum number of records allowed by the API.
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
}