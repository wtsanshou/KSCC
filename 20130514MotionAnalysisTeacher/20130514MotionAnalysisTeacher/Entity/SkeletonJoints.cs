using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace _20130514MotionAnalysisTeacher.Entity
{
    class SkeletonJoints
    {/*
         * the number of joint points we considered
         */
        const int JOINTCOUNT = 20;

        /*
         * use to store all Joints in one skeleton
         * */
        private Joint[] jointAl = new Joint[JOINTCOUNT];

        /// <summary>
        /// Initialize joints of the skeleton
        /// </summary>
        /// <function>Constructor</function>
        /// <param name="skeleton">the skeleton data</param>
        public SkeletonJoints(Skeleton skeleton)
        {

            //1
            this.jointAl[0] = skeleton.Joints[JointType.HandRight];

            //2
            this.jointAl[1] = skeleton.Joints[JointType.WristRight];

            //3
            this.jointAl[2] = skeleton.Joints[JointType.ElbowRight];

            //4
            this.jointAl[3] = skeleton.Joints[JointType.ShoulderRight];

            //5
            this.jointAl[4] = skeleton.Joints[JointType.Head];

            //6
            this.jointAl[5] = skeleton.Joints[JointType.ShoulderCenter];

            //7
            this.jointAl[6] = skeleton.Joints[JointType.ShoulderLeft];

            //8
            this.jointAl[7] = skeleton.Joints[JointType.ElbowLeft];

            //9
            this.jointAl[8] = skeleton.Joints[JointType.WristLeft];

            //10
            this.jointAl[9] = skeleton.Joints[JointType.HandLeft];

            //11
            this.jointAl[10] = skeleton.Joints[JointType.Spine];

            //12
            this.jointAl[11] = skeleton.Joints[JointType.HipCenter];

            //13
            this.jointAl[12] = skeleton.Joints[JointType.HipRight];

            //14
            this.jointAl[13] = skeleton.Joints[JointType.HipLeft];

            //15
            this.jointAl[14] = skeleton.Joints[JointType.KneeRight];

            //16
            this.jointAl[15] = skeleton.Joints[JointType.KneeLeft];

            //17
            this.jointAl[16] = skeleton.Joints[JointType.AnkleRight];

            //18
            this.jointAl[17] = skeleton.Joints[JointType.AnkleLeft];

            //19
            this.jointAl[18] = skeleton.Joints[JointType.FootRight];

            //20
            this.jointAl[19] = skeleton.Joints[JointType.FootLeft];

        }

        /// <summary>
        /// get all joints
        /// </summary>
        public Joint[] GetJoints()
        {
            return this.jointAl;
        }
    }
}


