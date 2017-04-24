using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace _20130514MotionAnalysisTeacher.Entity
{
    [Serializable]
    [XmlRoot("XmlJointsCollection")]
    public class XmlJointsCollection
    {
        [XmlArray("Joints")]
        public XmlOneJoint[] Joints
        {
            get;
            set;
        }
    }
}
