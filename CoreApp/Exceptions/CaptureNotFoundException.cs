namespace CoreApp.Exceptions;

public class CaptureNotFoundException : Exception
{
    public CaptureNotFoundException(Guid id) : base($"Capture with id={id} not found.") { }
}