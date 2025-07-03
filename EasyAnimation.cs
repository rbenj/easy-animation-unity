using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyAnimation
{
	/// <summary>
	/// Simple animation system for easy playback of animation clips with callbacks and looping support.
	/// </summary>
	public class EasyAnimation : MonoBehaviour {
		/// <summary>
		/// Single animation clip to play.
		/// </summary>
		public AnimationClip AnimationClip;
		
		/// <summary>
		/// Array of animation clips to cycle through.
		/// </summary>
		public AnimationClip[] AnimationClips;
		
		/// <summary>
		/// Whether to automatically play the animation on start.
		/// </summary>
		public bool Autoplay;

		private const float TIMEOUT = 10.0f;
		private const string ANIMATION = "EasyAnimationAnimation";
		private const string CONTROLLER = "EasyAnimationController";
		private const string TRIGGER_PARAM = "Play";
		private const string LOOP_PARAM = "IsNativeLoop";
		private Animator _animator;
		private AnimatorOverrideController _overrideController;
		private int _animationClipIndex = 0;
		private bool _complete = false;
		private readonly Dictionary<string, Action> _eventCallbacks = new();

		/// <summary>
		/// Plays the animation once.
		/// </summary>
		/// <param name="selfDestruct">Whether to destroy the GameObject when animation completes.</param>
		public void Play(bool selfDestruct = false) {
			ActualPlay(selfDestruct, null);
		}
		
		/// <summary>
		/// Plays the animation once with a callback when it completes.
		/// </summary>
		/// <param name="onEnd">Callback to invoke when animation completes.</param>
		public void Play(Action onEnd) {
			ActualPlay(false, onEnd);
		}
		
		/// <summary>
		/// Plays the animation once with optional self-destruction and callback.
		/// </summary>
		/// <param name="selfDestruct">Whether to destroy the GameObject when animation completes.</param>
		/// <param name="onEnd">Callback to invoke when animation completes.</param>
		public void Play(bool selfDestruct, Action onEnd) {
			ActualPlay(selfDestruct, onEnd);
		}

		/// <summary>
		/// Plays the animation after a delay.
		/// </summary>
		/// <param name="delay">Delay in seconds before playing.</param>
		/// <param name="selfDestruct">Whether to destroy the GameObject when animation completes.</param>
		public void PlayDelayed(float delay, bool selfDestruct = false) {
			StartCoroutine(ActualPlayDelayed(delay, selfDestruct, null));
		}
		
		/// <summary>
		/// Plays the animation after a delay with a callback.
		/// </summary>
		/// <param name="delay">Delay in seconds before playing.</param>
		/// <param name="onEnd">Callback to invoke when animation completes.</param>
		public void PlayDelayed(float delay, Action onEnd) {
			StartCoroutine(ActualPlayDelayed(delay, false, onEnd));
		}
		
		/// <summary>
		/// Plays the animation after a delay with optional self-destruction and callback.
		/// </summary>
		/// <param name="delay">Delay in seconds before playing.</param>
		/// <param name="selfDestruct">Whether to destroy the GameObject when animation completes.</param>
		/// <param name="onEnd">Callback to invoke when animation completes.</param>
		public void PlayDelayed(float delay, bool selfDestruct, Action onEnd) {
			StartCoroutine(ActualPlayDelayed(delay, selfDestruct, onEnd));
		}

		/// <summary>
		/// Plays the animation in a loop.
		/// </summary>
		/// <param name="interval">Delay between loop iterations in seconds.</param>
		public void PlayLoop(float interval = 0) {
			StartCoroutine(ActualPlayLoop(interval));
		}

		/// <summary>
		/// Plays the animation in a loop after an initial delay.
		/// </summary>
		/// <param name="delay">Initial delay in seconds before starting the loop.</param>
		/// <param name="interval">Delay between loop iterations in seconds.</param>
		public void PlayLoopDelayed(float delay, float interval = 0) {
			StartCoroutine(ActualPlayLoopDelayed(delay, interval));
		}

		/// <summary>
		/// Called by the animator when the animation state exits.
		/// </summary>
		public void OnAnimationExit() {
			_complete = true;
		}

		/// <summary>
		/// Handles animation events by invoking registered callbacks.
		/// </summary>
		/// <param name="eventName">Name of the animation event.</param>
		public void OnAnimationEvent(string eventName) {
			if (_eventCallbacks.TryGetValue(eventName, out Action callback)) {
				callback?.Invoke();
			}
		}

		/// <summary>
		/// Adds a callback for a specific animation event.
		/// </summary>
		/// <param name="eventName">Name of the animation event.</param>
		/// <param name="callback">Callback to invoke when the event occurs.</param>
		public void AddEventCallback(string eventName, Action callback) {
			if (!_eventCallbacks.ContainsKey(eventName)) {
				_eventCallbacks[eventName] = callback;
			} else {
				_eventCallbacks[eventName] += callback;
			}
		}

		/// <summary>
		/// Removes a callback for a specific animation event.
		/// </summary>
		/// <param name="eventName">Name of the animation event.</param>
		/// <param name="callback">Callback to remove.</param>
		public void RemoveEventCallback(string eventName, Action callback) {
			if (_eventCallbacks.ContainsKey(eventName)) {
				_eventCallbacks[eventName] -= callback;
			}
		}

		private void Awake() {
			_animator = GetComponent<Animator>();

			if (_animator == null) {
				Debug.LogError($"[{name}] Animator is missing.");
				return;
			}

			if (_animator.runtimeAnimatorController == null || _animator.runtimeAnimatorController.name != CONTROLLER) {
				Debug.LogError($"[{name}] Animator must have {CONTROLLER} controller assigned.");
				return;
			}

			// Copy the runtime animator controller to an override controller
			_overrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);
			_animator.runtimeAnimatorController = _overrideController;
		}

		private void Start() {
			if (Autoplay) {
				Play();
			}
		}

		private void OnDestroy() {
			StopAllCoroutines();
		}

		private int AnimationClipIndex {
			get => _animationClipIndex;
			set {
				if (AnimationClips != null && AnimationClips.Length > 0) {
					_animationClipIndex = ((value % AnimationClips.Length) + AnimationClips.Length) % AnimationClips.Length;
					AnimationClip = AnimationClips[_animationClipIndex];
				} else {
					_animationClipIndex = value;
				}
			}
		}

		private AnimationClip ChooseAnimationClip() {
			if (AnimationClips != null && AnimationClips.Length > 0) {
				return AnimationClips[AnimationClipIndex];
			}

			return AnimationClip;
		}

		private void StartAnimation() {
			if (_overrideController == null) {
				Debug.LogError($"[{name}] AnimatorOverrideController is missing.");
				return;
			}

			AnimationClip clip = ChooseAnimationClip();

			if (clip == null) {
				Debug.LogError($"[{name}] Animation clip is missing.");
				return;
			}

			// Assign the animation clip to the override controller
			_overrideController[ANIMATION] = clip;

			// Reset completion flag
			_complete = false;

			// Start the animation via the animator trigger
			_animator.SetTrigger(TRIGGER_PARAM);
		}

		private IEnumerator WaitForAnimationToEnd() {
			float elapsed = 0f;

			// Flag is restarted on every play and triggered by exit behavior
			while (!_complete) {
				elapsed += Time.deltaTime;

				// Allow timeout just in case it never gets triggered (should never happen)
				if (elapsed > TIMEOUT) {
					Debug.LogWarning($"[{name}] Animation completion timed out after {TIMEOUT} seconds.");
					break;
				}

				yield return null;
			}
		}

		private void ActualPlay(bool selfDestruct, Action onEnd) {
			AnimationClip clip = ChooseAnimationClip();

			// Use native animation clip looping when playing an animation clip with loop time checked
			if (clip.isLooping) {
				_animator.SetBool(LOOP_PARAM, true);

				StartAnimation();

				// Heads up, selfDestruct and onEnd will be ignored when using native looping
				return;
			}

			StartAnimation();

			// Maybe do stuff after
			if (selfDestruct || onEnd != null) {
				StartCoroutine(OnEnd(selfDestruct, onEnd));
			}
		}

		private IEnumerator ActualPlayDelayed(float delay, bool selfDestruct, Action onEnd) {
			yield return new WaitForSeconds(delay);

			ActualPlay(selfDestruct, onEnd);
		}

		private IEnumerator ActualPlayLoop(float interval) {
			while (true) {
				StartAnimation();

				yield return WaitForAnimationToEnd();

				if (interval > 0) {
					yield return new WaitForSeconds(interval);
				}
			}
		}

		private IEnumerator ActualPlayLoopDelayed(float delay, float interval) {
			yield return new WaitForSeconds(delay);

			StartCoroutine(ActualPlayLoop(interval));
		}

		private IEnumerator OnEnd(bool selfDestruct, Action onEnd) {
			yield return WaitForAnimationToEnd();

			onEnd?.Invoke();

			if (selfDestruct) {
				Destroy(gameObject);
			}
		}
	}
}
