using System.Collections.Generic;
using _Main.Scripts.Core;
using _Main.Scripts.Player;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.Factory;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class NetworkPlayerSpawnController : IBaseController, IActivatable
{
	private readonly INetworkService _networkService;
	private readonly INetworkConnectionEvents _connection;
	private readonly INetworkObjectFactory _objectFactory;
	private readonly PlayerFactory _playerFactory;
	private readonly LifecycleService _lifecycle;
	private readonly GameModelContext _gameModelContext;

	private readonly Dictionary<int, PlayerContext> _ownerContexts = new();

	public NetworkPlayerSpawnController(
		INetworkService networkService,
		INetworkConnectionEvents connectionEvents,
		IObjectFactory objectFactory,
		LifecycleService lifecycle,
		GameModelContext gameModelContext,
		PlayerFactory playerFactory)
	{
		_networkService = networkService;
		_connection = connectionEvents;
		_objectFactory = objectFactory as INetworkObjectFactory;
		_lifecycle = lifecycle;
		_gameModelContext = gameModelContext;
		_playerFactory = playerFactory;
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

		var index = _networkService.PlayersCount % _gameModelContext.SceneContext.PlayerSpawnPoints.Length;
		var spawnPoint = _gameModelContext.SceneContext.PlayerSpawnPoints[index];

		var connection = _networkService.GetClientConnection(clientId);
		var nob = await _objectFactory.CreateNetworkAsync(ResourcePaths.Characters.Player,
			spawnPoint.position, Quaternion.identity, connection);
		SceneManager.MoveGameObjectToScene(nob.gameObject, SceneManager.GetSceneByName(SceneNames.preloader));
		var view = nob.GetComponent<PlayerView>();
		var serverCtx = PlayerContext.Server.Create(clientId, view, _playerFactory);
		_ownerContexts[clientId] = serverCtx;	
	}

	public async UniTask RespawnAllPlayers(Transform[] spawnPoints)
	{
		if (!_networkService.IsServer)
		{
			return;
		}

		var i = 0;
		foreach (var kvp in _ownerContexts)
		{
			var ctx = kvp.Value;

			var spawnPoint = spawnPoints[i % spawnPoints.Length];
			i++;

			var t = ctx.View.transform;
			t.position = spawnPoint.position;
			t.rotation = spawnPoint.rotation;
		}

		await UniTask.Yield();
	}
}