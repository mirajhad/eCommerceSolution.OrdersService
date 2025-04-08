using BusinessLogicLayer.DTO;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly.Bulkhead;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BusinessLogicLayer.HttpClients
{
    public class ProductsMicroserviceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductsMicroserviceClient> _logger;
        private readonly IDistributedCache _distributedCache;

        public ProductsMicroserviceClient(HttpClient httpClient, ILogger<ProductsMicroserviceClient> logger, IDistributedCache distributedCache)
        {
            _httpClient = httpClient;
            _logger = logger;
            _distributedCache = distributedCache;
        }


        public async Task<ProductDTO?> GetProductByProductID(Guid productID)
        {
            try
            {

                // Check if the product is in the cache
                string cacheKey = $"product:{productID}";
                string? cachedProduct = await _distributedCache.GetStringAsync(cacheKey);

                if (cachedProduct != null)
                {
                    ProductDTO? productFromCache = JsonSerializer.Deserialize<ProductDTO>(cachedProduct);
                    return productFromCache;
                }


                HttpResponseMessage response = await _httpClient.GetAsync($"/api/products/search/product-id/{productID}");

                if (!response.IsSuccessStatusCode)
                {
                    if(response.StatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        ProductDTO? productFromFallback = await response.Content.ReadFromJsonAsync<ProductDTO>();

                        if (productFromFallback == null)
                        {
                            throw new NotImplementedException("Fallback policy was not implemented");
                        }
                        return productFromFallback;
                    }
                    else if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return null;
                    }
                    else if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        throw new HttpRequestException("Bad request", null, HttpStatusCode.BadRequest);
                    }
                    else
                    {
                        throw new HttpRequestException($"Http request failed with status code {response.StatusCode}");
                    }
                }


                ProductDTO? product = await response.Content.ReadFromJsonAsync<ProductDTO>();

                if (product == null)
                {
                    throw new ArgumentException("Invalid Product ID");
                }

                string productJson = JsonSerializer.Serialize(product);
                DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(30))
                    .SetSlidingExpiration(TimeSpan.FromSeconds(10));

                string cacheKeyToWrite = $"product:{productID}";
                await _distributedCache.SetStringAsync(cacheKeyToWrite, productJson, options);

                return product;

            }
            catch (BulkheadRejectedException ex)
            {
                _logger.LogError(ex, "Bulkhead isolation blocks the request since the request queue is full");

                return new ProductDTO(
                  ProductID: Guid.NewGuid(),
                  ProductName: "Temporarily Unavailable (Bulkhead)",
                  Category: "Temporarily Unavailable (Bulkhead)",
                  UnitPrice: 0,
                  QuantityInStock: 0);
            }
        }
    }
}
