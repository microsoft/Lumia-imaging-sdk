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
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace Lumia.Imaging.VideoEffectSample.AttachedProperties
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
