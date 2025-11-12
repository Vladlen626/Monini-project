using System;
using _Main.Scripts.Core;
using Cysharp.Threading.Tasks;
using _Main.Scripts.Core.Services;
using _Main.Scripts.Player;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services;
using PlatformCore.Services.Audio;
using UnityEngine;

namespace _Main.Scripts.Player
{
	public class PlayerSlamBounceController : IBaseController, IUpdatable
	{
		private readonly PlayerMovementController _movement;
		private readonly PlayerView _view;
		private readonly IInputService _input;
		private readonly ICameraShakeService _shake;
		private readonly PlayerModel _model;
		private readonly IAudioService _audio;

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

			public float AfterImpactCooldown = 0.5f;
			public float SuppressJumpTime = 0.15f;

			public float ForwardBoost = 5.5f;
			public float ShakeAmp = 1.1f, ShakeDur = 0.18f;
			public string AudioImpact = "event:/ground_slam_impact";
		}

		private readonly BounceConfig _cfg = new();

		public PlayerSlamBounceController(
			IInputService input,
			PlayerMovementController movement,
			PlayerView view,
			ICameraShakeService shake,
			PlayerModel model,
			IAudioService audio = null)
		{
			_input = input;
			_movement = movement;
			_view = view;
			_shake = shake;
			_model = model;
			_audio = audio;
			_view.OnLand += OnLand;
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


			if (_diving)
			{
				_movement.RequestVerticalOverride(_cfg.DiveDownSpeed + _cfg.DiveExtra);

				if (_view.IsGrounded && _awaitLand == false)
				{
					_diving = false;
					DoImpactAndBounce().Forget();
				}
			}
		}

		private void StartDive()
		{
			_diving = true;
			_awaitLand = true;
			_model.SetState(PlayerState.Slam);
			_movement.RequestVerticalOverride(_cfg.DiveDownSpeed);
			_shake?.ShakeAsync(0.35f, 0.08f).Forget();
		}

		private void OnLand()
		{
			if (!_awaitLand) return;
			_awaitLand = false;
			_diving = false;
			DoImpactAndBounce().Forget();
		}

		private async UniTaskVoid DoImpactAndBounce()
		{
			_shake?.ShakeAsync(_cfg.ShakeAmp * 0.6f, _cfg.ShakeDur * 0.5f).Forget();

			DoAreaImpact();
			_model.SetState(PlayerState.Normal);
			float maxJumpVy = Mathf.Sqrt(_model.config.jumpHeight * -2f * _model.config.gravity) * 1.1f;
			_movement.SuppressJumpFor(_cfg.SuppressJumpTime);
			_movement.RequestVerticalOverride(maxJumpVy);

			var fwd = _view.CameraRoot.forward;
			fwd.y = 0f;
			fwd.Normalize();
			_movement.AddImpulseXZ(fwd * _cfg.ForwardBoost);

			_cooldown = _cfg.AfterImpactCooldown;
		}

		private void DoAreaImpact()
		{
			_view.NotifySlamImpact(_cfg.ImpactRadius);
		}
	}
}