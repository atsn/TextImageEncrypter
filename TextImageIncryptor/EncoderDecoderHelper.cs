using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace TextImageEncrypter
{
    public class EncoderDecoderHelper
    {
        private Random random;

        public EncoderDecoderHelper()
        {
            random = new Random(Encoding.UTF8.GetBytes(Statics.passwordHash).Sum(i => i));
        }

        public void SetNextCurrentPosition(ref int currentPosition, List<int> positions)
        {
            currentPosition = positions.Last();
            positions.RemoveAt(positions.Count - 1);
        }

        public void FillPositionArray(Bitmap image, List<int> positions)
        {


            int i = 0;
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        positions.Add(i++);
                    }
                }
            }

            positions = positions.OrderBy(i => random.Next(0, positions.Count)).ToList();


        }

        public void FillPixelColorsArray(Bitmap image, byte[] pixelColors)
        {
            int i = 0;
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Color pixel = image.GetPixel(x, y);

                    pixelColors[i++] = pixel.R;
                    pixelColors[i++] = pixel.G;
                    pixelColors[i++] = pixel.B;
                }
            }
        }
    }
}