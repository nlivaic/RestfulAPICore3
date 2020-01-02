using System;

namespace API.Exceptions
{
    public class InvalidOrderByCriteriaException : Exception
    {
        public InvalidOrderByCriteriaException(string message) : base(message)
        {
        }
    }
}