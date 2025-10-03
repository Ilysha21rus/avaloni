using Avalonia;
using SteamWorkshopExplorer.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace SteamWorkshopExplorer.ViewModels
{
    public class MainViewModel
    {
        public ObservableCollection<ShapeModel> Shapes { get; set; } = new();
        public bool IsBezierMode { get; set; } = false;

        private const string SaveFile = "shapes.json";

        public void AddEllipse(double x, double y)
        {
            Shapes.Add(new ShapeModel
            {
                Type = "Ellipse",
                X = x,
                Y = y
            });
        }

        public void AddBezier(List<Point> points)
        {
            Shapes.Add(new ShapeModel
            {
                Type = "Bezier",
                ControlPoints = points
            });
        }

        public void Clear()
        {
            Shapes.Clear();
        }

        public void SaveToJson()
        {
            var json = JsonSerializer.Serialize(Shapes, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SaveFile, json);
        }

        public void LoadFromJson()
        {
            if (!File.Exists(SaveFile)) return;

            var json = File.ReadAllText(SaveFile);
            var shapes = JsonSerializer.Deserialize<ObservableCollection<ShapeModel>>(json);

            if (shapes != null)
            {
                Shapes.Clear();
                foreach (var s in shapes) Shapes.Add(s);
            }
        }
    }
}