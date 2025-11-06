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
			var inputService = new InputBaseService();
			var resourcesService = new ResourceService(logger);
			var objectFactory = new ObjectFactory(resourcesService, logger);
			var cameraService = new CameraAsyncService(objectFactory);
			var audioService = new AudioBaseService(logger);
			var uiService = new UIBaseService(logger, resourcesService, context.StaticCanvas, context.DynamicCanvas);
			var cursorService = new CursorService(uiService);
			var splashScreenService = new SplashScreenService(uiService);
			var sceneService = new SceneService(logger);

			Services.Register<ILoggerService, LoggerService>(logger);
			Services.Register<IInputService, InputBaseService>(inputService);
			Services.Register<IResourceService, ResourceService>(resourcesService);
			Services.Register<IObjectFactory, ObjectFactory>(objectFactory);
			Services.Register<ICameraService, CameraAsyncService>(cameraService);
			Services.Register<ICameraShakeService, CameraAsyncService>(cameraService);
			Services.Register<IAudioService, AudioBaseService>(audioService);
			Services.Register<IUIService, UIBaseService>(uiService);
			Services.Register<ICursorService, CursorService>(cursorService);
			Services.Register<ISplashScreenService, SplashScreenService>(splashScreenService);
			Services.Register<ISceneService, SceneService>(sceneService);

			Debug.Log("[GameRoot] Services finally registered.!");
		}

		protected override async UniTask LaunchGameAsync(GameContext context)
		{
			var splash = Services.Get<ISplashScreenService>();
			var input = Services.Get<IInputService>();
			var scene = Services.Get<ISceneService>();
			var audio = Services.Get<IAudioService>();
			var ui = Services.Get<IUIService>();
			var cursor = Services.Get<ICursorService>();

			input.DisableAllInputs();
			cursor.UnlockCursor();

			await scene.LoadSceneAsync(SceneNames.Hub, ApplicationCancellationToken);
			Vector3 spawn = Vector3.zero;

			var playerFactory = new PlayerFactory(Services);
			var playerModel = new PlayerConfig();
			var playerView = await playerFactory.CreatePlayerView(spawn);

			await UniTask.WhenAll(playerFactory.GetPlayerBaseControllers(playerModel, playerView)
				.Select(c => Lifecycle.RegisterAsync(c)));

			input.EnableAllInputs();
			cursor.LockCursor();
		}
	}

	public static class SceneNames
	{
		public const string Hub = "hub";
	}
}