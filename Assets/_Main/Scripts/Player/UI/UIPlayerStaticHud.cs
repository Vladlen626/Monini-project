using PlatformCore.Services.UI;
using TMPro;
using UnityEngine;

public class UIPlayerStaticHud : BaseUIElement
{
	[SerializeField]
	public TextMeshProUGUI _playerNameText;
	
	[SerializeField]
	public TextMeshProUGUI _locationNameText;

	public void SetPlayerName(string playerName)
	{
		_playerNameText.text = playerName;
	}

	public void SetLocationName(string locationName)
	{
		_locationNameText.text = locationName;
	}
}