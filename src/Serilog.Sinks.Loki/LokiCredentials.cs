namespace Serilog.Sinks.Loki
{
    public class LokiCredentials
    {
        public LokiCredentials(string userName, string? password = null)
        {
            Username = userName;
            Password = password;
        }
        public string Username { get; }
        public string? Password { get; }
    }
}
