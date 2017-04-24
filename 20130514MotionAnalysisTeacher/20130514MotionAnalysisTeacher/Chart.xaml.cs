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
using System.Windows.Shapes;

namespace _20130514MotionAnalysisTeacher
{
    /// <summary>
    /// Interaction logic for Chart.xaml
    /// </summary>
    public partial class Chart : Window
    {
        public Chart()
        {
            InitializeComponent();
            poseList = new List<KeyValuePair<int, double>>();

            rotationList = new List<KeyValuePair<int, double>>();
        }

        private List<KeyValuePair<int, double>> poseList;

        private List<KeyValuePair<int, double>> rotationList;

        public void showColumnChart()
        {

            poseChart.DataContext = this.poseList;

            rotationChart.DataContext = this.rotationList;
        }

        public void addPoseChart(List<KeyValuePair<int, double>> vl)
        {

            this.poseList.AddRange(vl);
        }

        public void addRotationChart(List<KeyValuePair<int, double>> vl)
        {

            this.rotationList.AddRange(vl);
        }
    }
}
