namespace CoreApp.Exceptions;

public class GateNotFoundException : Exception
{
    public GateNotFoundException(Guid id) : base($"Gate with id={id} not found.") { }
    public GateNotFoundException(string msg) : base(msg) { }
}