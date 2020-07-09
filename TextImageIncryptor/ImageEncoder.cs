using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
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

        private void GetLenthPositions()
        {
            for (int i = 0; i < 32; i++)
            {
                lengthPositions.Add(currentPosition);
                EncoderDecoderHelper.SetNextCurrentPosition(ref currentPosition, positions);
            }
        }

        public void AppedLineAndEncryptString(string textToBeEncrypted)
        {
            textToBeEncrypted += "\n";
            AppedAndEncryptString(textToBeEncrypted);
        }

        public void AppedAndEncryptString(string textToBeEncrypted)
        {
            allText.Append(textToBeEncrypted);
            var bits = textToBeEncrypted.GetAsUTF8BitArray();

            foreach (bool bit in bits)
            {
                pixelColors[currentPosition] = GetAlteredPixelColor(bit, pixelColors[currentPosition]);
                EncoderDecoderHelper.SetNextCurrentPosition(ref currentPosition, positions);
            }
        }

        public ImageEncoder(Bitmap image)
        {
            Initialize(image);
        }

        private void Initialize(Bitmap image)
        {
            allText = new StringBuilder();
            Image = image;
            pixelColors = new byte[image.Height * image.Width * 3];
            EncoderDecoderHelper.FillPositionArray(image, positions);
            EncoderDecoderHelper.SetNextCurrentPosition(ref currentPosition, positions);
            EncoderDecoderHelper.FillPixelColorsArray(image, pixelColors);
            GetLenthPositions();

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
                EncoderDecoderHelper.SetNextCurrentPosition(ref currentPosition, positions);
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
