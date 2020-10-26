using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace TextImageEncrypter
{
    class ImageDecoder
    {
        private Bitmap Image { get; set; }
        private byte[] pixelColors;
        private List<int> positions = new List<int>();
        private int currentPosition;
        private EncoderDecoderHelper helper;

        public string GetTextFromImage()
        {
            BitArray lengthArray = new BitArray(32);
            for (int i = 0; i < 32; i++)
            {
                lengthArray[i] = pixelColors[currentPosition].GetFirstBit();
                helper.SetNextCurrentPosition(ref currentPosition, positions);
            }

            var length = lengthArray.GetAsByteArray().GetAsInt();
            BitArray text = new BitArray(length);
            for (int i = 0; i < length; i++)
            {
                text[i] = pixelColors[currentPosition].GetFirstBit();
                helper.SetNextCurrentPosition(ref currentPosition, positions);
            }

            return Encryptor.Decrypt(Statics.passwordHash, text.GetAsByteArray());
        }

       

        public ImageDecoder(Bitmap image)
        {
            Initialize(image);
        }

        private void Initialize(Bitmap image)
        {
            helper = new EncoderDecoderHelper();
            Image = image;
            pixelColors = new byte[image.Height * image.Width * 3];
            helper.FillPositionArray(image, positions);
            helper.SetNextCurrentPosition(ref currentPosition, positions);
            helper.FillPixelColorsArray(image, pixelColors);
        }

        public void Reset(Bitmap image)
        {
            Initialize(image);
        }
    }
}
