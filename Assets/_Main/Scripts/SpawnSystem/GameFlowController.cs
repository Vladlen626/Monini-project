using System.Collections.Generic;
using _Main.Scripts.Core;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services;

namespace _Main.Scripts.Player.Network
{
	public class GameFlowController : IBaseController, IActivatable
	{
		private readonly ISceneService _sceneService;
		private readonly ILoggerService _loggerService;
		private readonly INetworkService _networkService;
		private readonly ISceneFlowService _sceneFlowService;
		private readonly GameModelContext _model;
		private readonly LifecycleService _lifecycle;
		private readonly NetworkPlayerSpawnController _spawnController;

		private readonly List<IBaseController> _sceneControllers = new List<IBaseController>();

		private string _currentAdditiveScene;
		private bool _isChangingScene;

		public GameFlowController(
			ServiceLocator services,
			GameModelContext model,
			NetworkPlayerSpawnController spawnController)
		{
			_model = model;
			_spawnController = spawnController;

			_sceneService = services.Get<ISceneService>();
			_sceneFlowService = services.Get<ISceneFlowService>();
			_lifecycle = services.Get<LifecycleService>();
			_loggerService = services.Get<ILoggerService>();
			_networkService = services.Get<INetworkService>();
		}

		public void Activate()
		{
			_sceneFlowService.OnSceneChangeRequested += SceneChangeRequestHandler;
		}

		public void Deactivate()
		{
			_sceneFlowService.OnSceneChangeRequested -= SceneChangeRequestHandler;
		}

		private void SceneChangeRequestHandler(string sceneName)
		{
			ChangeScene(sceneName).Forget();
		}

		private async UniTask ChangeScene(string sceneName)
		{
			if (_isChangingScene)
				return;

			_isChangingScene = true;
			_loggerService.Log($"[GameFlow] ChangeScene → {sceneName}");

			// ----- UNLOAD OLD -----
			if (_currentAdditiveScene != null)
			{
				await _sceneService.UnloadSceneAsync(_currentAdditiveScene);
			}

			// ----- LOAD NEW -----
			await _sceneService.LoadSceneAsync(sceneName);
			_currentAdditiveScene = sceneName;

			// ----- WAIT FOR CONTEXT -----
			SceneContext ctx = null;

			for (int i = 0; i < 100; i++)
			{
				if (_sceneService.TryGetSceneContext(sceneName, out ctx))
					break;

				await UniTask.Yield();
			}

			if (ctx == null)
			{
				_loggerService.LogError($"[GameFlow] SceneContext NOT FOUND: {sceneName}");
			}

			_model.SceneContext = ctx;

			// ----- REMOVE OLD SCENE CONTROLLERS -----
			foreach (var ctrl in _sceneControllers)
				_lifecycle.Unregister(ctrl);

			_sceneControllers.Clear();

			// ----- CREATE NEW SCENE CONTROLLERS -----
			if (ctx != null)
			{
				foreach (var area in ctx.NextAreaNetworkBehaviours)
				{
					var c = new NetworkPlayerNextAreaController(area, _networkService, _sceneFlowService);
					await _lifecycle.RegisterAsync(c);
					_sceneControllers.Add(c);
				}

				// TELEPORT PLAYERS
				await _spawnController.RespawnAllPlayers(ctx.PlayerSpawnPoints);
			}

			_isChangingScene = false;
		}
	}
}