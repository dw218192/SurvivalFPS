namespace SurvivalFPS.Messaging
{
    /// <summary>
    /// Represents the events used in the messenger class
    /// </summary>
    public class M_EventType
    {
        public static readonly M_EventType TestEvent = new M_EventType(0);
        public static readonly M_EventType OnGamePaused = new M_EventType(1);
        public static readonly M_EventType OnGameResumed = new M_EventType(2);
        public static readonly M_EventType OnGameSaving = new M_EventType(3);
        public static readonly M_EventType OnGameSaved = new M_EventType(4);

        private int m_Value;
        protected M_EventType(int value)
        {
            m_Value = value;
        }
    }

    /// <summary>
    /// Represents the events with event data involved, used in the messenger class
    /// </summary>
    public class M_DataEventType : M_EventType
    {
        public static readonly M_DataEventType TestDataEvent = new M_DataEventType(0);
        public static readonly M_DataEventType OnInventoryItemRemoved = new M_DataEventType(1);
        public static readonly M_DataEventType OnInventoryItemAdded = new M_DataEventType(2);

        protected M_DataEventType(int value) : base(value){}
    }
}