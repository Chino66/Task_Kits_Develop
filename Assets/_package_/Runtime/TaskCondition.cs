using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace TaskKits
{
    public class TaskCondition
    {
        public bool IsRunning;

        public int Timeout = 10000;

        public CancellationTokenSource TokenSource;

        public TaskCondition()
        {
        }

        public void Start(int timeout = 10000)
        {
            Timeout = timeout;
            IsRunning = true;
            TokenSource = new CancellationTokenSource();
        }

        public void Complete()
        {
            IsRunning = false;
            TokenSource?.Cancel();
        }

        public async Task<bool> WaitUntilComplete(int timeout = 10000)
        {
            Start(timeout);
            return await WaitUntil(this);
        }

        public async Task<bool> WaitUntilProgress([NotNull] Func<bool> check,
            int millisecondsDelay = 100,
            int timeout = 10000)
        {
            if (check == null)
            {
                Debug.LogError("check is null");
                return false;
            }

            var timecount = 0;
            while (check?.Invoke() == false)
            {
                await Task.Delay(millisecondsDelay);
                timecount += millisecondsDelay;
                if (timecount >= timeout)
                {
                    break;
                }
            }

            return check.Invoke();
        }

        #region Static

//        public static async Task<bool> WaitUntilCondition(TaskCondition condition, int millisecondsDelay = 100)
//        {
//            while (condition.Value == false)
//            {
//                await Task.Delay(millisecondsDelay);
//            }
//
//            return condition.Value;
//        }
//
//        public static async Task<bool> WaitUntilCondition(TaskCondition condition)
//        {
//            await WaitUntilCondition(condition, 100);
//
//            return condition.Value;
//        }

        public static async Task<bool> WaitUntil(TaskCondition condition)
        {
            var task = Task.Delay(condition.Timeout, condition.TokenSource.Token)
                .ContinueWith(tsk => tsk.Exception == default);

            try
            {
                await task;
            }
            catch (OperationCanceledException e)
            {
                Debug.LogError(e);
            }
            finally
            {
                condition.TokenSource?.Dispose();
            }

            // 如果condition.IsRunning是true,则返回false表示超时
            return !condition.IsRunning;
        }

        #endregion
    }
}