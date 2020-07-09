using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using TextImageEncryptor;

namespace TextImageIncryptor
{
    class ImageDecoder
    {
        private Bitmap Image { get; set; }
        private byte[] pixelColors;
        private List<int> positions = new List<int>();
        private int currentPosition;

        public string GetTextFromImage()
        {
            BitArray lengthArray = new BitArray(32);
            for (int i = 0; i < 32; i++)
            {
                lengthArray[i] = pixelColors[currentPosition].GetFirstBit();
                EncoderDecoderHelper.SetNextCurrentPosition(ref currentPosition, positions);
            }

            var length = lengthArray.GetAsByteArray().GetAsInt();
            BitArray text = new BitArray(length);
            for (int i = 0; i < length; i++)
            {
                text[i] = pixelColors[currentPosition].GetFirstBit();
                EncoderDecoderHelper.SetNextCurrentPosition(ref currentPosition, positions);
            }

            return text.GetAsUTF8String();
        }

       

        public ImageDecoder(Bitmap image)
        {
            Initialize(image);
        }

        private void Initialize(Bitmap image)
        {
            Image = image;
            pixelColors = new byte[image.Height * image.Width * 3];
            EncoderDecoderHelper.FillPositionArray(image, positions);
            EncoderDecoderHelper.SetNextCurrentPosition(ref currentPosition, positions);
            EncoderDecoderHelper.FillPixelColorsArray(image, pixelColors);
        }

        public void Reset(Bitmap image)
        {
            Initialize(image);
        }
    }
}
