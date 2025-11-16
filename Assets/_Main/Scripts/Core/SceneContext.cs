using _Main.Scripts.Location;
using UnityEngine;

public class SceneContext : MonoBehaviour
{
	[SerializeField] private Transform[] _playerSpawnPoints;
	[SerializeField] private NextAreaNetworkBehaviour[] _nextAreaNetworkBehaviours;
	public Transform[] PlayerSpawnPoints => _playerSpawnPoints;
	public NextAreaNetworkBehaviour[] NextAreaNetworkBehaviours => _nextAreaNetworkBehaviours;
}

public class GameModelContext
{
	public SceneContext SceneContext;
}