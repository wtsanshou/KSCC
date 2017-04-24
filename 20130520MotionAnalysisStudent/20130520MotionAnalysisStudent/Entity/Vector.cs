using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _20130520MotionAnalysisStudent.Entity
{
    class Vector
    {
        double X, Y, Z;
        /*
                /// <summary>
                /// Initialize a Vector
                /// </summary>
                /// <function>Constructor</function>
                /// <param name="p1">the first skeleton point data</param>
                /// <param name="p2">the second skeleton point data</param>
                public Vector(SkeletonPoint p1, SkeletonPoint p2)
                {
                    this.X = p2.X - p1.X;

                    this.Y = p2.Y - p1.Y;

                    this.Z = p2.Z - p1.Z;
                }
        */
        public Vector() { }
        /// <summary>
        /// Initialize a Vector
        /// </summary>
        /// <function>Constructor</function>
        /// <param name="p1">the first joint point data</param>
        /// <param name="p2">the second joint point data</param>
        public Vector(Position p1, Position p2)
        {
            this.X = p2.x - p1.x;
            //Console.WriteLine(this.getX());
            this.Y = p2.y - p1.y;

            this.Z = p2.z - p1.z;
        }

        /// <summary>
        /// Initialize a Vector
        /// </summary>
        /// <function>Constructor</function>
        /// <param name="x">the vector coordinate x </param>
        /// <param name="y">the vector coordinate y </param>
        /// <param name="z">the vector coordinate z </param>
        public Vector(double x, double y, double z)
        {
            this.X = x;

            this.Y = y;

            this.Z = z;
        }

        public double getX()
        {
            return this.X;
        }

        public double getY()
        {
            return this.Y;
        }

        public double getZ()
        {
            return this.Z;
        }

    }
}