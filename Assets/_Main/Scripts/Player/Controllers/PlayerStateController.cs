using System.Collections.Generic;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

namespace _Main.Scripts.Player.Controllers
{
	public class PlayerStateController : IBaseController, IActivatable 
	{
		private readonly PlayerModel _model;
		private readonly PlayerStateMachine _machine;
		private readonly PlayerNetworkBrain _brain;

		private static readonly HashSet<PlayerState> _slamTransitionableStates = new()
		{
			PlayerState.Normal,
			PlayerState.Slam
		};

		public PlayerStateController(
			PlayerModel model,
			PlayerStateMachine machine,
			PlayerNetworkBrain brain)
		{
			_model = model;
			_machine = machine;
			_brain = brain;
		}

		public void Activate()
		{
			ChangeState(PlayerState.Normal);
			if (_brain)
			{
				_brain.OnStartDiving += OnStartDivingHandler;
				_brain.OnStopDiving += OnStopDivingHandler;
			}
		}

		public void Deactivate()
		{
			if (_brain)
			{
				_brain.OnStartDiving -= OnStartDivingHandler;
				_brain.OnStopDiving -= OnStopDivingHandler;
			}
		}

		private void OnStartDivingHandler()
		{
			if (_slamTransitionableStates.Contains(_model.state))
			{
				ChangeState(PlayerState.Slam);
			}
		}
		
		private void OnStopDivingHandler()
		{
			if (_slamTransitionableStates.Contains(_model.state))
			{
				ChangeState(PlayerState.Normal);
			}
		}

		private void ChangeState(PlayerState newState)
		{
			_model.SetState(newState);
			_machine.ChangeState(newState);
		}
	}
}