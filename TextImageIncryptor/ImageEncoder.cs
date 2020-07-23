using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using TextImageEncrypter.Exceptions;
using TextImageIncryptor;

namespace TextImageEncryptor
{
    class ImageEncoder
    {
        private Bitmap Image { get; set; }
        private List<int> lengthPositions = new List<int>();
        private List<int> positions = new List<int>();
        private int currentPosition;
        private byte[] pixelColors ;
        private StringBuilder allText;
        private EncoderDecoderHelper helper;

        private void GetLengthPositions()
        {
            for (int i = 0; i < 32; i++)
            {
                lengthPositions.Add(currentPosition);
                helper.SetNextCurrentPosition(ref currentPosition, positions);
            }
        }

        public void AppedLineAndEncryptString(string textToBeEncrypted)
        {
            textToBeEncrypted += "\n";
            AppedAndEncryptString(textToBeEncrypted);
        }

        public void AppedAndEncryptString(string textToBeEncrypted)
        {
            var temp = (allText + textToBeEncrypted).GetAsUTF8BitArray().Length;
            if (temp >= ((Image.Width * Image.Height * 3) -32))
            {
                throw new NoMoreSpaceInImageException($"The image only have space for {((Image.Width * Image.Height * 3)/8)-4} bytes, But the total length of the text you want to encode is {(allText.ToString() + textToBeEncrypted).GetAsUTF8BitArray().Length/8}");
            }

            allText.Append(textToBeEncrypted);
            

            var bits = textToBeEncrypted.GetAsUTF8BitArray();

            Loading.StartLoading("Encrypting Image",200,0);
            for (var i = 0; i < bits.Count; i++)
            {
                Loading.SetProgress((i*100) /bits.Count);
                bool bit = bits[i];
                pixelColors[currentPosition] = GetAlteredPixelColor(bit, pixelColors[currentPosition]);
                helper.SetNextCurrentPosition(ref currentPosition, positions);
            }
            Loading.StopLoading();
        }

        public ImageEncoder(Bitmap image)
        {
            Initialize(image);
        }

        private void Initialize(Bitmap image)
        {
            helper = new EncoderDecoderHelper();
            allText = new StringBuilder();
            Image = image;
            pixelColors = new byte[image.Height * image.Width * 3];
            helper.FillPositionArray(image, positions);
            helper.SetNextCurrentPosition(ref currentPosition, positions);
            helper.FillPixelColorsArray(image, pixelColors);
            GetLengthPositions();

        }


        public void Reset(Bitmap image)
        {
            Initialize(image);
        }

        public Bitmap GetCurrentImage()
        {
            EncryptTextLength();

            int i = 0;
            for (int x = 0; x < Image.Width; x++)
            {
                for (int y = 0; y < Image.Height; y++)
                {
                    var pixel = Image.GetPixel(x, y);
                    Image.SetPixel(x,y ,Color.FromArgb(pixel.A, pixelColors[i++], pixelColors[i++], pixelColors[i++]));
                }
            }

            return Image;
        }

        private void EncryptTextLength()
        {
            var lengthArray = allText.ToString().GetAsUTF8BitArray().Length.GetASBytes().GetAsBitArray();
            int i = 0;

            foreach (int lenthPosition in lengthPositions)
            {
                pixelColors[lenthPosition] = GetAlteredPixelColor(lengthArray[i++], pixelColors[lenthPosition]);
            }
           
        }



        private static byte GetAlteredPixelColor(bool textBit, byte pixelColor)
        {
            BitArray bitarray = new BitArray(new byte[] { pixelColor });
            bitarray[0] = textBit;

            byte[] changedColor = new byte[1];
            bitarray.CopyTo(changedColor, 0);
            return changedColor[0];

        }

    }
}
