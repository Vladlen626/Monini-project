using System;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;

namespace _Main.Scripts.Core
{
	public interface INetworkService
	{
		NetworkManager NetworkManager { get; }
		bool IsServer { get; }
		bool IsClient { get; }
		bool IsHost { get; }
		bool IsServerStarted { get; }
		bool IsClientStarted { get; }
		int LocalClientId { get; }
		int PlayersCount { get; }
		void StartHost();
		void StartClient();
		void Stop();
		public void Spawn(NetworkObject nob, NetworkConnection conn, UnityEngine.SceneManagement.Scene scene = default);
		void Despawn(NetworkObject nob);
		NetworkConnection GetClientConnection(int clientId);
		bool TryGetSpawnedNetworkObject(int objectId, out NetworkObject networkObject);
	}
}