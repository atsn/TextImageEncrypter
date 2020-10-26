using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using TextImageEncrypter.Exceptions;

namespace TextImageEncrypter
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {

            List<string> strings = new List<string>();
            StringBuilder builder = new StringBuilder();
            Loading.StartLoading($"{0} : 200000");

            for (int i = 0; i < 1383294; i++)
            {
                Loading.ChangeMessage($"{i} : 200000");

                builder.Append("j");
                if (i > 1382294)
                {
                    strings.Add(builder.ToString());
                }
            }
           
            Loading.StopLoading();
            foreach (var s in strings)
            {
                var lenth = s.GetAsUTF8BitArray().Length / 8 + (((s.GetAsUTF8BitArray().Length / 8) % 16) == 0
                    ? 0
                    : (16 - ((s.GetAsUTF8BitArray().Length / 8) % 16)));


                    Console.WriteLine(lenth);
                    var password = "111111111111111111111111111111111111111111111111111111111111111111111";
                    Console.WriteLine(Encryptor.Encrypt(password, s).Length);
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine();
            }
            return;
            //* setPasswordHash(GetPassword());
            //* int answer = -1;
            //* bool done = false;
            //* while (!done)
            //* {
            //*     answer = GetAnswers.GetChoiceFromListAsInt("What do you wish to do", "Encrypt", "Decrypt", "Exit");
            //* 
            //*     switch (answer)
            //*     {
            //*         case 0:
            //*             WriteToImage();
            //*             break;
            //*         case 1:
            //*             ReadFromImage();
            //*             break;
            //*         default:
            //*             done = true;
            //*             break;
            //*     }
            //* }
        }

        private static void setPasswordHash(string Password)
        {
            if (Password.Length < 1)
            {
                Statics.passwordHash = "NoPasswordSet";
            }
            using (SHA512 shaM = new SHA512Managed())
            {
                var hash = shaM.ComputeHash(Encoding.UTF8.GetBytes(Password.ToString()));

                var hashedInputStringBuilder = new System.Text.StringBuilder(128);
                foreach (var b in hash)
                    hashedInputStringBuilder.Append(b.ToString("X2"));

                Statics.passwordHash = hashedInputStringBuilder.ToString();
            }

        }

        private static string GetPassword()
        {
            Console.WriteLine("Please enter password");
            string password = string.Empty;

            ConsoleKey key;
            do
            {
                var keyInfo = Console.ReadKey(intercept: true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && password.Length > 0)
                {
                    Console.Write("\b \b");
                    password = password[0..^1];
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");
                    password += keyInfo.KeyChar;
                }
            } while (key != ConsoleKey.Enter);

            Console.Clear();
            return password;
        }

        private static void ReadFromImage()
        {
            if (!TryGetImagePath(out var filename)) return;
            Bitmap img = new Bitmap(filename);
            ImageDecoder decoder = new ImageDecoder(img);
            Console.WriteLine(decoder.GetTextFromImage());
            Console.WriteLine("\n\nPress any key to continue");
            Console.ReadKey(true);
            img.Dispose();
            Console.Clear();
        }

        private static bool TryGetTextPath(out string filePath)
        {
            OpenFileDialog
                fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;
            fileDialog.CheckFileExists = true;
            var answer = DialogResult.Retry;
            while (answer == DialogResult.Retry)
            {
                answer = fileDialog.ShowDialog();
            }


            if (answer != DialogResult.OK && answer != DialogResult.Yes)
            {
                filePath = null;
                return false;
            }

            filePath = fileDialog.FileName;
            return true;
        }

        private static bool TryGetImagePath(out string imagePath)
        {
            OpenFileDialog
                fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;
            fileDialog.CheckFileExists = true;
            fileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png";
            var answer = DialogResult.Retry;
            while (answer == DialogResult.Retry)
            {
                answer = fileDialog.ShowDialog();
            }


            if (answer != DialogResult.OK && answer != DialogResult.Yes)
            {
                imagePath = null;
                return false;
            }

            imagePath = fileDialog.FileName;
            return true;
        }

        private static bool TryGetSaveImagePath(out string imagePath)
        {
            SaveFileDialog
                fileDialog = new SaveFileDialog();
            fileDialog.OverwritePrompt = true;
            fileDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
            var answer = DialogResult.Retry;
            while (answer == DialogResult.Retry)
            {
                answer = fileDialog.ShowDialog();
            }


            if (answer != DialogResult.OK && answer != DialogResult.Yes)
            {
                imagePath = null;
                return false;
            }

            imagePath = fileDialog.FileName;
            return true;
        }

        private static void WriteToImage()
        {
            if (!TryGetImagePath(out var filename)) return;
            Bitmap img = new Bitmap(filename);
            ImageEncoder encoder = new ImageEncoder(img);

            GetInputFromUser(encoder);


            var finalImage = encoder.GetImageAndResetEncoder();
            if (!TryGetSaveImagePath(out var newFilename)) return;
            File.WriteAllBytes(newFilename, finalImage.ToByteArray(ImageFormat.Png));
            Console.Clear();
        }

        private static void GetInputFromUser(ImageEncoder encoder)
        {
            bool done = false;

            while (!done)
            {
                Console.WriteLine(GetSpaceLeftLine(encoder.GetSizeLeftInImage()));

                var answer = GetAnswers.GetChoiceFromListAsInt("How do you wish to preceded", "Write Lines", "Write",
                    "Write Line", "Read From text file", "exit");

                switch (answer)
                {
                    case 0:
                        WriteLinesToImage(encoder);
                        break;
                    case 1:
                        Console.WriteLine("Please enter the text you want to write and finish by presing enter");
                        encoder.AppendAndEncryptString(Console.ReadLine());
                        Console.Clear();
                        break;
                    case 2:
                        Console.WriteLine("Please enter the line you want to write and finish by presing enter");
                        encoder.AppendLineAndEncryptString(Console.ReadLine());
                        Console.Clear();
                        break;
                    case 3:
                        encoder.AppendAndEncryptString(ReadTextFromFile(encoder));
                        break;
                    default:
                        done = true;
                        break;
                }
            }

        }

        private static string ReadTextFromFile(ImageEncoder encoder)
        {
            if (!TryGetTextPath(out var filename)) return null;

            return File.ReadAllText(filename);
        }

        private static void WriteLinesToImage(ImageEncoder encoder)
        {
            var line = string.Empty;
            var done = false;
            Console.WriteLine("You can now write lines to the image\n Press enter to complete line.\n Write a line containing only \"done\" to finish writing");
            while (!done)
            {
                line = Console.ReadLine();
                if (line?.ToLower() != "done")
                {
                    encoder.AppendLineAndEncryptString(line);
                }
                else
                {
                    done = true;
                }
            }
            Console.Clear();
        }

        private static string GetSpaceLeftLine(in int size)
        {
            return $"You now have {size} bytes of space left in the file";
        }
    }
}
