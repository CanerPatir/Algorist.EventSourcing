namespace Algorist.EventSourcing.Storage.EventStore
{
    public class EventStoreMetaDataHeader
    {
        public string ClrType { get; set; }
        public int CommitNumber { get; set; }
    }
}
