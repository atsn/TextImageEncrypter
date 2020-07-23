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
using TextImageEncrypter.Exceptions;
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
            Bitmap img = new Bitmap(@"..\..\..\TestData\TestImageEncoded.jpg");

            ImageDecoder decoder = new ImageDecoder(img);

            Console.WriteLine(decoder.GetTextFromImage());
        }

        private static void WriteToImage()
        {
            Bitmap img = new Bitmap(@"..\..\..\TestData\TestImage.jpg");

            ImageEncoder encoder = new ImageEncoder(img);

            encoder.AppedLineAndEncryptString("Hello World");

            try
            {
                for (int i = 0; i < 10000000; i++)
                {
                    encoder.AppedLineAndEncryptString(Statics.text);
                }
            }
            catch (NoMoreSpaceInImageException e)
            {
                try
                {
                    for (int i = 0; i < 10000000; i++)
                    {
                        encoder.AppedAndEncryptString("e");
                    }

                }
                catch (NoMoreSpaceInImageException)
                {

                }
            }

            var finalImage = encoder.GetCurrentImage();

            File.WriteAllBytes(@"..\..\..\TestData\TestImageEncoded.jpg", finalImage.ToByteArray(ImageFormat.Png));
        }
    }
}
