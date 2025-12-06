using System.Collections.Generic;

public class NetworkModel
{
	public IReadOnlyDictionary<int, PlayerContext.Server> ownerContexts => _ownerContexts;	

	private readonly Dictionary<int, PlayerContext.Server> _ownerContexts = new();

	public void AddPlayerContext(int networkPlayerId, PlayerContext.Server playerContext)
	{
		_ownerContexts[networkPlayerId] = playerContext;
	}

	public void RemovePlayerContext(int networkPlayerId)
	{
		_ownerContexts.Remove(networkPlayerId);
	}
}