using System;
using FishNet.Connection;
using FishNet.Managing;

namespace _Main.Scripts.Core
{
	public interface INetworkService
	{
		bool IsServer { get; }
		bool IsClient { get; }
		bool IsHost { get; }
		int LocalClientId { get; }
		void StartHost();
		void StartClient();
		void Stop();
		NetworkConnection GetClientConnection(int clientId);
	}
}