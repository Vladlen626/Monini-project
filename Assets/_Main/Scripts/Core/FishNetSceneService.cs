using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Managing.Scened;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace PlatformCore.Services
{
	public class FishNetSceneService : BaseAsyncService, ISceneService
	{
		private readonly ILoggerService _logger;

		public FishNetSceneService(ILoggerService logger)
		{
			_logger = logger;
		}

		// -------------------------------------------------------------------
		// PUBLIC API
		// -------------------------------------------------------------------

		public UniTask LoadSceneAsync(string sceneName, CancellationToken ct = default)
		{
			return LoadSceneInternal(sceneName, additive: true, ct);
		}

		public UniTask LoadGlobalSceneAsync(string sceneName, CancellationToken ct = default)
		{
			return LoadSceneInternal(sceneName, additive: false, ct);
		}

		public UniTask UnloadSceneAsync(string sceneName, CancellationToken ct = default)
		{
			var sm = InstanceFinder.SceneManager;

			if (!IsSceneLoaded(sceneName))
			{
				_logger.LogWarning($"[FishNetSceneService] Scene '{sceneName}' not loaded → skip unload.");
				return UniTask.CompletedTask;
			}

			_logger.Log($"[FishNetSceneService] Unload scene '{sceneName}'");

			var data = new SceneUnloadData(sceneName);
			sm.UnloadGlobalScenes(data);

			return UniTask.CompletedTask;
		}

		public async UniTask ReloadCurrentSceneAsync(CancellationToken ct = default)
		{
			var name = GetActiveSceneName();
			await LoadSceneInternal(name, additive: false, ct);
		}

		public string GetActiveSceneName()
			=> SceneManager.GetActiveScene().name;

		public bool IsSceneLoaded(string sceneName)
		{
			var scene = SceneManager.GetSceneByName(sceneName);
			return scene.IsValid() && scene.isLoaded;
		}

		public bool TryGetSceneContext(string sceneName, out SceneContext ctx)
		{
			ctx = null;

			var scene = SceneManager.GetSceneByName(sceneName);
			if (!scene.IsValid() || !scene.isLoaded)
				return false;

			foreach (var root in scene.GetRootGameObjects())
			{
				if (root.TryGetComponent(out ctx)) return true;

				ctx = root.GetComponentInChildren<SceneContext>(true);
				if (ctx != null) return true;
			}

			return false;
		}

		// -------------------------------------------------------------------
		// INTERNAL
		// -------------------------------------------------------------------

		private UniTask LoadSceneInternal(string sceneName, bool additive, CancellationToken ct)
		{
			if (string.IsNullOrWhiteSpace(sceneName))
				throw new ArgumentException("Scene name cannot be null or empty.", nameof(sceneName));

			var sm = InstanceFinder.SceneManager;

			SceneLoadData data = new(sceneName);

			if (additive)
			{
				// ADDITIVE GLOBAL SCENE
				data.ReplaceScenes = ReplaceOption.None;
				_logger.Log($"[FishNetSceneService] Load ADDITIVE global scene '{sceneName}'");
			}
			else
			{
				// BASE SCENE (replace all)
				data.ReplaceScenes = ReplaceOption.All;
				_logger.Log($"[FishNetSceneService] Load BASE global scene '{sceneName}'");
			}

			sm.LoadGlobalScenes(data);
			return UniTask.CompletedTask;
		}
	}
}