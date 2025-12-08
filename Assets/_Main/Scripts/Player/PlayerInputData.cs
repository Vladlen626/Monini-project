using FishNet.Object.Prediction;
using UnityEngine;

namespace _Main.Scripts.Player
{
	public struct PlayerInputData : IReplicateData
	{
		public Vector2 Move;
		public bool Jump;
		public bool Sprint;
		public bool Interact; 
		public float CameraYaw; // Куда смотрела камера в момент ввода

		private uint _tick;
		public void Dispose() { }
		public uint GetTick() => _tick;
		public void SetTick(uint value) => _tick = value;
	}
}