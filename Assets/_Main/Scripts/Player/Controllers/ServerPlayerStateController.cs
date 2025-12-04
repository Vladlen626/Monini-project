using _Main.Scripts.Player.StateMachine.States;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using UnityEngine;

namespace _Main.Scripts.Player.Controllers
{
	public class ServerPlayerStateController : IBaseController, IActivatable
	{
		private readonly PlayerModel _model;
		private readonly PlayerStateMachine _machine;
		private readonly PlayerNetworkBridge _bridge;

		public ServerPlayerStateController(PlayerContext.Server context)
		{
			_model = context.Model;
			_bridge = context.Bridge;
			_machine = new PlayerStateMachine(context.View, context.View.GetComponent<CharacterController>());
		}

		public void Activate()
		{
			_bridge.State.OnChange += OnStateChangedHandler;
		}

		public void Deactivate()
		{
			_bridge.State.OnChange -= OnStateChangedHandler;
		}

		private void OnStateChangedHandler(PlayerState state, PlayerState next, bool asServer)
		{
			_machine.ChangeState(next);
			_model.SetState(next);
		}
	}
}