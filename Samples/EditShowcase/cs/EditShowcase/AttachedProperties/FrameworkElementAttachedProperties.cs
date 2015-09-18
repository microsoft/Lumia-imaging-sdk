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
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace Lumia.Imaging.EditShowcase.AttachedProperties
{
    class FrameworkElementAttachedProperties
    {
        public static readonly DependencyProperty ObservedWidthProperty; 
        public static readonly DependencyProperty ObservedHeightProperty;
        public static readonly DependencyProperty ObserveProperty;
        public static readonly DependencyProperty LoadedCommandProperty;
        
        static FrameworkElementAttachedProperties()
        {
            ObserveProperty = DependencyProperty.RegisterAttached("Observe", typeof(bool), typeof(FrameworkElementAttachedProperties), new PropertyMetadata(null, new PropertyChangedCallback(OnObserveChanged)));
            ObservedWidthProperty = DependencyProperty.RegisterAttached("ObservedWidth", typeof(double), typeof(FrameworkElementAttachedProperties), new PropertyMetadata(null, null));
            ObservedHeightProperty = DependencyProperty.RegisterAttached("ObservedHeight", typeof(double), typeof(FrameworkElementAttachedProperties), new PropertyMetadata(null, null));
            LoadedCommandProperty = DependencyProperty.RegisterAttached("LoadedCommand", typeof(ICommand), typeof(FrameworkElementAttachedProperties), new PropertyMetadata(null, OnLoadedCommandChanged));
        }

        public static void SetLoadedCommand(FrameworkElement frameworkElement, ICommand value)
        {
            frameworkElement.SetValue(LoadedCommandProperty, value);
        }

        public static ICommand GetLoadedCommand(FrameworkElement frameworkElement)
        {
            return (ICommand)frameworkElement.GetValue(LoadedCommandProperty);
        }

        private static void OnLoadedCommandChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var frameworkElement = dependencyObject as FrameworkElement;
            if (frameworkElement == null)
                return;


            frameworkElement.Loaded += OnFrameworkElementLoaded;
        }

        private static void OnFrameworkElementLoaded(object sender, RoutedEventArgs e)
        {
            var frameworkElement = (FrameworkElement)sender;

            ICommand command = GetLoadedCommand(frameworkElement);

            command.Execute(frameworkElement);
        }

        public static bool GetObserve(FrameworkElement frameworkElement)
        {
            return (bool)frameworkElement.GetValue(ObserveProperty);
        }

        public static void SetObserve(FrameworkElement frameworkElement, bool observe)
        {
            frameworkElement.SetValue(ObserveProperty, observe);
        }

        public static double GetObservedWidth(FrameworkElement frameworkElement)
        {          
            return (double)frameworkElement.GetValue(ObservedWidthProperty);
        }

        public static void SetObservedWidth(FrameworkElement frameworkElement, double observedWidth)
        {         
            frameworkElement.SetValue(ObservedWidthProperty, observedWidth);
        }

        public static double GetObservedHeight(FrameworkElement frameworkElement)
        {
         
            return (double)frameworkElement.GetValue(ObservedHeightProperty);
        }

        public static void SetObservedHeight(FrameworkElement frameworkElement, double observedHeight)
        {         
            frameworkElement.SetValue(ObservedHeightProperty, observedHeight);
        }

        private static void OnObserveChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var frameworkElement = (FrameworkElement)dependencyObject;

            if ((bool)e.NewValue)
            {
                frameworkElement.SizeChanged += OnFrameworkElementSizeChanged;
                UpdateObservedSizesForFrameworkElement(frameworkElement);
            }
            else
            {
                frameworkElement.SizeChanged -= OnFrameworkElementSizeChanged;
            }
        }

        private static void OnFrameworkElementSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateObservedSizesForFrameworkElement((FrameworkElement)sender);
        }

        private static void UpdateObservedSizesForFrameworkElement(FrameworkElement frameworkElement)
        {
            SetObservedWidth(frameworkElement, frameworkElement.ActualWidth);
            SetObservedHeight(frameworkElement, frameworkElement.ActualHeight);
        }
    }
}
