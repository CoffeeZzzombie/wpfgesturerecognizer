﻿using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Org.Interactivity.Recognizer
{
    public sealed class GestureRecognizer : TriggerBase<FrameworkElement>
    {
        //This magic number corresponds to observations of what makes a tap versus a swipe or flick.
        private const int TapThreshold = 40;

        public static readonly DependencyProperty AutoManipulationEnabledProperty = DependencyProperty.Register(
            "AutoManipulationEnabled", typeof(bool), typeof(GestureRecognizer), new PropertyMetadata(true, HandleAutoManipulationEnabled));

        public static readonly DependencyProperty TriggerOnGestureProperty = DependencyProperty.Register(
            "TriggerOnGesture", typeof(Gesture), typeof(GestureRecognizer), new PropertyMetadata(Gesture.All));

        public bool AutoManipulationEnabled
        {
            get { return (bool)GetValue(AutoManipulationEnabledProperty); }
            set { SetValue(AutoManipulationEnabledProperty, value); }
        }

        public Gesture TriggerOnGesture
        {
            get { return (Gesture)GetValue(TriggerOnGestureProperty); }
            set { SetValue(TriggerOnGestureProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            if (AutoManipulationEnabled)
            {
                AssociatedObject.IsManipulationEnabled = AutoManipulationEnabled;
            }

            AssociatedObject.Loaded += (sender, args) =>
            {
                AssociatedObject.ManipulationStarting += HandleManipulationStarting;
                AssociatedObject.ManipulationCompleted += HandleManipulationCompleted;
            };
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.ManipulationStarting -= HandleManipulationStarting;
            AssociatedObject.ManipulationCompleted -= HandleManipulationCompleted;
        }

        private static void HandleAutoManipulationEnabled(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var instance = d as GestureRecognizer;
            if (instance != null && instance.AssociatedObject != null)
            {
                instance.AssociatedObject.IsManipulationEnabled = instance.AutoManipulationEnabled;
            }
        }

        private void HandleManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = AssociatedObject;
            e.Mode = ManipulationModes.All;
        }

        private void HandleManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            var gesture = ToSwipeGesture(e.TotalManipulation.Translation);
            if (TriggerOnGesture == Gesture.All || TriggerOnGesture == gesture)
            {
                InvokeActions(gesture);
            }
        }

        private static Gesture ToSwipeGesture(Vector translation)
        {
            var deltaX = translation.X;
            var deltaY = translation.Y;
            var distX = Math.Abs(deltaX);
            var distY = Math.Abs(deltaY);
            if (distX <= TapThreshold && distY <= TapThreshold)
            {
                return Gesture.Tap;
            }
            else if (distY >= distX) // bias towards vertical swipe over horizontal if distances are equal
            {
                return deltaY > 0 ? Gesture.SwipeDown : Gesture.SwipeUp;
            }
            else
            {
                return deltaX > 0 ? Gesture.SwipeRight : Gesture.SwipeLeft;
            }
        }
    }

    public enum Gesture : byte
    {
        All = 0,
        SwipeUp,
        SwipeDown,
        SwipeLeft,
        SwipeRight,
        Tap
    }
}