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
			var movementController = new PlayerMovementController(input, config, view, camera.GetCameraTransform(), bridge);
			var clientStateController = new ClientPlayerStateController(bridge, view);
			var uiService = Locator.Resolve<IUIService>();

			return new IBaseController[]
			{
				movementController,
				clientStateController,
				new PlayerSlamBounceController(input, movementController, view, camera, config, clientStateController),
				new PlayerCameraController(camera, input, view),
				new PlayerAnimationController(input, config, view, bridge),
				new PlayerDynamicContextController<UIPlayerDynamicHud>(uiService, bridge),
				new PlayerStaticContextController<UIPlayerStaticHud>(uiService, bridge),
			};
		}

		public static IBaseController[] GetPlayerServerControllers(PlayerContext.Server context)
		{
			return new IBaseController[]
			{
				new ServerPlayerSyncController(context),
				new PlayerFlatController(context.Model, context.Bridge),
			};
		}
	}
}