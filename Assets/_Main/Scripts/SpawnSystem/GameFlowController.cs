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
		private readonly GameModelContext _model;
		private readonly LifecycleService _lifecycle;
		private readonly NetworkPlayerSpawnController _spawn;

		private readonly List<IBaseController> _sceneControllers = new();

		private string _currentEnvironmentScene;
		private bool _isChangingScene;

		public GameFlowController(
			ServiceLocator services,
			GameModelContext model,
			NetworkPlayerSpawnController spawn)
		{
			_model = model;
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

			_isChangingScene = true;
			_logger.Log($"[GameFlow] Load environment: {sceneName}");

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
			
			if (!_sceneService.TryGetSceneContext(sceneName, out var context))
			{
				_logger.LogError($"[GameFlow] SceneContext not found: {sceneName}");
				_isChangingScene = false;
				return;
			}

			_model.SceneContext = context;

			foreach (var ctrl in _sceneControllers)
			{
				_lifecycle.Unregister(ctrl);
			}

			_sceneControllers.Clear();

			foreach (var area in context.NextAreaNetworkBehaviours)
			{
				var controller = new NetworkPlayerNextAreaController(area, _network, _sceneFlow, _model);
				await _lifecycle.RegisterAsync(controller);
				_sceneControllers.Add(controller);
			}

			await UniTask.Yield();
			await _spawn.SpawnAllPlayersForCurrentScene();
			await UniTask.Yield();
			await _spawn.RespawnAllPlayers(context.PlayerSpawnPoints);

			_isChangingScene = false;
		}
	}
}
