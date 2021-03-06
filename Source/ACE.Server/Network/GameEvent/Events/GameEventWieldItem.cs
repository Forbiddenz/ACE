﻿namespace ACE.Server.Network.GameEvent.Events
{
    public class GameEventWieldItem : GameEventMessage
    {
        public GameEventWieldItem(Session session, uint objectId, int newLocation)
            : base(GameEventType.WieldObject, GameMessageGroup.UIQueue, session)
        {
            Writer.Write(objectId);
            Writer.Write(newLocation);
        }
    }
}
