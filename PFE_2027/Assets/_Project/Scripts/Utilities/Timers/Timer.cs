using System;
using System.Threading;
using UnityEngine;

namespace PFE.Utilities.Scripts.Timers
{
    public class Timer
    {
        public bool IsFinish => elapsed >= duration;
        
        private readonly float duration;
        private readonly float interval;

        private readonly AwaitableCompletionSource resumeSignal = new();

        private CancellationTokenSource cts;
        private float elapsed;
        private bool isPaused;

        public Timer(float duration, float interval = .1f)
        {
            this.duration = duration;
            this.interval = interval;
        }

        public async void Run()
        {
            try
            {
                // A fresh run always gets a fresh token - a cancelled CancellationTokenSource can't be reused.
                cts?.Cancel();
                cts?.Dispose();
                cts = new CancellationTokenSource();
                elapsed = 0f;
                isPaused = false;
                
                while (elapsed < duration)
                {
                    if (isPaused)
                    {
                        // Reset before awaiting: the source completes once, so it must be armed again
                        // for every pause, not just the first one.
                        resumeSignal.Reset();
                        await resumeSignal.Awaitable;
                        continue;
                    }

                    await Awaitable.WaitForSecondsAsync(interval, cts.Token);
                    elapsed += interval;
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when Stop() cancels a run in progress - not an error.
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void Stop()
        {
            
            cts?.Cancel();
            resumeSignal.TrySetCanceled();
        }

        public void Pause() => isPaused = true;

        public void Resume()
        {
            isPaused = false;
            resumeSignal.TrySetResult();
        }
    }
}
