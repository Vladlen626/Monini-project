using System;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Transporting;
using PlatformCore.Core;
using Object = UnityEngine.Object;

namespace _Main.Scripts.Core
{
	public class NetworkService : INetworkService, IService
	{
		private readonly NetworkManager _networkManager;
		public bool IsServer => _networkManager.ServerManager.Started;

		public bool IsClient => _networkManager.ClientManager.Started;

		public bool IsHost => IsServer && IsClient;

		public int LocalClientId => _networkManager.ClientManager.Connection.ClientId;

		public NetworkService()
		{
			_networkManager = InstanceFinder.NetworkManager;
		}

		public NetworkConnection GetClientConnection(int clientId)
		{
			return _networkManager.ServerManager.Clients[clientId];
		}
		
		public bool TryGetSpawnedNetworkObject(int objectId, out NetworkObject networkObject)
		{
			return _networkManager.ServerManager.Objects.Spawned.TryGetValue(objectId, out networkObject);
		}
		
		public void StartHost()
		{
			_networkManager.ServerManager.StartConnection();
			_networkManager.ClientManager.StartConnection();
		}

		public void StartClient()
		{
			_networkManager.ClientManager.StartConnection();
		}

		public void Stop()
		{
			if (IsClient)
			{
				_networkManager.ClientManager.StopConnection();
			}

			if (_networkManager.ServerManager.Started)
			{
				_networkManager.ServerManager.StopConnection(true);
			}
		}

		public void Spawn(NetworkObject nob, NetworkConnection conn)
		{
			_networkManager.ServerManager.Spawn(nob, conn);
		}
		
		public void Despawn(NetworkObject nob)
		{
			_networkManager.ServerManager.Despawn(nob);
		}

		public void Dispose()
		{
		}
	}
}