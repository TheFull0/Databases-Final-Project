using System.Collections.Generic;
using System.Threading.Tasks;
using Structs;

namespace Managers
{
    public static class ServerFunctions
    {
        private const string ServerUrl = "https://yourserver.com/api"; // Replace with your server URL
        private const string LoginToken = "LoginToken";

        public static async Task<IEnumerable<Question>> GetRandomQuestions(int count)
        {
            // await 
            return null; // Implement server call to fetch random questions
        }
        
        public static MatchResult? PingGetMatchResults()
        {
            return null; // Implement server call to fetch average answer time
        }
        
        
    }
}