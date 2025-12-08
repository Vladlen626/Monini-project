using FishNet.Object.Prediction;
using UnityEngine;

namespace _Main.Scripts.Player
{
	public struct PlayerStateData : IReconcileData
	{
		public Vector3 Position;
		public Quaternion Rotation;
		public Vector3 Velocity;
		public float VerticalY;
		public bool IsGrounded;
		public float CoyoteTimer;

		public bool IsDiving;
		public bool AwaitLand;
		public float SlamCooldown;
		public float AirTime;

		private uint _tick;
		public void Dispose() { }
		public uint GetTick() => _tick;
		public void SetTick(uint value) => _tick = value;
	}
}