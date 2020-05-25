namespace Authenticator.Models
{
    public class TokenResponse
    {
        public string token { get; set; }
    }

    public class TokenRequest
    {
        public User user { get; set; }
        public Settings settings { get; set; }
    }

    public class User
    {
        public Id id { get; set; }
    }

    public class Id
    {
        public string value { get; set; }
    }

    public class Settings
    {
        public long project_id { get; set; }
    }
}
