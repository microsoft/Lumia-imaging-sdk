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

using Lumia.Imaging.Adjustments;
using System;
using Windows.Foundation;
using Windows.UI;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lumia.Imaging;
using Lumia.Imaging.Compositing;
using Windows.Storage.Streams;

namespace Lumia.Imaging.Extras.ImageSources
{
	/// <summary>
	/// A procedural image source that repeats the tile image to desired dimensions. 
	/// </summary>
	public class RepeatedTileImageSource : IImageProvider, IDisposable
	{
		Size m_requestedSize;
		Size m_tileSize;
		Size m_relativeTileSize;
		IImageProvider m_tileSource;
		
		IImageProvider m_imageProvider;
		List<IDisposable> m_disposables;
		
		/// <summary>
		/// Constructs an image source of a given size by repeating a given tile.
		/// </summary>
		/// <param name="requestedSize">The requested dimensions of a source.</param>
		/// <param name="tileSource">The IImageProvider that will be repeated.</param>
		/// <param name="tileSize">The size of the provided tile.</param>
		public RepeatedTileImageSource(Size requestedSize, IImageProvider tileSource, Size tileSize)
		{
			m_requestedSize = requestedSize;
			m_tileSize = tileSize;
			m_tileSource = tileSource;

			m_disposables = new List<IDisposable>();

			CreateImageProvider();
		}

		/// <summary>
		/// Constructs an image source of a given size by repeating a given tile.
		/// </summary>
		/// <param name="requestedSize">The requested dimensions of a source.</param>
		/// <param name="tileSource">The IImageProvider that will be repeated.</param>
		/// <remarks>This is a convinience method that will look up the tileSource size before calling the public constructor. A rendering operation may be needed to compute the size, depending on the source. This may lead to reduced performance.</remarks>
		public static async Task<RepeatedTileImageSource> CreateFromTileSource(Size requestedSize, IImageProvider tileSource)
		{
			var tileSize = (await tileSource.GetInfoAsync()).ImageSize;

			return new RepeatedTileImageSource(requestedSize, tileSource, tileSize);
		}

		~RepeatedTileImageSource()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			var disposableProvider = m_imageProvider as IDisposable;

			if (disposableProvider != null)
			{
				disposableProvider.Dispose();
				disposableProvider = null;
			}

			if (m_disposables != null)
			{
				foreach (var disposable in m_disposables)
				{
					if (disposable != null)
					{
						disposable.Dispose();
					}
				}

				m_disposables = null;
			}

			m_tileSource = null;
		}

		private void CreateImageProvider()
		{
			if ((int)Math.Min(m_requestedSize.Width, m_requestedSize.Height) <= 0)
			{
				throw new ArgumentOutOfRangeException("renderSize");
			}

			if ((int)Math.Min(m_tileSize.Width, m_tileSize.Height) <= 0)
			{
				throw new ArgumentOutOfRangeException("renderSize");
			}

			int outBgWidth = (int)m_requestedSize.Width;
			int outBgHeight = (int)m_requestedSize.Height;
			int tileWidth = (int)((Size)m_tileSize).Width;
			int tileHeight = (int)((Size)m_tileSize).Height;

			m_relativeTileSize = new Size(m_tileSize.Width / m_requestedSize.Width, m_tileSize.Height / m_requestedSize.Height);

			var backgroundCanvas = new ColorImageSource(new Size(outBgWidth, outBgHeight), Color.FromArgb(255, 0, 0, 0));
			m_disposables.Add(backgroundCanvas);

			m_imageProvider = backgroundCanvas;

			BlendTilesToFillCanvas(outBgWidth, outBgHeight, tileWidth, tileHeight);
		}

		private void BlendTilesToFillCanvas(int outBgWidth, int outBgHeight, int tileWidth, int tileHeight)
		{
			int currentBgWidth = 0;
			int currentBgHeight = 0;
			Point blendPosition = new Point(0, 0);

			while (currentBgHeight < outBgHeight)
			{
				while (currentBgWidth < outBgWidth)
				{
					BlendTileToCanvas(blendPosition);

					currentBgWidth += tileWidth;
					blendPosition.X = (double)currentBgWidth / (double)outBgWidth;
				}

				currentBgHeight += tileHeight;
				blendPosition.Y = (double)currentBgHeight / (double)outBgHeight;
				blendPosition.X = 0.0;
				currentBgWidth = 0;
			}
		}

		private void BlendTileToCanvas(Point blendPosition)
		{
			var blendEffect = new BlendEffect();
			m_disposables.Add(blendEffect);

			blendEffect.Source = m_imageProvider;
			blendEffect.ForegroundSource = m_tileSource;
			blendEffect.TargetArea = new Rect(blendPosition, m_relativeTileSize);
			blendEffect.TargetOutputOption = OutputOption.Stretch;

			m_imageProvider = blendEffect;
		}

		public IAsyncOperation<Bitmap> GetBitmapAsync(Bitmap bitmap, OutputOption outputOption)
		{
			return m_imageProvider.GetBitmapAsync(bitmap, outputOption);
		}

		public IAsyncOperation<ImageProviderInfo> GetInfoAsync()
		{
			return m_imageProvider.GetInfoAsync();
		}

		public bool Lock(RenderRequest renderRequest)
		{
			return m_imageProvider.Lock(renderRequest);
		}
		
		public IAsyncAction PreloadAsync()
		{
			return m_imageProvider.PreloadAsync();
		}
	}
}