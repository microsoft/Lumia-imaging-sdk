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

using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Lumia.Imaging.Extras.Layers
{
    /// <summary>
    /// Contextual information passed to a resolver delegate when converting a <see cref="LayerList" /> to an IImageProvider.
    /// </summary>
    public class LayerContext
    {
        internal class Invariants
        {
            internal Invariants(Layer backgroundLayer, MaybeTask<IImageProvider> backgroundImage, Size renderSizeHint)
            {
                BackgroundLayer = backgroundLayer;
                BackgroundImage = backgroundImage;
                HintedRenderSize = renderSizeHint;
            }

            internal readonly Layer BackgroundLayer;
            internal MaybeTask<IImageProvider> BackgroundImage;
            internal readonly Size HintedRenderSize;
        }

        // Single reference to data that is common for all layers.
        private readonly Invariants m_invariants;

        internal LayerContext(Invariants invariants, Layer previousLayer, Layer currentLayer, int currentLayerIndex)
        {
            m_invariants = invariants;
            PreviousLayer = previousLayer;
            CurrentLayer = currentLayer;
            CurrentLayerIndex = currentLayerIndex;
        }

        /// <summary>
        /// The background layer.
        /// </summary>
        public Layer BackgroundLayer 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get
            {
                return m_invariants.BackgroundLayer;
            } 
        }

        /// <summary>
        /// The image provider resolved from the background layer, as an asynchronous result.
        /// </summary>
        public MaybeTask<IImageProvider> BackgroundImage
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get
            {
                return m_invariants.BackgroundImage;
            }           
            [MethodImpl(MethodImplOptions.AggressiveInlining)] internal set
            {
                m_invariants.BackgroundImage = value;
            }
        }

        /// <summary>
        /// The hinted size of the rendered image, if one was specified in the LayerList.ToImageProvider call.
        /// </summary>
        public Size HintedRenderSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return m_invariants.HintedRenderSize;
            }
        }

        /// <summary>
        /// The previous layer.
        /// </summary>
        public Layer PreviousLayer { get; internal set; }

        /// <summary>
        /// The image provider resolved from the previous layer.
        /// </summary>
        public MaybeTask<IImageProvider> PreviousImage { get; internal set; }

        /// <summary>
        /// The current layer.
        /// </summary>
        public Layer CurrentLayer { get; internal set; }

        /// <summary>
        /// The index of the current layer in the LayerList that's being resolved.
        /// </summary>
        public int CurrentLayerIndex { get; internal set; }
    }
}
