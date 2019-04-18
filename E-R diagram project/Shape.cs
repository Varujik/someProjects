using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ER_W
{
    public class Shape
    {
        public static Canvas Canvas { get; set; }
        public static MainWindow Window { get; set; }

        public double PositionX { get; set; }
        public double PositionY { get; set; }
    }
}
