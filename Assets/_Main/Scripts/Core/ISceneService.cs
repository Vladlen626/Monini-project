using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using UnityEngine.SceneManagement;

namespace PlatformCore.Services
{
	public interface ISceneService
	{
		// _____________ Base _____________
		UniTask LoadSceneAsync(string sceneName, CancellationToken ct = default);
		UniTask LoadEnvironmentSceneAsync(string sceneName, bool unloadPrev, CancellationToken ct = default);
		string GetActiveSceneName();
		bool IsSceneLoaded(string sceneName);
		bool TryGetSceneContext(string sceneName, out SceneContext sceneContext);
		UniTask UnloadSceneAsync(string sceneName, CancellationToken ct = default);
	}
	
	public interface ISceneFlowService
	{
		event Action<string> OnSceneChangeRequested;
		void RequestSceneChange(string sceneName);
	}
}