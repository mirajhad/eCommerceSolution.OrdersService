
using Polly;

namespace BusinessLogicLayer.Policies
{
    public interface IUsersMicroservicePolicies
    {
        IAsyncPolicy<HttpResponseMessage> GetRetryPolicy();
    }
}
