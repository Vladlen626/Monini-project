using System.Collections.Generic;
using System.Threading;
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
	private readonly PlayerFactory _playerFactory;
	private readonly LifecycleManager _lifecycle;

	private Dictionary<ulong, PlayerContext> _ownerContexts = new();


	public NetworkPlayerSpawnController(ServiceLocator serviceLocator, LifecycleManager lifecycle)
	{
		_networkService = serviceLocator.Get<INetworkService>();
		_objectFactory = serviceLocator.Get<IObjectFactory>();
		_playerFactory = new PlayerFactory(serviceLocator);
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
		if (_ownerContexts.ContainsKey(clientId))
		{
			return;
		}

		SpawnPlayerAsync(clientId).Forget();
	}

	private void OnClientDisconnected(ulong clientId)
	{
		if (_ownerContexts.TryGetValue(clientId, out var ctx))
		{
			foreach (var valueController in ctx.Controllers)
			{
				_lifecycle.Unregister(valueController);
			}

			ctx.Dispose();
			_ownerContexts.Remove(clientId);
		}
	}

	//server
	private async UniTask SpawnPlayerAsync(ulong clientId)
	{
		if (!_networkService.IsServer)
		{
			return;
		}

		var playerView = await _playerFactory.CreatePlayerView(Vector3.zero);
		var bridge = playerView.GetComponent<PlayerNetworkBridge>();
		bridge.Initialize(playerView);
		bridge.NetworkObject.SpawnWithOwnership(clientId);

		var ctx = await PlayerContext.Server.CreateAsync(playerView, _objectFactory, _playerFactory,
			CancellationToken.None);
		foreach (var c in ctx.Controllers)
		{
			await _lifecycle.RegisterAsync(c);
		}

		_ownerContexts[clientId] = ctx;
	}

	//client
	private async void OnLocalPlayerSpawnedHandler(PlayerNetworkBridge bridge)
	{
		var ctx = await PlayerContext.Client.CreateAsync(bridge.playerView, _objectFactory, _playerFactory,
			CancellationToken.None);

		ctx.Camera.AttachTo(ctx.View.CameraRoot);

		foreach (var c in ctx.Controllers)
		{
			await _lifecycle.RegisterAsync(c);
		}

		_ownerContexts[bridge.OwnerClientId] = ctx;
	}
}