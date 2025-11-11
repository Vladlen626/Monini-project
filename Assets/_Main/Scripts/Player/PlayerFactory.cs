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
		public IBaseController[] GetPlayerBaseControllers(
			PlayerConfig cfg,
			PlayerView view,
			IInputService input,
			ICameraService camera)
		{
			var movementController = new PlayerMovementController(input, cfg, view, camera.GetCameraTransform());
			
			return new IBaseController[]
			{
				movementController,
				new PlayerSlamBounceController(input, movementController, view, camera, cfg),
				new PlayerCameraController(camera, input, view),
				new PlayerAnimationController(input, cfg, view),
			};
		}

		public IBaseController[] GetServerControllers(
			PlayerConfig cfg,
			PlayerView view)
		{
			return new IBaseController[]
			{
			};
		}
	}
}