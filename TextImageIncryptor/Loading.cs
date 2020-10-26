using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TextImageEncrypter
{
    /*
  MIT License
  Copyright (c) [2020] [Anders Stubberup]
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



    /// <summary>
    ///     Used to draw a loading-overlay with a specified interval and in different modes.
    /// </summary>
    internal static class Loading
    {
        /// <summary>
        ///     The animation used in the animation mode.
        /// </summary>
        private static string[] _animation = { "-", "\\", "|", "/" };

        /// <summary>
        ///     The current animation frame, used in animation mode
        /// </summary>
        private static int _currentAnimationFrame;

        /// <summary>
        ///     The message to be printed out in the terminal
        /// </summary>
        private static string _message;

        /// <summary>
        ///     The wait between each line drawn to the terminal
        /// </summary>
        private static TimeSpan? _interval;

        /// <summary>
        ///     Whether or not the loading-overlay is currently running
        /// </summary>
        private static bool _isOn;

        /// <summary>
        ///     The last drawn line.
        /// </summary>
        private static string _lastDrawnFrame = "";

        /// <summary>
        ///     The loading type of the loading-overlay
        /// </summary>
        private static LoadingType _loadingType;

        /// <summary>
        ///     The stream you want the current progress off
        ///     Only used in stream mode
        /// </summary>
        private static Stream _stream;

        /// <summary>
        ///     The token used to cancel the wait between each line drawn.
        ///     Used to stop the loading-overlay.
        /// </summary>
        private static CancellationTokenSource _stopLoading;

        /// <summary>
        ///     The current progress as an int representing a percentage done.
        ///     Only used in progress mode
        /// </summary>
        private static int _progress;

        /// <summary>
        ///     Whether or not the loading-overlay is ready to stop.
        /// </summary>
        private static bool _readyToStop;

        /// <summary>
        ///     Whether or not the loading-overlay has drawn the current line.
        /// </summary>
        private static bool _hasDrawnCurrentAnimation;

        /// <summary>
        ///     Checks if the loading-overlay is supported by the current terminal.
        /// </summary>
        public static bool IsSupported => CheckForUnsupportedTerminal();

        /// <summary>
        ///     Set the progress of the loading animation.
        ///     Value is only used if the loading is in progress mode.
        /// </summary>
        /// <param name="progress">The current progress you want displayed</param>
        public static void SetProgress(int progress)
        {
            _progress = progress;
        }

        /// <summary>
        ///     Change the message that is printed to the console with the loading animation/progress
        /// </summary>
        /// <param name="message">The message to be printed to the console.</param>
        public static void ChangeMessage(string message)
        {
            while (!_hasDrawnCurrentAnimation)
            {
            }

            _message = message;
        }

        /// <summary>
        ///     Set the animation you want the loading to use.
        ///     Value is only used if the loading is in Animation mode.
        /// </summary>
        /// <param name="animation">A string array where each string is one frame of the loading animation</param>
        public static void ChangeAnimation(params string[] animation)
        {
            if (animation.Length < 1) return;

            while (!_hasDrawnCurrentAnimation)
            {
            }

            _animation = animation;
            _currentAnimationFrame = 0;
        }

        /// <summary>
        ///     Stops the loading-overlay.
        /// </summary>
        public static void StopLoading()
        {
            if (!_isOn) return;
            _isOn = false;
            _stopLoading.Cancel();
            while (!_readyToStop)
            {
            }

            ClearLoadingLine();
        }

        /// <summary>
        ///     Check if the the terminal supports the Move curer feature.
        /// </summary>
        /// <returns>Whether or not the loading-overlay will work in the currently used terminal</returns>
        private static bool CheckForUnsupportedTerminal()
        {
            try
            {
                Console.CursorTop = Console.CursorTop;
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        /// <summary>
        ///     Goes to the next animation-frame
        /// </summary>
        private static void IncrementAnimationFrame()
        {
            if (_currentAnimationFrame >= _animation.Length - 1)
                _currentAnimationFrame = 0;
            else
                _currentAnimationFrame++;
        }

        /// <summary>
        ///     Gets the next line for the loading-overlay to print.
        /// </summary>
        /// <returns>The next line for the loading-overlay to print.</returns>
        private static string GetLoadingLine()
        {
            switch (_loadingType)
            {
                case LoadingType.Animation:
                    return
                        $"{_message}{(string.IsNullOrEmpty(_message) ? "" : ":")}[{_animation[_currentAnimationFrame]}]";
                case LoadingType.Stream:
                    return
                        $"{_message}{(string.IsNullOrEmpty(_message) ? "" : ":")}{_stream.Position}/{_stream.Length} ({_stream.Position * 100 / _stream.Length}%)";
                case LoadingType.Progress:
                    return $"{_message}{(string.IsNullOrEmpty(_message) ? "" : ":")} {_progress}%";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Draw the next line for the loading-overlay
        /// </summary>
        private static void DrawLoadingLine()
        {
            Console.Write(GetLoadingLine());
            _lastDrawnFrame = GetLoadingLine();
        }

        /// <summary>
        ///     Run the loading-overlay loop that will run until canceled by the caller
        /// </summary>
        private static async void RunLoadingLoop()
        {
            while (_isOn)
            {
                _readyToStop = false;
                _hasDrawnCurrentAnimation = false;
                ClearLoadingLine();
                DrawLoadingLine();
                if (_loadingType == LoadingType.Animation) IncrementAnimationFrame();

                if (_interval != null)
                    try
                    {
                        _hasDrawnCurrentAnimation = true;
                        await Task.Delay((int)_interval.Value.TotalMilliseconds, _stopLoading.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        ClearLoadingLine();
                        return;
                    }
                    finally
                    {
                        _readyToStop = true;
                    }
            }
        }

        /// <summary>
        ///     Moves the cursor to the beginning of the current line.
        /// </summary>
        private static void MoveCursorToBeginningOfCurrentLine()
        {
            Console.SetCursorPosition(0, Console.CursorTop);
        }

        /// <summary>
        ///     Deletes the last drawn line from the loading-overlay in the terminal.
        /// </summary>
        private static void ClearLoadingLine()
        {
            MoveCursorToBeginningOfCurrentLine();
            Console.Write(new string(' ', _lastDrawnFrame.Length));
            MoveCursorToBeginningOfCurrentLine();
        }

        /// <summary>
        ///     The different types of loading animations
        /// </summary>
        private enum LoadingType
        {
            /// <summary>
            ///     Specifies that the loading-overlay should use animation.
            /// </summary>
            Animation,

            /// <summary>
            ///     Specifies that the loading-overlay should use the stream location and size for the information.
            ///     Will print the current progress of the stream each cycle
            /// </summary>
            Stream,

            /// <summary>
            ///     Specifies that the loading-overlay should use the Progress for the information.
            /// </summary>
            Progress
        }

        #region Constructors

        /// <summary>
        ///     Starts the loading-overlay in animation mode with the default animation and interval.
        ///     The default interval is 100 milliseconds
        /// </summary>
        public static void StartLoading()
        {
            if (_isOn) StopLoading();

            if (!IsSupported)
            {
                Console.WriteLine(
                    "The program tried to use a loading-overlay, but it is unsupported by the current terminal.");
                return;
            }

            if (!_interval.HasValue) _interval = TimeSpan.FromMilliseconds(100);

            _isOn = true;
            _stopLoading = new CancellationTokenSource();
            RunLoadingLoop();
        }

        /// <summary>
        ///     Starts the loading-overlay in animation mode with a specified animation and default interval.
        ///     The default interval is 100 milliseconds
        /// </summary>
        /// <param name="animation">A string array where each string is one frame of the loading animation</param>
        public static void StartLoading(string[] animation)
        {
            if (_isOn) StopLoading();

            if (animation != null) _animation = animation;

            _loadingType = _loadingType = LoadingType.Animation;
            StartLoading();
        }

        /// <summary>
        ///     Starts the loading-overlay in animation mode with a message, an optional specified animation and default interval.
        ///     The default interval is 100 milliseconds
        /// </summary>
        /// <param name="message">The message to be printed to the console.</param>
        /// <param name="animation">
        ///     A string array where each string is one frame of the loading animation, If not supplied the
        ///     default animation will be used.
        /// </param>
        public static void StartLoading(string message, string[] animation = null)
        {
            if (_isOn) StopLoading();

            _message = message;
            StartLoading(animation);
        }

        /// <summary>
        ///     Starts the loading-overlay in animation mode with a optional specified animation and a specified interval
        /// </summary>
        /// <param name="interval">The wait between each line drawn to the terminal</param>
        /// <param name="animation">
        ///     A string array where each string is one frame of the loading animation, If not supplied the
        ///     default animation will be used.
        /// </param>
        public static void StartLoading(TimeSpan interval, string[] animation = null)
        {
            if (_isOn) StopLoading();

            _interval = interval;
            StartLoading(animation);
        }

        /// <summary>
        ///     Starts the loading-overlay in animation mode with a message, an optional specified animation and a specified
        ///     interval.
        /// </summary>
        /// <param name="message">The message to be printed to the console.</param>
        /// <param name="interval">The wait between each line drawn to the terminal</param>
        /// <param name="animation">
        ///     A string array where each string is one frame of the loading animation, If not supplied the
        ///     default animation will be used.
        /// </param>
        public static void StartLoading(string message, TimeSpan interval, string[] animation = null)
        {
            if (_isOn) StopLoading();

            _interval = interval;
            StartLoading(message, animation);
        }

        /// <summary>
        ///     Starts the loading-overlay in animation mode with a message, an optional specified animation and a specified
        ///     interval.
        /// </summary>
        /// <param name="message">The message to be printed to the console.</param>
        /// <param name="intervalInMilliseconds">The wait between each line drawn to the terminal in milliseconds</param>
        /// <param name="animation">
        ///     A string array where each string is one frame of the loading animation, If not supplied the
        ///     default animation will be used.
        /// </param>
        public static void StartLoading(string message, int intervalInMilliseconds, string[] animation = null)
        {
            if (_isOn) StopLoading();

            _interval = TimeSpan.FromMilliseconds(intervalInMilliseconds);
            StartLoading(message, animation);
        }

        /// <summary>
        ///     Starts the loading-overlay in stream mode with the default interval
        ///     The default interval is 100 milliseconds
        /// </summary>
        /// <param name="stream">The stream you want the current progress off</param>
        public static void StartLoading(Stream stream)
        {
            if (_isOn) StopLoading();

            _stream = stream;
            _loadingType = _loadingType = LoadingType.Stream;

            StartLoading();
        }

        /// <summary>
        ///     Starts the loading-overlay in stream mode with a message and the default interval
        ///     The default interval is 100 milliseconds
        /// </summary>
        /// <param name="message">The message to be printed to the console.</param>
        /// <param name="stream">The stream you want the current progress off</param>
        public static void StartLoading(string message, Stream stream)
        {
            if (_isOn) StopLoading();

            _message = message;
            StartLoading(stream);
        }

        /// <summary>
        ///     Starts the loading-overlay in stream mode with a specified interval
        /// </summary>
        /// <param name="interval">The wait between each line drawn to the terminal</param>
        /// <param name="stream">The stream you want the current progress off</param>
        public static void StartLoading(TimeSpan interval, Stream stream)
        {
            if (_isOn)
            {
                StopLoading();
                ;
            }

            _interval = interval;
            StartLoading(stream);
        }

        /// <summary>
        ///     Starts the loading-overlay in stream mode with a message and a specified interval
        /// </summary>
        /// <param name="message">The message to be printed to the console.</param>
        /// <param name="interval">The wait between each line drawn to the terminal</param>
        /// <param name="stream">The stream you want the current progress off</param>
        public static void StartLoading(string message, TimeSpan interval, Stream stream)
        {
            if (_isOn) StopLoading();

            _interval = interval;
            StartLoading(message, stream);
        }

        /// <summary>
        ///     Starts the loading-overlay in stream mode with a message and a specified interval
        /// </summary>
        /// <param name="message">The message to be printed to the console.</param>
        /// <param name="intervalInMilliseconds">The wait between each line drawn to the terminal in milliseconds</param>
        /// <param name="stream">The stream you want the current progress off</param>
        public static void StartLoading(string message, int intervalInMilliseconds, Stream stream)
        {
            if (_isOn) StopLoading();

            _interval = TimeSpan.FromMilliseconds(intervalInMilliseconds);
            StartLoading(message, stream);
        }

        /// <summary>
        ///     Starts the loading-overlay in progress mode with a message and the default interval
        ///     The default interval is 100 milliseconds
        /// </summary>
        /// <param name="progress">The current progress as an int representing a percentage done.</param>
        public static void StartLoading(int progress)
        {
            if (_isOn) StopLoading();

            _progress = progress;
            _loadingType = _loadingType = LoadingType.Progress;

            StartLoading();
        }

        /// <summary>
        ///     Starts the loading-overlay in progress mode with a message and the default interval
        ///     The default interval is 100 milliseconds
        /// </summary>
        /// <param name="message">The message to be printed to the console.</param>
        /// <param name="progress">The current progress as an int representing a percentage done.</param>
        public static void StartLoading(string message, int progress)
        {
            if (_isOn) StopLoading();

            _message = message;
            StartLoading(progress);
        }

        /// <summary>
        ///     Starts the loading-overlay in progress mode with a specified interval
        /// </summary>
        /// <param name="interval">The wait between each line drawn to the terminal</param>
        /// <param name="progress">The current progress as an int representing a percentage done.</param>
        public static void StartLoading(TimeSpan interval, int progress)
        {
            if (_isOn)
            {
                StopLoading();
                ;
            }

            _interval = interval;
            StartLoading(progress);
        }

        /// <summary>
        ///     Starts the loading-overlay in progress mode with a message and a specified interval
        /// </summary>
        /// <param name="message">The message to be printed to the console.</param>
        /// <param name="interval">The wait between each line drawn to the terminal</param>
        /// <param name="progress">The current progress as an int representing a percentage done.</param>
        public static void StartLoading(string message, TimeSpan interval, int progress)
        {
            if (_isOn) StopLoading();

            _interval = interval;
            StartLoading(message, progress);
        }

        /// <summary>
        ///     Starts the loading-overlay in progress mode with a message and a specified interval
        /// </summary>
        /// <param name="message">The message to be printed to the console.</param>
        /// <param name="intervalInMilliseconds">The wait between each line drawn to the terminal in milliseconds</param>
        /// <param name="progress">The current progress as an int representing a percentage done.</param>
        public static void StartLoading(string message, int intervalInMilliseconds, int progress)
        {
            if (_isOn) StopLoading();

            _interval = TimeSpan.FromMilliseconds(intervalInMilliseconds);
            StartLoading(message, progress);
        }

        #endregion
    }
}
