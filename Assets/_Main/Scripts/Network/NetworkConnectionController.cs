using System;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Transporting;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

public sealed class NetworkConnectionController : IBaseController, IActivatable, INetworkConnectionEvents
{
	public event Action OnLocalClientConnected;
	public event Action OnLocalClientDisconnected;
	public event Action OnLocalClientLoadedStartScenes;

	public event Action<int> OnRemoteClientConnected;
	public event Action<int> OnRemoteClientDisconnected;
	public event Action<int> OnRemoteClientLoadedStartScenes;

	private readonly NetworkManager _networkManager;

	public NetworkConnectionController()
	{
		_networkManager = InstanceFinder.NetworkManager;
	}

	public void Activate()
	{
		_networkManager.ClientManager.OnClientConnectionState += HandleLocalClientState;
		_networkManager.ServerManager.OnRemoteConnectionState += HandleRemoteConnectionState;
		_networkManager.SceneManager.OnClientLoadedStartScenes += HandleClientLoadedStartScenes;
	}

	public void Deactivate()
	{
		_networkManager.ClientManager.OnClientConnectionState -= HandleLocalClientState;
		_networkManager.ServerManager.OnRemoteConnectionState -= HandleRemoteConnectionState;
		_networkManager.SceneManager.OnClientLoadedStartScenes -= HandleClientLoadedStartScenes;
	}

	private void HandleLocalClientState(ClientConnectionStateArgs args)
	{
		switch (args.ConnectionState)
		{
			case LocalConnectionState.Started:
				OnLocalClientConnected?.Invoke();
				break;

			case LocalConnectionState.Stopped:
				OnLocalClientDisconnected?.Invoke();
				break;
		}
	}

	private void HandleRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
	{
		switch (args.ConnectionState)
		{
			case RemoteConnectionState.Started:
				OnRemoteClientConnected?.Invoke(args.ConnectionId);
				break;

			case RemoteConnectionState.Stopped:
				OnRemoteClientDisconnected?.Invoke(args.ConnectionId);
				break;
		}
	}

	private void HandleClientLoadedStartScenes(NetworkConnection conn, bool asServer)
	{
		if (!asServer && conn == _networkManager.ClientManager.Connection)
		{
			OnLocalClientLoadedStartScenes?.Invoke();
		}

		if (asServer)
		{
			OnRemoteClientLoadedStartScenes?.Invoke(conn.ClientId);
		}
	}
}