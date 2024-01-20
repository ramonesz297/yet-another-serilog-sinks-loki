namespace Serilog.Sinks.Loki
{
    /// <summary>
    /// Class contains basic authentication credentials
    /// like user name and password
    /// </summary>
    public class LokiCredentials
    {
        /// <summary>
        /// Creates new instance of LokiCredentials
        /// used for basic authentication
        /// </summary>
        /// <param name="userName">Username. Required</param>
        /// <param name="password">Password. Optional</param>
        public LokiCredentials(string userName, string? password = null)
        {
            Username = userName;
            Password = password;
        }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; }


        /// <summary>
        /// Password, optional
        /// </summary>
        public string? Password { get; }
    }
}
