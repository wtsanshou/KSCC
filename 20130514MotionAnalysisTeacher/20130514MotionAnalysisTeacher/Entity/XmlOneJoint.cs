using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace _20130514MotionAnalysisTeacher.Entity
{
    [Serializable]
    
    public class XmlOneJoint
    {
        [XmlAttribute("x")]
        public double x
        {
            get;
            set;
        }

        [XmlAttribute("y")]
        public double y
        {
            get;
            set;
        }

        [XmlAttribute("z")]
        public double z
        {
            get;
            set;
        }
    }
}
