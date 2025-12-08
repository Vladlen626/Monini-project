using _Main.Scripts.CameraFX._Main.Scripts.Player;
using _Main.Scripts.Core.Services;
using _Main.Scripts.Player.Controllers;
using PlatformCore.Core;
using PlatformCore.Services;
using PlatformCore.Services.UI;

namespace _Main.Scripts.Player
{
	public static class PlayerControllersFactory
	{
		public static IBaseController[] GetPlayerBaseControllers(
			PlayerNetworkBridge bridge,
			PlayerView view,
			IInputService input,
			ICameraService camera)
		{
			var config = new PlayerConfig();
			var clientStateController = new ClientPlayerStateController(bridge, view);
			var uiService = Locator.Resolve<IUIService>();
			
			//prediction movement
			var brain = bridge.GetComponent<PlayerNetworkBrain>();
			var movementController = new PlayerMovementController(config, view, bridge);
			var slamBounceController = new PlayerSlamBounceController(movementController, view, camera, config);
			brain.Construct(movementController, slamBounceController, input, camera.GetCameraTransform());

			return new IBaseController[]
			{
				movementController,
				slamBounceController,
				clientStateController,
				new PlayerCameraController(camera, input, view),
				new PlayerAnimationController(input, config, view, bridge),
				new PlayerDynamicContextController<UIPlayerDynamicHud>(uiService, bridge),
				new PlayerStaticContextController<UIPlayerStaticHud>(uiService, bridge),
			};
		}

		public static IBaseController[] GetPlayerServerControllers(PlayerContext.Server context)
		{
			var config = new PlayerConfig();
			var movementController = new PlayerMovementController(config, context.View, context.Bridge);
			var slamBounceController = new PlayerSlamBounceController(movementController, context.View, null, config);
			
			var brain = context.Bridge.GetComponent<PlayerNetworkBrain>();
			brain.Construct(movementController, slamBounceController, null, null);
			
			return new IBaseController[]
			{
				movementController,
				slamBounceController,
				new ServerPlayerSyncController(context),
				new PlayerFlatController(context.Model, context.Bridge),
			};
		}
	}
}