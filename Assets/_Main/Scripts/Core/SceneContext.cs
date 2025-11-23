using System;
using _Main.Scripts.Core;
using _Main.Scripts.Location;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneContext : MonoBehaviour
{
	private string _sceneName;
	
	[SerializeField] 
	private Transform[] _playerSpawnPoints;

	[SerializeField] 
	private NextAreaNetworkBehaviour[] _nextAreaNetworkBehaviours;

	[SerializeField, SceneName]
	private string _nextSceneName;
	
	public Transform[] PlayerSpawnPoints => _playerSpawnPoints;
	public NextAreaNetworkBehaviour[] NextAreaNetworkBehaviours => _nextAreaNetworkBehaviours;
	public string SceneName => _sceneName;
	public string NextSceneName => _nextSceneName;


	private void Awake()
	{
		_sceneName = SceneManager.GetActiveScene().name;
	}
}

public class GameModelContext
{
	public SceneContext SceneContext;
}