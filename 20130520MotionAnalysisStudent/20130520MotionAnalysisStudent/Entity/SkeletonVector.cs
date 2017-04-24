using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace _20130520MotionAnalysisStudent.Entity
{
    class SkeletonVector
    {
        /*
         * use to store the vectors which are consisted of every two joint points in one skeleton
         * */
        private ArrayList vectorAl;

        /*
         * the number of joint points we considered
         *
         */
        const int JOINTCOUNT = 20;

        /*
         * use to store all Joints in one skeleton
         * */
        private Position[] jointAl = new Position[JOINTCOUNT];

        /*
         * define the 19 vectors in a skeleton
         * */
        private Vector[] skeletonVectors = new Vector[JOINTCOUNT - 1];


        /// <summary>
        /// Initialize joints of the skeleton
        /// </summary>
        /// <function>Constructor</function>
        /// <param name="skeleton">the skeleton data</param>
        public SkeletonVector(Position[] positions)
        {
            this.jointAl = positions;

            ////////////////////////////////////////////////---19 vector in skeleton---//////////////////////////////////////////////////////
            skeletonVectors[0] = this.GetVector(positions[0], positions[1]);

            //2
            skeletonVectors[1] = this.GetVector(positions[1], positions[2]);

            //3
            skeletonVectors[2] = this.GetVector(positions[2], positions[3]);

            //4
            skeletonVectors[3] = this.GetVector(positions[3], positions[5]);

            //5
            skeletonVectors[4] = this.GetVector(positions[4], positions[5]);

            //6
            skeletonVectors[5] = this.GetVector(positions[5], positions[6]);

            //7
            skeletonVectors[6] = this.GetVector(positions[6], positions[7]);

            //8
            skeletonVectors[7] = this.GetVector(positions[7], positions[8]);

            //9
            skeletonVectors[8] = this.GetVector(positions[8], positions[9]);

            //10
            skeletonVectors[9] = this.GetVector(positions[5], positions[10]);

            //11
            skeletonVectors[10] = this.GetVector(positions[10], positions[11]);

            //12
            skeletonVectors[11] = this.GetVector(positions[11], positions[12]);

            //13
            skeletonVectors[12] = this.GetVector(positions[11], positions[13]);

            //14
            skeletonVectors[13] = this.GetVector(positions[12], positions[14]);

            //15
            skeletonVectors[14] = this.GetVector(positions[13], positions[15]);

            //16
            skeletonVectors[15] = this.GetVector(positions[14], positions[16]);

            //17
            skeletonVectors[16] = this.GetVector(positions[15], positions[17]);

            //18
            skeletonVectors[17] = this.GetVector(positions[16], positions[18]);

            //19
            skeletonVectors[18] = this.GetVector(positions[17], positions[19]);
        }

        /// <summary>
        /// get a vector by two joint positions
        /// </summary>
        /// <param name="joint1">the first joint position</param>
        /// <param name="joint2">the second joint position</param>
        private Vector GetVector(Position joint1, Position joint2)
        {
            return new Vector(joint1, joint2);
        }

        /// <summary>
        /// get all vectors in the skeleton
        /// </summary>
        public ArrayList GetVectors()
        {
            this.vectorAl = new ArrayList();
            for (int i = 0; i < JOINTCOUNT; i++)
            {
                for (int j = i + 1; j < JOINTCOUNT; j++)
                {
                    vectorAl.Add(GetVector(this.jointAl[i], this.jointAl[j]));
                }
            }
            return vectorAl;
        }

        /// <summary>
        /// return 19 vectors of the skeleton
        /// </summary>
        /// <returns>vectors array</returns>
        public Vector[] GetSkeletonVectors()
        {
            return this.skeletonVectors;
        }
    }
}

