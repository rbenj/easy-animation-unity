# EasyAnimation

A simple Unity animation system that makes it easy to play animation clips with callbacks, looping, and delayed playback.

## Features

- **Simple Setup** - Just add the component and assign animation clips
- **Callback Support** - Execute code when animations complete
- **Looping** - Play animations in loops with configurable intervals
- **Delayed Playback** - Start animations after delays
- **Self-Destruction** - Automatically destroy GameObjects when animations finish
- **Multiple Clips** - Cycle through arrays of animation clips
- **Animation Events** - Register callbacks for animation events

## Quick Start

1. Add an `Animator` component to your GameObject
2. Assign the `EasyAnimationController` to the Animator
3. Add the `EasyAnimation` component
4. Assign your animation clips in the inspector
5. Call `Play()` from code

## Basic Usage

```csharp
// Get the EasyAnimation component
var easyAnimation = GetComponent<EasyAnimation>();

// Play once
easyAnimation.Play();

// Play with callback
easyAnimation.Play(() => Debug.Log("Animation finished!"));

// Play with self-destruct
easyAnimation.Play(true);

// Play delayed
easyAnimation.PlayDelayed(2f);

// Play in loop
easyAnimation.PlayLoop();

// Play in loop with interval
easyAnimation.PlayLoop(1f);
```

## API Reference

### Properties
- `AnimationClip` - Single animation clip to play
- `AnimationClips` - Array of animation clips to cycle through
- `Autoplay` - Whether to automatically play on start

### Methods
- `Play()` - Play the animation once
- `PlayDelayed(delay)` - Play after a delay
- `PlayLoop(interval)` - Play in a loop
- `AddEventCallback(eventName, callback)` - Register animation event callbacks

## Requirements

- Unity 2020.3 or later
- Animator component with EasyAnimationController assigned
