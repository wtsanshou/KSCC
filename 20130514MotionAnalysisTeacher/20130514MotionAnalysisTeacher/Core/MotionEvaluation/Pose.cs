using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using _20130514MotionAnalysisTeacher.Entity;
using System.Collections;

namespace _20130514MotionAnalysisTeacher.Core.MotionEvaluation
{
    class Pose
    {
        /**
         * there are 19 joint vectors in the Skeleton
         * user 19 vectors as 19 joints
         * */
        public Vector[] joints = new Vector[19];

        /**
         * Function/Quaternions.cs
         * use it to get Quaternion by two vectors
         * */
        private Quaternions quaternions;


        /// <summary>
        /// Calculate each angle
        /// </summary>
        /// <function>Constructor</function>
        /// <param name="skeleton">the skeleton data</param>
        public Pose(Position[] positions)
        {
            this.quaternions = new Quaternions();

            SkeletonVector sv = new SkeletonVector(positions);

            this.joints = sv.GetSkeletonVectors();

        }



        /// <summary>
        /// get an array of Quaternion by every pairs of Joint Vectors
        /// </summary>
        public ArrayList GetQuternions()
        {
            ArrayList al = new ArrayList();
            for (int i = 0; i < this.joints.Length; i++)
            {
                for (int j = i + 1; j < this.joints.Length; j++)
                {
                    Quaternion q = this.GetQuaternion(joints[i], joints[j]);
                    al.Add(q);
                }
            }

            return al;
        }

        /// <summary>
        /// get a quaternion by two joint vectors
        /// </summary>
        /// <param name="joint1">the first joint vector</param>
        /// <param name="joint2">the second joint vector</param>
        private Quaternion GetQuaternion(Vector joint1, Vector joint2)
        {
            return this.quaternions.GetQuaternionByVectors(joint1, joint2);

        }


    }
}

