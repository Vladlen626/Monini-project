using _Main.Scripts.CameraFX._Main.Scripts.Player;
using _Main.Scripts.Core;
using _Main.Scripts.Core.Services;
using _Main.Scripts.Player.Controllers;
using PlatformCore.Core;
using PlatformCore.Services;
using PlatformCore.Services.UI;
using UnityEngine;

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
			var multiplayerService = Locator.Resolve<IMultiplayerService>();
			var cursorService = Locator.Resolve<ICursorService>();
			var networkService = Locator.Resolve<INetworkService>();

			var brain = bridge.GetComponent<PlayerNetworkBrain>();
			var movementController = new PlayerMovementController(config, view);
			var slamBounceController = new PlayerSlamBounceController(movementController, view, bridge, camera, config);
			var flatController = new PlayerFlatController(bridge, movementController, view, config);
			brain.Construct(movementController, slamBounceController, flatController, input, camera.GetCameraTransform());

			return new IBaseController[]
			{
				movementController,
				slamBounceController,
				clientStateController,
				flatController,
				new PlayerAnimationController(input, config, view, bridge),
				new PlayerDynamicContextController<UIPlayerDynamicHud>(uiService, bridge),
				new PlayerStaticContextController<UIPlayerStaticHud>(uiService, bridge),
				new LobbyController(uiService, multiplayerService, networkService, input, cursorService),
			};
		}

		public static IBaseController[] GetPlayerServerControllers(PlayerContext.Server context)
		{
			var config = new PlayerConfig();
			var movementController = new PlayerMovementController(config, context.View);
			var slamBounceController = new PlayerSlamBounceController(movementController, context.View,  context.Bridge, null, config);
			var flatController = new PlayerFlatController(context.Bridge, movementController, context.View, config, context.Model);
			var brain = context.Bridge.GetComponent<PlayerNetworkBrain>();
			brain.Construct(movementController, slamBounceController, flatController, null, null);
			
			var machine = new PlayerStateMachine(context.View, context.View.GetComponent<CharacterController>());
			var statePrediction = new PlayerStateController(context.Model, machine, brain);
			
			return new IBaseController[]
			{
				movementController,
				slamBounceController,
				statePrediction,
				flatController,
				new ServerPlayerSyncController(context)
			};
		}
	}
}