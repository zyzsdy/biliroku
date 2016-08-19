using System;
using System.Linq;
using System.Net.Sockets;

namespace BiliRoku.Commentlib
{
    public static class Utils
    {
        public static byte[] ToBigEndian(this byte[] b)
        {
            return BitConverter.IsLittleEndian ? b.Reverse().ToArray() : b;
        }

        public static void ReadB(this NetworkStream stream, byte[] buffer, int offset, int count)
        {
            if (offset + count > buffer.Length)
                throw new ArgumentException();
            var read = 0;
            while (read < count)
            {
                var available = stream.Read(buffer, offset, count - read);
                if (available == 0)
                {
                    throw new ObjectDisposedException(null);
                }
                read += available;
                offset += available;
            }
        }
    }
}
