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

using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lumia.Imaging.Extras.Tests
{
    [TestClass]
    public class MaybeTaskTest
    {
        [TestMethod]
        public void MaybeTaskRepresentsResult()
        {
            object o = new object();
            var mt = new MaybeTask<object>(o);
            Assert.IsTrue(mt.WasSynchronous);
            Assert.IsTrue(mt.IsSynchronous);
            Assert.IsFalse(mt.IsEmpty);
            Assert.IsNull(mt.Task);
            Assert.AreEqual(o, mt.Result);
        }

        [TestMethod]
        public void MaybeTaskRepresentsCompletingTask()
        {
            object o = new object();
            var tcs = new TaskCompletionSource<object>();
            var mt = new MaybeTask<object>(tcs.Task);
            Assert.IsFalse(mt.WasSynchronous);
            Assert.IsFalse(mt.IsSynchronous);
            Assert.IsFalse(mt.IsEmpty);
            Assert.IsNotNull(mt.Task);
            tcs.SetResult(o);
            Assert.IsFalse(mt.WasSynchronous);
            Assert.IsTrue(mt.IsSynchronous);
            Assert.AreEqual(o, mt.Result);
        }

        [TestMethod]
        public void MaybeTaskRepresentsAlreadyCompletedTask()
        {
            object o = new object();
            var mt = new MaybeTask<object>(Task.FromResult(o));
            Assert.IsFalse(mt.WasSynchronous);
            Assert.IsTrue(mt.IsSynchronous); // result is available now
            Assert.IsFalse(mt.IsEmpty);
            Assert.IsNotNull(mt.Task);
            Assert.AreEqual(o, mt.Result);
        }

        [TestMethod]
        public void MaybeTaskRepresentsCancelingTask()
        {
            object o = new object();
            var tcs = new TaskCompletionSource<object>();
            var mt = new MaybeTask<object>(tcs.Task);
            Assert.IsFalse(mt.WasSynchronous);
            Assert.IsFalse(mt.IsSynchronous);
            Assert.IsFalse(mt.IsEmpty);
            Assert.IsNotNull(mt.Task);
            tcs.SetCanceled();
            Assert.IsFalse(mt.WasSynchronous);
            Assert.IsTrue(mt.IsSynchronous);
            Assert.ThrowsException<AggregateException>(() => mt.Result);
        }

        [TestMethod]
        public void MaybeTaskRepresentsAlreadyCanceledTask()
        {
            object o = new object();
            var tcs = new TaskCompletionSource<object>();
            tcs.SetCanceled();
            var mt = new MaybeTask<object>(tcs.Task);
            Assert.IsFalse(mt.WasSynchronous);
            Assert.IsTrue(mt.IsSynchronous);
            Assert.IsFalse(mt.IsEmpty);
            Assert.IsNotNull(mt.Task);
            Assert.ThrowsException<AggregateException>(() => mt.Result);
        }

        [TestMethod]
        public void MaybeTaskRepresentsFailingTask()
        {
            object o = new object();
            var tcs = new TaskCompletionSource<object>();
            var mt = new MaybeTask<object>(tcs.Task);
            Assert.IsFalse(mt.WasSynchronous);
            Assert.IsFalse(mt.IsSynchronous);
            Assert.IsFalse(mt.IsEmpty);
            Assert.IsNotNull(mt.Task);
            tcs.SetException(new Exception());
            Assert.IsFalse(mt.WasSynchronous);
            Assert.IsTrue(mt.IsSynchronous);
            Assert.ThrowsException<AggregateException>(() => mt.Result);
        }

        [TestMethod]
        public void MaybeTaskRepresentsAlreadyFailedTask()
        {
            object o = new object();
            var tcs = new TaskCompletionSource<object>();
            tcs.SetException(new Exception());
            var mt = new MaybeTask<object>(tcs.Task);
            Assert.IsFalse(mt.WasSynchronous);
            Assert.IsTrue(mt.IsSynchronous);
            Assert.IsFalse(mt.IsEmpty);
            Assert.IsNotNull(mt.Task);
            Assert.ThrowsException<AggregateException>(() => mt.Result);
        }
    }
}
