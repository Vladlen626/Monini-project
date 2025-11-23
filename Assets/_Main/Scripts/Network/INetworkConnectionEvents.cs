using System;

public interface INetworkConnectionEvents
{
	public event Action OnLocalClientConnected;
	public event Action OnLocalClientDisconnected;
	public event Action OnLocalClientLoadedStartScenes;

	public event Action<int> OnRemoteClientConnected;
	public event Action<int> OnRemoteClientDisconnected;
	public event Action<int> OnRemoteClientLoadedStartScenes;
}