namespace AppCore.Exceptions;

public class InvalidPlateNumberException: Exception
{
    public InvalidPlateNumberException(string? message) : base(message)
    {
    }
}