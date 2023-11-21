using System.Collections.Generic;

namespace BDO.Bot.BDOSkillBot.Objects
{
    public class TokenStorage
    {
        public string Token { get; set; }
        public Dictionary<string, string> AccessTokens { get; set; } = new Dictionary<string, string>();
    }
}
