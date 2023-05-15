using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ET
{
    public class ScreenFadeHelper: MonoBehaviour
    {
        private Volume volume;
        private Vignette vignette;

        private ETCancellationToken fadeToken;

        private void Awake()
        {
            if (!volume.profile.TryGet(out vignette))
                throw new Exception("工程未实现黑屏效果");
        }

        private void Start()
        {
            SetFadeValue(1);
        }

        private void OnDestroy()
        {
            fadeToken?.Cancel();
        }

        private float GetFadeValue() => vignette.intensity.value;

        private void SetFadeValue(float value)
        {
            value = Mathf.Clamp01(value);

            vignette.center.Override(Vector2.one * 0.5f);

            if (Math.Abs(value - 1) < float.Epsilon)
            {
                value = 1;
                vignette.center.Override(Vector2.one * 2);
            }

            if (value < float.Epsilon)
            {
                value = 0;
            }

            vignette.intensity.Override(value);
        }

        /// <summary>
        /// 黑屏效果
        /// </summary>
        /// <param name="fadeTimeMs">过度时长, 小于等于0表示立即</param>
        /// <param name="restart">从头开始过度</param>
        public async ETTask FadeToBlack(int fadeTimeMs = 1000, bool restart = false,
        ETCancellationToken token = default)
        {
            fadeToken?.Cancel();
            var curFadeToken = fadeToken = new ETCancellationToken();

            try
            {
                token?.Add(CancelAction);
                fadeTimeMs = fadeTimeMs < 0? 0 : fadeTimeMs;
                if (fadeTimeMs < float.Epsilon)
                {
                    SetFadeValue(1);
                    return;
                }

                await FadeTaskAsync(fadeTimeMs, true, restart, curFadeToken);
            }
            finally
            {
                token?.Remove(CancelAction);
            }

            void CancelAction()
            {
                curFadeToken.Cancel();
            }
        }

        /// <summary>
        /// 亮屏效果
        /// </summary>
        /// <param name="fadeTimeMs">过度时长，小于等于0表示立即</param>
        /// <param name="restart">从头开始过度</param>
        public async ETTask FadeToWhite(int fadeTimeMs = 1000, bool restart = false,
        ETCancellationToken token = default)
        {
            fadeToken?.Cancel();
            var curFadeToken = fadeToken = new ETCancellationToken();

            try
            {
                token?.Add(CancelAction);
                fadeTimeMs = fadeTimeMs < 0? 0 : fadeTimeMs;
                if (fadeTimeMs < float.Epsilon)
                {
                    SetFadeValue(0);
                    return;
                }

                await FadeTaskAsync(fadeTimeMs, false, restart, curFadeToken);
            }
            finally
            {
                token?.Remove(CancelAction);
            }

            void CancelAction()
            {
                curFadeToken.Cancel();
            }
        }

        private async ETTask FadeTaskAsync(int fadeTimeMs, bool toback, bool restart, ETCancellationToken token)
        {
            float end = toback? 1 : 0;
            float current = restart? (toback? 0 : 1) : GetFadeValue();
            var delat = 1f / fadeTimeMs * (toback? 1 : -1);
            var startTime = TimeInfo.Instance.FrameTime;
            //这里不用Timer的原因是因为timer repeat的时间最小是100ms,看起来太卡了，还是每帧都更新比较舒服
            while (Mathf.Abs(current - end) > float.Epsilon)
            {
                if (token != null && token.IsCancel())
                    return;
                var curTime = TimeInfo.Instance.FrameTime;
                var deltaTime = curTime - startTime;
                startTime = curTime;
                current += deltaTime * delat;
                current = Mathf.Clamp01(current);
                SetFadeValue(current);
                await TimerComponent.Instance.WaitFrameAsync(token);
            }
        }
    }
}