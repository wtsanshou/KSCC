using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using _20130520MotionAnalysisStudent.Entity;

namespace _20130520MotionAnalysisStudent.Core.MotionEvaluation
{
    class Displacement
    {

        /// <summary>
        /// the number of skeleton joints
        /// </summary>
        const int JOINTNUMBER = 20;

        /*
         * for current position
         * the joints position in current skeleton stream
        */
        private Position[] curPoistions = new Position[JOINTNUMBER];


        /*
         * for previous position
         * previous positions which was used to compare with the current positions
        */
        private Position[] prePoistions = new Position[JOINTNUMBER];


        /// <summary>
        /// Calculate vectors of displacement between two skeleton
        /// </summary>
        /// <param name="preSkeleton">the previous skeleton positions</param>
        /// <param name="curSkeleton">the current skeleton positions</param>
        public Displacement(Position[] preSkeleton, Position[] curSkeleton)
        {

            this.prePoistions = preSkeleton;
            this.curPoistions = curSkeleton;


        }

        public Vector[] GetAllVectors()
        {
            Vector[] results = new Vector[JOINTNUMBER];
            Position p = new Position(0, 0, 0);




            //Console.WriteLine(c.getX());

            for (int i = 0; i < JOINTNUMBER; i++)
            {
                results[i] = new Vector(this.prePoistions[i], this.curPoistions[i]);

            }

            return results;
        }
    }
}
