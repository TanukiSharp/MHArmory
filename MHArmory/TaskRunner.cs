using System;
using System.Threading;
using System.Threading.Tasks;

namespace MHArmory
{
    /// <summary>
    /// Manages the lifetime of a single task, and automatically cancel the previous one when run a new one.
    /// It also awaits for previous task to be fully terminated before running the new one.
    /// </summary>
    public class TaskRunner
    {
        private CancellationTokenSource currentTokenSource;
        private Task currentTask;
        private int microThreadId = 0;

        /// <summary>
        /// Gets a value indicating whether a task is currently running or not.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return currentTask != null;
            }
        }

        /// <summary>
        /// Cancels the currently running task, if any.
        /// </summary>
        /// <returns>Returns true if nobody called CancelInternal when we return.</returns>
        private async Task<bool> CancelInternal(bool throwTaskCanceledException)
        {
            microThreadId = unchecked(microThreadId + 1);
            int localMicroThreadId = microThreadId;

            // This correspond to currentTokenSource being disposed and different from currentTokenSource being set to null
            if (currentTask == null)
                return true;

            if (currentTokenSource != null)
            {
                currentTokenSource.Cancel();

                // The above currentTokenSource.Cancel() can run the finally block of CancelAndExecute() and set currentTask to null
                if (currentTask != null)
                {
                    try
                    {
                        // await currentTask will dispose the current token source thanks to the finally
                        await currentTask;
                    }
                    catch (TaskCanceledException) when (throwTaskCanceledException == false)
                    {
                    }
                }
            }

            return localMicroThreadId == microThreadId;
        }

        /// <summary>
        /// Cancels the currently running task, if any.
        /// </summary>
        /// <param name="throwTaskCanceledException">Pass true allow to throw a TaskCancelledException</param>
        /// <returns>Return a Task that completes when the current job is fully cancelled.</returns>
        public async Task Cancel(bool throwTaskCanceledException = false)
        {
            await CancelInternal(throwTaskCanceledException);
        }

        // INTERNAL NOTE:
        // Both generic and typed methods CancelAndExecute look very much copy/paste.
        // This is done intentionally to save an extra (and unnecessary) heap allocation.

        /// <summary>
        /// Cancels the current task if any, and runs a new one.
        /// </summary>
        /// <param name="taskFactory">A task producer that receive a <see cref="CancellationToken"/>.</param>
        /// <param name="throwTaskCanceledException">Pass true allow to throw a TaskCancelledException</param>
        /// <returns>Returns the task produced by the <paramref name="taskFactory"/>.</returns>
        public async Task CancelAndExecute(Func<CancellationToken, Task> taskFactory, bool throwTaskCanceledException = false)
        {
            if (await CancelInternal(throwTaskCanceledException) == false)
            {
                if (throwTaskCanceledException == false)
                    return;
                else
                    throw new TaskCanceledException();
            }

            var localToken = new CancellationTokenSource();
            currentTokenSource = localToken;

            try
            {
                currentTask = taskFactory(currentTokenSource.Token);
                await currentTask;
            }
            catch (TaskCanceledException) when (throwTaskCanceledException == false)
            {
            }
            finally
            {
                currentTask = null;

                localToken.Dispose();
            }
        }

        /// <summary>
        /// Cancels the current task if any, and runs a new one.
        /// </summary>
        /// <typeparam name="T">Type of value returned by the task to run.</typeparam>
        /// <param name="taskFactory">A task producer that receive a <see cref="CancellationToken"/>.</param>
        /// <param name="throwTaskCanceledException">Pass true allow to throw a TaskCancelledException</param>
        /// <returns>Returns the task produced by the <paramref name="taskFactory"/>.</returns>
        public async Task<T> CancelAndExecute<T>(Func<CancellationToken, Task<T>> taskFactory, bool throwTaskCanceledException = false)
        {
            if (await CancelInternal(throwTaskCanceledException) == false)
            {
                if (throwTaskCanceledException == false)
                    return default;
                else
                    throw new TaskCanceledException();
            }

            var localToken = new CancellationTokenSource();
            currentTokenSource = localToken;

            try
            {
                Task<T> typedTask = taskFactory(currentTokenSource.Token);
                currentTask = typedTask;
                return await typedTask;
            }
            catch (TaskCanceledException) when (throwTaskCanceledException == false)
            {
                return default;
            }
            finally
            {
                currentTask = null;

                localToken.Dispose();
            }
        }
    }
}
