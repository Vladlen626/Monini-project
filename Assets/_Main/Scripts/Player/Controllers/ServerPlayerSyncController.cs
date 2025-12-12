using _Main.Scripts.Player.StateMachine.States;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using UnityEngine;

namespace _Main.Scripts.Player.Controllers
{
	public class ServerPlayerSyncController : IBaseController, IActivatable
	{
		private readonly PlayerModel _model;
		private readonly PlayerNetworkBridge _bridge;

		public ServerPlayerSyncController(PlayerContext.Server context)
		{
			_model = context.Model;
			_bridge = context.Bridge;
		}

		public void Activate()
		{
			_model.OnCrumbValueChanged += OnCrumbValueChanged;
			_model.OnPlayerNameChanged += ModelOnPlayerNameChangedHandler;
			_model.OnPlayerStateChanged += OnStateChangedHandler;
		}

		public void Deactivate()
		{
			_model.OnPlayerStateChanged -= OnStateChangedHandler;
			_model.OnCrumbValueChanged -= OnCrumbValueChanged;
			_model.OnPlayerNameChanged -= ModelOnPlayerNameChangedHandler;
		}

		private void ModelOnPlayerNameChangedHandler(string newName)
		{
			_bridge.PlayerName.Value = newName;
			_bridge.PlayerName.DirtyAll();
		}

		private void OnStateChangedHandler(PlayerState state)
		{
			_bridge.State.Value = state;
			_bridge.State.DirtyAll();
		}

		private void OnCrumbValueChanged(int crumbsValue)
		{
			_bridge.CrumbsCount.Value = crumbsValue;
			_bridge.CrumbsCount.DirtyAll();
		}
	}
}