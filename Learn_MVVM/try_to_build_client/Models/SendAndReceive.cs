// Models/SendAndReceive.cs
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using try_to_build_client.Helpers;

namespace try_to_build_client.Models
{
  internal class SendAndReceive
    {
      private NetworkStream netStream;

       // get the stream from outside
        public SendAndReceive(NetworkStream stream)
        {
            netStream = stream;
        }

       // method for send message to server
        public async Task<bool> SendAsync(string message, int headerCode)
       {
            try
            {
                byte[] data = message == null ? Array.Empty<byte>() : Encoding.ASCII.GetBytes(message);
                Header header = new Header
                {
                    code = (byte)headerCode,
                    length = data.Length
               };

               await netStream.WriteAsync(header.GetBytes(), 0, header.GetLength());
                await netStream.WriteAsync(data, 0, data.Length);
                return true;
            }
            catch (IOException e)
            {
               Console.WriteLine("Message send failed: {0}", message);
               Console.Error.WriteLine(e.Message);
                return false;
            }
        }

      // method for receive message from server
        public async Task<ReceiveResult> ReceiveAsync()
        {
            Header header = new Header();
            try
            {
                byte[] headerData = new byte[header.GetLength()];
                if (await netStream.ReadAsync(headerData, 0, headerData.Length) != headerData.Length)
                {
                   Console.Error.WriteLine("Header invalid");
                   return new ReceiveResult { Success = false, HeaderCode = -1, Message = null};
                }

                header.code = headerData[0];
                int headerCode = header.code;
                
                header.length = BitConverter.ToInt32(headerData, 1);
                // if no message sent, just return the header code
                if (header.length == 0)
                    return new ReceiveResult { Success = true, HeaderCode = headerCode, Message = string.Empty };
                // if there's message, pare message
                byte[] messageData = new byte[header.length];
                
                int count;
                count = await netStream.ReadAsync(messageData, 0, messageData.Length);
                // The Encoding.ASCII.GetBytes method expects a non-null string as input. If message is null ,
                // it throws an ArgumentNullException, that's why we check (header.length == 0) first
                string message = Encoding.ASCII.GetString(messageData, 0, count);
                return new ReceiveResult { Success = true, HeaderCode = headerCode, Message = message };

            }
           catch (IOException)
            {
                Console.WriteLine("Client disconnected unexpectedly");
               return new ReceiveResult { Success = false, HeaderCode = -1, Message = null };
            }
        }
   }
}