using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotApp.Model
{
    internal class Keno
    {
        public int DrawNbr { get; set; }
        public DateTime DrawDate { get; set; }
        public DateTime DrawTime { get; set; }
        public int[] DrawNbrs { get; set; }
        public String DrawBonus { get; set; }
    }
}
