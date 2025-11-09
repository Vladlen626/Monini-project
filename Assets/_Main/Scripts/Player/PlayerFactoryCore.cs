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
	public class PlayerFactoryCore
	{
		private readonly ServiceLocator _serviceLocator;

		public PlayerFactoryCore(ServiceLocator serviceLocator)
		{
			_serviceLocator = serviceLocator;
		}

		public async UniTask<PlayerView> CreatePlayerView(Vector3 spawnPosition)
		{
			var objectFactory = _serviceLocator.Get<IObjectFactory>();

			var playerView = await objectFactory.CreateAsync<PlayerView>(ResourcePaths.Characters.Player,
				spawnPosition, Quaternion.identity);

			return playerView;
		}

		public IBaseController[] GetPlayerBaseControllers(
			PlayerConfig cfg,
			PlayerView view,
			IInputService input,
			ICameraService camera)
		{
			return new IBaseController[]
			{
				new PlayerMovementController(input, cfg, view, camera.GetCameraTransform()),
				new PlayerCameraController(camera, input, view),
				new PlayerAnimationController(input, cfg, view),
			};
		}
	}
}