namespace Algorist.EventSourcing.Exceptions
{
    public class AggregateCreationException : System.Exception
    {
        public AggregateCreationException(string msg) : base(msg)
        {
        }
    }
}