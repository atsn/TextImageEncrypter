using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace TextImageIncryptor
{
    public static class EncoderDecoderHelper
    {
        public static void SetNextCurrentPosition(ref int currentPosition, List<int> positions)
        {
            currentPosition = positions.Last();
            positions.RemoveAt(positions.Count - 1);
        }

        public static void FillPositionArray(Bitmap image, List<int> positions)
        {

            var i = 0;
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    positions.Add(i++);
                }
            }

        }

        public static void FillPixelColorsArray(Bitmap image ,byte[] pixelColors)
        {
            int i = 0;
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    var pixel = image.GetPixel(x, y);

                    pixelColors[i++] = pixel.R;
                    pixelColors[i++] = pixel.G;
                    pixelColors[i++] = pixel.B;
                }
            }
        }
    }
}