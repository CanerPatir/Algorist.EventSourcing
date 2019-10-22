namespace Algorist.EventSourcing.Exceptions
{
    public class ConcurrencyException : System.Exception
    {
        public ConcurrencyException(string msg) : base(msg)
        {
        }
    }
}