using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TextImageEncryptor
{

    /*
    MIT License
    Copyright (c) [2019] [Anders Stubberup]
    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:
    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.
    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
    */

    internal class GetAnswers
    {
        /// <summary>
        ///     The cursor position after the question is written to the console
        /// </summary>
        private static int _afterQuestionCursorTop;

        /// <summary>
        ///     The string the user is searching after in the answers
        /// </summary>
        private static string _searchString;

        /// <summary>
        ///     The Dictionary containing the startlinNumber of an answer
        /// </summary>
        private static readonly Dictionary<string, int> LineNumbers = new Dictionary<string, int>();

        /// <summary>
        ///     Gets the user choice from the user as an enum,
        /// </summary>
        /// <typeparam name="TEnumType">The enum type you want to use to get answers</typeparam>
        /// <param name="question">The Question/Choice you want the user to Answer/Make</param>
        /// <returns>The answer of the user as a Enum of type TEnumType</returns>
        public static TEnumType GetChoiceFromEnum<TEnumType>(string question) where TEnumType : struct, Enum
        {
            var enumNames = Enum.GetNames(typeof(TEnumType));
            if (typeof(TEnumType).BaseType != typeof(Enum))
            {
                throw new InvalidCastException();
            }

            var answer = GetChoiceFromListAsString(question, enumNames);

            if (Enum.TryParse(answer, out TEnumType returnValue))
            {
                return returnValue;
            }
            else
            {
                throw new InvalidCastException("Cannot get value from the enum");
            }
        }

        /// <summary>
        ///     Gets the user choice from the user
        /// </summary>
        /// <param name="question">The Question/Choice you want the user to Answer/Make</param>
        /// <param name="answers">The Answers/Choices that the user can choose between</param>
        /// <returns>The answer the user selected as the int representing the position of the answer (starting from 0)</returns>
        public static int GetChoiceFromListAsInt(string question, params string[] answers)
        {
            try
            {
                int startBufferWidth = Console.BufferWidth;
                int startWindowWidth = Console.WindowWidth;
                Console.BufferWidth = short.MaxValue - 1;
                GetLineNumbers(answers);
                Console.BufferWidth = Math.Max(Math.Min(LineNumbers.Keys.Max(i => i.Length) + 1, short.MaxValue - 1),
                                               Console.WindowWidth);
                int setBufferWidth = Console.BufferWidth;
                int index = 0;
                int cursorTop = Console.CursorTop;
                ConsoleKey pressedKey = ConsoleKey.Enter;
                Console.CursorVisible = false;

                Console.WriteLine(question);
                _afterQuestionCursorTop = Console.CursorTop;

                Answers(answers, index);
                int afterAnswersCursorTop = Console.CursorTop;
                Console.CursorTop = cursorTop + 1;
                DateTime timeBetweenKeyPresses = DateTime.Now;
                do
                {
                    MakeAnswerLineNormal(index, answers);

                    index = GetNewIndexFromKeyPress(answers, index, pressedKey);
                    ClearLine(_afterQuestionCursorTop + LineNumbers[answers[index]]);
                    PrintIndexAnswer(index, answers);
                    int beforeResizeCursorTop = Console.CursorTop;
                    pressedKey = Console.ReadKey(true).Key;
                    if (Console.BufferWidth != setBufferWidth)
                    {
                        startBufferWidth = Console.BufferWidth;
                        startWindowWidth = Console.WindowWidth;
                    }

                    Console.BufferWidth = Math.Max(Math.Min(LineNumbers.Keys.Max(i => i.Length) + 1, short.MaxValue - 1),
                                                   Console.WindowWidth);
                    if ((DateTime.Now - timeBetweenKeyPresses).Seconds > 1)
                    {
                        _searchString = "";
                    }

                    timeBetweenKeyPresses = DateTime.Now;

                    CheckForResize(beforeResizeCursorTop);
                } while (pressedKey != ConsoleKey.Enter);

                Console.CursorTop = cursorTop;
                while (Console.CursorTop != afterAnswersCursorTop)
                {
                    Console.WriteLine(new string(' ', Console.BufferWidth - 1));
                }

                Console.CursorTop = cursorTop;
                Console.CursorVisible = true;
                Console.WindowWidth = startWindowWidth;
                Console.BufferWidth = startBufferWidth;
                LineNumbers.Clear();
                return index;

            }
            catch (IOException)
            {
                while (true)
                {
                    Console.WriteLine("This application have a gui interface that does not work correctly with your Terminal\nIn some terminals this application will not work at all\nFor the best experience run this application from windows terminal.\n\nPlease Enter the correct number and press enter");

                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine(question);

                    for (int index = 0; index < answers.Length; index++)
                    {
                        string answer = answers[index];
                        Console.WriteLine($"Enter {index} for: {answer}");
                    }

                    int choice;
                    if (!int.TryParse(Console.ReadLine(), out choice) || choice > answers.Length)
                    {
                        Console.WriteLine("Invalid choice\nPress Any Key To Continue");
                        Console.ReadKey(true);
                        Console.WriteLine();
                        Console.WriteLine();
                    }
                    else
                    {
                        return choice;
                    }
                }
            }
        }


        /// <summary>
        ///     Gets the lineNumber for each answer and saves it in the lineNumbers dictionary
        /// </summary>
        /// <param name="answers">The Answers/Choices that the user can choose between</param>
        private static void GetLineNumbers(params string[] answers)
        {
            int currentLineNumber = 0;
            foreach (string answer in answers)
            {
                string[] lines = answer.Split('\n');
                try
                {
                    LineNumbers.Add(answer, currentLineNumber + 1);
                }
                catch (ArgumentException e)
                {
                    throw new ArgumentException("Same answer appeared multible time.\nAnswers: " + answer + "\n", e);
                }

                foreach (string line in lines)
                {
                    currentLineNumber += line.Length / Console.BufferWidth + 1;
                }
            }
        }

        /// <summary>
        ///     Gets the user choice from the user
        /// </summary>
        /// <param name="question">The Question/Choice you want the user to Answer/Make</param>
        /// <param name="answers">The Answers/Choices that the user can choose between</param>
        /// <returns>The answer the user selected</returns>
        public static string GetChoiceFromListAsString(string question, params string[] answers)
        {
            int index = GetChoiceFromListAsInt(question, answers);
            return answers[index];
        }

        /// <summary>
        ///     Checks if the Console.CursorTop has changed do to a resize and makes sure to handle the resize so that the method
        ///     does not break;
        /// </summary>
        /// <param name="beforeResizeCursorTop">the position og the cursorTop before the resize</param>
        private static void CheckForResize(int beforeResizeCursorTop)
        {
            if (Console.CursorTop < beforeResizeCursorTop)
            {
                _afterQuestionCursorTop -= Math.Abs(Console.CursorTop - beforeResizeCursorTop);
            }
            else if (Console.CursorTop > beforeResizeCursorTop)
            {
                _afterQuestionCursorTop += Math.Abs(Console.CursorTop - beforeResizeCursorTop);
            }
        }

        /// <summary>
        ///     Gets the new selected index by checking the keypress.
        /// </summary>
        /// <param name="answers">The Answers/Choices that the user can choose between</param>
        /// <param name="index">The current index</param>
        /// <param name="pressedKey">The key that was pressed by the user</param>
        /// <returns></returns>
        private static int GetNewIndexFromKeyPress(string[] answers, int index, ConsoleKey pressedKey)
        {
            switch (pressedKey)
            {
                case ConsoleKey.UpArrow:
                    index--;
                    if (index < 0)
                    {
                        index = answers.Length - 1;
                    }

                    _searchString = "";
                    break;
                case ConsoleKey.DownArrow:
                    index++;
                    if (index >= answers.Length)
                    {
                        index = 0;
                    }

                    _searchString = "";
                    break;
                case ConsoleKey.PageUp:
                    index -= 5;
                    if (index < 0)
                    {
                        index = 0;
                    }

                    _searchString = "";
                    break;
                case ConsoleKey.PageDown:
                    index += 5;
                    if (index >= answers.Length)
                    {
                        index = answers.Length - 1;
                    }

                    _searchString = "";
                    break;
                case ConsoleKey.End:
                    index = answers.Length - 1;
                    _searchString = "";
                    break;
                case ConsoleKey.Home:
                    index = 0;
                    _searchString = "";
                    break;
                case ConsoleKey.Enter:
                    _searchString = "";
                    break;
                default:
                    string pressedKeyString = pressedKey.ToString().ToLower();
                    _searchString += pressedKeyString;
                    bool doBreak = FindMatchingAnswerIndex(answers, ref index);
                    if (doBreak)
                    {
                        break;
                    }

                    if (_searchString.Length > 1 && answers[index].StartsWith(_searchString))
                    {
                        break;
                    }

                    _searchString = pressedKeyString;
                    FindMatchingAnswerIndex(answers, ref index);
                    break;
            }

            return index;
        }

        /// <summary>
        ///     checks if any of the answers starts with the characters the user searches for
        /// </summary>
        /// <param name="answers">The Answers/Choices that the user can choose between</param>
        /// <param name="index">the current index (changes if answer is found)</param>
        /// <returns>whether an answer matching the search was found</returns>
        private static bool FindMatchingAnswerIndex(string[] answers, ref int index)
        {
            for (int i = index + 1; i < answers.Length; i++)
            {
                if (answers[i].ToLower().StartsWith(_searchString))
                {
                    index = i;
                    return true;
                }
            }

            for (int i = 0; i < index - 1; i++)
            {
                if (answers[i].ToLower().StartsWith(_searchString))
                {
                    index = i;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     prints the answers and highlights the selected answer
        /// </summary>
        /// <param name="answers">The Answers/Choices that the user can choose between</param>
        /// <param name="index">the current index</param>
        private static void Answers(string[] answers, int index)
        {
            for (int i = 0; i < answers.Length; i++)
            {
                if (i == index)
                {
                    PrintIndexAnswer(i, answers);
                }
                else
                {
                    Console.WriteLine(answers[i]);
                }
            }
        }

        /// <summary>
        ///     prints out the answer corresponding to the current index in the highlighted form
        /// </summary>
        /// <param name="index">the current index</param>
        /// <param name="answers">The Answers/Choices that the user can choose between</param>
        private static void PrintIndexAnswer(int index, string[] answers)
        {
            Console.SetCursorPosition(0, _afterQuestionCursorTop + LineNumbers[answers[index]]);
            ConsoleColor foregroundColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            if (foregroundColor == ConsoleColor.Cyan)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            Console.WriteLine(answers[index]);
            Console.ForegroundColor = foregroundColor;
        }

        /// <summary>
        ///     prints out the answer corresponding to the current index in the not highlighted form
        /// </summary>
        /// <param name="index"></param>
        /// <param name="answers"></param>
        private static void MakeAnswerLineNormal(int index, string[] answers)
        {
            Console.SetCursorPosition(0, _afterQuestionCursorTop + LineNumbers[answers[index]]);
            Console.Write(new string(' ', Console.WindowWidth - 1));
            Console.CursorLeft = 0;
            Console.WriteLine(answers[index]);
        }

        /// <summary>
        ///     clears the line specified
        /// </summary>
        /// <param name="lineNumber">the line you want to be cleared</param>
        private static void ClearLine(int lineNumber)
        {
            Console.CursorTop = lineNumber;
            Console.Write("\r" + new string(' ', Console.BufferWidth - 1) + "\r");
            Console.CursorLeft = 0;
        }
    }
}