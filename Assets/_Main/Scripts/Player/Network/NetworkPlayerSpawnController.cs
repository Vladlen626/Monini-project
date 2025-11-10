using System.Collections.Generic;
using System.Threading;
using _Main.Scripts.Core;
using _Main.Scripts.Player;
using Cysharp.Threading.Tasks;
using FishNet.Object;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.Factory;
using UnityEngine;

public sealed class NetworkPlayerSpawnController : IBaseController, IActivatable
{
	private readonly INetworkService _networkService;
	private readonly INetworkConnectionEvents _connection;
	private readonly IObjectFactory _objectFactory;
	private readonly PlayerFactory _playerFactory;
	private readonly LifecycleManager _lifecycle;

	private readonly Dictionary<int, PlayerContext> _ownerContexts = new();

	public NetworkPlayerSpawnController(
		INetworkService networkService,
		INetworkConnectionEvents connectionEvents,
		IObjectFactory objectFactory,
		PlayerFactory playerFactory,
		LifecycleManager lifecycle)
	{
		_networkService = networkService;
		_connection = connectionEvents;
		_objectFactory = objectFactory;
		_playerFactory = playerFactory;
		_lifecycle = lifecycle;
	}

	public void Activate()
	{
		_connection.OnLocalClientConnected += OnLocalClientConnected;
		_connection.OnLocalClientDisconnected += OnLocalClientDisconnected;
		_connection.OnRemoteClientConnected += OnRemoteClientConnected;
		_connection.OnRemoteClientDisconnected += OnRemoteClientDisconnected;
	}

	public void Deactivate()
	{
		_connection.OnLocalClientConnected -= OnLocalClientConnected;
		_connection.OnLocalClientDisconnected -= OnLocalClientDisconnected;
		_connection.OnRemoteClientConnected -= OnRemoteClientConnected;
		_connection.OnRemoteClientDisconnected -= OnRemoteClientDisconnected;
	}

	// ========== CLIENT ==========
	private void OnLocalClientConnected()
	{
	}

	private void OnLocalClientDisconnected()
	{
		foreach (var ctx in _ownerContexts.Values)
		{
			foreach (var c in ctx.Controllers)
			{
				_lifecycle.Unregister(c);
			}

			ctx.Dispose();
		}

		_ownerContexts.Clear();
	}

	// ========== SERVER ==========
	private void OnRemoteClientConnected(int clientId)
	{
		if (_ownerContexts.ContainsKey(clientId))
		{
			return;
		}

		SpawnPlayerAsync(clientId).Forget();
	}

	private void OnRemoteClientDisconnected(int clientId)
	{
		if (_ownerContexts.TryGetValue(clientId, out var ctx))
		{
			foreach (var c in ctx.Controllers)
			{
				_lifecycle.Unregister(c);
			}

			ctx.Dispose();
			_ownerContexts.Remove(clientId);
		}
	}

	// ========== SERVER: SPAWN ==========
	private async UniTask SpawnPlayerAsync(int clientId)
	{
		if (!_networkService.IsServer)
		{
			return;
		}

		var playerView = await _playerFactory.CreatePlayerView(Vector3.zero);
		var bridge = playerView.GetComponent<PlayerNetworkBridge>();
		bridge.Initialize(playerView);
		
		var nob = bridge.GetComponent<NetworkObject>();
		var connection = _networkService.GetClientConnection(clientId);
		nob.Spawn(nob);
		await UniTask.WaitUntil(() => nob.NetworkManager != null && nob.IsSpawned);
		await UniTask.Yield(); 
		nob.GiveOwnership(connection);

		var ctx = await PlayerContext.Server.CreateAsync(
			playerView, _objectFactory, _playerFactory, CancellationToken.None);

		foreach (var c in ctx.Controllers)
		{
			await _lifecycle.RegisterAsync(c);
		}

		_ownerContexts[clientId] = ctx;
	}
}