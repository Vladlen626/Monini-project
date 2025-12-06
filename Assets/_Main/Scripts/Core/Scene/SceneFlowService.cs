using System;

namespace PlatformCore.Services
{
	public class SceneFlowService :  BaseAsyncService, ISceneFlowService
	{
		public event Action<string> OnSceneChangeRequested;

		public void RequestSceneChange(string sceneName)
		{
			OnSceneChangeRequested?.Invoke(sceneName);
		}
	}
}