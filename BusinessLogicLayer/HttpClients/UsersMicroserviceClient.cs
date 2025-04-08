using BusinessLogicLayer.DTO;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;
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
        private readonly IDistributedCache _distributedCache;

        public UsersMicroserviceClient(HttpClient httpClient, ILogger<UsersMicroserviceClient> logger, IDistributedCache distributedCache)
        {
            _httpClient = httpClient;
            _logger = logger;
            _distributedCache = distributedCache;
        }

        public async Task<UserDTO?> GetUserByUserID(Guid userID)
        {
            try
            {
                // Check if the user is in the cache
                string cacheKeyToRead = $"user:{userID}";
                string? cachedUser = await _distributedCache.GetStringAsync(cacheKeyToRead);

                if(cachedUser != null)
                {
                    UserDTO? userFromCache = JsonSerializer.Deserialize<UserDTO>(cachedUser);
                    return userFromCache;
                }

                HttpResponseMessage response = await _httpClient.GetAsync($"/api/users/{userID}");

                if (!response.IsSuccessStatusCode)
                {

                    if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                    {
                        UserDTO? userFromFallback = await response.Content.ReadFromJsonAsync<UserDTO>();
                        if (userFromFallback == null)
                        {
                            throw new NotImplementedException("Fallback policy was not implemented");
                        }
                        return userFromFallback;
                    }

                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
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

                // Check if the user is in the cache

                string cacheKey = $"user:{userID}";
                string userJson = JsonSerializer.Serialize(user);
                DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(DateTimeOffset.UtcNow.AddMinutes(5))
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(3));
                await _distributedCache.SetStringAsync(cacheKey, userJson, options);
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

            catch (TimeoutRejectedException ex)
            {
                _logger.LogError(ex, "Timeout occurred while fetching user data. Returning dummy data");

                return new UserDTO(
                        PersonName: "Temporarily Unavailable (timeout)",
                        Email: "Temporarily Unavailable (timeout)",
                        Gender: "Temporarily Unavailable (timeout)",
                        UserID: Guid.Empty);
            }
        }
            
    }
}
