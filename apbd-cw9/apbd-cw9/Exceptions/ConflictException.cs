namespace apbd_cw9.Exceptions;

public class ConflictException : Exception
{
    public ConflictException() { }
    public ConflictException(string msg) : base(msg) { }
    public ConflictException(string msg, Exception inner) : base(msg, inner) { }
}