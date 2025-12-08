using Cysharp.Threading.Tasks;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PlatformCore.Services.Factory
{
	public interface INetworkObjectFactory
	{
		UniTask<NetworkObject> CreateNetworkAsync(
			string address,
			Vector3 position,
			Quaternion rotation,
			NetworkConnection owner = null,
			Scene scene = default);

		void DestroyNetwork(NetworkObject nob);
	}
}