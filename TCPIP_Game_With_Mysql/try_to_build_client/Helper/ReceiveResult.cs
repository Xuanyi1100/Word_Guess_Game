// Helpers/ReceiveResult.cs
namespace try_to_build_client.Helpers
{
    public class ReceiveResult
    {
        public int HeaderCode { get; set; }
        public ServerMessage ServerMessage { get; set; }
        
    }
}