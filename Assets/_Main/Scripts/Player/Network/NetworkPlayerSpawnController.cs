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
	private readonly INetworkObjectFactory _objectFactory;
	private readonly PlayerFactory _playerFactory;
	private readonly LifecycleService _lifecycle;

	private readonly Dictionary<int, PlayerContext> _ownerContexts = new();

	public NetworkPlayerSpawnController(
		INetworkService networkService,
		INetworkConnectionEvents connectionEvents,
		IObjectFactory objectFactory,
		PlayerFactory playerFactory,
		LifecycleService lifecycle)
	{
		_networkService = networkService;
		_connection = connectionEvents;
		_objectFactory = objectFactory as INetworkObjectFactory;
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

		var connection = _networkService.GetClientConnection(clientId);
		await _objectFactory.CreateNetworkAsync(ResourcePaths.Characters.Player,
			Vector3.zero, Quaternion.identity, connection);
	}
}