using System;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Definition;

namespace Runtime.Animation
{
    public enum LoopType
    {
        Repeat,
        Yoyo
    }

    /// <summary>
    /// This controls the animation that uses sprites.
    /// </summary>
    public class SpriteAnimator : MonoBehaviour
    {
        #region Members

        [SerializeField]
        protected SpriteRenderer spriteRenderer;
        public List<SpriteAnimation> animations;

        [SerializeField]
        protected bool ignoreTimeScale = false;
        protected bool playing = false;
        protected bool waitingLoop = false;
        protected bool currentOneShot = false;
        protected bool currentBackwards = false;
        protected bool useAnimatorFramerate = false;
        protected int frameIndex = 0;
        protected int stopAtFrame = -1;
        protected int framesInAnimation = 0;
        protected int frameDurationCounter = 0;
        protected int startingFrame = -1;
        protected int currentFramerate = 60;
        protected float animationTimer = 0f;
        protected float currentAnimationTime = 0f;
        protected float loopTimer = 0f;
        protected float timePerFrame = 0f;
        protected LoopType currentLoopType;
        protected SpriteAnimation currentAnimation;

        #endregion Members

        #region Properties

        /// <summary>
        /// Gets if the animator is playing an animation or not.
        /// </summary>
        public bool IsPlaying
        {
            get { return playing; }
        }

        /// <summary>
        /// If true, the timescale of the game will be ignored.
        /// </summary>
        public bool IgnoreTimeScale
        {
            set { ignoreTimeScale = value; }
        }

        /// <summary>
        /// The current frame of the animation.
        /// </summary>
        public int CurrentFrame
        {
            get { return frameIndex; }
            set
            {
                frameIndex = value;
                if (CurrentSkillEventTriggeredCallbackAction != null)
                {
                    if (CurrentEventTriggeredFrame == frameIndex)
                        CurrentSkillEventTriggeredCallbackAction?.Invoke();
                }
            }
        }

        /// <summary>
        /// The current FPS of the animator (it could be the animation FPS or an overrided FPS).
        /// </summary>
        public int CurrentFrameRate
        {
            get { return currentFramerate; }
        }

        /// <summary>
        /// The current time in seconds of the playing animation.
        /// </summary>
        public float CurrentAnimationTime
        {
            get { return currentAnimationTime; }
        }

        /// <summary>
        /// The currently playing animation name.
        /// </summary>
        public string CurrentAnimation
        {
            get { return (currentAnimation != null) ? currentAnimation.Name : string.Empty; }
        }

        /// <summary>
        /// The currently playing animation.
        /// </summary>
        public SpriteAnimation PlayingAnimation
        {
            get { return currentAnimation; }
        }

        /// <summary>
        /// Called when the animation starts playing.
        /// </summary>
        public Action AnimationStartAction { get; set; }

        /// <summary>
        /// Called when the animation ends.
        /// </summary>
        public Action AnimationFinishedAction { get; set; }

        /// <summary>
        /// Called when the animation is stopped.
        /// </summary>
        public Action AnimationStoppedAction { get; set; }

        private Action CurrentSkillEventTriggeredCallbackAction { get; set; }
        private int CurrentEventTriggeredFrame { get; set; }

        #endregion Properties

        #region API Methods

        private void Update()
        {
            // We do nothing if the current FPS <= 0.
            if (currentAnimation == null || currentFramerate <= 0 || !playing)
                return;

            // Add the delta time to the timer and the total time.
            if (!ignoreTimeScale)
            {
                animationTimer += Time.deltaTime;
                currentAnimationTime = (!currentBackwards) ? currentAnimationTime + Time.deltaTime : currentAnimationTime - Time.deltaTime;
            }
            else
            {
                animationTimer += Time.unscaledDeltaTime;
                currentAnimationTime = (!currentBackwards) ? currentAnimationTime + Time.unscaledDeltaTime : currentAnimationTime - Time.unscaledDeltaTime;
            }

            if (!waitingLoop && animationTimer >= timePerFrame)
            {
                // Check frame skips.
                while (animationTimer >= timePerFrame)
                {
                    frameDurationCounter++;
                    animationTimer -= timePerFrame;
                }

                // Change frame only if have passed the desired frames.
                if (frameDurationCounter >= currentAnimation.FramesDuration[CurrentFrame] && playing)
                {
                    while (frameDurationCounter >= currentAnimation.FramesDuration[CurrentFrame])
                    {
                        frameDurationCounter -= currentAnimation.FramesDuration[CurrentFrame];
                        CurrentFrame = (currentBackwards) ? CurrentFrame - 1 : CurrentFrame + 1;

                        // Check last or first frame.
                        if (CheckLastFrame())
                        {
                            // Last frame, reset index and stop if is one shot.
                            AnimationFinishedAction?.Invoke();

                            if (currentOneShot)
                            {
                                Stop();
                                return;
                            }
                            else
                            {
                                waitingLoop = true;
                                loopTimer = 0;
                            }
                        }
                        else
                        {
                            // Change sprite.
                            ChangeFrame(CurrentFrame);

                            if (CurrentFrame == stopAtFrame)
                                Stop();
                        }
                    }
                }
            }

            if (waitingLoop)
            {
                // Continue looping if enought time have passed.
                loopTimer -= Time.deltaTime;
                if (loopTimer <= 0)
                {
                    if (currentLoopType == LoopType.Yoyo)
                        currentBackwards = !currentBackwards;

                    waitingLoop = false;
                    // Continue playing the same animation.
                    animationTimer = 0;
                    currentAnimationTime = (currentBackwards) ? currentAnimation.AnimationDuration * timePerFrame : 0;
                    CurrentFrame = (currentBackwards) ? framesInAnimation - 1 : 0;

                    ChangeFrame(CurrentFrame);
                }
            }
        }

        #endregion API Methods

        #region Class Methods

        /// <summary>
        /// Plays the specified animation.
        /// </summary>
        public void Play(SpriteAnimation animation, bool playOneShot = false, Action eventTriggeredCallbackAction = null,
                         int eventTriggeredFrame = -1, bool playBackwards = false, LoopType loopType = LoopType.Repeat)
        {
            CurrentSkillEventTriggeredCallbackAction = null;
            if (eventTriggeredCallbackAction != null)
            {
                CurrentSkillEventTriggeredCallbackAction = () => eventTriggeredCallbackAction.Invoke();
                CurrentEventTriggeredFrame = eventTriggeredFrame;
            }

            currentOneShot = playOneShot;
            currentBackwards = playBackwards;
            currentLoopType = loopType;

            // If it's the same animation but not playing, reset it, if playing, do nothing.
            if (currentAnimation != null && currentAnimation.Equals(animation))
            {
                if (!playing)
                {
                    Restart();
                    Resume();
                }
                else return;
            }
            // If the animation is new, save it as current animation and play it.
            else currentAnimation = animation;

            StartPlay();
        }

        /// <summary>
        /// Plays the first animation of the animation list.
        /// </summary>
        public void Play(bool playOneShot = false, Action eventTriggeredCallbackAction = null, int eventTriggeredFrame = -1,
                         bool playBackwards = false, LoopType loopType = LoopType.Repeat)
        {
            Play(animations[0].Name, playOneShot, eventTriggeredCallbackAction, eventTriggeredFrame, playBackwards, loopType);
        }

        /// <summary>
        /// Plays an animation.
        /// </summary>
        public void Play(string animation, bool playOneShot = false, Action eventTriggeredCallbackAction = null,
                         int eventTriggeredFrame = -1, bool playBackwards = false, LoopType loopType = LoopType.Repeat)
        {
            CurrentSkillEventTriggeredCallbackAction = null;
            if (eventTriggeredCallbackAction != null)
            {
                CurrentSkillEventTriggeredCallbackAction = () => eventTriggeredCallbackAction.Invoke();
                CurrentEventTriggeredFrame = eventTriggeredFrame;
            }

            currentOneShot = playOneShot;
            currentBackwards = playBackwards;
            currentLoopType = loopType;

            // If it's the same animation but not playing, reset it, if playing, do nothing.
            if (currentAnimation != null && currentAnimation.Name.Equals(animation))
            {
                if (!playing)
                {
                    Restart();
                    Resume();
                }
                else return;
            }
            // Look for the animation only if its new or current animation is null
            else if (currentAnimation == null || !currentAnimation.Name.Equals(animation))
                currentAnimation = GetAnimation(animation);

            StartPlay();
        }

        /// <summary>
        /// Tint the sprite's color.
        /// </summary>
        public void TintColor(Color color)
            => spriteRenderer.material.SetColor(Constants.HIT_MATERIAL_COLOR_PROPERTY, color);

        /// <summary>
        /// Resumes the animation.
        /// </summary>
        public void Resume()
        {
            if (currentAnimation != null)
                playing = true;
        }

        /// <summary>
        /// Stops when reaches the desired frame. If the desired frame has already passed and the animation is not looped it will stop at the end of the animation anyway.
        /// </summary>
        public void StopAtFrame(int frame)
        {
            stopAtFrame = frame;
        }

        /// <summary>
        /// Stops the animation.
        /// </summary>
        public void Stop()
        {
            stopAtFrame = -1;
            playing = false;
            AnimationStoppedAction?.Invoke();
        }

        /// <summary>
        /// Sets the animator FPS overriding the FPS of the animation.
        /// </summary>
        public void UseAnimatorFPS(int frameRate)
        {
            currentFramerate = frameRate;
            timePerFrame = 1f / currentFramerate;
            useAnimatorFramerate = true;
        }

        /// <summary>
        /// Sets the animator FPS to the current animation FPS.
        /// </summary>
        public void UseAnimationFPS()
        {
            if (currentAnimation != null)
            {
                currentFramerate = currentAnimation.FPS;
                timePerFrame = 1f / currentFramerate;
                useAnimatorFramerate = false;
            }
        }

        /// <summary>
        /// Restarts the animation. If the animation is not playing the effects will apply when starts playing.
        /// </summary>
        public void Restart()
        {
            animationTimer = 0;
            CurrentFrame = (currentBackwards) ? framesInAnimation - 1 : 0;
            frameDurationCounter = 0;
            ChangeFrame(CurrentFrame);
        }

        private void StartPlay()
        {
            // If we have an animation to play, flag as playing, reset timer and take frame count.
            if (currentAnimation != null)
            {
                // Failsafe for old animations without the total animation duration calculated.
                if (currentAnimation.AnimationDuration == -1)
                    currentAnimation.Setup();

                if (!useAnimatorFramerate)
                    currentFramerate = currentAnimation.FPS;
                timePerFrame = 1f / currentFramerate;
                framesInAnimation = currentAnimation.FramesCount;
                currentAnimationTime = (currentBackwards) ? currentAnimation.AnimationDuration * timePerFrame : 0;

                // Check if the animation have frames. Show warning if not.
                if (framesInAnimation == 0)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.LogWarning("Animation '" + name + "' has no frames.", gameObject);
#endif
                    playing = false;
                    return;
                }

                Restart();

                // The first loop will have a random start frame if desired.
                if (startingFrame != -1)
                {
                    CurrentFrame = startingFrame;
                    if (CheckLastFrame())
                    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        Debug.LogWarning("Starting frame out of bounds.", gameObject);
#endif
                        CurrentFrame = 0;
                    }
                    startingFrame = -1;
                }

                AnimationStartAction?.Invoke();
                playing = true;

                if (!waitingLoop)
                    ChangeFrame(CurrentFrame);
            }
        }

        /// <summary>
        /// Search an animation with the given name.
        /// </summary>
        /// <returns>
        /// The animation. Null if not found.
        /// </returns>  
        private SpriteAnimation GetAnimation(string animationName)
        {
            return animations.Find(x => x.Name.Equals(animationName));
        }

        /// <summary>
        /// Changes the renderer to the given sprite
        /// </summary>
        private void ChangeFrame(int frameIndex)
            => spriteRenderer.sprite = currentAnimation.GetFrame(frameIndex);

        /// <summary>
        /// Sets the animation time to the specified time, updating de sprite to the correspondent frame at that time.
        /// </summary>
        /// <param name="time">Time in seconds</param>
        public void SetAnimationTime(float time)
        {
            if (currentAnimation != null)
            {
                float timePerFrame = 1f / currentFramerate;
                float totalAnimationTime = currentAnimation.AnimationDuration * timePerFrame;

                if (time >= totalAnimationTime)
                {
                    currentAnimationTime = totalAnimationTime;
                    animationTimer = timePerFrame;
                    CurrentFrame = framesInAnimation - 1;
                    frameDurationCounter = currentAnimation.FramesDuration[CurrentFrame] - 1;
                }
                else if (time <= 0)
                {
                    animationTimer = 0;
                    CurrentFrame = 0;
                    frameDurationCounter = 0;
                    currentAnimationTime = 0;
                }
                else
                {
                    CurrentFrame = 0;
                    frameDurationCounter = 0;
                    currentAnimationTime = time;

                    while (time >= timePerFrame)
                    {
                        time -= timePerFrame;
                        frameDurationCounter++;

                        if (frameDurationCounter >= currentAnimation.FramesDuration[CurrentFrame])
                        {
                            CurrentFrame++;
                            frameDurationCounter = 0;
                        }
                    }

                    if (CurrentFrame >= framesInAnimation)
                        CurrentFrame = framesInAnimation - 1;

                    animationTimer = time;
                }

                ChangeFrame(CurrentFrame);
            }
        }

        /// <summary>
        /// Sets the animation time to the specified normalized time (between 0 and 1), updating de sprite to the correspondent frame at that time.
        /// </summary>
        /// <param name="time">Time normalized (between 0 and 1).</param>
        public void SetAnimationNormalizedTime(float normalizedTime)
        {
            if (currentAnimation != null)
            {
                normalizedTime = Mathf.Clamp(normalizedTime, 0f, 1f);
                SetAnimationTime(currentAnimation.AnimationDuration * timePerFrame * normalizedTime);
            }
        }

        /// <summary>
        /// Check the last frame (backwards or not).
        /// </summary>
        private bool CheckLastFrame()
        {
            if ((!currentBackwards && CurrentFrame > framesInAnimation - 1))
            {
                CurrentFrame = framesInAnimation - 1;
                return true;
            }
            else if (currentBackwards && CurrentFrame < 0)
            {
                CurrentFrame = 0;
                return true;
            }

            return false;
        }

        #endregion Class Methods
    }
}