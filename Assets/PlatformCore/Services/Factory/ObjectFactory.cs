using _Main.Scripts.Core;
using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PlatformCore.Services.Factory
{
	public class ObjectFactory : BaseAsyncService, IObjectFactory, INetworkObjectFactory
	{
		private readonly IResourceService _resourceService;
		private readonly INetworkService _networkService;
		private readonly ILoggerService _loggerService;

		public ObjectFactory(IResourceService resourceService, ILoggerService loggerService,
			INetworkService networkService)
		{
			_networkService = networkService;
			_resourceService = resourceService;
			_loggerService = loggerService;
		}

		public async UniTask<GameObject> CreateAsync(string address, Vector3 position, Quaternion rotation,
			Transform parent = null)
		{
			_loggerService?.Log($"[ObjectFactory] Creating GameObject from: {address}");

			var prefab = await _resourceService.LoadAsync<GameObject>(address);

			if (prefab == null)
			{
				_loggerService?.LogError($"[ObjectFactory] ❌ Failed to load prefab at '{address}' (type: GameObject)");
				return null;
			}

			var instance = parent != null
				? Object.Instantiate(prefab, position, rotation, parent)
				: Object.Instantiate(prefab, position, rotation);

			instance.name = prefab.name;
			_loggerService?.Log($"[ObjectFactory] ✅ Created GameObject '{instance.name}' at {position}");
			return instance;
		}

		public async UniTask<T> CreateAsync<T>(string address, Vector3 position, Quaternion rotation,
			Transform parent = null)
			where T : Component
		{
			_loggerService?.Log($"[ObjectFactory] Creating component '{typeof(T).Name}' from: {address}");

			var gameObject = await CreateAsync(address, position, rotation, parent);
			if (gameObject == null)
			{
				_loggerService?.LogError($"[ObjectFactory] ❌ Prefab not found for '{typeof(T).Name}' at '{address}'");
				return null;
			}

			var component = gameObject.GetComponent<T>();
			if (component == null)
			{
				_loggerService?.LogError(
					$"[ObjectFactory] ❌ Component '{typeof(T).Name}' missing on prefab '{address}'");
				Object.Destroy(gameObject);
				return null;
			}

			_loggerService?.Log($"[ObjectFactory] ✅ Component '{typeof(T).Name}' loaded successfully from '{address}'");
			return component;
		}

		public void Destroy(GameObject obj)
		{
			if (!obj)
			{
				_loggerService?.LogWarning("[ObjectFactory] ⚠️ Tried to destroy null object");
				return;
			}

			_loggerService?.Log($"[ObjectFactory] Destroying: {obj.name}");
			Object.Destroy(obj);
		}

		public async UniTask<NetworkObject> CreateNetworkAsync(
			string address, Vector3 position, Quaternion rotation, NetworkConnection owner = null,
			Scene scene = default)
		{
			var prefab = await _resourceService.LoadAsync<GameObject>(address);
			if (prefab == null)
			{
				_loggerService.LogError($"[ObjectFactory] Network prefab not found: {address}");
				return null;
			}

			var nob = prefab.GetComponent<NetworkObject>();
			if (nob == null)
			{
				_loggerService.LogError($"[ObjectFactory] Prefab {address} has no NetworkObject");
				return null;
			}

			if (!_networkService.IsServer)
			{
				_loggerService.LogWarning($"[ObjectFactory] Tried to spawn network object from client: {address}");
				return null;
			}

			var instance = Object.Instantiate(nob, position, rotation);
			_networkService.Spawn(instance, owner, scene);
			return instance;
		}

		public void DestroyNetwork(NetworkObject nob)
		{
			if (nob == null) return;

			if (!_networkService.IsServer)
			{
				_loggerService.LogWarning("[ObjectFactory] ⚠️ Tried to despawn network object from client");
				return;
			}

			if (nob.IsSpawned)
			{
				_networkService.Despawn(nob);
			}
			else
			{
				Object.Destroy(nob.gameObject);
			}
		}
	}
}