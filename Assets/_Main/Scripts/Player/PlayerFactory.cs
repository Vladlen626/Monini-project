using _Main.Scripts.CameraFX._Main.Scripts.Player;
using _Main.Scripts.Core.Services;
using _Main.Scripts.Player.Controllers;
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
			PlayerModel model,
			PlayerView view,
			IInputService input,
			ICameraService camera)
		{
			var movementController =
				new PlayerMovementController(input, model.config, view, camera.GetCameraTransform());
			var charController = view.GetComponent<CharacterController>();
			
			
			return new IBaseController[]
			{
				new PlayerStateController(model, view, charController),
				movementController,
				new PlayerSlamBounceController(input, movementController, view, camera, model),
				new PlayerCameraController(camera, input, view),
				new PlayerAnimationController(input, model.config, view),
			};
		}

		public IBaseController[] GetServerControllers(
			PlayerModel model,
			PlayerView view)
		{
			return new IBaseController[]
			{
			};
		}
	}
}