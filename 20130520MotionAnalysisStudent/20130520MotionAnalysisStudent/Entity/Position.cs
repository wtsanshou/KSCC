using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _20130520MotionAnalysisStudent.Entity
{
    class Position
    {
        public double x;
        public double y;
        public double z;

        public Position()
        { }

        public Position(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}
