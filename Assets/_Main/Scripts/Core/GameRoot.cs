using _Main.Scripts.Core.Services;
using _Main.Scripts.Player;
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
		protected override void RegisterServices(GameContext context)
		{
			Debug.Log("[GameRoot] Register services...");

			var logger = new LoggerService();
			var resourcesService = new ResourceService(logger);
			var objectFactory = new ObjectFactory(resourcesService, logger);
			var audioService = new AudioBaseService(logger);
			var uiService = new UIBaseService(logger, resourcesService, context.StaticCanvas, context.DynamicCanvas);
			var cursorService = new CursorService(uiService);
			var splashScreenService = new SplashScreenService(uiService);
			var sceneService = new SceneService(logger);

			//Network
			var networkService = new NetworkService();

			//Register
			_serviceLocator.Register<ILoggerService, LoggerService>(logger);
			_serviceLocator.Register<IResourceService, ResourceService>(resourcesService);
			_serviceLocator.Register<IObjectFactory, ObjectFactory>(objectFactory);
			_serviceLocator.Register<IAudioService, AudioBaseService>(audioService);
			_serviceLocator.Register<IUIService, UIBaseService>(uiService);
			_serviceLocator.Register<ICursorService, CursorService>(cursorService);
			_serviceLocator.Register<ISplashScreenService, SplashScreenService>(splashScreenService);
			_serviceLocator.Register<ISceneService, SceneService>(sceneService);

			//Network Services Register
			_serviceLocator.Register<INetworkService, NetworkService>(networkService);

			Debug.Log("[GameRoot] Services finally registered.!");
		}

		protected override async UniTask LaunchGameAsync(GameContext context)
		{
			var splash = _serviceLocator.Get<ISplashScreenService>();
			var scene = _serviceLocator.Get<ISceneService>();
			var audio = _serviceLocator.Get<IAudioService>();
			var ui = _serviceLocator.Get<IUIService>();
			var cursor = _serviceLocator.Get<ICursorService>();
			var network = _serviceLocator.Get<INetworkService>();
			
			cursor.UnlockCursor();

			await scene.LoadSceneAsync(SceneNames.Hub, ApplicationCancellationToken);

			var networkControllers = new IBaseController[]
			{
				new NetworkPlayerSpawnController(_serviceLocator, _lifecycle),
			};

			foreach (var controller in networkControllers)
			{
				await _lifecycle.RegisterAsync(controller);
			}

#if UNITY_EDITOR
			var tags = Unity.Multiplayer.Playmode.CurrentPlayer.ReadOnlyTags();
			if (tags is { Length: > 0 })
			{
				var tag = tags[0];
				if (tag == "Client") // Тег твоего второго игрока
				{
					network.StartClient();
				}
				else
				{
					network.StartHost();
				}
			}
			else
			{
				network.StartHost();
			}
#else
    network.StartHost(); // обычный билд
#endif
	
			cursor.LockCursor();
		}
	}

	public static class SceneNames
	{
		public const string Hub = "hub";
	}
}