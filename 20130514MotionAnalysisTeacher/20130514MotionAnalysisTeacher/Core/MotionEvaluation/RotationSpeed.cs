using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using _20130514MotionAnalysisTeacher.Entity;

namespace _20130514MotionAnalysisTeacher.Core.MotionEvaluation
{
    class RotationSpeed
    {
        /*
         * Function/Quaternions
         * class Quaternions
         * */
        private Quaternions quaternions;

        /*
         * use to store the vectors which are consisted of every two joint points in one skeleton
         * */
        private ArrayList preAL;

        private ArrayList curAL;



        /// <summary>
        /// Calculate each quaternion of every joint vector
        /// </summary>
        /// <param name="preSkeleton">the previous skeleton data</param>
        /// <param name="curSkeleton">the current skeleton data</param>
        public RotationSpeed(Position[] preSkeleton, Position[] curSkeleton)
        {


            this.quaternions = new Quaternions();

            SkeletonVector preSV = new SkeletonVector(preSkeleton);

            preAL = preSV.GetVectors();

            SkeletonVector curSV = new SkeletonVector(curSkeleton);

            curAL = curSV.GetVectors();


        }

        /// <summary>
        /// get an ArrayList of Quaternion 
        /// </summary>
        public ArrayList GetQuaternions()
        {
            ArrayList quaternionAl = new ArrayList();
            for (int i = 0; i < preAL.Count; i++)
            {
                Vector v1 = preAL[i] as Vector;
                Vector v2 = curAL[i] as Vector;

                //get quaternion by function Quaternions.GetQuaternionByVectors(v1, v2)
                quaternionAl.Add(this.quaternions.GetQuaternionByVectors(v1, v2));
            }

            return quaternionAl;
        }



        /// <summary>
        /// normalize a vectors
        /// </summary>
        /// <param name="x">the vector coordinate x </param>
        /// <param name="y">the vector coordinate y </param>
        /// <param name="z">the vector coordinate z </param>
        private Vector Normalize(double x, double y, double z)
        {
            Vector v;

            double key = 1;

            key = x * x + y * y + z * z;

            if (key != 0)
            {
                key = 1 / key;
            }

            key = Math.Sqrt(key);

            v = new Vector(x * key, y * key, z * key);

            return v;
        }
    }
}


