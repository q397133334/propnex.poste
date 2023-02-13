using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Propnex.Poster.PropertyGuru.Mobile.Dto
{
    public class Token
    {
        public User User { get; set; }

        public Client Client { get; set; }

        public string accessToken { get; set; }

        public string accessTokenExpiresAt { get; set; }

        public string refreshToken { get; set; }

        public string refreshTokenExpiresAt { get; set; }

        public string scope { get; set; }
    }

    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; }

        public string scope { get; set; }

        public string UserApiId { get; set; }

        public string Umstid { get; set; }

        public int AgentId { get; set; }

        public int UserId { get; set; }
    }

    public class Client
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string ClientId { get; set; }

        public int accessTokenLifetime { get; set; }

        public int refreshTokenLifetime { get; set; }
    }

}
