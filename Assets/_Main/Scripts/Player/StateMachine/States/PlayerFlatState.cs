using UnityEngine;

namespace _Main.Scripts.Player.StateMachine.States
{
	public class PlayerFlatState : PlayerStateBase
	{
		public PlayerFlatState(PlayerModel model, PlayerView view, CharacterController cc)
			: base(model, view, cc) {}

		public override void Enter()
		{
			CC.excludeLayers = LayerMask.GetMask("Player");
			CC.gameObject.layer = LayerMask.NameToLayer("Ghost");
			View.EnableFlatForm();
		}

		public override void Exit()
		{
			CC.gameObject.layer = LayerMask.NameToLayer("Player");
			CC.excludeLayers = 0;
			View.DisableFlatForm();
		}
	}
}