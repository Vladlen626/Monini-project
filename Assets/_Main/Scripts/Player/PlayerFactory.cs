using _Main.Scripts.CameraFX._Main.Scripts.Player;
using _Main.Scripts.Core.Services;
using _Main.Scripts.Player.Controllers;
using PlatformCore.Core;
using PlatformCore.Services;
using UnityEngine;

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
				new PlayerMovementController(input, model.config, view, camera.GetCameraTransform(), model);
			var charController = view.GetComponent<CharacterController>();
			var stateMachine = new PlayerStateMachine(model, view, charController);
			
			return new IBaseController[]
			{
				movementController,
				new PlayerStateController(model, stateMachine),
				new PlayerFlatController(model, view),
				new PlayerSlamBounceController(input, movementController, view, camera, model),
				new PlayerCameraController(camera, input, view),
				new PlayerAnimationController(input, model.config, view),
			};
		}
	}
}