using UnityEngine;

public class SceneContext : MonoBehaviour
{
	[SerializeField] private Transform[] playerSpawnPoints;
	public Transform[] PlayerSpawnPoints => playerSpawnPoints;
}