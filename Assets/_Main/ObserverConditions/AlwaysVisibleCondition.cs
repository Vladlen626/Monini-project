using FishNet.Connection;
using FishNet.Observing;
using UnityEngine;

[CreateAssetMenu(menuName = "FishNet/Observer Conditions/Always Visible")]
public class AlwaysVisibleCondition : ObserverCondition
{
	public override bool ConditionMet(NetworkConnection connection, bool currentlyAdded, out bool notProcessed)
	{
		notProcessed = false;
		return true;
	}

	public override ObserverConditionType GetConditionType()
	{
		return ObserverConditionType.Normal;
	}
}