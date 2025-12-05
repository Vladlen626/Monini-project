using System.Collections.Generic;

public class NetworkModel
{
	public IReadOnlyDictionary<int, PlayerContext> ownerContexts => _ownerContexts;	

	private readonly Dictionary<int, PlayerContext> _ownerContexts = new();

	public void AddPlayerContext(int networkPlayerId, PlayerContext playerContext)
	{
		_ownerContexts[networkPlayerId] = playerContext;
	}

	public void RemovePlayerContext(int networkPlayerId)
	{
		_ownerContexts.Remove(networkPlayerId);
	}
}