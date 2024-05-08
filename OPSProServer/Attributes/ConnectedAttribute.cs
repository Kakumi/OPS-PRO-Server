namespace OPSProServer.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ConnectedAttribute : Attribute
    {
        public bool HasRoom { get; }
        public bool InGame { get; }
        public bool IsTurn { get; }
        public bool IsRoomMandatory { get; }

        public ConnectedAttribute(bool hasRoom = false, bool inGame = false, bool isTurn = false, bool isRoomMandatory = false) { 
            HasRoom = hasRoom;
            InGame = inGame;
            IsTurn = isTurn;
            IsRoomMandatory = isRoomMandatory;
        }
    }
}
