using System;
using PlatformCore.Core;
using Unity.Netcode;
using Object = UnityEngine.Object;

namespace _Main.Scripts.Core
{
	public class NetworkService : INetworkService, IService
	{
		public event Action<ulong> OnClientConnected;
		public event Action<ulong> OnClientDisconnected;
		public event Action<PlayerNetworkBridge> OnLocalPlayerSpawned;

		public bool IsServer => _manager.IsServer;
		public bool IsClient => _manager.IsClient;
		public bool IsHost => _manager.IsHost;
		public ulong LocalClientId => _manager.LocalClientId;

		private readonly NetworkManager _manager;

		public NetworkService()
		{
			_manager = Object.FindFirstObjectByType<NetworkManager>();
			_manager.OnClientConnectedCallback += OnClientConnectedHandler;
			_manager.OnClientDisconnectCallback += OnClientDisconnectHandler;
		}

		public void StartHost()
		{
			_manager.StartHost();
		}

		public void StartClient()
		{
			_manager.StartClient();
		}

		public void Stop()
		{
			_manager.Shutdown();
		}

		public void InvokeLocalPlayerSpawned(PlayerNetworkBridge bridge, bool isOwner)
		{
			if (isOwner)
			{
				OnLocalPlayerSpawned?.Invoke(bridge);
			}
		}

		private void OnClientConnectedHandler(ulong clientId)
		{
			OnClientConnected?.Invoke(clientId);
		}

		private void OnClientDisconnectHandler(ulong clientId)
		{
			OnClientConnected?.Invoke(clientId);
		}

		public void Dispose()
		{
			_manager.OnClientConnectedCallback -= OnClientConnectedHandler;
			_manager.OnClientDisconnectCallback -= OnClientDisconnectHandler;
		}
	}
}