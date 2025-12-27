using _Main.Scripts.Core.Services;
using _Main.Scripts.Player.Controllers;
using _Main.Scripts.Player.Network;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure;
using PlatformCore.Services;
using PlatformCore.Services.Audio;
using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;
using PlatformCore.Services.UI.SplashScreen;
using UnityEngine;

namespace _Main.Scripts.Core
{
	public class GameRoot : BaseGameRoot
	{
		protected override void RegisterServices(PersistentSceneContext context)
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
			var multiplayerService = new MultiplayerRelayService(logger);

			//NetworkDepended
			var objectFactory = new NetworkObjectFactory(resourcesService, logger, networkService);

			//Register
			_serviceLocator.Register<ISceneFlowService, SceneFlowService>(sceneFlowService);
			_serviceLocator.Register<ILoggerService, LoggerService>(logger);
			_serviceLocator.Register<IResourceService, ResourceService>(resourcesService);
			_serviceLocator.Register<IObjectFactory, NetworkObjectFactory>(objectFactory);
			_serviceLocator.Register<IAudioService, AudioBaseService>(audioService);
			_serviceLocator.Register<IUIService, UIBaseService>(uiService);
			_serviceLocator.Register<ICursorService, CursorService>(cursorService);
			_serviceLocator.Register<ISplashScreenService, SplashScreenService>(splashScreenService);
			_serviceLocator.Register<ISceneService, FishNetSceneService>(sceneService);

			//Network Services Register
			_serviceLocator.Register<INetworkService, NetworkService>(networkService);
			_serviceLocator.Register<IMultiplayerService, MultiplayerRelayService>(multiplayerService);

			Debug.Log("[GameRoot] Services finally registered.!");
		}

		protected override async UniTask LaunchGameAsync(PersistentSceneContext context)
		{
			var cursor = _serviceLocator.Get<ICursorService>();
			var networkService = _serviceLocator.Get<INetworkService>();
			var multiplayerService = _serviceLocator.Get<IMultiplayerService>();
			var objectFactory = _serviceLocator.Get<IObjectFactory>();
			var sceneFlowService = _serviceLocator.Get<ISceneFlowService>();
			var gameModelContext = new GameModelContext(context);
			
			cursor.UnlockCursor();
			var networkConnectionController = new NetworkConnectionController();
			var networkSpawnController = new NetworkPlayerSpawnController(networkService, networkConnectionController,
				objectFactory, _lifecycle, gameModelContext);
			

			var baseControllers = new IBaseController[]
			{
				networkConnectionController,
				networkSpawnController,
				new GameFlowController(_serviceLocator, gameModelContext, networkSpawnController),
				new PlayerClientInitController(networkConnectionController, objectFactory, _lifecycle),
			};

			foreach (var controller in baseControllers)
			{
				await _lifecycle.RegisterAsync(controller);
			}

			await StartNetworkWithRelayAsync(networkService, multiplayerService);
			await UniTask.WaitUntil(() => networkService.IsServerStarted || networkService.IsClientStarted);

			sceneFlowService.RequestSceneChange(SceneNames.Hub);
			cursor.LockCursor();
		}

		private async UniTask StartNetworkWithRelayAsync(INetworkService networkService, IMultiplayerService multiplayerService)
		{
#if UNITY_EDITOR
			var tags = Unity.Multiplayer.Playmode.CurrentPlayer.ReadOnlyTags();
			if (tags is { Length: > 0 })
			{
				var tag = tags[0];
				if (tag == "Client")
				{
					// Для клиента нужен join code (запроси у пользователя)
					Debug.LogWarning("[NETWORK] Client tag detected, but no join code provided!");
					return;
				}
				else
				{
					// Host
					Debug.Log("[NETWORK] Starting Host with Relay...");
					string joinCode = await multiplayerService.CreateRoomAsync("TestRoom", 4);
					Debug.Log($"[NETWORK] Join code: {joinCode}");
					networkService.StartHost();
				}
			}
			else
			{
				// Host по умолчанию
				Debug.Log("[NETWORK] Starting Host with Relay...");
				string joinCode = await multiplayerService.CreateRoomAsync("TestRoom", 4);
				Debug.Log($"[NETWORK] Join code: {joinCode}");
				networkService.StartHost();
			}
#else
    // Production билд — Host
    Debug.Log("[NETWORK] Starting Host with Relay...");
    string joinCode = await multiplayerService.CreateRoomAsync("ProductionRoom", 4);
    Debug.Log($"[NETWORK] Join code: {joinCode}");
    networkService.StartHost();
#endif
		}

	}
}