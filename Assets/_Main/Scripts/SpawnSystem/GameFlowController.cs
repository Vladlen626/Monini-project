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

		public GameFlowController(ServiceLocator serviceLocator, GameModelContext model,
			NetworkPlayerSpawnController spawnController)
		{
			_model = model;
			_sceneService = serviceLocator.Get<ISceneService>();
			_sceneFlowService = serviceLocator.Get<ISceneFlowService>();
			_lifecycle = serviceLocator.Get<LifecycleService>();
			_loggerService = serviceLocator.Get<ILoggerService>();
			_networkService = serviceLocator.Get<INetworkService>();
			_spawnController = spawnController;
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
			{
				return;
			}

			_isChangingScene = true;

			if (_currentAdditiveScene != null)
			{
				await UnloadOldAdditiveScene();
			}

			await _sceneService.LoadSceneAsync(sceneName);
			_currentAdditiveScene = sceneName;
			if (!_sceneService.TryGetSceneContext(sceneName, out SceneContext ctx))
			{
				_loggerService.LogError($"[GameFlowController] TryGetSceneContext failed: {sceneName}");
			}

			_model.SceneContext = ctx;

			foreach (var sceneController in _sceneControllers)
			{
				_lifecycle.Unregister(sceneController);
			}

			_sceneControllers.Clear();

			foreach (var zone in ctx.NextAreaNetworkBehaviours)
			{
				var controller = new NetworkPlayerNextAreaController(zone, _networkService, _sceneFlowService);
				await _lifecycle.RegisterAsync(controller);
				_sceneControllers.Add(controller);
			}

			await _spawnController.RespawnAllPlayers(ctx.PlayerSpawnPoints);
			_isChangingScene = false;
		}

		private UniTask UnloadOldAdditiveScene()
		{
			return _sceneService.UnloadSceneAsync(_currentAdditiveScene);
		}
	}
}