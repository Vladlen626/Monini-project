using _Main.Scripts.Core.Services;
using _Main.Scripts.Interactables.Crumb.Controllers;
using _Main.Scripts.Player;
using _Main.Scripts.Player.Network;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure;
using PlatformCore.Services;
using PlatformCore.Services.Audio;
using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;
using PlatformCore.Services.UI.SplashScreen;
using Unity.Collections;
using UnityEngine;

namespace _Main.Scripts.Core
{
	public class GameRoot : BaseGameRoot
	{
		protected override void RegisterServices(GameContext context)
		{
			Debug.Log("[GameRoot] Register services...");

			//Global
			var logger = new LoggerService();
			var resourcesService = new ResourceService(logger);
			var audioService = new AudioBaseService(logger);
			var uiService = new UIBaseService(logger, resourcesService, context.StaticCanvas, context.DynamicCanvas);
			var cursorService = new CursorService(uiService);
			var splashScreenService = new SplashScreenService(uiService);
			var sceneService = new FishNetSceneService(logger);
			var sceneFlowService = new SceneFlowService();

			//Network
			var networkService = new NetworkService();

			//NetworkDepended
			var objectFactory = new ObjectFactory(resourcesService, logger, networkService);

			//Register
			_serviceLocator.Register<ISceneFlowService, SceneFlowService>(sceneFlowService);
			_serviceLocator.Register<ILoggerService, LoggerService>(logger);
			_serviceLocator.Register<IResourceService, ResourceService>(resourcesService);
			_serviceLocator.Register<IObjectFactory, ObjectFactory>(objectFactory);
			_serviceLocator.Register<IAudioService, AudioBaseService>(audioService);
			_serviceLocator.Register<IUIService, UIBaseService>(uiService);
			_serviceLocator.Register<ICursorService, CursorService>(cursorService);
			_serviceLocator.Register<ISplashScreenService, SplashScreenService>(splashScreenService);
			_serviceLocator.Register<ISceneService, FishNetSceneService>(sceneService);

			//Network Services Register
			_serviceLocator.Register<INetworkService, NetworkService>(networkService);

			Debug.Log("[GameRoot] Services finally registered.!");
		}

		protected override async UniTask LaunchGameAsync(GameContext context)
		{
			var cursor = _serviceLocator.Get<ICursorService>();
			var network = _serviceLocator.Get<INetworkService>();
			var objectFactory = _serviceLocator.Get<IObjectFactory>();
			var sceneFlowService = _serviceLocator.Get<ISceneFlowService>();
			var gameModelContext = new GameModelContext();

			StartNetwork(network);

			await UniTask.WaitUntil(() => network.IsServerStarted);

			cursor.UnlockCursor();
			var networkConnectionController = new NetworkConnectionController();
			var networkSpawnController = new NetworkPlayerSpawnController(network, networkConnectionController,
				objectFactory, _lifecycle, gameModelContext);

			var gameFlowController = new GameFlowController(_serviceLocator, gameModelContext, networkSpawnController);

			var baseControllers = new IBaseController[]
			{
				networkConnectionController,
				networkSpawnController,
				gameFlowController
			};

			foreach (var controller in baseControllers)
			{
				await _lifecycle.RegisterAsync(controller);
			}

			sceneFlowService.RequestSceneChange(SceneNames.Hub);
			cursor.LockCursor();
		}

		private void StartNetwork(INetworkService networkService)
		{
#if UNITY_EDITOR
			var tags = Unity.Multiplayer.Playmode.CurrentPlayer.ReadOnlyTags();
			if (tags is { Length: > 0 })
			{
				var tag = tags[0];
				if (tag == "Client")
				{
					networkService.StartClient();
				}
				else
				{
					networkService.StartHost();
				}
			}
			else
			{
				networkService.StartHost();
			}
#else
    network.StartHost(); // обычный билд
#endif
		}
	}
}