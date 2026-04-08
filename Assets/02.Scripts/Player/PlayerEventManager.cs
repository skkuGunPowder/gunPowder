using System.Collections.Generic;
using Photon.Pun;

public class PlayerEventManager : DontDestroySingleton<PlayerEventManager>
{
    private Dictionary<int, PlayerEvents> _playerEvents = new();

    public PlayerEvents Local => GetEvents(PhotonNetwork.LocalPlayer.ActorNumber);

    public PlayerEvents GetEvents(int actorNumber)
    {
        if (!_playerEvents.TryGetValue(actorNumber, out var events))
        {
            events = new PlayerEvents();
            _playerEvents[actorNumber] = events;
        }
        return events;
    }

    public void RemoveEvents(int actorNumber)
    {
        if (_playerEvents.TryGetValue(actorNumber, out var events))
        {
            events.Clear();
            _playerEvents.Remove(actorNumber);
        }
    }
}
