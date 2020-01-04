using System;

namespace API.Exceptions
{
    public class InvalidPropertyMappingException : Exception
    {
        public InvalidPropertyMappingException(string message) : base(message)
        {
        }
    }
}