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

using System.Threading.Tasks;

namespace Lumia.Imaging.Extras
{
    /// <summary>
    /// Wrapper value type around either a result of type T or a Task&lt;T&gt;. Helps to avoid managed heap allocations.
    /// </summary>
    /// <typeparam name="T">The type of the result. Must be a class type (in particular because null means "no result").</typeparam>
    public struct MaybeTask<T>
        where T : class
    {
        private readonly T m_result;

        /// <summary>
        /// Construct a new MaybeTask&lt;T&gt; that wraps the specified synchronous result.
        /// </summary>
        /// <param name="result">The result to wrap.</param>
        public MaybeTask(T result)
        {
            m_result = result;
            Task = null;
        }

        /// <summary>
        /// Construct a new MaybeTask&lt;T&gt; that wraps the specified Task&lt;T&gt;.
        /// </summary>
        /// <param name="task">The task to wrap.</param>
        public MaybeTask(Task<T> task)
        {
            m_result = null;
            Task = task;
        }
        
        /// <summary>
        /// The result, if available. This should only be used when IsSynchronous returns true.
        /// </summary>
        public T Result
        {
            get { return Task != null ? Task.Result : m_result; }
        }

        /// <summary>
        /// The wrapped Task&lt;T&gt;, if one is wrapped. Note that there may be a synchronous result even if this is non-null, in case the Task has already completed.
        /// </summary>
        public readonly Task<T> Task;

        /// <summary>
        /// Returns true if the result was synchronous to begin with or if a wrapped Task&lt;T&gt; has completed.
        /// </summary>
        public bool IsSynchronous
        {
            get { return m_result != null || (Task != null && (Task.IsCompleted || Task.IsCanceled || Task.IsFaulted)); }
        }

        /// <summary>
        /// Returns true if the result was synchronous to begin with.
        /// </summary>
        public bool WasSynchronous
        {
            get { return m_result != null; }
        }

        /// <summary>
        /// Returns true if there is neither a result or a Task&lt;T&gt;.
        /// </summary>
        public bool IsEmpty
        {
            get { return m_result == null && Task == null; }
        }

        /// <summary>
        /// Converts the MaybeTask&lt;T&gt; to a Task&lt;T&gt;. This method should only be called when a Task&lt;T&gt; is absolutely needed. To avoid further allocations, the MaybeTask&lt;T&gt; should no longer be used.
        /// </summary>
        /// <returns></returns>
        public Task<T> AsTask()
        {
            if (Task != null)
            {
                return Task;
            }

            return System.Threading.Tasks.Task.FromResult(m_result);
        }
    }
}
