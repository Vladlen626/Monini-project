using System;
using Cysharp.Threading.Tasks;
using _Main.Scripts.Core.Services;
using _Main.Scripts.Player.Controllers;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services;
using UnityEngine;

namespace _Main.Scripts.Player
{
	public class PlayerSlamBounceController : IBaseController, IUpdatable, IActivatable
	{
		private readonly PlayerMovementController _movement;
		private readonly PlayerView _view;
		private readonly IInputService _input;
		private readonly ICameraShakeService _shake;
		private readonly PlayerConfig _config;
		private readonly ClientPlayerStateController _stateController;

		// Состояние
		private bool _diving;
		private bool _awaitLand;
		private bool _prevE;
		private float _airTime;
		private float _cooldown;

		private class BounceConfig
		{
			public float MinAirTime = 0.03f;
			public float DiveDownSpeed = -38f;
			public float DiveExtra = -48f;

			public float ImpactRadius = 1f;
			public float DelayAfterImact = 0.075f;

			public float AfterImpactCooldown = 0.65f;
			public float SuppressJumpTime = 0.15f;

			public float ForwardBoost = 5.5f;
			public float ShakeAmp = 1.1f, ShakeDur = 0.15f;
			public string AudioImpact = "event:/ground_slam_impact";
		}

		private readonly BounceConfig _cfg = new();

		public PlayerSlamBounceController(
			IInputService input,
			PlayerMovementController movement,
			PlayerView view,
			ICameraShakeService shake,
			PlayerConfig config,
			ClientPlayerStateController stateController)
		{
			_input = input;
			_movement = movement;
			_view = view;
			_shake = shake;
			_config = config;
			_stateController = stateController;
		}
		
		public void Activate()
		{
			_view.OnLand += OnLand;
		}

		public void Deactivate()
		{
			_view.OnLand -= OnLand;
		}



		// ReSharper disable Unity.PerformanceAnalysis
		public void OnUpdate(float dt)
		{
			if (_cooldown > 0f)
			{
				_cooldown -= dt;
			}

			if (!_view.IsGrounded)
			{
				_airTime += dt;
			}
			else
			{
				_airTime = 0f;
			}

			bool ePressed = _input.IsInteract && !_prevE;
			_prevE = _input.IsInteract;

			if (!_diving && _airTime >= _cfg.MinAirTime && ePressed && _cooldown <= 0f)
			{
				StartDive();
			}
		}

		private void StartDive()
		{
			_diving = true;
			_stateController.ChangeState(PlayerState.Slam);
			_movement.RequestVerticalOverride(_cfg.DiveDownSpeed);
			_shake?.ShakeAsync(0.35f, 0.08f).Forget();
			_awaitLand = true;
		}

		private void OnLand()
		{
			if (!_awaitLand) return;
			DoImpactAndBounce().Forget();
		}

		private async UniTaskVoid DoImpactAndBounce()
		{
			DoAreaImpact();
			_shake?.ShakeAsync(_cfg.ShakeAmp, _cfg.ShakeDur).Forget();
			await UniTask.Delay(TimeSpan.FromSeconds(_cfg.DelayAfterImact));
			
			float maxJumpVy = Mathf.Sqrt(_config.jumpHeight * -2f * _config.gravity) * 1.1f;
			_movement.SuppressJumpFor(_cfg.SuppressJumpTime);
			_movement.RequestVerticalOverride(maxJumpVy);

			var fwd = _view.CameraRoot.forward;
			fwd.y = 0f;
			fwd.Normalize();
			_movement.AddImpulseXZ(fwd * _cfg.ForwardBoost);
			_cooldown = _cfg.AfterImpactCooldown;
			
			_stateController.ChangeState(PlayerState.Normal);
			_awaitLand = false;
			_diving = false;
		}

		private void DoAreaImpact()
		{
			_view.NotifySlamImpact();
		}
	}
}