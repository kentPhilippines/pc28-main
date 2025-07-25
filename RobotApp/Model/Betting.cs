using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotApp.Model
{
    internal class Betting
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string NickName { get; set; }
        public int Issue { get; set; }
        public int Jifen { get; set; }
        public int Slyk { get; set; }
        public int Blyk { get; set; }
        public int Blzf { get; set; }
        public int BlWater { get; set; }
        public string ccnr { get; set; }
        public bool IsDummy { get; set; }
        public bool WaterFinish { get; set; }
    }
}
