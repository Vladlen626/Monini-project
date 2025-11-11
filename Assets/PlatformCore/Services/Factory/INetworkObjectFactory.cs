using Cysharp.Threading.Tasks;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

namespace PlatformCore.Services.Factory
{
	public interface INetworkObjectFactory
	{
		UniTask<NetworkObject> CreateNetworkAsync(
			string address,
			Vector3 position,
			Quaternion rotation,
			NetworkConnection owner = null,
			Transform parent = null);

		void DestroyNetwork(NetworkObject nob);
	}
}