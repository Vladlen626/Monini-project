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
			View.EnableFlatForm();
		}

		public override void Exit()
		{
			CC.excludeLayers = 0;
			View.DisableFlatForm();
		}
	}
}