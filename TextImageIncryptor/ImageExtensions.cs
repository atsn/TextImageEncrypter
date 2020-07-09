using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace TextImageEncryptor
{
    public static class Extensions
    {
        public static byte[] ToByteArray(this Image image, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                return ms.ToArray();
            }
        }

        public static byte[] GetASBytes(this int x)
        {
            return BitConverter.GetBytes(x);
        }

        public static int GetAsInt(this byte[] bytes)
        {
            return BitConverter.ToInt32(bytes);
        }

        public static BitArray GetAsBitArray(this byte[] bytes)
        {
           return new BitArray(bytes);
        }

        public static byte[] GetAsByteArray(this BitArray bits)
        {
            var tempBytes = new byte[(bits.Length / 8)];
            bits.CopyTo(tempBytes, 0);
            return tempBytes;
        }

        public static BitArray GetAsUTF8BitArray(this string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);

            return new BitArray(bytes);
        }

        public static BitArray PrePendBytes(this BitArray bits, byte[] bytes)
        {
            var tempBytes = new byte[(bits.Length / 8) + bytes.Length];
            bytes.CopyTo(tempBytes, 0);
            bits.CopyTo(tempBytes, bytes.Length);
            return new BitArray(tempBytes);
        }

        public static bool GetFirstBit(this byte pixelColor)
        {
            BitArray bitarray = new BitArray(new byte[] {pixelColor});
            return bitarray[0];
        }

        public static string GetAsUTF8String(this BitArray bits)
        {
            return Encoding.UTF8.GetString(bits.GetAsByteArray());
        }
    }

    public static class StringExtensions
    {
        
    }
}