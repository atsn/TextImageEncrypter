using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using TextImageEncrypter.Exceptions;

namespace TextImageEncrypter 
{
    class ImageEncoder :IDisposable
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

        public void AppendLineAndEncryptString(string textToBeEncrypted)
        {
            textToBeEncrypted += "\n";
            AppendAndEncryptString(textToBeEncrypted);
        }

    
        public void AppendAndEncryptString(string textToBeEncrypted)
        {
            var temp = Encryptor.Encrypt(Statics.passwordHash,allText + textToBeEncrypted).GetAsUTF8BitArray().Length;

            var size = ((Image.Width * Image.Height * 3) - 32);
            var What = temp > ((Image.Width * Image.Height * 3) - 32);
            if (temp > ((Image.Width * Image.Height * 3) -32))
            {
                throw new NoMoreSpaceInImageException($"The image only have space for {((Image.Width * Image.Height * 3)/8)-4} bytes, But the total length of the text you want to encode is {(allText.ToString() + textToBeEncrypted).GetAsUTF8BitArray().Length/8}");
            }

            allText.Append(textToBeEncrypted);
        }
        
        public int GetSizeLeftInImage()
        {
            var allTextsize = allText.ToString().GetAsUTF8BitArray().Length / 8;

            if (allTextsize % 16 != 0)
            {
                allTextsize = allTextsize + (16 - ((allText.ToString().GetAsUTF8BitArray().Length / 8) % 16));
            }

            var SpaceLeft = (((Image.Width * Image.Height * 3) / 8) - 4) - allTextsize;
           
            return SpaceLeft;
        }

        private void FinalizeImage(string textToBeEncrypted)
        {
            var bits = Encryptor.Encrypt(Statics.passwordHash, textToBeEncrypted).GetAsUTF8BitArray();

            Loading.StartLoading("Saving Pixel Colors", 200, 0);
            for (var i = 0; i < bits.Count; i++)
            {
                Loading.SetProgress((i * 100) / bits.Count);
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

        public void Reset()
        {
            helper = new EncoderDecoderHelper();
            allText = new StringBuilder();
            pixelColors = new byte[Image.Height * Image.Width * 3];
            helper.FillPositionArray(Image, positions);
            helper.SetNextCurrentPosition(ref currentPosition, positions);
            helper.FillPixelColorsArray(Image, pixelColors);
            GetLengthPositions();
        }

        public Bitmap GetImageAndResetEncoder()
        {
            FinalizeImage(allText.ToString());
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
            var lengthArray = Encryptor.Encrypt(Statics.passwordHash, allText.ToString()).GetAsUTF8BitArray().Length.GetASBytes().GetAsBitArray();
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

        public void Dispose()
        {
            Image?.Dispose();
        }
    }
}
