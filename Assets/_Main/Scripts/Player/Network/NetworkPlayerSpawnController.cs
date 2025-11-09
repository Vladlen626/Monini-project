using System.Collections.Generic;
using _Main.Scripts.Core;
using _Main.Scripts.Core.Services;
using _Main.Scripts.Player;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services;
using PlatformCore.Services.Factory;
using UnityEngine;

public class NetworkPlayerSpawnController : IBaseController, IActivatable
{
	private readonly INetworkService _networkService;
	private readonly IObjectFactory _objectFactory;
	private readonly PlayerFactoryCore _playerFactoryCore;
	private readonly LifecycleManager _lifecycle;

	private Dictionary<ulong, PlayerContext> _ownerContexts = new ();


	public NetworkPlayerSpawnController(ServiceLocator serviceLocator, LifecycleManager lifecycle)
	{
		_networkService = serviceLocator.Get<INetworkService>();
		_objectFactory = serviceLocator.Get<IObjectFactory>();
		_playerFactoryCore = new PlayerFactoryCore(serviceLocator);
		_lifecycle = lifecycle;
	}

	public void Activate()
	{
		_networkService.OnClientConnected += OnClientConnectedHandler;
		_networkService.OnClientDisconnected += OnClientDisconnected;
		_networkService.OnLocalPlayerSpawned += OnLocalPlayerSpawnedHandler;
	}

	public void Deactivate()
	{
		_networkService.OnClientConnected -= OnClientConnectedHandler;
		_networkService.OnClientDisconnected -= OnClientDisconnected;
		_networkService.OnLocalPlayerSpawned -= OnLocalPlayerSpawnedHandler;
	}

	private void OnClientConnectedHandler(ulong clientId)
	{
		SpawnPlayerAsync(clientId).Forget();
	}

	private void OnClientDisconnected(ulong clientId)
	{
		if (_ownerContexts.TryGetValue(clientId, out var ctx))
		{
			foreach (var valueController in ctx.controllers)
			{
				_lifecycle.Unregister(valueController);
			}

			ctx.Dispose();
			_ownerContexts.Remove(clientId);
		}
	}

	private async UniTask SpawnPlayerAsync(ulong clientId)
	{
		if (!_networkService.IsServer)
		{
			return;
		}

		var playerView = await _playerFactoryCore.CreatePlayerView(Vector3.zero);
		var bridge = playerView.GetComponent<PlayerNetworkBridge>();
		bridge.Initialize(playerView);
		bridge.NetworkObject.SpawnWithOwnership(clientId);
	}

	private async void OnLocalPlayerSpawnedHandler(PlayerNetworkBridge bridge)
	{
		var factory = _playerFactoryCore;

		var ctx = await PlayerContext.CreateAsync(bridge.playerView, _objectFactory, default);
		ctx.сamera.AttachTo(ctx.view.CameraRoot);

		var controllers = factory.GetPlayerBaseControllers(ctx.сonfig, bridge.playerView, ctx.input, ctx.сamera);

		foreach (var baseController in controllers)
		{
			ctx.AddController(baseController);
			await _lifecycle.RegisterAsync(baseController);
		}

		_ownerContexts[bridge.OwnerClientId] = ctx;
	}
}