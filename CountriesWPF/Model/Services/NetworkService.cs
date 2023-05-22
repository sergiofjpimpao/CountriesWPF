using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CountriesWPF.Modelos.Servicos
{
    /// <summary>
    /// Provides network-related functionality.
    /// </summary>
    public class NetworkService
    {
        /// <summary>
        /// Checks the internet connection by making a request to a Google endpoint.
        /// </summary>
        /// <returns>A response indicating the success of the connection check.</returns>
        public Response CheckConnection()
        {
            var client = new WebClient();
            try
            {
                using (client.OpenRead("http://clients3.google.com/generate_204"))
                {
                    return new Response
                    {
                        IsSuccess = true,
                    };
                }
            }
            catch
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "Configure a sua ligação à internet",
                };
            }
        }
        /// <summary>
        /// Checks the API connection by making a request to the specified endpoint.
        /// </summary>
        /// <param name="apiEndpoint">The URL of the API endpoint to check.</param>
        /// <returns>A response indicating the success of the API connection check.</returns>
        public async Task<Response> CheckApiConnection(string apiEndpoint)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync(apiEndpoint);
                    response.EnsureSuccessStatusCode();

                    return new Response
                    {
                        IsSuccess = true,
                    };
                }
                catch (Exception ex)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = $"Failed to connect to the API: {ex.Message}",
                    };
                }
            }
        }
    }
}
