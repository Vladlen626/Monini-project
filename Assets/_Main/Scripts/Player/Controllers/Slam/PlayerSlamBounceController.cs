using System;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services;
using UnityEngine;

namespace _Main.Scripts.Player
{
	public class PlayerSlamBounceController : IBaseController, IActivatable
	{
		public event Action OnStartDiving;
		public event Action OnStopDiving;
		public bool WasDivingLastFrame { get; private set; }
		private readonly PlayerMovementController _movement;
		private readonly PlayerView _view;
		private readonly ICameraShakeService _shake;
		private readonly PlayerConfig _config;
		private readonly PlayerNetworkBridge _bridge;

		// Состояние
		private bool _diving;
		private bool _awaitLand;
		private bool _prevInteract;
		private float _airTime;
		private float _cooldown;

		private float _impactDelayTimer;
		private bool _isImpactDelayed;
		private float _landingCameraYaw; 
		
		private bool _wasDivingLastTick;
		private uint _lastDiveStartTick;
		private uint _lastDiveStopTick;

		private class BounceConfig
		{
			public float MinAirTime = 0.03f;
			public float DiveDownSpeed = -38f;
			public float DelayAfterImact = 0.075f;
			public float AfterImpactCooldown = 0.65f;
			public float SuppressJumpTime = 0.15f;
			public float ForwardBoost = 5.5f;
			public float ImpactRadius = 1f;
			public float ShakeAmp = 1.1f, ShakeDur = 0.15f;
		}

		private readonly BounceConfig _cfg = new();

		public PlayerSlamBounceController(
			PlayerMovementController movement,
			PlayerView view,
			PlayerNetworkBridge bridge,
			ICameraShakeService shake,
			PlayerConfig config)
		{
			_movement = movement;
			_view = view;
			_shake = shake;
			_config = config;
			_bridge = bridge;
		}

		public void Activate()
		{
			_view.OnLand += OnLand;
		}

		public void Deactivate()
		{
			_view.OnLand -= OnLand;
		}


		public void Simulate(float dt, PlayerInputData input)
		{
			WasDivingLastFrame = _diving;
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

			bool interactPressed = input.Interact && !_prevInteract;
			_prevInteract = input.Interact;

			if (!_diving && _airTime >= _cfg.MinAirTime && interactPressed && _cooldown <= 0f)
			{
				StartDive();
			}

			if (_diving && _awaitLand)
			{
				_landingCameraYaw = input.CameraYaw;
			}

			if (_isImpactDelayed)
			{
				_impactDelayTimer -= dt;
				if (_impactDelayTimer <= 0f)
				{
					ApplyBounceForce(_landingCameraYaw);
					_isImpactDelayed = false;
				}
			}
			
			bool justStartedDiving = _diving && !_wasDivingLastTick;
			bool justStoppedDiving = !_diving && _wasDivingLastTick;

			if (justStartedDiving && _bridge)
			{
				uint currentTick = _bridge.TimeManager.Tick;
				if (_lastDiveStartTick != currentTick)
				{
					_lastDiveStartTick = currentTick;
					OnStartDiving?.Invoke();
				}
			}

			if (justStoppedDiving && _bridge)
			{
				uint currentTick = _bridge.TimeManager.Tick;
				if (_lastDiveStopTick != currentTick)
				{
					_lastDiveStopTick = currentTick;
					OnStopDiving?.Invoke();
				}
			}

			_wasDivingLastTick = _diving;
		}

		// ReSharper disable Unity.PerformanceAnalysis
		private void StartDive()
		{
			_diving = true;
			_awaitLand = true;
			_movement.RequestVerticalOverride(_cfg.DiveDownSpeed);
			_shake?.ShakeAsync(0.35f, 0.08f).Forget();
		}

		// ReSharper disable Unity.PerformanceAnalysis
		private void OnLand()
		{
			if (!_awaitLand) return;
			
			if (_bridge)
			{
				_bridge.ServerBreakNearbyBoxes(_view.Position, _cfg.ImpactRadius);
			}
			
			_view.NotifySlamImpact();
			_shake?.ShakeAsync(_cfg.ShakeAmp, _cfg.ShakeDur).Forget();
			_awaitLand = false;
			_diving = false;
			_isImpactDelayed = true;
			_impactDelayTimer = _cfg.DelayAfterImact;
		}

		private void ApplyBounceForce(float cameraYaw)
		{
			float maxJumpVy = Mathf.Sqrt(_config.jumpHeight * -2f * _config.gravity) * 1.1f;
			_movement.SuppressJumpFor(_cfg.SuppressJumpTime);
			_movement.RequestVerticalOverride(maxJumpVy);

			var camRot = Quaternion.Euler(0, cameraYaw, 0);
			var fwd = camRot * Vector3.forward;

			_movement.AddImpulseXZ(fwd * _cfg.ForwardBoost);
			_cooldown = _cfg.AfterImpactCooldown;
		}

		public void WriteState(ref PlayerStateData data)
		{
			data.IsDiving = _diving;
			data.AwaitLand = _awaitLand;
			data.SlamCooldown = _cooldown;
			data.AirTime = _airTime;
			data.IsImpactDelayed = _isImpactDelayed;
			data.ImpactDelayTimer = _impactDelayTimer;
			data.LandingCameraYaw = _landingCameraYaw;
			
			data.WasDivingLastTick = _wasDivingLastTick;
			data.LastDiveStartTick = _lastDiveStartTick;
			data.LastDiveStopTick = _lastDiveStopTick;
			data.PrevInteract = _prevInteract;
		}

		public void ReadState(PlayerStateData data)
		{
			_diving = data.IsDiving;
			_awaitLand = data.AwaitLand;
			_cooldown = data.SlamCooldown;
			_airTime = data.AirTime;
			_isImpactDelayed = data.IsImpactDelayed;
			_impactDelayTimer = data.ImpactDelayTimer;
			_landingCameraYaw = data.LandingCameraYaw;

			_wasDivingLastTick = data.WasDivingLastTick;
			_lastDiveStartTick = data.LastDiveStartTick;
			_lastDiveStopTick = data.LastDiveStopTick;
			_prevInteract = data.PrevInteract;
		}
	}
}