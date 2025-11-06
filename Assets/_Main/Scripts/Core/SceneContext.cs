using UnityEngine;

public class SceneContext : MonoBehaviour
{
	[SerializeField] private Transform playerSpawnPoint;
	public Vector3 PlayerSpawnPos => playerSpawnPoint.position;
}