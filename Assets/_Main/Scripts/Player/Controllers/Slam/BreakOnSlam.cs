using System;
using Cysharp.Threading.Tasks;
using FishNet.Object;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BreakOnSlam : NetworkBehaviour, ISlamImpactReceiver
{
	[SerializeField] private GameObject _boxView;
	[SerializeField] private GameObject _breakFx;

	private Collider boxCollider;

	private void Start()
	{
		boxCollider = gameObject.GetComponent<Collider>();
	}

	public void OnSlamImpact()
	{
		if (ServerManager.Started)
		{
			Destruct();
		}
	}

	private void Destruct()
	{
		boxCollider.enabled = false;
		Rpc_SpawnBreakFx();
		DespawnOnNextFrame().Forget();
	}
	
	private async UniTaskVoid DespawnOnNextFrame()
	{
		await UniTask.Yield();
		if (ServerManager.Started)
		{
			Despawn();
		}
	}

	[ObserversRpc]
	private void Rpc_SpawnBreakFx()
	{
		if (_breakFx)
		{
			Instantiate(_breakFx, transform.position, Quaternion.identity);
		}
	}
}