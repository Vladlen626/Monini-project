using System;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;

namespace _Main.Scripts.Core
{
	public interface INetworkService
	{
		bool IsServer { get; }
		bool IsClient { get; }
		bool IsHost { get; }
		int LocalClientId { get; }
		int PlayersCount { get; }
		void StartHost();
		void StartClient();
		void Stop();
		void Spawn(NetworkObject nob, NetworkConnection conn);
		void Despawn(NetworkObject nob);
		NetworkConnection GetClientConnection(int clientId);
		bool TryGetSpawnedNetworkObject(int objectId, out NetworkObject networkObject);
	}
}