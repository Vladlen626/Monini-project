using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using UnityEngine;

namespace _Main.Scripts.Player.Controllers
{
	public class ClientPlayerStateController : IBaseController, IActivatable
	{
		private readonly PlayerStateMachine _machine;
		private readonly PlayerNetworkBridge _bridge;

		public ClientPlayerStateController(PlayerNetworkBridge bridge, PlayerView view)
		{
			_bridge = bridge;
			_machine = new PlayerStateMachine(view, view.GetComponent<CharacterController>());
		}

		public void Activate()
		{
			_bridge.State.OnChange += OnChangeStateHandler;
			_machine.ChangeState(_bridge.State.Value);
		}

		public void Deactivate()
		{
			_bridge.State.OnChange -= OnChangeStateHandler;
		}

		private void OnChangeStateHandler(PlayerState prev, PlayerState next, bool asServer)
		{
			_machine.ChangeState(next);
		}
	}
}