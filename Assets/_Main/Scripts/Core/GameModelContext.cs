using PlatformCore.Core;
using PlatformCore.Services;
using PlatformCore.Services.Factory;

public class GameModelContext
{
	public PersistentSceneContext PersistentSceneContext {get; private set;}
	public SceneContext SceneContext {get; private set;}
	public NetworkModel NetworkModel {get; private set;} = new();

	public GameModelContext(PersistentSceneContext persistentSceneContext)
	{
		PersistentSceneContext = persistentSceneContext;
	}
	
	public void SetSceneContext(SceneContext sceneContext)
	{
		SceneContext = sceneContext;
	}
}