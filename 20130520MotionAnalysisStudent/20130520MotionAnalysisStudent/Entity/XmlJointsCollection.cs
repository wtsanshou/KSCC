using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace _20130520MotionAnalysisStudent.Entity
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
