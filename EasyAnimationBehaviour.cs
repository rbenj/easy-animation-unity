using UnityEngine;

namespace EasyAnimation
{
	/// <summary>
	/// State machine behaviour that notifies the EasyAnimation component when animation state exits.
	/// </summary>
	public class EasyAnimationBehaviour : StateMachineBehaviour {
		/// <summary>
		/// Called when the animator state exits. Notifies the EasyAnimation component.
		/// </summary>
		/// <param name="animator">The animator component.</param>
		/// <param name="stateInfo">Information about the animator state.</param>
		/// <param name="layerIndex">Index of the layer.</param>
		override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			if (animator.TryGetComponent<EasyAnimation>(out var easyAnimation)) {
				easyAnimation.OnAnimationExit();
			}
		}
	}
}
