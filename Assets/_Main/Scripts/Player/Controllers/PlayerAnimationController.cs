using _Main.Scripts.Core.Services;
using _Main.Scripts.Player;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using UnityEngine;

namespace _Main.Scripts.CameraFX._Main.Scripts.Player
{
	public class PlayerAnimationController : IBaseController, IUpdatable
	{
		private readonly IInputService _input;
		private readonly PlayerConfig _config;
		private readonly PlayerView _view;
		private readonly Animator _anim;
		private readonly PlayerNetworkBridge _bridge;

		// Animator hashes
		private static readonly int SpeedHash = Animator.StringToHash("Speed");
		private static readonly int GroundedHash = Animator.StringToHash("Grounded");
		private static readonly int YVelHash = Animator.StringToHash("YVel");
		private static readonly int SprintHash = Animator.StringToHash("Sprint");
		private static readonly int JumpHash = Animator.StringToHash("Jump");
		private static readonly int LandHash = Animator.StringToHash("Land");
		private static readonly int FlatHash = Animator.StringToHash("Flat");

		// локальное состояние
		private bool _wasGrounded;

		// тюнинги демпфирования
		private const float SpeedDamp = 0.1f;
		private const float YVelDamp = 0.1f;

		public PlayerAnimationController(IInputService input, PlayerConfig config, PlayerView view, PlayerNetworkBridge bridge)
		{
			_input = input;
			_config = config;
			_view = view;
			_anim = view.Animator;
			_bridge = bridge;
		}

		public void OnUpdate(float dt)
		{
			if (!_anim)
			{
				return;
			}

			bool grounded = _view.IsGrounded;
			Vector3 vel = _view.Velocity;
			Vector2 planar = new Vector2(vel.x, vel.z);
			float planarSpeed = planar.magnitude;
			float yVel = vel.y;
			
			float animSpeed = 1f;
			if (!grounded)
			{
				if (yVel > 1f) animSpeed = 1.2f;
				else if (yVel < -2f) animSpeed = 1.5f;
			}
			else animSpeed = 1f;

			_anim.speed = animSpeed;

			float maxRun = Mathf.Max(0.01f, _config.sprintSpeed);
			float speed01 = Mathf.Clamp01(planarSpeed / maxRun);

			_anim.SetFloat(SpeedHash, speed01, SpeedDamp, dt);
			_anim.SetBool(GroundedHash, grounded);
			_anim.SetFloat(YVelHash, yVel, YVelDamp, dt);
			_anim.SetBool(SprintHash, _input.IsSprinting);

			if (_wasGrounded && !grounded && yVel > 0.05f)
			{
				_anim.SetTrigger(JumpHash);
			}

			if (!_wasGrounded && grounded)
			{
				_anim.SetTrigger(LandHash);
			}
			
			_anim.SetBool(FlatHash, _bridge.State.Value == PlayerState.Flat);

			_wasGrounded = grounded;
		}
	}
}