using System;
using System.Collections.Generic;
using System.Text;

namespace TextImageEncrypter.Exceptions
{
    class NoMoreSpaceInImageException : Exception
    {
        public NoMoreSpaceInImageException() : base()
        {
            
        }

        public NoMoreSpaceInImageException(string message) : base(message)
        {
            
        }
    }
}
