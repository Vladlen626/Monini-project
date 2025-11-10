using Unity.Netcode;
using UnityEngine;

public struct PlayerInputData : INetworkSerializable
{
	public Vector2 Move;
	public bool IsJumping;
	public bool IsSprinting;

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref Move);
		serializer.SerializeValue(ref IsJumping);
		serializer.SerializeValue(ref IsSprinting);
	}
}