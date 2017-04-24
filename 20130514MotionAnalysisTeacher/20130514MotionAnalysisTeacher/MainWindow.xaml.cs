using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Kinect.Toolbox.Record;
using Microsoft.Kinect;
using System.IO;
using System.Collections;
using Microsoft.Win32;
using _20130514MotionAnalysisTeacher.Entity;
using _20130514MotionAnalysisTeacher.Core.MotionEvaluation;
using _20130514MotionAnalysisTeacher.Core.UnifiedCoordinate;
using _20130514MotionAnalysisTeacher.GestureManage;
using System.Diagnostics;
using Kinect.Toolbox;
using System.Net;
using System.Net.Sockets;
using _20130514MotionAnalysisTeacher.SocketSetup;
using System.Timers;

namespace _20130514MotionAnalysisTeacher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /*
          *read and write the stream of recording
          */
        Stream recordStream;

        /*
        *class from Kinect.Toolbox
        *use to replay stream
        */
        KinectReplay Replay;

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();


        }


        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {

            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                // Turn on the skeleton stream to receive skeleton frames
                this.sensor.SkeletonStream.Enable();

                //It's important to set DepthStream or ColorStream Enable. 
                //In method Kinect.Toolbox.Tool.Vector2, the position(x,y) of SkeletonPoint need ColorStream or DepthStream Enable
                this.sensor.DepthStream.Enable();

                this.sensor.ColorStream.Enable();

                // Add an event handler to be called whenever there is new skeleton frame data
                this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.ColorFrameReady += this.SensorColorFrameReady;

                // Start the sensor!
                try
                {
                    this.sensor.Start();

                    /*
                     * set the pitch angle of the kinect sensor to 0 degree.
                     * */
                    this.sensor.ElevationAngle = 0;
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
        }

        /// <summary>
        /// Event handler for Kinect sensor's ColorFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame imageframe = e.OpenColorImageFrame())
            {
                if (imageframe != null)
                {
                    byte[] pixelData = new byte[imageframe.PixelDataLength];
                    imageframe.CopyPixelDataTo(pixelData);

                    Image.Source = BitmapImage.Create(imageframe.Width, imageframe.Height, 96, 96,
                                                                    PixelFormats.Bgr32, null, pixelData,
                                                                    imageframe.Width * imageframe.BytesPerPixel);

                }
            }
        }

        /// <summary>
        /// Event handler for Kinect sensor's SkeletonFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);

                    Skeleton skeleton = GetPrimarySkeleton(skeletons);

                    if (skeleton != null)
                    {
                        this.goalSkeleton = skeleton;

                        this.collectSkeleton = skeleton;
                        //start to detect gesture
                        if (isGesturestarted)
                        {
                            this.gestureManager.TrackPose(skeleton);
                        }

                        ///detect the Cross gesture
                        if (this.initial != null)
                        {
                            ///capture the Cross gesture
                            if (this.startcollect)
                            {
                                ///start to collect every skeleton
                                this.initial.Collect(skeleton);
                            }

                            ///the collection is finished
                            if (this.initial.IsInitialFinished())
                            {
                                ///get the 4 initial results 
                                double[] initialResults = this.initial.GetInitialResults();

                                ///put the 4 initial results into translation class
                                this.translation = new Translation(initialResults[0], initialResults[1], initialResults[2], initialResults[3]);

                                ///begin translation
                                this.IsStartTranslation = true;

                                ///close collect
                                this.startcollect = false;

                                this.allowSend = true;

                            }
                        }

                        ///start to translation
                        if (this.IsStartTranslation && this.translation != null)
                        {
                            ///get the 20 position of a skeleton joints
                            Position[] positions = this.translation.GetNewCoordinates(skeleton);

                            ///send the 20 position to students
                            if (this.cs != null && this.allowSend && this.isProcess)
                            {

                                XmlJointsCollection sendOutData = new XmlJointsCollection();

                                XmlOneJoint[] sendOutJoints = new XmlOneJoint[JOINTNUMBER]; 

                                for (int i = 0; i < JOINTNUMBER; i++)
                                {
                                    XmlOneJoint j = new XmlOneJoint() { x = positions[i].x, y = positions[i].y, z = positions[i].z };
                                    sendOutJoints[i] = j;
                                }

                                sendOutData.Joints = sendOutJoints;
                                cs.sendOutPositions(sendOutData);
                                this.isProcess = false;
                            }

                            ls.Content = string.Format("<{0:0.00},{1:0.00},{2:0.00}>", positions[6].x, positions[6].y, positions[6].z);

                            rs.Content = string.Format("<{0:0.00},{1:0.00},{2:0.00}>", positions[3].x, positions[3].y, positions[3].z);

                        }
                    }

                    this.DrawStickMen(skeletons);
                }
            }


        }

        /// <summary>
        /// Array of arrays of contiguous line segements that represent a skeleton.
        /// </summary>
        private static readonly JointType[][] SkeletonSegmentRuns = new JointType[][]
        {
            new JointType[] 
            { 
                JointType.Head, JointType.ShoulderCenter,JointType.Spine, JointType.HipCenter 
            },
            new JointType[] 
            { 
                JointType.HandLeft, JointType.WristLeft, JointType.ElbowLeft, JointType.ShoulderLeft,
                JointType.ShoulderCenter,
                JointType.ShoulderRight, JointType.ElbowRight, JointType.WristRight, JointType.HandRight
            },
            new JointType[]
            {
                JointType.FootLeft, JointType.AnkleLeft, JointType.KneeLeft, JointType.HipLeft,
                JointType.HipCenter,
                JointType.HipRight, JointType.KneeRight, JointType.AnkleRight, JointType.FootRight
            }
        };

        /// <summary>
        /// Draw stick men for all the tracked skeletons.
        /// </summary>
        /// <param name="skeletons">The skeletons to draw.</param>
        private void DrawStickMen(Skeleton[] skeletons)
        {
            // Remove any previous skeletons.
            StickMen.Children.Clear();
            curSkeleton.Children.Clear();

            foreach (var skeleton in skeletons)
            {
                // Only draw tracked skeletons.
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                {
                    // Draw a background for the next pass.
                    this.DrawStickMan(skeleton, Brushes.WhiteSmoke, 7, StickMen);
                    //this.DrawStickMan(skeleton, Brushes.WhiteSmoke, 7, curSkeleton);
                }
            }

            foreach (var skeleton in skeletons)
            {
                // Only draw tracked skeletons.
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                {
                    // Pick a brush, Red for a skeleton that has recently gestures, black for the nearest, gray otherwise.
                    /*
                    var brush = DateTime.UtcNow < this.highlightTime && skeleton.TrackingId == this.highlightId ? Brushes.Red :
                        skeleton.TrackingId == this.nearestId ? Brushes.Black : Brushes.Gray;
                     * */



                    // Draw the individual skeleton.
                    this.DrawStickMan(skeleton, brush, 3, StickMen);
                    this.DrawStickMan(skeleton, brush, 3, curSkeleton);
                }
            }
        }

        SolidColorBrush brush = Brushes.Blue;

        /// <summary>
        /// Draw an individual skeleton.
        /// </summary>
        /// <param name="skeleton">The skeleton to draw.</param>
        /// <param name="brush">The brush to use.</param>
        /// <param name="thickness">This thickness of the stroke.</param>
        private void DrawStickMan(Skeleton skeleton, Brush brush, int thickness, Canvas stickMan)
        {
            Debug.Assert(skeleton.TrackingState == SkeletonTrackingState.Tracked, "The skeleton is being tracked.");

            foreach (var run in SkeletonSegmentRuns)
            {
                var next = this.GetJointPoint(skeleton, run[0]);



                for (var i = 1; i < run.Length; i++)
                {

                    if (skeleton.Joints[run[i]].TrackingState == JointTrackingState.Tracked)
                    {
                        brush = Brushes.Blue;
                    }
                    else if (skeleton.Joints[run[i]].TrackingState == JointTrackingState.Inferred)
                    {
                        brush = Brushes.Red;

                    }

                    var prev = next;

                    var circle = new Ellipse
                    {
                        Stroke = System.Windows.Media.Brushes.Black,
                        Fill = System.Windows.Media.Brushes.Orange,
                        Margin = new Thickness(next.X, next.Y, 0, 0),
                        Width = 10,
                        Height = 10
                    };

                    next = this.GetJointPoint(skeleton, run[i]);

                    var circle1 = new Ellipse
                    {
                        Stroke = System.Windows.Media.Brushes.Black,
                        Fill = System.Windows.Media.Brushes.Orange,
                        Margin = new Thickness(next.X, next.Y, 0, 0),
                        Width = 10,
                        Height = 10
                    };

                    var line = new Line
                    {
                        Stroke = brush,
                        StrokeThickness = thickness,
                        X1 = prev.X,
                        Y1 = prev.Y,
                        X2 = next.X,
                        Y2 = next.Y,
                        StrokeEndLineCap = PenLineCap.Round,
                        StrokeStartLineCap = PenLineCap.Round
                    };

                    stickMan.Children.Add(circle);
                    stickMan.Children.Add(circle1);
                    stickMan.Children.Add(line);
                }
            }
        }

        /// <summary>
        /// Convert skeleton joint to a point on the StickMen canvas.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <param name="jointType">The joint to project.</param>
        /// <returns>The projected point.</returns>
        private Point GetJointPoint(Skeleton skeleton, JointType jointType)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = this.sensor.MapSkeletonPointToDepth(
                                                                             skeleton.Joints[jointType].Position,
                                                                             DepthImageFormat.Resolution640x480Fps30);

            return new Point(depthPoint.X, depthPoint.Y);
        }

        /// <summary>
        /// Get primary skeleton from all skeletons
        /// </summary>
        /// <param name="skeletons">all Skeltons</param>
        private static Skeleton GetPrimarySkeleton(Skeleton[] skeletons)
        {
            Skeleton skeleton = null;
            if (skeletons != null)
            {
                for (int i = 0; i < skeletons.Length; i++)
                {
                    if (skeletons[i].TrackingState == SkeletonTrackingState.Tracked)
                    {
                        if (skeleton == null)
                        {
                            skeleton = skeletons[i];
                        }
                        else
                        {
                            if (skeleton.Position.Z > skeletons[i].Position.Z)
                            {
                                skeleton = skeletons[i];
                            }
                        }
                    }
                }
            }
            return skeleton;
        }




        /****************************************************Evaluate Rotation Pose and Rotation Speed **********************************************************/


        /*
         * the skeleton which will be evaluate
        */
        Skeleton goalSkeleton;

        /*
         * class from Kinect.Toolbox
         * use to draw the skeleton of recording
        */
        SkeletonDisplayManager skeletonDisplayManager = null;

        /*
         * Display the final score of evaluating
        */
        double finalScore;

        /*
         * Display the final pose score of evaluating
        */
        double totalPoseScore;

        /*
         * Display the final rotation score of evaluating
        */
        double totalRotationScore;

        /*
         * record how many skeletons 
        */
        int skeletonID;



        /// <summary>
        /// the number of skeleton joints
        /// </summary>
        const int JOINTNUMBER = 20;

        /*
         * for current movement
         * the joints position in current skeleton stream
        */
        private Position[] goalPoistions;


        private Position[] firstGoalPositions = new Position[JOINTNUMBER];

        private Position[] firstRecordPositions = new Position[JOINTNUMBER];

        /*
         * Frame Span 
        */
        const int FRAMESPAN = 10;

        /*
         * Window Chart.xaml
        */

        /// <summary>
        /// if start to evaluate the two motions
        /// </summary>
        private bool startEvaluate = false;

        List<KeyValuePair<int, double>> poseList;

        List<KeyValuePair<int, double>> rotationList;

        /// <summary>
        /// play record 
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void PlayRecord()
        {
            try
            {
                //open the file which just be recorded
                recordStream = File.OpenRead(@testFileName);

                //instance class SkeletonDisplayManager to set the current Kinect sensor and where to draw
                skeletonDisplayManager = new SkeletonDisplayManager(this.sensor, recordSkeleton);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            /*
             * initialize the scores and Skeleton ID
             * */
            finalScore = 0;

            totalPoseScore = 0;

            totalRotationScore = 0;

            skeletonID = 0;


            this.Replay = new KinectReplay(recordStream);



            poseList = new List<KeyValuePair<int, double>>();

            rotationList = new List<KeyValuePair<int, double>>();

            //// Add an event handler to be called whenever there is new record color frame data
            if (showBackground.IsChecked == true)
            {
                recordImage.Visibility = Visibility.Visible;
                this.Replay.ColorImageFrameReady += Replay_ColorImageFrameReady;
            }
            else
            {
                recordImage.Visibility = Visibility.Hidden;
            }

            //// Add an event handler to be called whenever there is new record skeleton frame data
            this.Replay.SkeletonFrameReady += Replay_SkeletonFrameReady;

            this.Replay.Start();

        }

     

        /// <summary>
        /// the event handle a record color frame data
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">replay color image frame event arguments</param>
        private void Replay_ColorImageFrameReady(object sender, ReplayColorImageFrameReadyEventArgs e)
        {
            ProcessColorFrame(e.ColorImageFrame);
            
        }

        /// <summary>
        /// the function handle a record color frame data
        /// </summary>
        /// <param name="colorFrame">record color frame data</param>
        private void ProcessColorFrame(ReplayColorImageFrame colorFrame)
        {
            if (colorFrame != null)
            {
                byte[] pixelData = new byte[colorFrame.PixelDataLength];
                colorFrame.CopyPixelDataTo(pixelData);

                recordImage.Source = BitmapImage.Create(colorFrame.Width, colorFrame.Height, 96, 96,
                                                                PixelFormats.Bgr32, null, pixelData,
                                                                colorFrame.Width * colorFrame.BytesPerPixel);
            }
        }

        /// <summary>
        /// Event handler for Kinect Replay's SkeletonFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Replay_SkeletonFrameReady(object sender, ReplaySkeletonFrameReadyEventArgs e)
        {
            ProcessFrame(e.SkeletonFrame);
            
        }



        /// <summary>
        /// method to process the Skeleton Frame of Record Stream
        /// </summary>
        /// <param name="frame">the Skeleton Frame of Record Stream</param>
        private void ProcessFrame(ReplaySkeletonFrame frame)
        {
            ///get the most close skeleton
            Skeleton skeleton = GetPrimarySkeleton(frame.Skeletons);
           
            if (skeleton == null || this.goalSkeleton == null)
            {
                return;
            }

            ///class from Kinect.Toolbox
            ///use to draw the skeletons of recorded
            try
            {
                skeletonDisplayManager.Draw(frame.Skeletons, false);
            }
            catch (Exception ex)
            { }

            ///start to collect recrod skeleton
            if (this.recStartCollect)
            {
                this.recordInitial.Collect(skeleton);
            }

            ///the record initial is finished
            if (this.recordInitial.IsInitialFinished())
            {
                double[] recordInitialResults = this.recordInitial.GetInitialResults();

                ///create translation class for record motion using 4 initial results
                this.recordTranslation = new Translation(recordInitialResults[0], recordInitialResults[1], recordInitialResults[2], recordInitialResults[3]);

                ///close collection
                this.recStartCollect = false;

                ///begin evaluation two motions
                this.startEvaluate = true;
            }

            if (this.startEvaluate && this.recordTranslation != null && this.goalPoistions != null)
            {

                ///get 20 positions from translation
                Position[] recordPositions = this.recordTranslation.GetNewCoordinates(skeleton);


                ///initialize the previous skeleton data at the most first time
                if (this.skeletonID == 0)
                {
                    ///save the 20 joint positions
                    for (int i = 0; i < JOINTNUMBER; i++)
                    {
                        this.firstRecordPositions[i] = recordPositions[i];
                        this.firstGoalPositions[i] = this.goalPoistions[i];
                    }
                }


                //record the ID of skeletons
                this.skeletonID += 1;

                ///evaulate pose of every skeleton
                EvaluatePose(recordPositions, this.goalPoistions);


                ///FRAMESPAN = 10
                ///calculate rotation speed at FRAMESPAN frames intervals

                ///save the fisrt skeleton positions at the most first time

                if (this.skeletonID % FRAMESPAN == 0)
                {
                    EvaluateDisplacement(recordPositions, this.goalPoistions);
                    //Console.WriteLine(this.preRecordSkeleton.Position.X);
                }

            }
        }

        /// <summary>
        /// Calculate some distances from two position array
        /// </summary>
        /// <param name="startPositions">the first position array</param>
        /// <param name="endPositions">the second position array</param>
        /// <returns>a distances array</returns>
        private double[] GetDistancesFromPositions(Position[] startPositions, Position[] endPositions)
        {
            double[] results = new double[startPositions.Length];

            for (int i = 0; i < startPositions.Length; i++)
            {
                results[i] = this.GetDistanceFromTwoPositions(startPositions[i], endPositions[i]);
            }

            return results;
        }

        /// <summary>
        /// Calculate the distance between two poistions
        /// </summary>
        /// <param name="startPosition">the first position</param>
        /// <param name="endPosition">the second position</param>
        /// <returns>a distance</returns>
        private double GetDistanceFromTwoPositions(Position startPosition, Position endPosition)
        {
            double result = 0;

            var a = endPosition.x - startPosition.x;
            var b = endPosition.y - startPosition.y;
            var c = endPosition.z - startPosition.z;

            result = Math.Sqrt(a * a + b * b + c * c);

            return result;
        }

        /// <summary>
        /// evaluate the displacement between goal skeleton and record skeleton
        /// </summary>
        /// <param name="goalSkeleton">the current Skeleton's 20 joints position</param>
        /// <param name="recordSkeleton">the record Skeleton's 20 joints position</param>
        private void EvaluateDisplacement(Position[] recordPositions, Position[] goalPoistions)
        {

            Displacement recordDisplacement = new Displacement(this.firstRecordPositions, recordPositions);
            Displacement goalDisplacement = new Displacement(this.firstGoalPositions, goalPoistions);

            var recordVectors = recordDisplacement.GetAllVectors();
            var goalVectors = goalDisplacement.GetAllVectors();

            var angles = this.GetAnglesFromVectors(recordVectors, goalVectors);

            ///distances between the current skeleton positions and the first skeleton positions
            double[] recordDistance = this.GetDistancesFromPositions(this.firstRecordPositions, recordPositions);
            double[] goalDistance = this.GetDistancesFromPositions(this.firstGoalPositions, goalPoistions);

            //the correct amount in one skeleton
            double correct = 0;

            //the acceptable error
            double error = 0.8;

            ///distance error
            double dError = 0.01;

            for (int i = 0; i < goalVectors.Length; i++)
            {
                //Console.WriteLine(angles[i]);
                if (angles[i] < error || (recordDistance[i] < dError && goalDistance[i] < dError))
                {
                    correct++;
                }
            }

            // the percentage of matching pose quaternion in current skeleton
            correct = (correct / goalVectors.Length) * 100;

            ///record the score into the Chart
            this.rotationList.Add(new KeyValuePair<int, double>(this.skeletonID, correct));

            rotationScore.Content = "" + correct;

            ///Highlight the low scores
            if (correct < 60)
            {
                rotationScore.Foreground = Brushes.Red;
            }
            else
            {
                rotationScore.Foreground = Brushes.Black;
            }

            // the total matching roation speed score in the whole movement
            this.totalRotationScore += correct;

            if (this.skeletonID > 0)
            {
                //the percentage of matching roation speed  in the whole movement
                double finalValue = (this.totalRotationScore / this.skeletonID) * FRAMESPAN;

                rotationAveScore.Content = finalValue.ToString();

                ///Highlight the low scores
                if (finalValue < 60)
                {
                    rotationAveScore.Foreground = Brushes.Red;
                }
                else
                {
                    rotationAveScore.Foreground = Brushes.Black;
                }

                //the percentage of matching dance  in the whole movement
                this.finalScore = (Convert.ToDouble(angleAveScore.Content) + finalValue) / 2;

                totalScore.Content = "" + this.finalScore;

                ///Highlight the low scores
                if (finalScore < 60)
                {
                    totalScore.Foreground = Brushes.Red;
                }
                else
                {
                    totalScore.Foreground = Brushes.Black;
                }
            }


        }

        /// <summary>
        /// evaluate the pose between goal skeleton and record skeleton
        /// </summary>
        /// <param name="goalSkeleton">the current Skeleton's 20 joints position</param>
        /// <param name="recordSkeleton">the record Skeleton's 20 joints position</param>
        private void EvaluatePose(Position[] recordPositions, Position[] goalPoistions)
        {
            Pose recordPost = new Pose(recordPositions);
            Pose goalPost = new Pose(goalPoistions);

            //the correct amount in one skeleton
            double correct = 0;

            //the acceptable error
            double error = 0.03;

            /*
            * get all quaternions by every two joint vector in one skeleton
            * */
            ArrayList recordQuaternions = recordPost.GetQuternions();
            ArrayList goalQuaternions = goalPost.GetQuternions();


            /*
            * quaternions amount = 171
            * compare quaternions in record skeleton and current skeleton
            * */
            for (int i = 0; i < recordQuaternions.Count; i++)
            {
                Quaternion recordQ = recordQuaternions[i] as Quaternion;
                Quaternion goalQ = goalQuaternions[i] as Quaternion;


                if (CompareQuaternion(recordQ, goalQ) < error)
                {
                    correct += 1;
                }
            }

            // the percentage of matching pose quaternion in current skeleton
            correct = (correct / recordQuaternions.Count) * 100;

            this.poseList.Add(new KeyValuePair<int, double>(this.skeletonID, correct));



            angleScore.Content = "" + correct;

            ///Highlight the low scores
            if (correct < 60)
            {
                angleScore.Foreground = Brushes.Red;
            }
            else
            {
                angleScore.Foreground = Brushes.Black;
            }

            this.totalPoseScore += correct;

            if (this.skeletonID > 0)
            {
                //// the percentage of matching pose quaternion in the whole movement
                double finalValue = (this.totalPoseScore / this.skeletonID);

                angleAveScore.Content = finalValue.ToString();

                ///Highlight the low scores
                if (finalValue < 60)
                {
                    angleAveScore.Foreground = Brushes.Red;
                }
                else
                {
                    angleAveScore.Foreground = Brushes.Black;
                }
            }
        }

        /// <summary>
        /// calculate 20 angles between displacement vectors of two skeleton
        /// </summary>
        /// <param name="recordVectors">displacement vectors of the record skeleton</param>
        /// <param name="goalVectors">displacement vectors of the current skeleton</param>
        /// <returns></returns>
        private double[] GetAnglesFromVectors(_20130514MotionAnalysisTeacher.Entity.Vector[] recordVectors, _20130514MotionAnalysisTeacher.Entity.Vector[] goalVectors)
        {
            double[] angles = new double[JOINTNUMBER];

            for (int i = 0; i < JOINTNUMBER; i++)
            {
                angles[i] = this.GetAngleFromTwoVector(recordVectors[i], goalVectors[i]);
            }

            return angles;
        }

        /// <summary>
        /// calculate the angle between two angles
        /// </summary>
        /// <param name="a">one vector</param>
        /// <param name="b">another vector</param>
        /// <returns>the angle of the two angles</returns>
        private double GetAngleFromTwoVector(_20130514MotionAnalysisTeacher.Entity.Vector a, _20130514MotionAnalysisTeacher.Entity.Vector b)
        {
            double result = 0;

            var molecule = a.getX() * b.getX() + a.getY() * b.getY() + a.getZ() * b.getZ();

            var aValue = this.GetValueOfVector(a);

            var bValue = this.GetValueOfVector(b);

            var denominator = aValue * bValue;
            if (denominator != 0)
            {
                result = Math.Acos(molecule / denominator);
                //result = result * (180 / Math.PI);
            }

            return result;
        }

        /// <summary>
        /// calculate the length of the vector
        /// </summary>
        /// <param name="vector">the vector</param>
        /// <returns>the length of the vector</returns>
        private double GetValueOfVector(_20130514MotionAnalysisTeacher.Entity.Vector vector)
        {
            return Math.Sqrt(vector.getX() * vector.getX() + vector.getY() * vector.getY() + vector.getZ() * vector.getZ());
        }

        /*
        /// <summary>
        /// evaluate the rotation speed between goal skeleton and record skeleton
        /// </summary>
        /// <param name="goalSkeleton">the current Skeleton</param>
        /// <param name="recordSkeleton">the record Skeleton</param>
        private void EvaluateRotaion(Skeleton goalSkeleton, Skeleton recordSkeleton)
        {
            RotationSpeed recordRotation = new RotationSpeed(this.preRecordSkeleton, recordSkeleton);
            RotationSpeed goalRotation = new RotationSpeed(this.preGoalSkeleton, goalSkeleton);

            //the correct amount in one skeleton
            double correct = 0;

            //the acceptable error
            double error = 0.004;

            
             ///get all quaternions by every two joint points in previous skeleton and current skeleton
            
            ArrayList recordQuaternions = recordRotation.GetQuaternions();
            ArrayList goalQuaternions = goalRotation.GetQuaternions();



            ///quaternions amount = 190
            ///compare quaternions in record skeleton and current skeleton
            
            for (int i = 0; i < recordQuaternions.Count; i++)
            {
                Quaternion recordQ = recordQuaternions[i] as Quaternion;
                Quaternion goalQ = goalQuaternions[i] as Quaternion;

                //double test = CompareQuaternion(recordQ, goalQ);
                //Console.WriteLine(test);

                if (CompareQuaternion(recordQ, goalQ) < error)
                {
                    correct += 1;
                }
            }

            // the percentage of matching roation speed in one skeleton
            correct = (correct / recordQuaternions.Count) * 100;

            rotationList.Add(new KeyValuePair<int, double>(this.skeletonID, correct));

            rotationScore.Content = "" + correct;

            // the total matching roation speed score in the whole movement
            this.totalRotationScore += correct;

            if (this.skeletonID > 0)
            {
                //the percentage of matching roation speed  in the whole movement
                double finalValue = (this.totalRotationScore / this.skeletonID) * FRAMESPAN;

                rotationAveScore.Content = finalValue.ToString();

                //the percentage of matching dance  in the whole movement
                this.finalScore = (Convert.ToDouble(angleAveScore.Content) + finalValue) / 2;

                totalScore.Content = "" + this.finalScore;
            }

            ///set the current skeleton as the previous skeleton of next evaluating
             
            this.preRecordSkeleton = recordSkeleton;

            this.preGoalSkeleton = goalSkeleton;
        }
*/
        /// <summary>
        /// the function of compare two Quaternions
        /// </summary>
        /// <param name="goalQ">the current quaternion</param>
        /// <param name="recordQ">the record quaternion</param>
        private double CompareQuaternion(Quaternion recordQ, Quaternion goalQ)
        {
            double result = 0;


            result = Math.Abs(Math.Sqrt(recordQ.getW() * goalQ.getW() + recordQ.getX() * goalQ.getX() + recordQ.getY() * goalQ.getY() + recordQ.getZ() * goalQ.getZ()) - 1);
            return result;
        }




        /// <summary>
        /// ///////////////////////////////////////////////////////Show Chart////////////////////////////////////////////////////////////////////////////
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (this.poseList != null && this.rotationList != null)
            {
                Chart chart1 = new Chart();
                chart1.Show();
                chart1.addPoseChart(this.poseList);
                chart1.addRotationChart(this.rotationList);
                chart1.showColumnChart();
            }
        }

        /// <summary>
        /// //////////////////////////////////////////////////////////Open test file//////////////////////////////////////////////////////////////////////////////////////
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private OpenFileDialog openFileDialog = null;

        private string testFileName = "E:\\Kinect\\data\\20130211test2.recorded";

        /// <summary>
        /// Open a test file 
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">routed event arguments</param>
        private void openFile_Click(object sender, RoutedEventArgs e)
        {
            openFileDialog = new OpenFileDialog();
            openFileDialog.FileOk += openFileDialogFileOk;

            openFileDialog.ShowDialog();  //show a file dialog
        }

        private void openFileDialogFileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string fullPathname = openFileDialog.FileName;

            //fileName.Content = fullPathname;

            testFileName = fullPathname;
        }


        ////////////////////////////////////////////////////////Gesture control//////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create class GestureManager
        /// </summary>
        private GestureManager gestureManager;

        /// <summary>
        /// record the skeleton data
        /// </summary>
        private Skeleton collectSkeleton;

        /// <summary>
        /// if start to gesture control
        /// </summary>
        private bool isGesturestarted = false;

        /// <summary>
        /// if start to collect data
        /// </summary>
        private bool startcollect = false;


        /// <summary>
        /// if start to collect data
        /// </summary>
        private bool recStartCollect = false;

        /// <summary>
        /// set the point to start to translate the whole Joint position in skeleton 
        /// </summary>
        private bool IsStartTranslation = false;

        /// <summary>
        /// statement the class Translation for current motion
        /// //Function/Translation.cs
        /// </summary>
        private Translation translation;

        /// <summary>
        /// statement the class Translation for record motion
        /// //Function/Translation.cs
        /// </summary>
        private Translation recordTranslation;

        /// <summary>
        /// Active the Gesture Control
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void gestureControl_Click(object sender, RoutedEventArgs e)
        {

            this.gestureManager = new GestureManager(this.sensor);

            ///don't evaluate before collection
            this.startEvaluate = false;

            ///if start to capture gesture
            this.isGesturestarted = true;



            this.gestureManager.CollectCoordinate += new EventHandler(BeginCollect);
        }

        /************************************************************Initial coordinate and angle************************************************************************/

        /// <summary>
        /// define the initial class for current motion
        /// \Core\MotionEvaluation
        /// </summary>
        private Initial initial;

        /// <summary>
        /// define the initial class for record motion
        /// \Core\MotionEvaluation
        /// </summary>
        private Initial recordInitial;

        /// <summary>
        /// start to collect skeleton data and Stop gesture control function 
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void BeginCollect(object sender, EventArgs e)
        {
            this.startcollect = true;

            this.recStartCollect = true;

            this.isGesturestarted = false;

            this.initial = new Initial();

            if (cs != null)
            {
                double startTime = cs.GetStartTime();
                Timer timer = new Timer();

                timer.Interval = 1000;

                timer.Enabled = true;

                timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);

                timer.AutoReset = true;
            }
        }

        private bool isProcess = false;

        private double startTime;

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (cs != null)
            {
                if (this.startTime == 0)
                {
                    this.startTime = cs.GetStartTime();
                }
                else
                {
                    double nowTime = DateTime.Now.ToOADate();

                    if (nowTime >= (startTime + LATE))
                    {
                        isProcess = true;
                    }

                }
            }

        }

        /// <summary>
        /// //////////////////////////////////////////////////////set up server///////////////////////////////////////////////////////////////////
        /// 
        /// 

        /// <summary>
        /// the flag of sending
        /// </summary>
        private bool allowSend = false;

        /// <summary>
        /// let the start sending time late about 5s
        /// </summary>
        private const double LATE = 0.00006;

        /// set up a server waiting for students
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void setUp_Click(object sender, RoutedEventArgs e)
        {
            IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());
            IPAddress _IP = null;
            foreach (IPAddress ip in ips)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    this.ipAddr.Content = ip.ToString();
                    _IP = ip;
                }
            }

            int port = Convert.ToInt32(this.port.Text);

            cs = new CreateServer(port);
           
        }

        /// <summary>
        /// SocketSetup/CreateServer
        /// </summary>
        private CreateServer cs;

    
        /// <summary>
        /// stop sending5
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stopConnect_Click(object sender, RoutedEventArgs e)
        {
            this.allowSend = false;
        }


    }
}




