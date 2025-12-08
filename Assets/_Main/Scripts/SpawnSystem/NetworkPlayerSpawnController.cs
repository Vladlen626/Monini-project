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
	private readonly LifecycleService _lifecycle;
	private readonly GameModelContext _gameModelContext;
	private NetworkModel _networkModel => _gameModelContext.NetworkModel;
	
	private readonly HashSet<int> _connectedClientIds = new();

	public NetworkPlayerSpawnController(
		INetworkService networkService,
		INetworkConnectionEvents connectionEvents,
		IObjectFactory objectFactory,
		LifecycleService lifecycle,
		GameModelContext gameModelContext)
	{
		_networkService = networkService;
		_connection = connectionEvents;
		_objectFactory = objectFactory as INetworkObjectFactory;
		_lifecycle = lifecycle;
		_gameModelContext = gameModelContext;
	}

	public void Activate()
	{
		_connection.OnRemoteClientConnected += OnRemoteClientConnected;
		_connection.OnRemoteClientDisconnected += OnRemoteClientDisconnected;
		_connection.OnRemoteClientLoadedStartScenes += OnRemoteClientLoadedStartScenes;
	}

	public void Deactivate()
	{
		_connection.OnRemoteClientConnected -= OnRemoteClientConnected;
		_connection.OnRemoteClientDisconnected -= OnRemoteClientDisconnected;
		_connection.OnRemoteClientLoadedStartScenes -= OnRemoteClientLoadedStartScenes;
	}

	// ========================
	// SERVER EVENTS
	// ========================

	private void OnRemoteClientConnected(int clientId)
	{
		if (!_networkService.IsServer)
		{
			return;
		}

		_connectedClientIds.Add(clientId);
	}

	private void OnRemoteClientDisconnected(int clientId)
	{
		_connectedClientIds.Remove(clientId);

		if (_networkModel.ownerContexts.TryGetValue(clientId, out var ctx))
		{
			foreach (var c in ctx.Controllers)
			{
				_lifecycle.Unregister(c);
			}

			ctx.Dispose();
			_networkModel.RemovePlayerContext(clientId);
		}
	}

	private void OnRemoteClientLoadedStartScenes(int clientId)
	{
		if (!_networkService.IsServer)
		{
			return;
		}

		SpawnPlayerForCurrentScene(clientId).Forget();
	}

	// ========================
	// PUBLIC API
	// ========================
	public async UniTask SpawnAllPlayersForCurrentScene()
	{
		if (!_networkService.IsServer)
		{
			return;
		}

		var ctx = _gameModelContext.SceneContext;
		if (ctx == null || ctx.PlayerSpawnPoints == null || ctx.PlayerSpawnPoints.Length == 0)
		{
			return;
		}

		foreach (int clientId in _connectedClientIds)
		{
			if (!_networkModel.ownerContexts.ContainsKey(clientId))
			{
				await SpawnPlayerForCurrentScene(clientId);
			}
		}
	}

	public async UniTask SpawnPlayerForCurrentScene(int clientId)
	{
		if (!_networkService.IsServer)
		{
			return;
		}

		if (_networkModel.ownerContexts.ContainsKey(clientId))
		{
			return;
		}

		var sceneCtx = _gameModelContext.SceneContext;
		if (sceneCtx == null ||
		    sceneCtx.PlayerSpawnPoints == null ||
		    sceneCtx.PlayerSpawnPoints.Length == 0)
		{
			return;
		}

		int index = clientId % sceneCtx.PlayerSpawnPoints.Length;
		var spawnPoint = sceneCtx.PlayerSpawnPoints[index];

		var connection = _networkService.GetClientConnection(clientId);
		if (connection == null)
		{
			return;
		}

		var nob = await _objectFactory.CreateNetworkAsync(
			ResourcePaths.Characters.Player,
			spawnPoint.position,
			spawnPoint.rotation,
			connection,
			_gameModelContext.PersistentSceneContext.scene);

		var view = nob.GetComponent<PlayerView>();
		var bridge = nob.GetComponent<PlayerNetworkBridge>();
		var serverCtx = PlayerContext.Server.Create(clientId, view, bridge);
		
		foreach (var serverCtxController in serverCtx.Controllers)
		{
			await _lifecycle.RegisterAsync(serverCtxController);
		}

		_networkModel.AddPlayerContext(clientId, serverCtx);
		serverCtx.Model.SetPlayerName($"Player {clientId}");
	}

	public async UniTask RespawnAllPlayers(Transform[] spawnPoints)
	{
		if (!_networkService.IsServer)
		{
			return;
		}

		if (spawnPoints == null || spawnPoints.Length == 0)
		{
			return;
		}

		foreach (var kvp in _networkModel.ownerContexts)
		{
			int clientId = kvp.Key;
			var ctx = kvp.Value;

			int index = clientId % spawnPoints.Length;
			var point = spawnPoints[index];

			ctx.Bridge.Server_TeleportOwner(point.position, point.rotation);
		}
		
		await UniTask.Yield();
	}
}