

// Helpers/ServerMessage.cs
namespace server.Helper
{
    public class ServerMessage
    {
        public string SessionId { get; set; }
        public string CharacterString { get; set; }
        public int TotalWords { get; set; }
        public int WordsFound { get; set; }
    }
}