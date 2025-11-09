using System;
using Unity.Netcode;

namespace _Main.Scripts.Core
{
	public interface INetworkService
	{
		bool IsServer { get; }
		bool IsClient { get; }
		bool IsHost { get; }
		ulong LocalClientId { get; }

		event Action<ulong> OnClientConnected;
		event Action<ulong> OnClientDisconnected;
		event Action<PlayerNetworkBridge> OnLocalPlayerSpawned;

		void StartHost();
		void StartClient();
		void Stop();
		
		void InvokeLocalPlayerSpawned(PlayerNetworkBridge bridge, bool isOwner);
	}
}