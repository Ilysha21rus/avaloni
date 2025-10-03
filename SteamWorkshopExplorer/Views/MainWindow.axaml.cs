using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Controls.Shapes;
using SteamWorkshopExplorer.ViewModels;
using System;
using System.Collections.Generic;

namespace SteamWorkshopExplorer.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel ViewModel => DataContext as MainViewModel;
        
        private Ellipse? _selectedEllipse;
        private Point _startMousePos;
        private double _startWidth, _startHeight;
        private bool _isResizing;


        private List<Ellipse> _bezierPoints = new();
        private Polyline? _currentBezierLine = null;

        public MainWindow()
        {
            InitializeComponent();
        }


        private void OnEllipseMode(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (ViewModel != null) ViewModel.IsBezierMode = false;
        }

        private void OnBezierMode(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (ViewModel != null) ViewModel.IsBezierMode = true;
        }

        private void OnSaveClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ViewModel?.SaveToJson();
        }

        private void OnLoadClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ViewModel?.LoadFromJson();
            RedrawCanvas();
        }

        private void OnClearClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ViewModel?.Clear();
            DrawingCanvas.Children.Clear();
            _bezierPoints.Clear();
            _currentBezierLine = null;
        }
        
        private void DrawingCanvas_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (ViewModel == null || sender is not Canvas canvas) return;

            var point = e.GetPosition(canvas);

            if (!ViewModel.IsBezierMode)
            {
                foreach (var child in canvas.Children)
                {
                    if (child is Ellipse ellipse)
                    {
                        var left = Canvas.GetLeft(ellipse);
                        var top = Canvas.GetTop(ellipse);
                        var right = left + ellipse.Width;
                        var bottom = top + ellipse.Height;
                        
                        if (point.X >= right - 10 && point.X <= right &&
                            point.Y >= bottom - 10 && point.Y <= bottom)
                        {
                            _selectedEllipse = ellipse;
                            _isResizing = true;
                            _startMousePos = point;
                            _startWidth = ellipse.Width;
                            _startHeight = ellipse.Height;
                            return;
                        }
                        
                        if (point.X >= left && point.X <= right &&
                            point.Y >= top && point.Y <= bottom)
                        {
                            _selectedEllipse = ellipse;
                            _isResizing = false;
                            _startMousePos = point;
                            _startWidth = ellipse.Width;
                            _startHeight = ellipse.Height;
                            return;
                        }
                    }
                }

                var ellipseNew = new Ellipse
                {
                    Width = 100,
                    Height = 60,
                    Fill = Brushes.DeepPink,
                    Stroke = Brushes.OrangeRed,
                    StrokeThickness = 2
                };
                Canvas.SetLeft(ellipseNew, point.X - 50);
                Canvas.SetTop(ellipseNew, point.Y - 30);
                canvas.Children.Add(ellipseNew);

                ViewModel.AddEllipse(point.X - 50, point.Y - 30);
            }
            else
            {
                var ellipse = new Ellipse { Width = 10, Height = 10, Fill = Brushes.Blue };
                Canvas.SetLeft(ellipse, point.X - 5);
                Canvas.SetTop(ellipse, point.Y - 5);
                canvas.Children.Add(ellipse);
                _bezierPoints.Add(ellipse);

                if (_currentBezierLine == null)
                {
                    _currentBezierLine = new Polyline
                    {
                        Stroke = Brushes.Red,
                        StrokeThickness = 2
                    };
                    canvas.Children.Add(_currentBezierLine);
                }

                UpdateBezierLine();
            }
        }

        private void DrawingCanvas_PointerMoved(object? sender, PointerEventArgs e)
        {
            if (_selectedEllipse != null && sender is Canvas canvas)
            {
                var point = e.GetPosition(canvas);
                var dx = point.X - _startMousePos.X;
                var dy = point.Y - _startMousePos.Y;

                if (_isResizing)
                {
                    _selectedEllipse.Width = Math.Max(10, _startWidth + dx);
                    _selectedEllipse.Height = Math.Max(10, _startHeight + dy);
                }
                else
                {
                    Canvas.SetLeft(_selectedEllipse, Canvas.GetLeft(_selectedEllipse) + dx);
                    Canvas.SetTop(_selectedEllipse, Canvas.GetTop(_selectedEllipse) + dy);
                    _startMousePos = point;
                }
            }
        }

        private void DrawingCanvas_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            _selectedEllipse = null;
        }

        private void UpdateBezierLine()
        {
            if (_currentBezierLine == null) return;

            var points = new Avalonia.Collections.AvaloniaList<Point>();
            foreach (var e in _bezierPoints)
            {
                points.Add(new Point(Canvas.GetLeft(e) + e.Width / 2,
                                     Canvas.GetTop(e) + e.Height / 2));
            }
            _currentBezierLine.Points = points;
        }

        private void RedrawCanvas()
        {
            DrawingCanvas.Children.Clear();
            _bezierPoints.Clear();
            _currentBezierLine = null;

            if (ViewModel == null) return;

            foreach (var shape in ViewModel.Shapes)
            {
                if (shape.Type == "Ellipse")
                {
                    var ellipse = new Ellipse
                    {
                        Width = shape.Width,
                        Height = shape.Height,
                        Fill = Brushes.DeepPink,
                        Stroke = Brushes.OrangeRed,
                        StrokeThickness = 2
                    };
                    Canvas.SetLeft(ellipse, shape.X);
                    Canvas.SetTop(ellipse, shape.Y);
                    DrawingCanvas.Children.Add(ellipse);
                }
                else if (shape.Type == "Bezier" && shape.ControlPoints != null)
                {
                    var bezPoints = new List<Ellipse>();
                    foreach (var pt in shape.ControlPoints)
                    {
                        var e = new Ellipse { Width = 10, Height = 10, Fill = Brushes.Blue };
                        Canvas.SetLeft(e, pt.X - 5);
                        Canvas.SetTop(e, pt.Y - 5);
                        DrawingCanvas.Children.Add(e);
                        bezPoints.Add(e);
                    }
                    
                    var polyline = new Polyline
                    {
                        Stroke = Brushes.Red,
                        StrokeThickness = 2
                    };
                    var pts = new Avalonia.Collections.AvaloniaList<Point>();
                    foreach (var pt in shape.ControlPoints)
                        pts.Add(pt);
                    polyline.Points = pts;
                    DrawingCanvas.Children.Add(polyline);
                }
            }
        }
    }
}
