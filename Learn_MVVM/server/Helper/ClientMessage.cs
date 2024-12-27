// server/Helpers/ClientMessage.cs

namespace server.Helper
{
    public class ClientMessage
    {
        public string SessionId { get; set; }
        public int ClientPort { get; set; }
        public string UserGuess { get; set; }
        public string Username { get; set; }       
        
        
    }
}
