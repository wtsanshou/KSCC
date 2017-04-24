using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace _20130520MotionAnalysisStudent.Core.UnifiedCoordinate
{
    class Initial
    {
        /// <summary>
        /// the coordinate of left shoulder
        /// </summary>
        private double leftShoulderX;
        private double leftShoulderY;
        private double leftShoulderZ;

        /// <summary>
        /// the coordinate of right shoulder
        /// </summary>
        private double rightShoulderX;
        private double rightShoulderY;
        private double rightShoulderZ;

        /// <summary>
        /// the lenght of initial
        /// </summary>
        private const int FRAMENUMBER = 120;

        /// <summary>
        /// the id of frames in the initial period
        /// </summary>
        private int frameID;

        /// <summary>
        /// the average centre of the skeleton in the initial period
        /// //Function/SkeletonCentre.cs
        /// </summary>
        private SkeletonCentre skeletonCentre;

        /// <summary>
        /// the value of translation from X axis
        /// </summary>
        private double XAxisTranslation;

        /// <summary>
        /// the value of translation from Y axis
        /// </summary>
        private double YAxisTranslation;

        /// <summary>
        /// the value of translation from Z axis
        /// </summary>
        private double ZAxisTranslation;

        /// <summary>
        /// the value of flexing the x, y coordinates
        /// </summary>
        //private double XYFlex = 1;

        /// <summary>
        /// the angle of rotation
        /// </summary>
        private double rotationAngle;


        public Initial()
        {
            this.XAxisTranslation = 0;
            this.YAxisTranslation = 0;
            this.ZAxisTranslation = 0;
            this.rotationAngle = 0;

            leftShoulderX = 0;
            leftShoulderY = 0;
            leftShoulderZ = 0;

            rightShoulderX = 0;
            rightShoulderY = 0;
            rightShoulderZ = 0;

            frameID = 0;
        }

        /// <summary>
        /// get the results of initial
        /// </summary>
        /// <returns>(0,0,0) coordinate, and angle between user and kinect</returns>
        public double[] GetInitialResults()
        {

            double[] results = { this.XAxisTranslation, this.YAxisTranslation, this.ZAxisTranslation, this.rotationAngle };

            //Console.WriteLine(results[0] + "--" + results[1] + "--" + results[2] + "--" + results[3]);

            return results;
        }

        public bool IsInitialFinished()
        {
            if (this.frameID > FRAMENUMBER)
            {
                //Console.WriteLine(this.XAxisTranslation);
                this.frameID = 0;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// collect skeleton data 
        /// </summary>
        /// <param name="skeleton">skeleton data</param>
        public void Collect(Skeleton skeleton)
        {
            /*
             * left shoulder
             * */
            double lx = 0;
            double ly = 0;
            double lz = 0;

            /*
             * right shoulder
             * */
            double rx = 0;
            double ry = 0;
            double rz = 0;

            /*
             * the collection is finish
             * */
            if (this.frameID > FRAMENUMBER - 1)
            {
                this.frameID++;
                /*
                 * stop collect
                 * */
                //this.startcollect = false;

                /*
                 * calcuate the center of the skeleton
                 * */
                if (skeletonCentre != null)
                {
                    var center = skeletonCentre.GetSumCentre();

                    var x = center.x / frameID;
                    var y = center.y / frameID;
                    var z = center.z / frameID;

                    this.XAxisTranslation = x;
                    this.YAxisTranslation = y;
                    this.ZAxisTranslation = z;

                    //this.XYFlex = z / 3;
                }


                /*
                 * the average coordinate of left shoulder
                 * */
                lx = leftShoulderX / frameID;
                ly = leftShoulderY / frameID;
                lz = leftShoulderZ / frameID;

                /*
                 * the average coordinate of right shoulder
                 * */
                rx = rightShoulderX / frameID;
                ry = rightShoulderY / frameID;
                rz = rightShoulderZ / frameID;

                /*
                 * calcuate the angle between shoulder and kinect
                 * */
                var w = Math.Abs(rx - lx);
                var h = Math.Abs(rz - lz);
                if (w != 0)
                {
                    if (lz == rz)
                    {
                        this.rotationAngle = 0;

                    }
                    else if (lz < rz)
                    {
                        this.rotationAngle = Math.Atan(h / w);

                    }
                    else
                    {
                        this.rotationAngle = -Math.Atan(h / w);

                    }
                }
                //this.translation = new Translation(this.XAxisTranslation, this.YAxisTranslation, this.ZAxisTranslation, this.rotationAngle);
                //this.IsStartTranslation = true;
                //
            }
            else
            {
                frameID++;

                SkeletonPoint leftShoulder = skeleton.Joints[JointType.ShoulderLeft].Position;
                SkeletonPoint rightShoulder = skeleton.Joints[JointType.ShoulderRight].Position;

                leftShoulderX += leftShoulder.X;
                leftShoulderY += leftShoulder.Y;
                leftShoulderZ += leftShoulder.Z;

                rightShoulderX += rightShoulder.X;
                rightShoulderY += rightShoulder.Y;
                rightShoulderZ += rightShoulder.Z;


                lx = leftShoulderX / frameID;
                ly = leftShoulderY / frameID;
                lz = leftShoulderZ / frameID;

                rx = rightShoulderX / frameID;
                ry = rightShoulderY / frameID;
                rz = rightShoulderZ / frameID;


                if (frameID == 1)
                {
                    skeletonCentre = new SkeletonCentre(skeleton);
                }
                else if (skeletonCentre != null)
                {
                    skeletonCentre.SumCenter(skeleton);
                }
            }
        }
    }
}
