using _Main.Scripts.CameraFX._Main.Scripts.Player;
using _Main.Scripts.Core.Services;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services;
using PlatformCore.Services.Audio;
using PlatformCore.Services.Factory;
using UnityEngine;
using IObjectFactory = PlatformCore.Services.Factory.IObjectFactory;

namespace _Main.Scripts.Player
{
	public class PlayerFactory
	{
		private readonly ServiceLocator _serviceLocator;

		public PlayerFactory(ServiceLocator serviceLocator)
		{
			_serviceLocator = serviceLocator;
		}

		public async UniTask<PlayerView> CreatePlayerView(Vector3 spawnPosition)
		{
			var objectFactory = _serviceLocator.Get<IObjectFactory>();
			var cameraService = _serviceLocator.Get<ICameraService>();

			var playerView = await objectFactory.CreateAsync<PlayerView>(ResourcePaths.Characters.Player,
				spawnPosition, Quaternion.identity);
			
			cameraService.AttachTo(playerView.CameraRoot);

			return playerView;
		}

		public IBaseController[] GetPlayerBaseControllers(PlayerConfig playerConfig, PlayerView playerView)
		{
			var inputService = _serviceLocator.Get<IInputService>();
			var cameraService = _serviceLocator.Get<ICameraService>();
			var audioService = _serviceLocator.Get<IAudioService>();
			
			var playerMovementController = new PlayerMovementController(inputService, playerConfig, playerView,
				cameraService.GetCameraTransform());
			
			return new IBaseController[]
			{
				playerMovementController,
				new PlayerCameraController(_serviceLocator, playerView),
				new PlayerAnimationController(inputService, playerConfig, playerView),
			};
		}
	}
}