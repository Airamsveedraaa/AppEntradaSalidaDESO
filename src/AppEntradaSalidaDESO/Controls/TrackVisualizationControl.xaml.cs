using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using AppEntradaSalidaDESO.Models;

namespace AppEntradaSalidaDESO.Controls
{
    public partial class TrackVisualizationControl : UserControl
    {
        public static readonly DependencyProperty StepsProperty =
            DependencyProperty.Register("Steps", typeof(List<AlgorithmStep>), typeof(TrackVisualizationControl), new PropertyMetadata(null, OnStepsChanged));

        public static readonly DependencyProperty MaxTrackProperty =
            DependencyProperty.Register("MaxTrack", typeof(int), typeof(TrackVisualizationControl), new PropertyMetadata(199, OnParamsChanged));

        public static readonly DependencyProperty MinTrackProperty =
            DependencyProperty.Register("MinTrack", typeof(int), typeof(TrackVisualizationControl), new PropertyMetadata(0, OnParamsChanged));

        public List<AlgorithmStep> Steps
        {
            get { return (List<AlgorithmStep>)GetValue(StepsProperty); }
            set { SetValue(StepsProperty, value); }
        }

        public int MaxTrack
        {
            get { return (int)GetValue(MaxTrackProperty); }
            set { SetValue(MaxTrackProperty, value); }
        }

        public int MinTrack
        {
            get { return (int)GetValue(MinTrackProperty); }
            set { SetValue(MinTrackProperty, value); }
        }

        public TrackVisualizationControl()
        {
            InitializeComponent();
        }

        private static void OnStepsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TrackVisualizationControl control)
            {
                control.DrawGraph();
            }
        }

        private static void OnParamsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TrackVisualizationControl control)
            {
                control.DrawGraph();
            }
        }

        private void GraphCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawGraph();
        }

        private void DrawGraph()
        {
            GraphCanvas.Children.Clear();
            YAxisCanvas.Children.Clear();
            XAxisCanvas.Children.Clear();

            if (Steps == null || Steps.Count == 0 || GraphCanvas.ActualWidth == 0 || GraphCanvas.ActualHeight == 0)
                return;

            double width = GraphCanvas.ActualWidth;
            double height = GraphCanvas.ActualHeight - 20; // Padding bottom
            double paddingTop = 10;

            int trackRange = Math.Max(1, MaxTrack - MinTrack);
            int stepCount = Steps.Count; 
            // StepCount includes step 0 (initial) to N. Visualization usually starts at index 0 (initial state) -> X=0
            
            // Steps usually come as detailed steps. Step 0 is implicitly the initial state if not provided, 
            // but our parser adds Initial State as Step 0 (maybe?). 
            // In ViewModel logic: "StepsTable.Add(new StepRow(0...))"
            // But 'Steps' property is result.Steps. 
            // Let's assume result.Steps contains all movements. We might need to inject the Initial Position manually implies index 0?
            // Actually, result.Steps contains step 1..N. The initial position is result.InitialPosition.
            // But wait, in visualization we want P0(Initial) -> P1 -> P2.
            // If Steps contains "From -> To", then for Step 1: From=Initial, To=S1.
            // So we can plot points based on 'From' of first step + 'To' of all steps.
            
            // Build points list
            var points = new List<PointInfo>();
            
            // First point (Initial)
            if (Steps.Count > 0)
            {
                 points.Add(new PointInfo(0, Steps[0].From));
            }

            for (int i = 0; i < Steps.Count; i++)
            {
                points.Add(new PointInfo(i + 1, Steps[i].To, Steps[i].Distance));
            }

            if (points.Count < 2) return;

            double xStep = width / (points.Count - 1);

            // Draw Y Axis Labels (Min, Max)
            DrawText(YAxisCanvas, MaxTrack.ToString(), 0, 0, true);
            DrawText(YAxisCanvas, MinTrack.ToString(), 0, height + paddingTop - 10, true);

            // Draw Guidelines
            DrawGuideline(0 + paddingTop); // Max
            DrawGuideline(height + paddingTop); // Min

            // Draw Lines & Points
            for (int i = 0; i < points.Count - 1; i++)
            {
                var p1 = points[i];
                var p2 = points[i + 1];

                double x1 = p1.Index * xStep;
                double y1 = MapToY(p1.Track, height, paddingTop);
                
                double x2 = p2.Index * xStep;
                double y2 = MapToY(p2.Track, height, paddingTop);

                // Detect Jump (Circular)
                // Logic matches reference: distance > range/2 AND (touching extremes)
                bool isJump = IsCircularJump(p1.Track, p2.Track, p2.Distance, trackRange);

                var line = new Line
                {
                    X1 = x1,
                    Y1 = y1,
                    X2 = x2,
                    Y2 = y2,
                    Stroke = isJump ? Brushes.Red : Brushes.DodgerBlue,
                    StrokeThickness = 2
                };

                if (isJump)
                {
                    line.StrokeDashArray = new DoubleCollection { 4, 2 };
                }

                GraphCanvas.Children.Add(line);

                // Draw Point (Start of segment)
                DrawPoint(x1, y1, i == 0 ? Brushes.Green : (isJump ? Brushes.Red : Brushes.DodgerBlue), i == 0 ? 8 : 6);
                
                // Draw Step Label on X Axis
                if (points.Count < 20 || i % (points.Count / 10) == 0) // Optimize labels
                {
                   DrawText(XAxisCanvas, i.ToString(), x1, 0, false);
                }
            }

            // Draw Last Point
            var lastP = points.Last();
            DrawPoint(lastP.Index * xStep, MapToY(lastP.Track, height, paddingTop), Brushes.Orange, 8);
            DrawText(XAxisCanvas, lastP.Index.ToString(), lastP.Index * xStep, 0, false);
        }

        private void DrawPoint(double x, double y, Brush color, double size)
        {
            var ellipse = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = color,
                Stroke = Brushes.White,
                StrokeThickness = 1
            };
            Canvas.SetLeft(ellipse, x - size / 2);
            Canvas.SetTop(ellipse, y - size / 2);
            GraphCanvas.Children.Add(ellipse);
        }

        private void DrawText(Canvas canvas, string text, double x, double y, bool rightAlign)
        {
            var tb = new TextBlock
            {
                Text = text,
                FontSize = 10,
                Foreground = Brushes.Gray
            };
            
            canvas.Children.Add(tb);
            tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            
            if (rightAlign)
                 Canvas.SetLeft(tb, 30 - tb.DesiredSize.Width - 2); 
            else
                 Canvas.SetLeft(tb, Math.Max(0, x - tb.DesiredSize.Width / 2));
                 
            Canvas.SetTop(tb, y);
        }

        private void DrawGuideline(double y)
        {
             var line = new Line
             {
                 X1 = 0,
                 Y1 = y,
                 X2 = GraphCanvas.ActualWidth,
                 Y2 = y,
                 Stroke = Brushes.LightGray,
                 StrokeThickness = 1,
                 StrokeDashArray = new DoubleCollection { 2, 2 }
             };
             GraphCanvas.Children.Add(line);
        }

        private double MapToY(int track, double height, double paddingTop)
        {
            // Y = 0 is top. We want MaxTrack at top. minTrack at bottom.
            // Normalized (0-1): (track - min) / (max - min)
            // Inverted Y: 1 - normalized
            
            int range = Math.Max(1, MaxTrack - MinTrack);
            double normalized = (double)(track - MinTrack) / range;
            
            // Limit to 0-1 range to avoid drawing outside
            normalized = Math.Max(0, Math.Min(1, normalized));

            return paddingTop + (height * (1 - normalized));
        }

        private bool IsCircularJump(int from, int to, int distance, int range)
        {
             // Heuristic: If distance is large (> 50% of disk) and involves extremes
             return distance > range * 0.5;
        }

        private class PointInfo
        {
            public int Index { get; }
            public int Track { get; }
            public int Distance { get; }

            public PointInfo(int index, int track, int distance = 0)
            {
                Index = index;
                Track = track;
                Distance = distance;
            }
        }
    }
}
