namespace Core
{
    public struct OnCollectPointEvent : IEvent
    {
        public int PointAmount;
    }
    
    public struct OnPacmanNodeChangedEvent : IEvent
    {
        public Node Node;
    }
    
    public class Events
    {
    }
}