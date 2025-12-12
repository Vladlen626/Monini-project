using System;
using GameKit.Dependencies.Utilities;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerView : MonoBehaviour
{
	public event Action OnLand;
	[SerializeField] private Transform _cameraRoot;
	[SerializeField] private Transform _playerVisualTransform;
	[SerializeField] private Animator _animator;
	[SerializeField] private float _rotateSpeedDeg = 720f;

	private PlayerNetworkBridge _bridge;
	private CharacterController _characterController;
	public bool IsGrounded => _characterController.isGrounded;
	public Vector3 Position => transform.position;
	public Transform CameraRoot => _cameraRoot;
	public Vector3 Velocity => _characterController.velocity;
	public Animator Animator => _animator;
	
	public uint LastLandingTick => _lastLandingTick;

	private bool _wasGrounded;

	private uint _lastLandingTick;

	private void Awake()
	{
		_characterController = GetComponent<CharacterController>();
		_bridge = GetComponent<PlayerNetworkBridge>();
	}

	public void NotifyLanding()
	{
		uint currentTick = _bridge.TimeManager.Tick;
		if (_lastLandingTick == currentTick)
		{
			return;
		}

		_lastLandingTick = currentTick;
		OnLand?.Invoke();
	}
	public void ResetLandingTick(uint tick)
	{
		_lastLandingTick = tick;
	}
	public void ApplyMovement(Vector3 velocity, float dt)
	{
		_characterController.Move(velocity * dt);

		var horizontal = new Vector3(velocity.x, 0, velocity.z);
		if (horizontal.sqrMagnitude > 0.0001f)
		{
			var target = Quaternion.LookRotation(horizontal, Vector3.up);
			_playerVisualTransform.rotation =
				Quaternion.RotateTowards(_playerVisualTransform.rotation, target, _rotateSpeedDeg * dt);
		}
	}

	public void TeleportTo(Vector3 position, Quaternion rotation)
	{
		if (_characterController)
		{
			_characterController.enabled = false;
		}

		transform.SetPositionAndRotation(position, rotation);

		if (_characterController)
		{
			_characterController.enabled = true;
			_characterController.Move(Vector3.zero);
		}
	}

	public void SetRotateSpeed(float degPerSec)
	{
		_rotateSpeedDeg = degPerSec;
	}

	public void EnableFlatForm()
	{
		_playerVisualTransform.SetScale(new Vector3(1, 0.01f, 1));
	}

	public void DisableFlatForm()
	{
		_playerVisualTransform.SetScale(Vector3.one);
	}

	public void NotifySlamImpact()
	{
		_bridge.Rpc_PlaySlamFX(Position);
	}
}