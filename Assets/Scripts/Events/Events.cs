namespace Core
{
    public struct OnGhostDiedEvent : IEvent { } 
    public struct OnCollectPointEvent : IEvent
    {
        public int PointAmount;
    }
    
    public struct OnPacmanNodeChangedEvent : IEvent
    {
        public Node Node;
    }

    public struct OnCollectPowerPelletEvent : IEvent{}
    
    public struct OnPowerPelletEffectFinishedEvent : IEvent{}

    public struct OnBattleStartedEvent : IEvent
    {
        public Character Pacman;
        public Character Ghost;
        public Node Node;
    }
    
    public struct OnBattleFinishedEvent : IEvent
    {
        public Character Winner;
        public Character Loser;
        public Node Node;
    }
    
    public class Events
    {
    }
}