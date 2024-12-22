// Helpers/ReceiveResult.cs
namespace try_to_build_client.Helpers
{
    public class ReceiveResult
    {
        public string Message { get; set; }
        public int HeaderCode { get; set; }
        public bool Success { get; set; }
    }
}