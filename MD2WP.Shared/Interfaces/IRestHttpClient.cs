using System.Net.Http;
using System.Threading.Tasks;
using MoonspaceLabs.Shared.BaseClasses;

namespace MoonspaceLabs.Shared.Interfaces
{
    public interface IRestHttpClient
    {
        Task<T> GetAsync<T>(AuthenticationBase authentication, string address);

        Task PutAsync(AuthenticationBase authentication, string address, StringContent content);

        Task<T> PutAsync<T>(AuthenticationBase authentication, string address, StringContent content);

        Task<T> PostAsync<T>(AuthenticationBase authentication, string address, T model);

        Task<T> PatchAsync<T, M>(AuthenticationBase authentication, string address, M model);

        Task DeleteAsync(AuthenticationBase authentication, string address);
    }
}