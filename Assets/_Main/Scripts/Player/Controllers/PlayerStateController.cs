using _Main.Scripts.Player.StateMachine.States;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using UnityEngine;

namespace _Main.Scripts.Player.Controllers
{
	public class PlayerStateController : IBaseController, IActivatable
	{
		private readonly PlayerModel _model;
		private readonly PlayerStateMachine _machine;

		public PlayerStateController(PlayerModel model, PlayerStateMachine machine)
		{
			_model = model;
			_machine = machine;
		}

		public void Activate()
		{
			_model.OnPlayerStateChanged += OnStateChanged;
			OnStateChanged(_model.State);
		}

		public void Deactivate()
		{
			_model.OnPlayerStateChanged -= OnStateChanged;
		}

		private void OnStateChanged(PlayerState state)
		{
			_machine.HandleStateChange(state);
		}
	}
}