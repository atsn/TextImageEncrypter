using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.IO;
using System.Net.Mime;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using TextImageIncryptor;

namespace TextImageEncryptor
{
    class Program
    {
        static void Main(string[] args)
        {
            WriteToImage();

            ReadFromImage();
        }

        private static void ReadFromImage()
        {
            Bitmap img = new Bitmap(@"C:\Users\Peter\Desktop\UdklipEncoded.PNG");

            ImageDecoder decoder = new ImageDecoder(img);

            Console.WriteLine(decoder.GetTextFromImage());
        }

        private static void WriteToImage()
        {
            Bitmap img = new Bitmap(@"C:\Users\Peter\Desktop\Udklip.PNG");

            ImageEncoder encoder = new ImageEncoder(img);

            encoder.AppedLineAndEncryptString("Hello World");

            encoder.AppedLineAndEncryptString("Some Cake");
            encoder.AppedAndEncryptString(Statics.text);

            var finalImage = encoder.GetCurrentImage();

            File.WriteAllBytes(@"C:\Users\Peter\Desktop\UdklipEncoded.PNG", finalImage.ToByteArray(ImageFormat.Png));
        }
    }
}
