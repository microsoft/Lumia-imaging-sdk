//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************
using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Lumia.Imaging.VideoEffectSample.Utilities
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
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            if (!dispatcher.HasThreadAccess)
            {
                await dispatcher.RunAsync(priority, () => action()).AsTask(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                action();
            }
        }

        public static async Task RunOnDispatcherThreadAsync(Func<Task> action, CancellationToken cancellationToken = default(CancellationToken))
        {

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
        }
    }

}
