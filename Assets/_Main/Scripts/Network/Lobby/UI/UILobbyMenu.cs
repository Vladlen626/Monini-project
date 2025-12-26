using System;
using PlatformCore.Services.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILobbyMenu : BaseUIElement
{
	[SerializeField]
	private CanvasGroup _canvasGroup;
	
	[Header("Create Room")] [SerializeField]
	private TMP_InputField _roomNameInput;

	[SerializeField] 
	private Button _createButton;

	[Header("Join Room")]
	[SerializeField] 
	private TMP_InputField _joinCodeInput;

	[SerializeField] 
	private Button _joinButton;

	[Header("Status")]
	[SerializeField]
	private TMP_Text _statusText;

	[SerializeField]
	private GameObject _joinCodePanel;

	[SerializeField]
	private TMP_Text _joinCodeText;

	[SerializeField]
	private Button _copyCodeButton;

	[Header("Navigation")]
	[SerializeField]
	private Button _backButton;

	public event Action<string> OnCreateRoom;
	public event Action<string> OnJoinRoom;
	public event Action OnBack;

	private void Awake()
	{
		_createButton.onClick.AddListener(HandleCreateClick);
		_joinButton.onClick.AddListener(HandleJoinClick);
		_copyCodeButton.onClick.AddListener(HandleCopyCode);
		_backButton.onClick.AddListener(() => OnBack?.Invoke());

		_joinCodePanel.SetActive(false);
	}

	protected override void OnShow()
	{
		base.OnShow();
		_canvasGroup.alpha = 1;
	}

	protected override void OnHide()
	{
		base.OnHide();
		_canvasGroup.alpha = 0;
	}

	private void HandleCreateClick()
	{
		string roomName = _roomNameInput.text.Trim();
		if (string.IsNullOrEmpty(roomName))
		{
			roomName = $"Room {UnityEngine.Random.Range(1000, 9999)}";
		}

		OnCreateRoom?.Invoke(roomName);
	}

	private void HandleJoinClick()
	{
		string code = _joinCodeInput.text.ToUpper().Trim();
		OnJoinRoom?.Invoke(code);
	}

	private void HandleCopyCode()
	{
		GUIUtility.systemCopyBuffer = _joinCodeText.text;
		SetStatus("Код скопирован!");
	}

	public void SetStatus(string text)
	{
		_statusText.text = text;
	}

	public void ShowJoinCode(string code)
	{
		_joinCodeText.text = code;
		_joinCodePanel.SetActive(true);
	}

	public void HideJoinCode()
	{
		_joinCodePanel.SetActive(false);
	}

	public void SetInteractable(bool interactable)
	{
		_createButton.interactable = interactable;
		_joinButton.interactable = interactable;
		_roomNameInput.interactable = interactable;
		_joinCodeInput.interactable = interactable;
	}
}