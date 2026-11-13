namespace BookWise.Application.Common;

public sealed class ConflictException(string message, Exception? innerException = null)
    : Exception(message, innerException);

public sealed class AuthenticationFailedException(string message) : Exception(message);
