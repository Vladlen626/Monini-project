using System.Collections.Generic;
using _Main.Scripts.Core;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services;
using UnityEngine.SceneManagement;

namespace _Main.Scripts.Player.Network
{
	public class GameFlowController : IBaseController, IActivatable
	{
		private readonly ISceneService _sceneService;
		private readonly ILoggerService _logger;
		private readonly INetworkService _network;
		private readonly ISceneFlowService _sceneFlow;
		private readonly GameModelContext _modelContext;
		private readonly LifecycleService _lifecycle;
		private readonly NetworkPlayerSpawnController _spawn;

		private readonly List<IBaseController> _sceneControllers = new();

		private string _currentEnvironmentScene;
		private bool _isChangingScene;

		public GameFlowController(
			ServiceLocator services,
			GameModelContext modelContext,
			NetworkPlayerSpawnController spawn)
		{
			_modelContext = modelContext;
			_spawn = spawn;

			_sceneService = services.Get<ISceneService>();
			_sceneFlow = services.Get<ISceneFlowService>();
			_lifecycle = services.Get<LifecycleService>();
			_logger = services.Get<ILoggerService>();
			_network = services.Get<INetworkService>();
		}

		public void Activate()
		{
			_sceneFlow.OnSceneChangeRequested += OnSceneChange;
		}

		public void Deactivate()
		{
			_sceneFlow.OnSceneChangeRequested -= OnSceneChange;
		}

		private void OnSceneChange(string sceneName)
		{
			if (!_network.IsServer)
			{
				return;
			}

			ChangeScene(sceneName).Forget();
		}

		private async UniTask ChangeScene(string sceneName)
		{
			if (_isChangingScene)
			{
				return;
			}
			
			// Clear previous scene controllers
			foreach (var ctrl in _sceneControllers)
			{
				_lifecycle.Unregister(ctrl);
			}

			_sceneControllers.Clear();

			_isChangingScene = true;
			_logger.Log($"[GameFlow] Load environment: {sceneName}");

			// Setup scene and unload previous
			if (!string.IsNullOrEmpty(_currentEnvironmentScene))
			{
				var persistent = SceneManager.GetSceneByName(SceneNames.PersistentScene);
				if (persistent.IsValid())
				{
					SceneManager.SetActiveScene(persistent);
				}
				
				await _sceneService.UnloadSceneAsync(_currentEnvironmentScene);
			}

			await _sceneService.LoadEnvironmentSceneAsync(sceneName, false);
			_currentEnvironmentScene = sceneName;
			
			await UniTask.Yield();
			
			// Get scene context
			if (!_sceneService.TryGetSceneContext(sceneName, out var context))
			{
				_logger.LogError($"[GameFlow] SceneContext not found: {sceneName}");
				_isChangingScene = false;
				return;
			}

			_modelContext.SetSceneContext(context);
			
			// Register new scene controllers
			
			_sceneControllers.AddRange(GetSceneControllersByType(context.SceneType));

			foreach (var area in context.NextAreaNetworkBehaviours)
			{
				var controller = new NetworkPlayerNextAreaController(area, _network, _sceneFlow, _modelContext);
				_sceneControllers.Add(controller);
			}
			
			foreach (var sceneController in _sceneControllers)
			{
				await _lifecycle.RegisterAsync(sceneController);
			}

			// Spawn players
			await UniTask.Yield();
			await _spawn.SpawnAllPlayersForCurrentScene();
			await UniTask.Yield();
			await _spawn.RespawnAllPlayers(context.PlayerSpawnPoints);

			_isChangingScene = false;
		}


		private List<IBaseController> GetSceneControllersByType(SceneType type)
		{
			var controllers = new List<IBaseController>();
			
			
			switch (type)
			{
				case SceneType.Hub:
					break;
				case SceneType.Extraction:
					controllers.AddRange(SceneControllersFactory.GetExtractionSceneControllers(_modelContext, _network));
					break;
			}
			
			return controllers;
		}
	}
}
