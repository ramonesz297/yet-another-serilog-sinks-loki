// This file is part of the project licensed under the MIT License.
// See the LICENSE file in the project root for more information.


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
