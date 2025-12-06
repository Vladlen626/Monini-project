using PlatformCore.Core;
using UnityEngine;

namespace _Main.Scripts.Player.Controllers
{
	public class ClientPlayerStateController : IBaseController
	{
		private readonly PlayerStateMachine _machine;
		private readonly PlayerNetworkBridge _bridge;

		public ClientPlayerStateController(PlayerNetworkBridge bridge, PlayerView view)
		{
			_bridge = bridge;
			_machine = new PlayerStateMachine(view, view.GetComponent<CharacterController>());
		}

		public void ChangeState(PlayerState state)
		{
			_machine.ChangeState(state);
			_bridge.Server_ChangeState(state);
		}
	}
}