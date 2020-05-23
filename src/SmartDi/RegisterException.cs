using System;
namespace SmartDi
{
    public class RegisterException : Exception
    {
        //public RegistrationException() : base()
        //{ }
        public RegisterException(string message) : base(message)
        { }
        public RegisterException(string message, Exception innerException) : base(message, innerException)
        { }

    }
}
