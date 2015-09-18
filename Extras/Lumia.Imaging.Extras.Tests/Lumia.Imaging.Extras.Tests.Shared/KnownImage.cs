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
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Storage;
using System.IO;
using System.Text.RegularExpressions;

namespace Lumia.Imaging.Extras.Tests
{
	public class KnownImage
	{
		public KnownImage(string path, Size size, ImageFormat imageFormat = ImageFormat.Jpeg)
		{
			Path = path;
			Size = size;
			ImageFormat = imageFormat;
		
			var regex = new Regex(@"(?<name>\w+)\.\w+?$", RegexOptions.IgnoreCase);
			var matches = regex.Matches(Path);
			Name = matches[0].Groups["name"].Value;
		}

		public Task<StorageFile> GetFileAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			return Package.Current.InstalledLocation.GetFileAsync(Path).AsTask(cancellationToken);
		}

		public async Task<StorageFileImageSource> GetImageSourceAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			return new StorageFileImageSource(await GetFileAsync(cancellationToken).ConfigureAwait(false));
		}
	 
		public async Task<StreamImageSource> GetStreamImageSourceAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			var storageFile = await GetFileAsync(cancellationToken).ConfigureAwait(false);
			var stream = await storageFile.OpenStreamForReadAsync().ConfigureAwait(false);                                        
			return new StreamImageSource(stream); 
		}
		
		public string Path { get; private set; }
		public Size Size { get; private set; }
		public ImageFormat ImageFormat { get; private set; }
		public string Name { get; private set; }
	}
}