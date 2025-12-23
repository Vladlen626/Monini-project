using System;
using _Main.Scripts.Core.Services;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services;
using UnityEngine;

namespace _Main.Scripts.Player
{
	public class PlayerFlatController : IBaseController, IActivatable
	{
		private readonly PlayerModel _playerModel;
		private readonly PlayerNetworkBridge _bridge;
		private readonly PlayerMovementController _movement;
		private readonly PlayerView _view;

		private bool _isFlat;
		private float _gravity;

		public PlayerFlatController(
			PlayerNetworkBridge bridge,
			PlayerMovementController movement,
			PlayerView view,
			PlayerConfig config,
			PlayerModel playerModel = null)
		{
			_playerModel = playerModel;
			_bridge = bridge;
			_movement = movement;
			_view = view;
			_gravity = config?.gravity ?? -20f;
		}

		public void Activate()
		{
			_bridge.OnSlamReceived += OnSlamReceivedHandler;
		}

		public void Deactivate()
		{
			_bridge.OnSlamReceived -= OnSlamReceivedHandler;
		}
		
		public void Simulate(float dt, PlayerInputData input)
		{
			PlayerState currentState = _bridge.State.Value;
			bool wasFlat = _isFlat;
			_isFlat = (currentState == PlayerState.Flat);

			if (_isFlat)
			{
				_movement.SuppressMovement();
			}
			else if (wasFlat)
			{
				_movement.RestoreMovement();
			}
		}

		private void OnSlamReceivedHandler()
		{
			if (_playerModel == null)
			{
				return;
			}
			
			if (_playerModel.state == PlayerState.Normal && _view.IsGrounded)
			{
				FlatStateProcess().Forget();
			}
		}
		
		private async UniTask FlatStateProcess()
		{
			_playerModel.SetState(PlayerState.Flat);
			await UniTask.Delay(5000);
			_playerModel.SetState(PlayerState.Normal);
		}
		
	}
}