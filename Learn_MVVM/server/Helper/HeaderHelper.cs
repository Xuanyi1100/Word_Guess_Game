
// Helpers/NetworkHelper.cs
using System;
using System.Runtime.InteropServices;

namespace server.Helper
{
    // structure the message
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        public byte code;
        public int length; //the length of the message
    }

    // make header content flag number and length of message
    public static class HeaderExtension
    {
        // Converts the Header struct to a byte array for sending over the network
        public static byte[] GetBytes(this Header header)
        {
            int size = Marshal.SizeOf(header);
            byte[] data = new byte[size];
            IntPtr pointer = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(header, pointer, true);
            Marshal.Copy(pointer, data, 0, size);
            Marshal.FreeHGlobal(pointer);
            return data;
        }

        // Calculates the size (in bytes) of the Header
        public static int GetLength(this Header header)
        {
            return Marshal.SizeOf(header);
        }
    }

    public struct Message
    {
        public string time;
        public string content;
    }
}