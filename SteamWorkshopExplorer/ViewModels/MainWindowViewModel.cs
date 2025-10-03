using Avalonia;
using SteamWorkshopExplorer.Models;
using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace SteamWorkshopExplorer.ViewModels
{
    public class MainViewModel
    {
        public ObservableCollection<ShapeModel> Shapes { get; set; } = new();
        public bool IsEllipseMode { get; set; } = true;

        private const string SaveFile = "shapes.json";

        public void AddEllipse(double x, double y, double width, double height)
        {
            Shapes.Add(new ShapeModel
            {
                Type = "Ellipse",
                X = x,
                Y = y,
                Width = width,
                Height = height
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
            var json = JsonConvert.SerializeObject(Shapes, Formatting.Indented);
            File.WriteAllText(SaveFile, json);
        }

        public void LoadFromJson()
        {
            if (!File.Exists(SaveFile)) return;
            var json = File.ReadAllText(SaveFile);
            var shapes = JsonConvert.DeserializeObject<List<ShapeModel>>(json);
            if (shapes != null)
            {
                Shapes.Clear();
                foreach (var s in shapes)
                    Shapes.Add(s);
            }
        }
    }
}