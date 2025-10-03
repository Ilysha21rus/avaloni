using Avalonia;
using System.Collections.Generic;

namespace SteamWorkshopExplorer.Models
{
    public class ShapeModel
    {
        public string Type { get; set; } = ""; // "Ellipse" или "Bezier"
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; } = 100;
        public double Height { get; set; } = 60;

        public List<Point>? ControlPoints { get; set; } // Для Безье
    }
}