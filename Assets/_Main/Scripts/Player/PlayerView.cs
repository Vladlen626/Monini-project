using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerView : MonoBehaviour
{
	[SerializeField] private CharacterController _characterController;
	[SerializeField] private Transform _cameraRoot;
	[SerializeField] private Transform _playerTransform;
	[SerializeField] private float _rotateSpeedDeg = 720f;
	[SerializeField] private Animator _animator;
	[SerializeField] private PlayerNetworkBridge _bridge;
	[SerializeField] private Collider _slamTrigger;
	public bool IsGrounded => _characterController.isGrounded;
	public Vector3 Position => transform.position;
	public Transform CameraRoot => _cameraRoot;
	public Vector3 Velocity => _characterController.velocity;
	public Animator Animator => _animator;
	public event Action OnLand;

	private bool _wasGrounded;

	private void Update()
	{
		DetectLanding();
	}

	// ReSharper disable Unity.PerformanceAnalysis
	private void DetectLanding()
	{
		if (!_wasGrounded && IsGrounded)
		{
			OnLand?.Invoke();
		}

		_wasGrounded = IsGrounded;
	}

	public void ApplyMovement(Vector3 velocity)
	{
		_characterController.Move(velocity * Time.deltaTime);

		var horizontal = new Vector3(velocity.x, 0, velocity.z);
		if (horizontal.sqrMagnitude > 0.0001f)
		{
			var target = Quaternion.LookRotation(horizontal, Vector3.up);
			_playerTransform.rotation =
				Quaternion.RotateTowards(_playerTransform.rotation, target, _rotateSpeedDeg * Time.deltaTime);
		}
	}

	public void EnableSlamTrigger()
	{
		_slamTrigger.enabled = true;
	}

	public void DisableSlamTrigger()
	{
		_slamTrigger.enabled = false;
	}

	public void SetRotateSpeed(float degPerSec)
	{
		_rotateSpeedDeg = degPerSec;
	}

	public void NotifySlamImpact(float radius)
	{
		if (_bridge.IsOwner)
		{
			_bridge.Rpc_PlaySlamFX(Position);
		}
	}
}