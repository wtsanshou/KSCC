using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using _20130520MotionAnalysisStudent.Entity;

namespace _20130520MotionAnalysisStudent.Core.UnifiedCoordinate
{
    class SkeletonCentre
    {
        /// <summary>
        /// A struct of center point
        /// </summary>
        public struct Position
        {
            public double x;
            public double y;
            public double z;
        };

        /// <summary>
        /// the number of skeleton joints
        /// </summary>
        const int JOINTNUMBER = 20;

        /// <summary>
        /// defin a center position
        /// </summary>
        private Position center;

        /// <summary>
        /// get the first skeleton and calcuate the centre of the skeleton
        /// </summary>
        /// <param name="skeleton">the skeleton data</param>
        public SkeletonCentre(Skeleton skeleton)
        {
            SkeletonJoints sj = new SkeletonJoints(skeleton);

            ///the sum value x,y,z of all joints in one skeleton
            for (int i = 0; i < JOINTNUMBER; i++)
            {
                this.center.x += sj.GetJoints()[i].Position.X;
                this.center.y += sj.GetJoints()[i].Position.Y;
                this.center.z += sj.GetJoints()[i].Position.Z;
            }

            ///average
            this.center.x /= JOINTNUMBER;
            this.center.y /= JOINTNUMBER;
            this.center.z /= JOINTNUMBER;
        }

        /// <summary>
        /// add a skeleton center, and calcuate the new average center
        /// </summary>
        /// <param name="skeleton">the next skeleton data</param>
        /// <returns>the average center in the past period time</returns>
        public void SumCenter(Skeleton skeleton)
        {
            Position c = new Position();

            SkeletonJoints nextSJ = new SkeletonJoints(skeleton);

            ///the sum value x,y,z of all joints in one skeleton
            for (int i = 0; i < JOINTNUMBER; i++)
            {
                c.x += nextSJ.GetJoints()[i].Position.X;
                c.y += nextSJ.GetJoints()[i].Position.Y;
                c.z += nextSJ.GetJoints()[i].Position.Z;
            }

            ///average
            c.x /= JOINTNUMBER;
            c.y /= JOINTNUMBER;
            c.z /= JOINTNUMBER;

            ///Sum
            this.center.x += c.x;
            this.center.y += c.y;
            this.center.z += c.z;

        }

        public Position GetSumCentre()
        {
            return this.center;
        }
    }
}

