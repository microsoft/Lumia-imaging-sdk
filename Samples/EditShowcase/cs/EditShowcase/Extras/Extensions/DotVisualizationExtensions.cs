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
using Lumia.Imaging.Compositing;
using Lumia.Imaging.Transforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Lumia.Imaging.Extras.Extensions
{
    public static class DotVisualizationExtensions
    {
        private const string graphFormatString = "digraph {0} {{ graph [fontname = \"helvetica\", fontsize=10]; node [fontname = \"helvetica\", fontsize=10]; edge [fontname = \"helvetica\", fontsize=10]; {1} }}";

        // Returns a string that can be used as input to Dot, part of graphviz.
        // Graphviz can be found here: http://www.graphviz.org/Download..php
        // To generate a processing graph in PDF-format, save the returned string to a file graph.dot, and do:
        // C:>dot -Tpdf graph.dot > graph.pdf
        public static string ToDotString(this IImageConsumer imageConsumer, string graphName = "graph")
        {
            var dotGen = new DotVisualizer(graphName, imageConsumer);
            return dotGen.DotString;
        }

        public static string ToDotString(this ImageAligner imageAligner, string graphName = "graph")
        {
            var dotGen = new DotVisualizer(graphName, imageAligner);
            return dotGen.DotString;
        }

        private class DotVisualizer
        {

            [Flags]
            private enum NodePart
            {
                None = 0x00,
                GeneralProperties = 0x01,
                ImageProviderProperties = 0x02,
                All = 0x03
            };

            private string m_dotString = "";
            private Dictionary<object, NodePart> m_visitedNodes = new Dictionary<object, NodePart>();

            internal DotVisualizer(string graphName, IImageConsumer imageConsumer)
            {
                Dot(imageConsumer);
                DotString = String.Format(graphFormatString, graphName, m_dotString);
            }
            internal DotVisualizer(string graphName, ImageAligner imageAligner)
            {
                Node(imageAligner);
                DotString = String.Format(graphFormatString, graphName, m_dotString);
            }

            internal string DotString { get; private set; }

            private void Dot(IImageConsumer imageConsumer)
            {
                if (imageConsumer == null)
                {
                    return;
                }

                else if (imageConsumer is LensBlurEffect)
                {
                    Dot(imageConsumer as LensBlurEffect);
                }
                else
                {
                    Node(imageConsumer);
                }
            }

            private void Dot(IImageProvider imageProvider)
            {
                if (imageProvider == null)
                {
                    return;
                }

                if (imageProvider is IImageConsumer)
                {
                    Dot(imageProvider as IImageConsumer);
                }
                else
                {
                    Node(imageProvider);
                }
            }

            #region LensBlurEffect
            private void Dot(LensBlurEffect lensBlurEffect)
            {
                Subgraph(lensBlurEffect);

                if (lensBlurEffect.Kernels != null)
                {
                    Dot(lensBlurEffect, lensBlurEffect.Kernels);
                }
            }

            private void Dot(LensBlurEffect lensBlurEffect, IReadOnlyList<ILensBlurKernel> kernels)
            {
                foreach (var kernel in kernels)
                {
                    if (kernel is LensBlurCustomKernel)
                    {
                        var customKernel = kernel as LensBlurCustomKernel;
                        Dot(customKernel.Shape);
                    }
                }
            }

            private void Subgraph(LensBlurEffect lensBlurEffect)
            {
                Node(lensBlurEffect, "parallelogram", NodePart.ImageProviderProperties);

                m_dotString += String.Format("subgraph cluster_{0} {{ rank = same; label=\"LensBlurEffect with Kernels\"; graph[style=dotted]", NodeId(lensBlurEffect));

                Node(lensBlurEffect, "parallelogram", NodePart.GeneralProperties);

                foreach (var kernel in lensBlurEffect.Kernels)
                {
                    Node(kernel, "box", NodePart.GeneralProperties);
                }

                m_dotString += " } ";

                foreach (var kernel in lensBlurEffect.Kernels)
                {
                    Node(kernel, "box", NodePart.ImageProviderProperties);
                }
            }
            #endregion // LensBlurEffect

            private void Node(object obj, string shape = "oval", NodePart propertyCategory = NodePart.All)
            {
                NodePart visitedPart = NodePart.None;

                if (m_visitedNodes.TryGetValue(obj, out visitedPart))
                {
                    if ((visitedPart & propertyCategory) != 0)
                    {
                        return;
                    }

                    m_visitedNodes.Remove(obj);
                }

                m_visitedNodes.Add(obj, visitedPart | propertyCategory);

                string propertiesString = "";

                var properties = GetProperties(obj);

                foreach (var property in properties)
                {
                    var type = GetPropertyType(property);

                    if ((propertyCategory & NodePart.GeneralProperties) != 0 && type.IsPrimitive || type.IsValueType)
                    {
                        propertiesString += String.Format("\\n{0}: {1}", property.Name, property.GetValue(obj));
                    }

                    var enumerableTypes = GetGenericIEnumerables(type);
                    foreach (var enumerableType in enumerableTypes)
                    {
                        if (enumerableType == typeof(IImageProvider))
                        {
                            var imageProviders = property.GetValue(obj) as IEnumerable<IImageProvider>;
                            var i = 0;
                            foreach (var imageProvider in imageProviders)
                            {
                                Edge(imageProvider, obj, String.Format("{0}[{1}]", property.Name, i++));
                                Dot(imageProvider);
                            }
                        }
                    }

                    if ((propertyCategory & NodePart.ImageProviderProperties) != 0 && property.GetValue(obj) is IImageProvider)
                    {
                        Edge(property.GetValue(obj), obj, property.Name);
                        Dot(property.GetValue(obj) as IImageProvider);
                    }
                }

                if ((propertyCategory & NodePart.GeneralProperties) != 0)
                {
                    m_dotString += String.Format("{0} [label=\"{1}{2}\", shape=\"{3}\"]; ", NodeId(obj), obj.GetType().Name, propertiesString, shape);
                }
            }

            public List<Type> GetGenericIEnumerables(TypeInfo typeInfo)
            {                
                return typeInfo.ImplementedInterfaces
                        .Where(t => IntrospectionExtensions.GetTypeInfo(t).IsGenericType == true && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        .Select(t => t.GenericTypeArguments[0]).ToList();
            }

#if WINDOWS_PHONE

            private IEnumerable<System.Reflection.PropertyInfo> GetProperties(object obj)
            {
                return obj.GetType().GetProperties();
            }

            private System.Type GetPropertyType(System.Reflection.PropertyInfo property)
            {
                return property.PropertyType;
            }

#else

            private IEnumerable<System.Reflection.PropertyInfo> GetProperties(object obj)
            {
                return System.Reflection.IntrospectionExtensions.GetTypeInfo(obj.GetType()).DeclaredProperties;
            }

            private System.Reflection.TypeInfo GetPropertyType(System.Reflection.PropertyInfo property)
            {
                return System.Reflection.IntrospectionExtensions.GetTypeInfo(property.PropertyType);
            }

#endif
            private void Edge(object srcObj, object sinkObj, string label = "")
            {
                if (srcObj == null || sinkObj == null)
                {
                    return;
                }

                m_dotString += String.Format("{0} -> {1} [label=\"{2}\"]; ", NodeId(srcObj), NodeId(sinkObj), label);
            }


            private static string NodeId(object obj)
            {
                return obj.GetType().Name + obj.GetHashCode();
            }
        }
    }
}
