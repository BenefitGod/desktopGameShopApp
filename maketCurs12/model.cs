// Game.cs
using System.Windows.Media;

namespace maketCurs12.Models
{
    public class Game
    {
        public string Name { get; set; }
        public string Genre { get; set; }
        public string IconPath { get; set; }
        public string Status { get; set; }
        public string StatusColor { get; set; }
        public string IconColor { get; set; }
        public string ImagePath { get; set; }
        public string OverlayColor { get; set; } = "#80000000";
        public string Description { get; set; } // Добавлено описание
    }
}