using System;
using UnityEngine;

namespace ET.GameDemo
{
    public class ScreenFadeHelper: Singleton<ScreenFadeHelper>
    {
        private ScreenFadeTimerValue timerValue = new ScreenFadeTimerValue();
        private long timerID = 0;
        private long repeatMs = 100;
        private ETCancellationToken token;

        public override void Dispose()
        {
            TimerComponent.Instance.Remove(ref this.timerID);
            if (timerValue.task != null)
                timerValue.task.SetResult();
        }

        /// <summary>
        /// 黑屏效果
        /// </summary>
        /// <param name="fadeTimeMs">过度时长, 小于等于0表示立即</param>
        /// <param name="restart">从头开始过度</param>
        public async ETTask FadeToBlack(int fadeTimeMs = 1000, bool restart = false)
        {
            fadeTimeMs = fadeTimeMs < 0? 0 : fadeTimeMs;
            if (fadeTimeMs == 0)
            {
                SetFadeValue(1);
                return;
            }

            if (restart) SetFadeValue(0);
            //
            TimerComponent.Instance.Remove(ref this.timerID);
            this.timerValue.progress = restart? 0 : this.GetFadeValue();
            this.timerValue.toBlack = true;
            this.timerValue.deltaProgress = repeatMs * 1f / fadeTimeMs;
            this.timerValue.task = ETTask.Create(true);
            this.timerValue.timerID = TimerComponent.Instance.NewRepeatedTimer(repeatMs, GameDemoTimerType_MV.ScreenFade, this.timerValue);

            await this.timerValue.task;
        }

        /// <summary>
        /// 亮屏效果
        /// </summary>
        /// <param name="fadeTimeMs">过度时长，小于等于0表示立即</param>
        /// <param name="restart">从头开始过度</param>
        public async ETTask FadeToWhite(int fadeTimeMs = 1000, bool restart = false)
        {
            fadeTimeMs = fadeTimeMs < 0? 0 : fadeTimeMs;
            if (fadeTimeMs == 0)
            {
                SetFadeValue(0);
                return;
            }

            if (restart) SetFadeValue(1);
            //
            TimerComponent.Instance.Remove(ref this.timerID);
            this.timerValue.progress = restart? 1 : this.GetFadeValue();
            this.timerValue.toBlack = false;
            this.timerValue.deltaProgress = repeatMs * 1f / fadeTimeMs;
            this.timerValue.task = ETTask.Create(true);
            this.timerValue.timerID = TimerComponent.Instance.NewRepeatedTimer(repeatMs, GameDemoTimerType_MV.ScreenFade, this.timerValue);

            await this.timerValue.task;
        }

        private void SetFadeValue(float value)
        {
        }

        private float GetFadeValue()
        {
            return 0;
        }

        public class ScreenFadeTimerValue
        {
            public float progress;
            public long timerID;
            public bool toBlack;
            public float deltaProgress;
            public ETTask task;
        }

        [Invoke(GameDemoTimerType_MV.ScreenFade)]
        public class ScreenFadeTimer: ATimer<ScreenFadeTimerValue>
        {
            protected override void Run(ScreenFadeTimerValue t)
            {
                t.progress += t.deltaProgress;
                t.progress = Mathf.Clamp01(t.progress);
                if (Math.Abs(t.progress - 1) <= float.Epsilon) t.progress = 1;
                if (Math.Abs(t.progress) <= float.Epsilon) t.progress = 0;
                Instance.SetFadeValue(t.progress);

                if (t.toBlack && Math.Abs(t.progress - 1) < float.Epsilon)
                {
                    t.task.SetResult();
                    t.task = null;
                    TimerComponent.Instance.Remove(ref t.timerID);
                    return;
                }

                if (!t.toBlack && t.progress == 0)
                {
                    t.task.SetResult();
                    t.task = null;
                    TimerComponent.Instance.Remove(ref t.timerID);
                    return;
                }
            }
        }
    }
}