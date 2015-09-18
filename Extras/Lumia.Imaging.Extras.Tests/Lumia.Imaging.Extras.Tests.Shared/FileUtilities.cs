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

using System.Threading;
#if WINDOWS_PHONE
using System.Windows.Media.Imaging;
using Microsoft.Xna.Framework.Media;
#else
using Windows.UI.Xaml.Media.Imaging;
#endif
using Lumia.InteropServices.WindowsRuntime;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.ApplicationModel;
using System.Collections.Generic;

namespace Lumia.Imaging.Extras.Tests
{
    public class FileUtilities
    {
        public static async Task SaveToPicturesLibraryAsync(IBuffer bufImage, string fileName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var folder = await KnownFolders.PicturesLibrary.CreateFolderAsync("ImageVerificationExtras", CreationCollisionOption.OpenIfExists).AsTask(cancellationToken).ConfigureAwait(false);
            var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting).AsTask(cancellationToken).ConfigureAwait(false);
            using (var stream = await file.OpenStreamForWriteAsync().ConfigureAwait(false))
            {
                var data = bufImage.ToArray();
                await stream.WriteAsync(data, 0, data.Length, cancellationToken).ConfigureAwait(false);
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
