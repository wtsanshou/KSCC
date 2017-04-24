using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace _20130514MotionAnalysisTeacher
{
    [Serializable]
    [XmlRoot("GatherData")]
    public class GatherData
    {
        /// <summary>
        /// 数据
        /// </summary>
        [XmlArray("Data")]
        public string[] Data
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        [XmlElement("ModuleSht")]
        public string ModuleSht
        {
            get;
            set;
        }
    }
}
