using BusinessLogicLayer.DTO;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BusinessLogicLayer.HttpClients
{
    public class UsersMicroserviceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UsersMicroserviceClient> _logger;

        public UsersMicroserviceClient(HttpClient httpClient, ILogger<UsersMicroserviceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<UserDTO?> GetUserByUserID(Guid userID)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"/api/users/{userID}");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return null;
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        throw new HttpRequestException("Bad request", null, System.Net.HttpStatusCode.BadRequest);
                    }
                    else
                    {
                        //throw new HttpRequestException($"Http request failed with status code {response.StatusCode}");

                        return new UserDTO(
                          PersonName: "Temporarily Unavailable",
                          Email: "Temporarily Unavailable",
                          Gender: "Temporarily Unavailable",
                          UserID: Guid.Empty);
                    }
                }


                UserDTO? user = await response.Content.ReadFromJsonAsync<UserDTO>();

                if (user == null)
                {
                    throw new ArgumentException("Invalid User ID");
                }

                return user;
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError(ex, "Request failed because of circuit breaker is in Open state. Returning dummy data.");

                return new UserDTO(
                        PersonName: "Temporarily Unavailable",
                        Email: "Temporarily Unavailable",
                        Gender: "Temporarily Unavailable",
                        UserID: Guid.Empty);
            }
        }
            
    }
}
