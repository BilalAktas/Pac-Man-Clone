namespace Core
{
    public struct OnCollectPointEvent : IEvent
    {
        public int PointAmount;
    }
    
    public struct OnCollectableCollectedEvent : IEvent { }
    
    public struct OnCollectPowerPelletEvent : IEvent { }
    
    public struct OnPowerPelletWarningEvent : IEvent { }

    public struct OnPowerPelletEffectFinishedEvent : IEvent { }
    
    public struct OnPacmanNodeChangedEvent : IEvent
    {
        public Node Node;
    }

    public struct OnGhostDiedEvent : IEvent { }
    
    public struct OnBattleFinishedEvent : IEvent
    {
        public Pacman Pacman;
        public Ghost Ghost;
        public bool PacmanWon;
    }
    
    public struct OnLivesChangedEvent : IEvent
    {
        public int Lives;
        public int MaxLives;
    }
    
    public struct OnRoundResetEvent : IEvent { }

    public struct OnGameWonEvent : IEvent { }

    public struct OnGameLostEvent : IEvent { }
}
