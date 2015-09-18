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

using Windows.Foundation;

namespace Lumia.Imaging.Extras.Effects.DepthOfField
{
    /// <summary>
    /// Defines the focus area within the image that will be blurred.
    /// Defined by two points. The distance between the points defines the width of the band.
    /// The focus band within the band is then perpendicular to the line defines by the BandEdge1 and BandEdge2.
    /// </summary>
    public class FocusBand
    {
        public Point Edge1 { get; private set; }
        public Point Edge2 { get; private set; }

        /// <summary>
        /// Creates and initializes a new FocusBand given the two points.
        /// The distance between the points defines the width of the band.
        /// The focus band within the band is then perpendicular to the line defines by the BandEdge1 and BandEdge2.
        /// </summary>
        /// <param name="edge1">The first point. Expressed in the unit coordinate space of the image area,
        /// i.e., the top left corner of the image is at (0.0), and the bottom right corner is at (1, 1).param>
        /// <param name="edge2">The second point. Expressed in the unit coordinate space of the image area,
        /// i.e., the top left corner of the image is at (0.0), and the bottom right corner is at (1, 1).param>
        public FocusBand(Point edge1, Point edge2)
        {
            Edge1 = edge1;
            Edge2 = edge2;
        }
    }
}