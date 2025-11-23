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

		/// <summary>
		/// Load additive global scene (environment).
		/// Persistent scenes не трогаются.
		/// </summary>
		public UniTask LoadSceneAsync(string sceneName, CancellationToken ct = default)
		{
			return LoadSceneInternal(sceneName, unloadOthers: false, ct);
		}

		/// <summary>
		/// Load environment scene, optionally выгружая предыдущие environment-сцены.
		/// Persistent-сцена НЕ выгружается.
		/// </summary>
		public UniTask LoadEnvironmentSceneAsync(string sceneName, bool unloadPrevious, CancellationToken ct = default)
		{
			return LoadSceneInternal(sceneName, unloadPrevious, ct);
		}

		/// <summary>
		/// Unload a scene by name.
		/// </summary>
		public async UniTask UnloadSceneAsync(string sceneName, CancellationToken ct = default)
		{
			if (!IsSceneLoaded(sceneName))
			{
				_logger.LogWarning($"[FishNetSceneService] Scene '{sceneName}' not loaded → skip unload.");
				return;
			}

			_logger.Log($"[FishNetSceneService] Unload scene '{sceneName}'");

			var sm = InstanceFinder.SceneManager;
			var data = new SceneUnloadData(sceneName);

			var tcs = new UniTaskCompletionSource();

			void OnUnload(SceneUnloadEndEventArgs args)
			{
				if (args.UnloadedScenesV2 != null)
				{
					foreach (var s in args.UnloadedScenesV2)
					{
						if (s.Name == sceneName)
						{
							sm.OnUnloadEnd -= OnUnload;
							tcs.TrySetResult();
							return;
						}
					}
				}
			}

			sm.OnUnloadEnd += OnUnload;
			sm.UnloadGlobalScenes(data);

			await tcs.Task.AttachExternalCancellation(ct);
		}

		public string GetActiveSceneName()
		{
			return SceneManager.GetActiveScene().name;
		}

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
			{
				return false;
			}

			foreach (var root in scene.GetRootGameObjects())
			{
				if (root.TryGetComponent(out ctx))
				{
					return true;
				}

				ctx = root.GetComponentInChildren<SceneContext>(true);
				if (ctx != null)
				{
					return true;
				}
			}

			return false;
		}

		// -------------------------------------------------------------------
		// INTERNAL
		// -------------------------------------------------------------------

		private async UniTask LoadSceneInternal(
			string sceneName,
			bool unloadOthers,
			CancellationToken ct)
		{
			if (string.IsNullOrWhiteSpace(sceneName))
			{
				throw new ArgumentException("Scene name cannot be null or empty.", nameof(sceneName));
			}

			var sm = InstanceFinder.SceneManager;

			_logger.Log($"[FishNetSceneService] Load scene '{sceneName}' (unloadOthers={unloadOthers})");

			var data = new SceneLoadData(sceneName);

			if (unloadOthers)
			{
				// Выгружаются только online-сцены (а persistent — нет)
				data.ReplaceScenes = ReplaceOption.OnlineOnly;
			}
			else
			{
				// Классическая аддитивная загрузка environment-сцен
				data.ReplaceScenes = ReplaceOption.None;
			}

			var tcs = new UniTaskCompletionSource();

			void OnLoad(SceneLoadEndEventArgs args)
			{
				foreach (var s in args.LoadedScenes)
				{
					if (s.name == sceneName)
					{
						SceneManager.SetActiveScene(s);
						sm.OnLoadEnd -= OnLoad;
						tcs.TrySetResult();
						return;
					}
				}
			}

			sm.OnLoadEnd += OnLoad;
			sm.LoadGlobalScenes(data);

			await tcs.Task.AttachExternalCancellation(ct);
		}
	}
}
