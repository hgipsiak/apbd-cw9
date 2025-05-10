namespace apbd_cw9.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException() { }
    public NotFoundException(string msg) : base(msg) { }
    public NotFoundException(string msg, Exception inner) : base(msg, inner) { }
}