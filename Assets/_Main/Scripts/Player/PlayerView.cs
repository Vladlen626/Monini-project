using System;
using GameKit.Dependencies.Utilities;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerView : MonoBehaviour
{
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
	public event Action OnLand;

	private bool _wasGrounded;

	private void Awake()
	{
		_characterController = GetComponent<CharacterController>();
		_bridge = GetComponent<PlayerNetworkBridge>();
	}

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
		if (_characterController != null)
		{
			_characterController.enabled = false;
		}

		transform.SetPositionAndRotation(position, rotation);

		if (_characterController != null)
		{
			_characterController.enabled = true;
		}

		_wasGrounded = _characterController != null && _characterController.isGrounded;
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
		_bridge.Server_PlaySlamFX(Position);
	}
}