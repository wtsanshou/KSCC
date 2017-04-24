using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using _20130514MotionAnalysisTeacher.Entity;
using Microsoft.Kinect;

namespace _20130514MotionAnalysisTeacher.Core.UnifiedCoordinate
{
    class Translation
    {
        /*
         * the number of joint points we considered
         */
        const int JOINTCOUNT = 20;

        /*
         * use to store all Joints in one skeleton
         * */
        private Joint[] jointAl = new Joint[JOINTCOUNT];

        /// <summary>
        /// 
        /// </summary>
        private Position[] positions = new Position[JOINTCOUNT];

        /// <summary>
        /// the value of translation from X axis
        /// </summary>
        private double XAxisTranslation = 0;

        /// <summary>
        /// the value of translation from Y axis
        /// </summary>
        private double YAxisTranslation = 0;

        /// <summary>
        /// the value of translation from Z axis
        /// </summary>
        private double ZAxisTranslation = 0;


        /// <summary>
        /// the angle of rotation
        /// </summary>
        private double rotationAngle = 0;

        /// <summary>
        /// define the coordinat of skeleton centre
        /// </summary>
        private double centreX = 0;
        private double centreY = 0;
        private double centreZ = 0;

        /// <summary>
        /// Initialize parameters of translation
        /// </summary>
        /// <function>Constructor</function>
        public Translation(double XAxisTranslation, double YAxisTranslation, double ZAxisTranslation, double rotationAngle)
        {
            this.XAxisTranslation = XAxisTranslation;
            this.YAxisTranslation = YAxisTranslation;
            this.ZAxisTranslation = ZAxisTranslation;

            this.rotationAngle = rotationAngle;
        }

        public Position testAJoint()
        {

            //Position p = new Position(this.jointAl[6].Position.X, this.jointAl[6].Position.Y, this.jointAl[6].Position.Z);
            //return p;

            var x1 = jointAl[6].Position.X - this.centreX;
            var z1 = jointAl[6].Position.Z - this.centreZ;

            var angle = this.rotationAngle;

            var x = x1 * Math.Cos(angle) + z1 * Math.Sin(angle);

            var z = z1 * Math.Cos(angle) - x1 * Math.Sin(angle);

            x += this.centreX;
            z += this.centreZ;

            x = (x - this.XAxisTranslation);
            var y = (jointAl[6].Position.Y - this.YAxisTranslation);
            z = z - this.ZAxisTranslation;

            Position p = new Position(x, y, z);

            return p;

        }

        /// <summary>
        /// get the new coordinates of all joints in skeleton
        /// </summary>
        /// <param name="skeleton">the skeleton data</param>
        /// <returns>the new coordinates</returns>
        public Position[] GetNewCoordinates(Skeleton skeleton)
        {
            /*
             * get joints array from the skeleton
             * */
            this.SetJoints(skeleton);

            /*
             * rotation every joint
             * */
            for (int i = 0; i < JOINTCOUNT; i++)
            {
                var x1 = this.jointAl[i].Position.X - this.XAxisTranslation;
                var z1 = this.jointAl[i].Position.Z - this.ZAxisTranslation;

                var angle = this.rotationAngle;

                var x = x1 * Math.Cos(angle) + z1 * Math.Sin(angle);

                var z = z1 * Math.Cos(angle) - x1 * Math.Sin(angle);

                var y = (this.jointAl[i].Position.Y - this.YAxisTranslation);

                this.positions[i] = new Position(x, y, z);
            }

            return this.positions;
        }

        /// <summary>
        /// Initialize joints of the skeleton
        /// </summary>
        /// <param name="skeleton">the skeleton data</param>
        private void SetJoints(Skeleton skeleton)
        {
            SkeletonJoints sj = new SkeletonJoints(skeleton);

            this.jointAl = sj.GetJoints();
        }

        /// <summary>
        /// Initialize the centre coordinate
        /// </summary>
        private void SetCentre()
        {
            Position p = new Position(0, 0, 0);

            this.centreX = 0;
            this.centreY = 0;
            this.centreZ = 0;

            ///the sum value x,y,z of all joints in one skeleton
            for (int i = 0; i < JOINTCOUNT; i++)
            {
                this.centreX += this.jointAl[i].Position.X;
                this.centreY += this.jointAl[i].Position.Y;
                this.centreZ += this.jointAl[i].Position.Z;

            }

            ///average
            this.centreX /= JOINTCOUNT;
            this.centreY /= JOINTCOUNT;
            this.centreZ /= JOINTCOUNT;

        }

        /// <summary>
        /// translation joint position to (0,0,0) coordinate system
        /// </summary>
        /// <param name="joint">a joint position of the skeleton</param>
        /// <returns>a new position in coordinate system</returns>
        private Position TranslateAxis(Position position, double x, double y, double z, bool isPlus)
        {
            Position p = new Position();

            if (isPlus)
            {
                p.x = position.x + x;
                p.y = position.y + y;
                p.z = position.z + z;
            }
            else
            {
                p.x = position.x - x;
                p.y = position.y - y;
                p.z = position.z - z;
            }

            return p;
        }



        /// <summary>
        /// Rotation the position around the center axis
        /// </summary>
        /// <param name="position">a joint position in the new coordnate</param>
        /// <returns>the final coordinate</returns>
        private Position Rotation(Position position)
        {
            //Position p = this.TranslateAxis(position, this.centreX, this.centreY, this.centreZ, false);
            Position p = position;
            var x = p.x;
            var z = p.z;

            var angle = this.rotationAngle;

            p.x = x * Math.Cos(angle) + z * Math.Sin(angle);
            p.z = z * Math.Cos(angle) - x * Math.Sin(angle);

            //return this.TranslateAxis(p, this.centreX, this.centreY, this.centreZ, true);
            return p;
        }
    }
}

