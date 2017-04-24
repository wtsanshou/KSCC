using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Windows;

namespace _20130514MotionAnalysisTeacher.GestureManage
{
    class GestureManager
    {

        private Skeleton _skeleton;
        private KinectSensor _kinectSensor;

        private bool _isArmsExtended = false;



        /*
         * register Event Handler start to collect coordinate data of left shoulder and right shoulder function
         * */
        public event EventHandler CollectCoordinate;


        /// <summary>
        /// Load Pose Library
        /// </summary>
        /// <function>Constructor</function>
        /// <param name="kinectSensor">The kinect sensor which is using</param>
        public GestureManager(KinectSensor kinectSensor)
        {
            this._kinectSensor = kinectSensor;
            PopulatePoseLibrary();
        }

        /// <summary>
        /// start to track pose
        /// </summary>
        /// <param name="skeleton">the skeleton data</param>
        public void TrackPose(Skeleton skeleton)
        {

            this._skeleton = skeleton;

            Joint head = new Joint();
            Joint rightHand = new Joint();
            Joint leftHand = new Joint();

            head = skeleton.Joints[JointType.Head];
            rightHand = skeleton.Joints[JointType.HandRight];
            leftHand = skeleton.Joints[JointType.HandLeft];

            ProcessPose();
        }


        /// <summary>
        /// Store angel , joints and acceptable error
        /// </summary>
        /// <param name="skeleton">the skeleton data</param>
        public class PoseAngle
        {
            public JointType CenterJoint { get; private set; }
            public JointType AngleJoint { get; private set; }
            public double Angle { get; private set; }

            /// <summary>
            /// acceptable error
            /// </summary>
            public double Threshold { get; private set; }

            public PoseAngle(JointType centerJoint, JointType angleJoint, double angle, double threshold)
            {
                this.CenterJoint = centerJoint;
                this.AngleJoint = angleJoint;
                this.Angle = angle;
                this.Threshold = threshold;
            }
        }

        /// <summary>
        /// A Pose content
        /// </summary>
        public struct Pose
        {
            public string Title;
            public PoseAngle[] Angles;
        }

        /// <summary>
        /// pose library
        /// </summary>
        private Pose[] _PoseLibrary;

        /// <summary>
        /// one start pose
        /// </summary>
        private Pose _StartPose;

        /// <summary>
        /// defind personal pose library
        /// </summary>
        private void PopulatePoseLibrary()
        {
            this._PoseLibrary = new Pose[2];
            PoseAngle[] angles;

            ///Arms Extended
            this._StartPose = new Pose();
            this._StartPose.Title = "Start to calculate angle";
            angles = new PoseAngle[4];
            angles[0] = new PoseAngle(JointType.ShoulderLeft, JointType.ElbowLeft, 180, 20);
            angles[1] = new PoseAngle(JointType.ElbowLeft, JointType.WristLeft, 180, 20);
            angles[2] = new PoseAngle(JointType.ShoulderRight, JointType.ElbowRight, 0, 20);
            angles[3] = new PoseAngle(JointType.ElbowRight, JointType.WristRight, 0, 20);
            this._StartPose.Angles = angles;

            ///Both Hands Up
            this._PoseLibrary[0] = new Pose();
            this._PoseLibrary[0].Title = "Surrender!";
            angles = new PoseAngle[4];
            angles[0] = new PoseAngle(JointType.ShoulderLeft, JointType.ElbowLeft, 180, 20);
            angles[1] = new PoseAngle(JointType.ElbowLeft, JointType.WristLeft, 90, 20);
            angles[2] = new PoseAngle(JointType.ShoulderRight, JointType.ElbowRight, 0, 20);
            angles[3] = new PoseAngle(JointType.ElbowRight, JointType.WristRight, 90, 20);
            this._PoseLibrary[0].Angles = angles;

            ///Both Hands Down
            this._PoseLibrary[1] = new Pose();
            this._PoseLibrary[1].Title = "Scarecrow!";
            angles = new PoseAngle[4];
            angles[0] = new PoseAngle(JointType.ShoulderLeft, JointType.ElbowLeft, 180, 20);
            angles[1] = new PoseAngle(JointType.ElbowLeft, JointType.WristLeft, 270, 20);
            angles[2] = new PoseAngle(JointType.ShoulderRight, JointType.ElbowRight, 0, 20);
            angles[3] = new PoseAngle(JointType.ElbowRight, JointType.WristRight, 270, 20);
            this._PoseLibrary[1].Angles = angles;
        }

        /// <summary>
        /// Judge if the pose is in the Pose Library
        /// </summary>
        /// <param name="skeleton">the skeleton data</param>
        /// <param name="pose">a pose</param>
        private bool IsPose(Skeleton skeleton, Pose pose)
        {
            bool isPose = true;
            double angle;
            double poseAngle;
            double poseThreshold;
            double loAngle;
            double hiAngle;

            for (int i = 0; i < pose.Angles.Length && isPose; i++)
            {
                poseAngle = pose.Angles[i].Angle;
                poseThreshold = pose.Angles[i].Threshold;
                angle = GetJointAngle(skeleton.Joints[pose.Angles[i].CenterJoint], skeleton.Joints[pose.Angles[i].AngleJoint]);

                hiAngle = poseAngle + poseThreshold;
                loAngle = poseAngle - poseThreshold;

                if (hiAngle >= 360 || loAngle < 0)
                {
                    loAngle = (loAngle < 0) ? 360 + loAngle : loAngle;
                    hiAngle = hiAngle % 360;
                    isPose = !(loAngle > angle && angle > hiAngle);
                }
                else
                {
                    isPose = (loAngle <= angle && hiAngle >= angle);
                }
            }
            return isPose;
        }

        /// <summary>
        /// calcuate the angle of the point
        /// </summary>
        /// <param name="zeroJoint">one joint</param>
        /// <param name="angleJoint">another joint</param>
        private double GetJointAngle(Joint zeroJoint, Joint angleJoint)
        {
            Point zeroPoint = getJointPoint(zeroJoint);
            Point anglePoint = getJointPoint(angleJoint);
            Point x = new Point(zeroPoint.X + anglePoint.X, zeroPoint.Y);

            double a;
            double b;
            double c;

            a = Math.Sqrt(Math.Pow(zeroPoint.X - anglePoint.X, 2) + Math.Pow(zeroPoint.Y - anglePoint.Y, 2));
            b = anglePoint.X;
            c = Math.Sqrt(Math.Pow(anglePoint.X - x.X, 2) + Math.Pow(anglePoint.Y - x.Y, 2));

            double angleRad = Math.Acos((a * a + b * b - c * c) / (2 * a * b));
            double angleDeg = angleRad * 180 / Math.PI;

            if (zeroPoint.Y < anglePoint.Y)
            {
                angleDeg = 360 - angleDeg;
            }
            return angleDeg;
        }

        /// <summary>
        /// Get a point from depth image
        /// </summary>
        /// <param name="zeroJoint">one joint</param>
        private Point getJointPoint(Joint joint)
        {
            DepthImagePoint point = this._kinectSensor.MapSkeletonPointToDepth(joint.Position, DepthImageFormat.Resolution640x480Fps30);

            return new Point((double)point.X, (double)point.Y);
        }

        /// <summary>
        /// Process the StartPose 
        /// </summary>
        private void ProcessPose()
        {
            if (IsPose(this._skeleton, this._StartPose))
            {
                if (!this._isArmsExtended)
                {
                    if (CollectCoordinate != null)
                    {

                        CollectCoordinate(this, new EventArgs());
                    }
                    this._isArmsExtended = true;
                }
            }
            else
            {
                this._isArmsExtended = false;
            }


        }

    }
}


