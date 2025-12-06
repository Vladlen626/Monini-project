using PlatformCore.Services.UI;
using TMPro;
using UnityEngine;

public class UIPlayerDynamicHud : BaseUIElement
{
	[SerializeField]
	public TextMeshProUGUI _crumbsText;

	public void SetCrumbsNumber(int number)
	{
		_crumbsText.text = $"{number}";
	}
}