using _Main.Scripts.Player.StateMachine.States;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using UnityEngine;

namespace _Main.Scripts.Player.Controllers
{
	public class ServerPlayerSyncController : IBaseController, IActivatable
	{
		private readonly PlayerModel _model;
		private readonly PlayerStateMachine _machine;
		private readonly PlayerNetworkBridge _bridge;

		public ServerPlayerSyncController(PlayerContext.Server context)
		{
			_model = context.Model;
			_bridge = context.Bridge;
			_machine = new PlayerStateMachine(context.View, context.View.GetComponent<CharacterController>());
		}

		public void Activate()
		{
			_bridge.State.OnChange += OnStateChangedHandler;
			_model.OnCrumbValueChanged += OnCrumbValueChanged;
			_model.OnPlayerNameChanged += ModelOnPlayerNameChangedHandler;
		}

		public void Deactivate()
		{
			_bridge.State.OnChange -= OnStateChangedHandler;
			_model.OnCrumbValueChanged -= OnCrumbValueChanged;
			_model.OnPlayerNameChanged -= ModelOnPlayerNameChangedHandler;
		}

		private void ModelOnPlayerNameChangedHandler(string newName)
		{
			_bridge.PlayerName.Value = newName;
			_bridge.PlayerName.DirtyAll();
		}

		private void OnStateChangedHandler(PlayerState state, PlayerState next, bool asServer)
		{
			_machine.ChangeState(next);
			_model.SetState(next);
		}

		private void OnCrumbValueChanged(int crumbsValue)
		{
			_bridge.CrumbsCount.Value = crumbsValue;
			_bridge.CrumbsCount.DirtyAll();
		}
	}
}