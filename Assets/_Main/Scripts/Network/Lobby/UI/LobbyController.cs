using _Main.Scripts.Core;
using _Main.Scripts.Core.Services;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services.UI;

public class LobbyController : BaseContextController<UILobbyMenu>
{
	private readonly IMultiplayerService _multiplayerService;
	private readonly INetworkService _networkService;
	private readonly IInputService _inputService;
	private readonly ICursorService _cursorService;
	
	private bool showed = false;

	public LobbyController(
		IUIService uiService,
		IMultiplayerService multiplayerService,
		INetworkService networkService,
		IInputService inputService,
		ICursorService cursorService)
		: base(uiService)
	{
		_multiplayerService = multiplayerService;
		_networkService = networkService;
		_inputService = inputService;
		_cursorService = cursorService;
	}

	protected override void OnActivate()
	{
		base.OnActivate();
		_context.OnCreateRoom += OnCreateRoomHandler;
		_context.OnJoinRoom += OnJoinRoomHandler;
		_context.OnBack += OnBackHandler;

		_context.SetStatus("Ready");
		_context.HideJoinCode();
		_inputService.OnPausePressed += OnPausePressedHandler;
		
		Hide();
	}

	protected override void OnDeactivate()
	{
		_context.OnCreateRoom -= OnCreateRoomHandler;
		_context.OnJoinRoom -= OnJoinRoomHandler;
		_context.OnBack -= OnBackHandler;
		_inputService.OnPausePressed -= OnPausePressedHandler;
		base.OnDeactivate();
	}

	private void OnPausePressedHandler()
	{
		if (showed)
		{
			Hide();
		}
		else
		{
			Show();
		}
	}

	private async void OnCreateRoomHandler(string roomName)
	{
		_context.SetInteractable(false);
		_context.SetStatus("Room creation...");

		try
		{
			// Реальная логика: создаем комнату через Relay
			string joinCode = await _multiplayerService.CreateRoomAsync(roomName, 4);

			// Показываем код игроку
			_context.ShowJoinCode(joinCode);
			_context.SetStatus($"Room '{roomName}' created!");

			// Хост УЖЕ в игре, просто продолжает играть
			// Другие игроки подключатся к нему по коду
		}
		catch (System.Exception e)
		{
			_context.SetStatus($"Error: {e.Message}");
		}
		finally
		{
			_context.SetInteractable(true);
		}
	}

	private async void OnJoinRoomHandler(string code)
	{
		if (string.IsNullOrEmpty(code))
		{
			_context.SetStatus("Enter room code!");
			return;
		}

		_context.SetInteractable(false);
		_context.SetStatus("Connection...");

		try
		{
			// Настраиваем Relay для подключения
			await _multiplayerService.JoinRoomAsync(code);

			// Останавливаем текущее Host соединение
			_networkService.Stop();

			// Небольшая задержка для корректной остановки
			await UniTask.Delay(500);

			// Запускаем как клиент (подключаемся к хосту)
			_networkService.StartClient();

			_context.SetStatus($"Connected to {code}!");

			// Скрываем лобби после успешного подключения
			await UniTask.Delay(1000);
			_context.Hide();
		}
		catch (System.Exception e)
		{
			_context.SetStatus($"Error: {e.Message}");
			_context.SetInteractable(true);
		}
	}

	private void Show()
	{
		showed = true;
		_context.Show();
		_cursorService.UnlockCursor();
		_inputService.DisablePlayerInputs();
	}

	private void Hide()
	{
		showed = false;
		_context.Hide();
		_cursorService.LockCursor();
		_inputService.EnablePlayerInputs();
	}

	private void OnBackHandler()
	{
		Hide();
	}
}