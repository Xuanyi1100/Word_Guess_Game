// Helpers/ServerMessage.cs
namespace try_to_build_client.Helpers
{
    public class ServerMessage
    {
        public string SessionId { get; set; }
        public string CharacterString { get; set; }
        public int TotalWords { get; set; }
        public int WordsFound { get; set; }
    }
}