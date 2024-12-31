// Helpers/ClientMessage.cs
namespace try_to_build_client.Helpers
{
    public class ClientMessage
    {
        public string Username { get; set; }
        public string SessionId { get; set; }
        public string UserGuess { get; set; }
        public int ClientPort { get; set; }
    }
}