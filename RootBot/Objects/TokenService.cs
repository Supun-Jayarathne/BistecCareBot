using System.Threading.Tasks;

namespace Microsoft.BotBuilderSamples.RootBot.Objects
{
    public interface ITokenService
    {
        Task<string> GetToken();
        void SetToken(string token);
    }

    public class TokenService : ITokenService
    {
        private string _token;

        public async Task<string>  GetToken() => _token;
        public void SetToken(string token) => _token = token;
    }

}
