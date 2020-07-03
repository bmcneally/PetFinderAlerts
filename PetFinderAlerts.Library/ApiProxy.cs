using Newtonsoft.Json;
using PetFinderAlerts.Library.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;

namespace PetFinderAlerts.Library
{
    public class ApiProxy
    {
        private string _clientId;
        private string _clientSecret;
        private PetFinderAuthorization _accessToken = new PetFinderAuthorization();

        public ApiProxy(string clientId, string clientSecret)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        public List<Animal> GetMatchingAnimals(AnimalSearchParameters searchParameters)
        {
            var animals = new List<Animal>();

            // Create the initial request, passing in the desired search parameters.
            var request = GetNewApiRequest(searchParameters);

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
                    request = GetNewApiRequest(null);

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

        private RestRequest GetNewApiRequest(AnimalSearchParameters searchParameters)
        {
            var request = new RestRequest(Method.GET);

            // Only get a new token from the API when we need one.
            if (_accessToken.IsExpired)
            {
                GetAuthorization();
            }

            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddHeader("authorization", $"Bearer {_accessToken.access_token}");

            if (searchParameters != null)
            {
                var requestParameters = GetRequestParameters(searchParameters);
                request.Parameters.AddRange(requestParameters);
            }

            return request;
        }

        private void GetAuthorization()
        {
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", $"grant_type=client_credentials&client_id={_clientId}&client_secret={_clientSecret}", ParameterType.RequestBody);

            var client = new RestClient("https://api.petfinder.com/v2/oauth2/token");
            IRestResponse response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                _accessToken = JsonConvert.DeserializeObject<PetFinderAuthorization>(response.Content);
                _accessToken.expireDateTime = DateTime.Now.AddSeconds(_accessToken.expires_in);
            }
            else
            {
                throw new Exception($"response.StatusCode was not OK ({response.StatusCode})");
            }
        }

        private IEnumerable<Parameter> GetRequestParameters(AnimalSearchParameters searchParameters)
        {
            // Always return the maximum number of records allowed by the API.
            yield return new Parameter("limit", "100", ParameterType.QueryString);

            if (!String.IsNullOrWhiteSpace(searchParameters.Type))
            {
                yield return new Parameter("type", searchParameters.Type, ParameterType.QueryString);
            }

            if (!String.IsNullOrWhiteSpace(searchParameters.Breed))
            {
                yield return new Parameter("breed", searchParameters.Breed, ParameterType.QueryString);
            }

            if (!String.IsNullOrWhiteSpace(searchParameters.Size))
            {
                yield return new Parameter("size", searchParameters.Size, ParameterType.QueryString);
            }

            if (!String.IsNullOrWhiteSpace(searchParameters.Gender))
            {
                yield return new Parameter("gender", searchParameters.Gender, ParameterType.QueryString);
            }

            if (!String.IsNullOrWhiteSpace(searchParameters.Age))
            {
                yield return new Parameter("age", searchParameters.Age, ParameterType.QueryString);
            }

            if (!String.IsNullOrWhiteSpace(searchParameters.Color))
            {
                yield return new Parameter("color", searchParameters.Color, ParameterType.QueryString);
            }

            if (!String.IsNullOrWhiteSpace(searchParameters.Coat))
            {
                yield return new Parameter("coat", searchParameters.Coat, ParameterType.QueryString);
            }

            if (!String.IsNullOrWhiteSpace(searchParameters.Status))
            {
                yield return new Parameter("status", searchParameters.Status, ParameterType.QueryString);
            }

            if (!String.IsNullOrWhiteSpace(searchParameters.Name))
            {
                yield return new Parameter("name", searchParameters.Name, ParameterType.QueryString);
            }

            if (!String.IsNullOrWhiteSpace(searchParameters.Organization))
            {
                yield return new Parameter("organization", searchParameters.Organization, ParameterType.QueryString);
            }

            if (searchParameters.GoodWithChildren.HasValue)
            {
                yield return new Parameter("good_with_children", searchParameters.GoodWithChildren.Value, ParameterType.QueryString);
            }

            if (searchParameters.GoodWithDogs.HasValue)
            {
                yield return new Parameter("good_with_dogs", searchParameters.GoodWithDogs.Value, ParameterType.QueryString);
            }

            if (searchParameters.GoodWithCats.HasValue)
            {
                yield return new Parameter("good_with_cats", searchParameters.GoodWithCats.Value, ParameterType.QueryString);
            }

            if (!String.IsNullOrWhiteSpace(searchParameters.Location))
            {
                yield return new Parameter("location", searchParameters.Location, ParameterType.QueryString);
            }

            if (searchParameters.Distance.HasValue)
            {
                yield return new Parameter("distance", searchParameters.Distance.Value, ParameterType.QueryString);
            }

            if (!String.IsNullOrWhiteSpace(searchParameters.Before))
            {
                yield return new Parameter("before", searchParameters.Before, ParameterType.QueryString);
            }

            if (!String.IsNullOrWhiteSpace(searchParameters.After))
            {
                yield return new Parameter("after", searchParameters.After, ParameterType.QueryString);
            }

            if (!String.IsNullOrWhiteSpace(searchParameters.Sort))
            {
                yield return new Parameter("sort", searchParameters.Sort, ParameterType.QueryString);
            }
        }
    }
}