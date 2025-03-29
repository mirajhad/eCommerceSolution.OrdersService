using BusinessLogicLayer.DTO;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BusinessLogicLayer.HttpClients
{
    public class UsersMicroserviceClient
    {
        private readonly HttpClient _httpClient;

        public UsersMicroserviceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<UserDTO?> GetUserByUserID(Guid userID)
        {
            //Check for null parameter
            if (userID == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(userID));
            }
            //Call the Users Microservice to get the user
            HttpResponseMessage response = await _httpClient.GetAsync($"api/users/{userID}");
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                UserDTO user = JsonSerializer.Deserialize<UserDTO>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return user;
            }
            else
            {
                return null;
            }
        }
    }
}
