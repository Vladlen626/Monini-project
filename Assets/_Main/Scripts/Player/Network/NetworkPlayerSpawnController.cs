using _Main.Scripts.Core;
using _Main.Scripts.Player;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services;
using UnityEngine;

public class NetworkPlayerSpawnController : IBaseController, IActivatable
{
	private readonly INetworkService _networkService;
	private readonly PlayerFactoryCore _playerFactoryCore;
	private readonly LifecycleManager _lifecycle;
	private readonly ICameraService _cameraService;

	public NetworkPlayerSpawnController(ServiceLocator serviceLocator, LifecycleManager lifecycle)
	{
		_networkService = serviceLocator.Get<INetworkService>();
		_cameraService = serviceLocator.Get<ICameraService>();
		_playerFactoryCore = new PlayerFactoryCore(serviceLocator);
		_lifecycle = lifecycle;
	}

	public void Activate()
	{
		_networkService.OnClientConnected += OnClientConnectedHandler;
		_networkService.OnLocalPlayerSpawned += OnLocalPlayerSpawnedHandler;
	}

	public void Deactivate()
	{
		_networkService.OnClientConnected -= OnClientConnectedHandler;
		_networkService.OnLocalPlayerSpawned -= OnLocalPlayerSpawnedHandler;
	}

	private void OnClientConnectedHandler(ulong clientId)
	{
		SpawnPlayerAsync(clientId).Forget();
	}

	private async UniTask SpawnPlayerAsync(ulong clientId)
	{
		if (!_networkService.IsServer)
		{
			return;
		}

		var playerView = await _playerFactoryCore.CreatePlayerView(Vector3.zero);
		var bridge = playerView.GetComponent<PlayerNetworkBridge>();
		bridge.Initialize(_networkService, playerView);
		bridge.NetworkObject.SpawnWithOwnership(clientId);
	}

	private async void OnLocalPlayerSpawnedHandler(PlayerNetworkBridge bridge)
	{
		var factory = _playerFactoryCore;
		var config = new PlayerConfig();
		_cameraService.AttachTo(bridge.playerView.CameraRoot);
		var controllers = factory.GetPlayerBaseControllers(config, bridge.playerView);

		foreach (var baseController in controllers)
		{
			await _lifecycle.RegisterAsync(baseController);
		}
		
	}
}