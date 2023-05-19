using CountriesWPF.Modelos;
using Newtonsoft.Json;
using Syncfusion.Windows.Shared;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CountriesWPF.Modelos
{
    public class ApiService
    {
        private static readonly HttpClient _client = new HttpClient();
        /// <summary>
        /// Retrieves a list of countries from the specified API endpoint.
        /// </summary>
        /// <param name="urlBase">The base URL of the API.</param>
        /// <param name="controller">The controller or endpoint to fetch countries.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the response with a list of countries.</returns>
        public async Task<Response> GetCountriesAsync(string urlBase, string controller)
        {
            try
            {
                _client.BaseAddress = new Uri(urlBase);

                HttpResponseMessage response = await _client.GetAsync(controller).ConfigureAwait(false);
                string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = result,
                    };
                }

                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore,
                };

                List<Country> countries = JsonConvert.DeserializeObject<List<Country>>(result, settings);

                return new Response
                {
                    IsSuccess = true,
                    Result = countries
                };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message,
                };
            }
        }
    }
}
