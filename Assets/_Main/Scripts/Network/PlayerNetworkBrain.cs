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
		private PlayerMovementController _movement;
		private PlayerSlamBounceController _slam;
		private IInputService _localInput;
		private Transform _localCamera;

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
		}

		private void Awake() => SetTickCallbacks(TickCallback.Tick);

		protected override void TimeManager_OnTick()
		{
			if (IsOwner)
			{
				Replicate(BuildInput());
			}
			else if (IsServerStarted)
			{
				Replicate(default);
			}
		}

		private PlayerInputData BuildInput()
		{
			if (_localInput == null) return default;
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
			float dt = (float)TimeManager.TickDelta;
			
			_movement.Simulate(dt, input);
			_slam.Simulate(dt, input);
		}

		public override void CreateReconcile()
		{
			var state = new PlayerStateData();
			_movement.WriteState(ref state);
			_slam.WriteState(ref state);

			Reconcile(state);
		}

		[Reconcile]
		private void Reconcile(PlayerStateData data, Channel channel = Channel.Unreliable)
		{
			_movement.ReadState(data);
			_slam.ReadState(data);
		}
	}
}