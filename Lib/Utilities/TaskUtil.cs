namespace Lib.Utilities
{
    public static class TaskExUtil
    {
        /// <summary>
        /// Debounce
        /// </summary>
        /// <param name="delayMilliseconds">延遲執行</param>
        /// <remarks>https://stackoverflow.com/questions/28472205/c-sharp-event-debounce</remarks>
        public static Action Debounce(this Action action, int delayMilliseconds = 1000)
        {
            // Calling Cancel is enough to dispose of the CTS.
            // A successfully completed CTS is not canceled/disposed until the next call.
            // Tasks get disposed so no need to call Dispose on the task.

            // usage:
            // 以 QueryViewNameListDebounce() 取代 QueryViewNameList() 執行

            //private Action _queryViewNameListDebounce;
            //private Action QueryViewNameListDebounce => _queryViewNameListDebounce ??
            //    (_queryViewNameListDebounce = ((Action)QueryViewNameList).Debounce(2000));

            // private async void QueryViewNameList() { await ... }

            CancellationTokenSource? cancelTokenSource = null;

            return () =>
            {
                cancelTokenSource?.Cancel();
                cancelTokenSource = new CancellationTokenSource();

                Task.Delay(delayMilliseconds, cancelTokenSource.Token)
                    .ContinueWith(task =>
                     {
                         if (!(task.IsCanceled || task.IsFaulted)) // .NetCore: if (task.IsCompletedSuccessfully)
                             action?.Invoke();
                     }, TaskScheduler.Default); // TaskScheduler.Default is the thread pool context.
            };
        }

        /// <summary>
        /// Debounce
        /// </summary>
        /// <param name="delayMilliseconds">延遲執行</param>
        /// <remarks>https://stackoverflow.com/questions/28472205/c-sharp-event-debounce</remarks>
        public static Func<Task> Debounce(this Func<CancellationToken, Task> func, int delayMilliseconds = 1000)
        {
            // Calling Cancel is enough to dispose of the CTS.
            // A successfully completed CTS is not canceled/disposed until the next call.
            // Tasks get disposed so no need to call Dispose on the task.

            // usage:
            // 以 QueryViewNameListDebounce() 取代 QueryViewNameList() 執行

            //private Func<Task> _queryViewNameListDebounce;
            //private Func<Task> QueryViewNameListDebounce => _queryViewNameListDebounce ??
            //    (_queryViewNameListDebounce = ((Func<CancellationToken, Task>)QueryViewNameList).Debounce(2000));

            //private async Task QueryViewNameList(CancellationToken cancellationToken = default) { await ... }

            CancellationTokenSource? cancelTokenSource = null;

            return () =>
            {
                cancelTokenSource?.Cancel();
                cancelTokenSource = new CancellationTokenSource();

                return Task.Delay(delayMilliseconds, cancelTokenSource.Token)
                    .ContinueWith(task =>
                    {
                        if (!(task.IsCanceled || task.IsFaulted)) // .NetCore: if (task.IsCompletedSuccessfully)
                            return func.Invoke(cancelTokenSource.Token);
                        else
                            return task;
                    }, TaskScheduler.Default).Unwrap(); // TaskScheduler.Default is the thread pool context.
            };
        }


        /// <summary>
        /// To start a task but don't want to wait for it to finish.
        /// </summary>
        /// <param name="errorHandler">error handler will be called when the task throws an exception</param>
        /// <remarks>
        /// <para>e.g. SendEmailAsync().FireAndForget(ex => Console.WriteLine(ex.Message));</para>
        /// <para>https://steven-giesel.com/blogPost/d38e70b4-6f36-41ff-8011-b0b0d1f54f6e</para>
        /// </remarks>
        public static void FireAndForget(this Task task, Action<Exception>? errorHandler = null)
        {
            task.ContinueWith(t =>
            {
                if (t.IsFaulted && errorHandler != null)
                    errorHandler(t.Exception);
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        /// <summary>
        /// To use a fallback value when a task fails.
        /// </summary>
        /// <remarks>
        /// <para>e.g. var result = await GetResultAsync().Fallback("fallback");</para>
        /// <para>https://steven-giesel.com/blogPost/d38e70b4-6f36-41ff-8011-b0b0d1f54f6e</para>
        /// </remarks>
        public static async Task<TResult> Fallback<TResult>(this Task<TResult> task, TResult fallbackValue)
        {
            try
            {
                return await task.ConfigureAwait(false);
            }
            catch
            {
                return fallbackValue;
            }
        }

        /// <summary>
        /// Executes a callback function when a Task encounters an exception.
        /// </summary>
        /// <remarks>
        /// <para>e.g. await GetResultAsync().OnFailure(ex => Console.WriteLine(ex.Message));</para>
        /// <para>https://steven-giesel.com/blogPost/d38e70b4-6f36-41ff-8011-b0b0d1f54f6e</para>
        /// </remarks>
        public static async Task OnFailure(this Task task, Action<Exception> errorHandler)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                errorHandler(ex);
            }
        }

        /// <summary>
        /// To set a timeout for a task. If the task takes longer than the timeout the task will be cancelled.
        /// </summary>
        /// <remarks>
        /// <para>e.g. await GetResultAsync().WithTimeout(TimeSpan.FromSeconds(1));</para>
        /// <para>https://steven-giesel.com/blogPost/d38e70b4-6f36-41ff-8011-b0b0d1f54f6e</para>
        /// </remarks>
        public static async Task WithTimeout(this Task task, TimeSpan timeout)
        {
            var delayTask = Task.Delay(timeout);
            var completedTask = await Task.WhenAny(task, delayTask).ConfigureAwait(false);
            if (completedTask == delayTask)
                throw new TimeoutException();

            await task;
        }

        /// <summary>
        ///  To retry the task until it succeeds or the maximum number of retries is reached.
        /// </summary>
        /// <param name="maxRetries">maximum number of retries</param>
        /// <param name="delay">a delay between retries</param>
        /// <remarks>
        /// <para>e.g. var result = await (() => GetResultAsync()).Retry(3, TimeSpan.FromSeconds(1));</para>
        /// <para>https://steven-giesel.com/blogPost/d38e70b4-6f36-41ff-8011-b0b0d1f54f6e</para>
        /// </remarks>
        public static async Task<TResult?> Retry<TResult>(this Func<Task<TResult>> taskFactory, int maxRetries, TimeSpan delay)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    return await taskFactory().ConfigureAwait(false);
                }
                catch
                {
                    if (i == maxRetries - 1)
                        throw;
                    await Task.Delay(delay).ConfigureAwait(false);
                }
            }

            return default; // Should not be reached
        }

    }
}
