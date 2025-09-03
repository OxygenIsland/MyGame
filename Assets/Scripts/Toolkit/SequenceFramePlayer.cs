using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace StarWorld.Common.Utility
{
    /// <summary>
    /// 序列帧图片播放器
    /// </summary>
    public class SequenceFramePlayer : MonoBehaviour
    {
        /// <summary>
        /// 播放状态
        /// </summary>
        public enum PlayState
        {
            Once = 0,
            Loop
        }

        /// <summary>
        /// 图片状态
        /// </summary>
        public enum SpriteState
        {
            Normal,
            Highlighted,
            Pressed,
            Selected,
            Disabled
        }

        [Tooltip("目标图片组件")] public Image targetImage;
        [Tooltip("播放方式")] public PlayState playState = PlayState.Loop;
        [Tooltip("帧率")] [Range(1, 60)] public int frameRate = 24;
        [Tooltip("序列帧图片数组")] public Sprite[] frameSprites;
        [Tooltip("是否自动播放")] public bool autoPlay = true;

        [Header("状态图片设置")]
        [Tooltip("普通状态图片")] public Sprite normalSprite;
        [Tooltip("高亮状态图片")] public Sprite highlightedSprite;
        [Tooltip("按下状态图片")] public Sprite pressedSprite;
        [Tooltip("选中状态图片")] public Sprite selectedSprite;
        [Tooltip("禁用状态图片")] public Sprite disabledSprite;
        [Tooltip("是否使用状态图片")] public bool useStateSprites = false;

        [System.NonSerialized] public int currentFrameIndex = 0;
        [System.NonSerialized] public bool isPlaying = false;
        [System.NonSerialized] public SpriteState currentState = SpriteState.Normal;

        private float frameInterval = 0;
        private Coroutine playCoroutine;

        private void OnEnable()
        {
            frameInterval = 1.0f / frameRate;
            currentFrameIndex = 0;

            if (autoPlay && frameSprites != null && frameSprites.Length > 0 && !useStateSprites)
            {
                Play();
            }
            else if (useStateSprites)
            {
                SetState(currentState);
            }
        }

        private void OnDisable()
        {
            Stop();
        }

        /// <summary>
        /// 开始播放序列帧
        /// </summary>
        public void Play()
        {
            if (isPlaying || frameSprites == null || frameSprites.Length == 0 || useStateSprites)
                return;

            isPlaying = true;
            playCoroutine = StartCoroutine(PlayUpdate());
        }

        /// <summary>
        /// 暂停播放
        /// </summary>
        public void Pause()
        {
            if (!isPlaying)
                return;

            isPlaying = false;
            if (playCoroutine != null)
            {
                StopCoroutine(playCoroutine);
                playCoroutine = null;
            }
        }

        /// <summary>
        /// 停止播放并重置到第一帧
        /// </summary>
        public void Stop()
        {
            isPlaying = false;
            if (playCoroutine != null)
            {
                StopCoroutine(playCoroutine);
                playCoroutine = null;
            }
            currentFrameIndex = 0;
            UpdateFrame();
        }

        /// <summary>
        /// 更新当前帧
        /// </summary>
        private void UpdateFrame()
        {
            if (targetImage == null)
                return;

            if (useStateSprites)
            {
                SetState(currentState);
                return;
            }

            if (frameSprites == null || frameSprites.Length == 0 || currentFrameIndex >= frameSprites.Length)
                return;

            targetImage.sprite = frameSprites[currentFrameIndex];
        }

        /// <summary>
        /// 播放更新协程
        /// </summary>
        private IEnumerator PlayUpdate()
        {
            while (isPlaying)
            {
                UpdateFrame();
                
                yield return new WaitForSeconds(frameInterval);
                
                currentFrameIndex++;
                
                if (currentFrameIndex >= frameSprites.Length)
                {
                    if (playState == PlayState.Once)
                    {
                        Stop();
                        break;
                    }
                    else if (playState == PlayState.Loop)
                    {
                        currentFrameIndex = 0;
                    }
                }
            }
        }

        /// <summary>
        /// 设置特定帧
        /// </summary>
        public void SetFrame(int frameIndex)
        {
            if (useStateSprites)
                return;
                
            if (frameSprites == null || frameSprites.Length == 0)
                return;
                
            currentFrameIndex = Mathf.Clamp(frameIndex, 0, frameSprites.Length - 1);
            UpdateFrame();
        }

        /// <summary>
        /// 设置图片状态
        /// </summary>
        public void SetState(SpriteState state)
        {
            if (!useStateSprites || targetImage == null)
                return;

            currentState = state;
            
            switch (state)
            {
                case SpriteState.Normal:
                    if (normalSprite != null)
                        targetImage.sprite = normalSprite;
                    break;
                case SpriteState.Highlighted:
                    if (highlightedSprite != null)
                        targetImage.sprite = highlightedSprite;
                    break;
                case SpriteState.Pressed:
                    if (pressedSprite != null)
                        targetImage.sprite = pressedSprite;
                    break;
                case SpriteState.Selected:
                    if (selectedSprite != null)
                        targetImage.sprite = selectedSprite;
                    break;
                case SpriteState.Disabled:
                    if (disabledSprite != null)
                        targetImage.sprite = disabledSprite;
                    break;
            }
        }

        /// <summary>
        /// 设置普通状态
        /// </summary>
        public void SetNormal()
        {
            SetState(SpriteState.Normal);
        }

        /// <summary>
        /// 设置高亮状态
        /// </summary>
        public void SetHighlighted()
        {
            SetState(SpriteState.Highlighted);
        }

        /// <summary>
        /// 设置按下状态
        /// </summary>
        public void SetPressed()
        {
            SetState(SpriteState.Pressed);
        }

        /// <summary>
        /// 设置选中状态
        /// </summary>
        public void SetSelected()
        {
            SetState(SpriteState.Selected);
        }

        /// <summary>
        /// 设置禁用状态
        /// </summary>
        public void SetDisabled()
        {
            SetState(SpriteState.Disabled);
        }
    }
} 