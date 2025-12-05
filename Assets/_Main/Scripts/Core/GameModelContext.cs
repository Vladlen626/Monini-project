public class GameModelContext
{
	public SceneContext SceneContext {get; private set;}
	public NetworkModel NetworkModel {get; private set;} = new();
	public void SetSceneContext(SceneContext sceneContext)
	{
		SceneContext = sceneContext;
	}
}