using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Interactivity;
using SteamWorkshopExplorer.Models;
using SteamWorkshopExplorer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SteamWorkshopExplorer.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel ViewModel => DataContext as MainViewModel
            ?? throw new InvalidOperationException("DataContext не установлен");

        private Ellipse? _selectedEllipse;
        private Point _startMousePos;
        private double _startWidth;
        private double _startHeight;
        private const double ResizeHandleSize = 10;
        private bool _isResizing;

        private List<Ellipse> _bezierPoints = new();
        private Polyline _bezierLine = new Polyline { Stroke = Brushes.Red, StrokeThickness = 2 };

        public MainWindow()
        {
            InitializeComponent();

            if (DataContext == null)
                DataContext = new MainViewModel();

            
            ViewModel.LoadFromJson();
            foreach (var shape in ViewModel.Shapes)
                DrawShape(shape);
        }

        
        private void OnEllipseModeClicked(object? sender, RoutedEventArgs e) => ViewModel.IsEllipseMode = true;
        private void OnBezierModeClicked(object? sender, RoutedEventArgs e) => ViewModel.IsEllipseMode = false;
        private void OnClear(object? sender, RoutedEventArgs e)
        {
            ViewModel.Clear();
            DrawingCanvas.Children.Clear();
            _bezierPoints.Clear();
            _bezierLine = new Polyline { Stroke = Brushes.Red, StrokeThickness = 2 };
        }
        private void OnSave(object? sender, RoutedEventArgs e)
        {
            ViewModel.SaveToJson();
        }

        
        private void Canvas_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var pos = e.GetPosition(DrawingCanvas);

            
            foreach (var child in DrawingCanvas.Children)
            {
                if (child is Ellipse ellipse && ellipse.Fill == Brushes.DeepPink)
                {
                    double left = Canvas.GetLeft(ellipse);
                    double top = Canvas.GetTop(ellipse);
                    double right = left + ellipse.Width;
                    double bottom = top + ellipse.Height;

                    if (pos.X >= right - ResizeHandleSize && pos.X <= right &&
                        pos.Y >= bottom - ResizeHandleSize && pos.Y <= bottom)
                    {
                        _selectedEllipse = ellipse;
                        _isResizing = true;
                        _startMousePos = pos;
                        _startWidth = ellipse.Width;
                        _startHeight = ellipse.Height;
                        return;
                    }

                    if (pos.X >= left && pos.X <= right && pos.Y >= top && pos.Y <= bottom)
                    {
                        _selectedEllipse = ellipse;
                        _isResizing = false;
                        _startMousePos = pos;
                        _startWidth = ellipse.Width;
                        _startHeight = ellipse.Height;
                        return;
                    }
                }
            }

            if (ViewModel.IsEllipseMode)
            {
               
                var ellipse = new Ellipse
                {
                    Width = 100,
                    Height = 60,
                    Fill = Brushes.DeepPink,
                    Stroke = Brushes.OrangeRed,
                    StrokeThickness = 2
                };
                Canvas.SetLeft(ellipse, pos.X - 50);
                Canvas.SetTop(ellipse, pos.Y - 30);
                DrawingCanvas.Children.Add(ellipse);

                ViewModel.AddEllipse(pos.X, pos.Y, ellipse.Width, ellipse.Height);

                _selectedEllipse = ellipse;
                _startMousePos = pos;
                _startWidth = ellipse.Width;
                _startHeight = ellipse.Height;
                _isResizing = false;
            }
            else
            {
               
                var point = new Ellipse { Width = 10, Height = 10, Fill = Brushes.Blue };
                Canvas.SetLeft(point, pos.X - 5);
                Canvas.SetTop(point, pos.Y - 5);
                DrawingCanvas.Children.Add(point);
                _bezierPoints.Add(point);

                if (!DrawingCanvas.Children.Contains(_bezierLine))
                    DrawingCanvas.Children.Add(_bezierLine);

                var pointsList = new Avalonia.Collections.AvaloniaList<Point>();
                foreach (var p in _bezierPoints)
                    pointsList.Add(new Point(Canvas.GetLeft(p) + 5, Canvas.GetTop(p) + 5));

                _bezierLine.Points = pointsList;

                ViewModel.AddBezier(pointsList.ToList());
            }
        }

        private void Canvas_PointerMoved(object? sender, PointerEventArgs e)
        {
            if (_selectedEllipse == null) return;

            var pos = e.GetPosition(DrawingCanvas);
            double dx = pos.X - _startMousePos.X;
            double dy = pos.Y - _startMousePos.Y;

            if (_isResizing)
            {
                _selectedEllipse.Width = Math.Max(10, _startWidth + dx);
                _selectedEllipse.Height = Math.Max(10, _startHeight + dy);
            }
            else
            {
                Canvas.SetLeft(_selectedEllipse, Canvas.GetLeft(_selectedEllipse) + dx);
                Canvas.SetTop(_selectedEllipse, Canvas.GetTop(_selectedEllipse) + dy);
                _startMousePos = pos;
            }
        }

        private void Canvas_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            _selectedEllipse = null;
        }

        private void DrawShape(ShapeModel shape)
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
                Canvas.SetLeft(ellipse, shape.X - shape.Width / 2);
                Canvas.SetTop(ellipse, shape.Y - shape.Height / 2);
                DrawingCanvas.Children.Add(ellipse);
            }
            else if (shape.Type == "Bezier" && shape.ControlPoints != null)
            {
                var bezierLine = new Polyline { Stroke = Brushes.Red, StrokeThickness = 2 };
                var pointsList = new Avalonia.Collections.AvaloniaList<Point>();
                foreach (var p in shape.ControlPoints)
                    pointsList.Add(p);
                bezierLine.Points = pointsList;
                DrawingCanvas.Children.Add(bezierLine);
            }
        }
    }
}
