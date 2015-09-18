/*
* Copyright (c) 2014 Microsoft Mobile
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/
using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Lumia.Imaging.EditShowcase.Utilities
{
    public static class TaskUtilities
    {
        public static void RunOnDispatcherThreadSync(Action func, CancellationToken cancellationToken = default(CancellationToken), CoreDispatcher dispatcher = null)
        {
            RunOnDispatcherThreadSync(() =>
            {
                func();
                return true;
            }, cancellationToken, dispatcher);
        }

        public static T RunOnDispatcherThreadSync<T>(Func<T> func, CancellationToken cancellationToken = default(CancellationToken), CoreDispatcher dispatcher = null)
        {
            if (dispatcher == null)
            {
                dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            }

            if (dispatcher.HasThreadAccess)
            {
                return func();
            }

            ExceptionDispatchInfo exceptionDispatchInfo = null;

            T result = default(T);

            dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    try
                    {
                        result = func();
                    }
                    catch (Exception ex)
                    {
                        exceptionDispatchInfo = ExceptionDispatchInfo.Capture(ex);
                    }

                }).AsTask().Wait();

            if (exceptionDispatchInfo != null)
            {
                exceptionDispatchInfo.Throw();
            }

            return result;
        }

        public static async Task RunOnDispatcherThreadAsync(Action action, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal, CancellationToken cancellationToken = default(CancellationToken))
        {
#if WINDOWS_PHONE
            await Task.Run( () =>
            {
                Exception exception = null;
                var doneEvent = new ManualResetEvent(false);

                Deployment.Current.Dispatcher.BeginInvoke(async () =>
                {
                    try
                    {
                        action();
                    }
                    catch (Exception dispatchedException)
                    {
                        exception = dispatchedException;
                    }
                    finally
                    {
                        doneEvent.Set();
                    }
                });

                doneEvent.WaitOne();

                if (exception != null)
                {
                    throw exception;
                }
            });

#else
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            if (!dispatcher.HasThreadAccess)
            {
                await dispatcher.RunAsync(priority, () => action()).AsTask(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                action();
            }
#endif
        }

        public static async Task RunOnDispatcherThreadAsync(Func<Task> action, CancellationToken cancellationToken = default(CancellationToken))
        {
#if WINDOWS_PHONE
            await Task.Run( () =>
            {
                Exception exception = null;
                var doneEvent = new ManualResetEvent(false);

                Deployment.Current.Dispatcher.BeginInvoke(async () =>
                {
                    try
                    {
                        await action();
                    }
                    catch (Exception dispatchedException)
                    {
                        exception = dispatchedException;
                    }
                    finally
                    {
                        doneEvent.Set();
                    }
                });

                doneEvent.WaitOne();

                if (exception != null)
                {
                    throw exception;
                }
            });
#else
            Task temp = null;
            var tcs = new TaskCompletionSource<bool>();

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                temp = action();

                temp.ContinueWith(_ =>
                {
                    tcs.TrySetResult(true);

                }, TaskContinuationOptions.OnlyOnRanToCompletion);

                temp.ContinueWith(task =>
                {
                    tcs.TrySetCanceled();

                }, TaskContinuationOptions.OnlyOnCanceled);

                temp.ContinueWith(task =>
                {
                    tcs.TrySetException(task.Exception);

                }, TaskContinuationOptions.OnlyOnFaulted);

            }).AsTask(cancellationToken).ConfigureAwait(false);

            try
            {
                await tcs.Task.ConfigureAwait(false);
            }
            catch (AggregateException aggregateException)
            {
                throw aggregateException.InnerException;
            }
#endif
        }
    }

}
