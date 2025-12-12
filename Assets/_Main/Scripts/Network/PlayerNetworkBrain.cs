using System;
using FishNet.Object;
using FishNet.Object.Prediction;
using UnityEngine;
using _Main.Scripts.Player.Network;
using _Main.Scripts.Core.Services;
using FishNet.Transporting;
using FishNet.Utility.Template;

namespace _Main.Scripts.Player
{
	public class PlayerNetworkBrain : TickNetworkBehaviour
	{
		public event Action OnStartDiving;
		public event Action OnStopDiving;

		private PlayerMovementController _movement;
		private PlayerSlamBounceController _slam;
		private IInputService _localInput;
		private Transform _localCamera;

		private bool isInitialized = false;

		public void Construct(
			PlayerMovementController movement,
			PlayerSlamBounceController slam,
			IInputService inputService = null,
			Transform cameraTransform = null)
		{
			_movement = movement;
			_slam = slam;
			_localInput = inputService;
			_localCamera = cameraTransform;
			isInitialized = true;

			_slam.OnStartDiving += () => OnStartDiving?.Invoke();
			_slam.OnStopDiving += () => OnStopDiving?.Invoke();
		}
		
		private void Awake() 
		{
			SetTickCallbacks(TickCallback.Tick);
		}

		protected override void TimeManager_OnTick()
		{
			Replicate(BuildInput());
			if (IsServerStarted)
			{
				CreateReconcile();
			}
		}

		private PlayerInputData BuildInput()
		{
			if (!IsOwner)
			{
				return default;
			}
			
			if (_localInput == null)
			{
				return default;
			}
			return new PlayerInputData
			{
				Move = _localInput.Move,
				Jump = _localInput.IsJumping,
				Sprint = _localInput.IsSprinting,
				Interact = _localInput.IsInteract,
				CameraYaw = _localCamera.eulerAngles.y
			};
		}

		[Replicate]
		private void Replicate(PlayerInputData input, ReplicateState state = ReplicateState.Invalid,
			Channel channel = Channel.Unreliable)
		{
			if (!isInitialized)
			{
				return;
			}
			float dt = (float)TimeManager.TickDelta;
			_movement.Simulate(dt, input);
			_slam.Simulate(dt, input);
		}

		public override void CreateReconcile()
		{
			if (!isInitialized)
			{
				return;
			}
			var state = new PlayerStateData();
			_movement.WriteState(ref state);
			_slam.WriteState(ref state);

			Reconcile(state);
		}

		[Reconcile]
		private void Reconcile(PlayerStateData data, Channel channel = Channel.Unreliable)
		{
			if (!isInitialized)
			{
				return;
			}
			_movement.ReadState(data);
			_slam.ReadState(data);
		}
	}
}