using System;
using System.Collections.Generic;
using System.Threading;
using _Main.Scripts.Core.Services;
using _Main.Scripts.Player;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services;
using PlatformCore.Services.Factory;

public sealed class PlayerContext : IDisposable
{
	public PlayerView view { get; }
	public IInputService input { get; }
	public ICameraService сamera { get; }
	public PlayerConfig сonfig { get; }
	public List<IBaseController> controllers => _controllers;

	private readonly List<IPlayerLocalService> _locals = new();
	private readonly List<IBaseController> _controllers = new();

	private PlayerContext(PlayerView view, IInputService input, ICameraService сamera, PlayerConfig сonfig)
	{
		this.view = view;
		this.input = input;
		this.сamera = сamera;
		this.сonfig = сonfig;
	}

	public void AddController(IBaseController controller)
	{
		if (controllers.Contains(controller))
		{
			return;
		}

		controllers.Add(controller);
	}

	public static async UniTask<PlayerContext> CreateAsync(
		PlayerView view, IObjectFactory factory, CancellationToken ct)
	{
		var input = new InputLocalService();
		var camera = new CameraLocalService(factory);

		var localServices = new IPlayerLocalService[]
		{
			input,
			camera
		};

		foreach (var s in localServices)
		{
			await s.InitAsync(ct);
		}

		var ctx = new PlayerContext(view, input, camera, new PlayerConfig());
		ctx._locals.AddRange(localServices);

		return ctx;
	}

	// ReSharper disable Unity.PerformanceAnalysis
	public void Dispose()
	{
		foreach (var s in _locals)
		{
			s.Dispose();
		}

		_locals.Clear();
	}
}