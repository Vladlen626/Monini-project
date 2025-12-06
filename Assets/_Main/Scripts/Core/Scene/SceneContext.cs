using System;
using _Main.Scripts.Core;
using _Main.Scripts.Interactables.Crumb;
using _Main.Scripts.Location;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneContext : MonoBehaviour
{
	private string _sceneName;
	
	[SerializeField]
	private SceneType _sceneType;
	
	[SerializeField] 
	private Transform[] _playerSpawnPoints;

	[SerializeField] 
	private NextAreaNetworkBehaviour[] _nextAreaNetworkBehaviours;
	
	[SerializeField] 
	private CrumbPlayerTrigger[] _crumbsNetworkBehaviours;

	[SerializeField, SceneName]
	private string _nextSceneName;
	
	public Transform[] PlayerSpawnPoints => _playerSpawnPoints;
	public NextAreaNetworkBehaviour[] NextAreaNetworkBehaviours => _nextAreaNetworkBehaviours;
	public CrumbPlayerTrigger[] CrumbsNetworkBehaviours => _crumbsNetworkBehaviours;
	public string SceneName => _sceneName;
	public string NextSceneName => _nextSceneName;
	public SceneType SceneType => _sceneType;


	private void Awake()
	{
		_sceneName = SceneManager.GetActiveScene().name;
	}
}

public enum SceneType
{
	Hub,
	Extraction,
}