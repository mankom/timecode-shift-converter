using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace timecode_shift_converter
{
    /// <summary>
    /// Immutable timecode.
    /// </summary>
    /// <remarks>
    /// Represents a timecode in hh:mm:ss:ff format.  Rolls over at the 24 hour mark.
    /// </remarks>
    [Serializable]
    public class Timecode
    {
        private static readonly Regex TimecodeRegex = new Regex(@"^(?<hours>\d{1,2}):(?<minutes>\d{1,2}):(?<seconds>\d{1,2}):(?<frames>\d{1,3})$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        private int totalFrames;
        private short frameRate;

        public Timecode(int totalFrames, short frameRate = 25)
        {
            if (frameRate <= 0) throw new ArgumentOutOfRangeException("frameRate");

            TotalFrames = totalFrames % FramesPerDay(frameRate);
            FrameRate = frameRate;
            TotalSeconds = (float)TotalFrames / (float)FrameRate; ;
            Frames = (int)totalFrames % frameRate;
            Seconds = (int)TotalFrames / FrameRate / 60 % 60;
            Minutes = (int)TotalFrames / FrameRate / 60 % 60;
            Hours = (int)TotalFrames / FrameRate / 60 / 60;
        }

        public Timecode(int hours, int minutes, int seconds, int frames, short frameRate = 25)
        {
            frames += seconds * frameRate;
            frames += minutes * 60 * frameRate;
            frames += hours * 3600 * frameRate;

            TotalFrames = frames % FramesPerDay(frameRate);
            FrameRate = frameRate;
            Hours = hours;
            Minutes = minutes;
            Seconds = seconds;
        }

        /// <summary>
        /// The hours segment of the timecode.
        /// </summary>
        public int Hours { get; private set; }

        /// <summary>
        /// The minutes segment of the timecode.
        /// </summary>
        public int Minutes { get; private set; }

        /// <summary>
        /// The seconds segment of the timecode.
        /// </summary>
        public int Seconds { get; private set; }

        /// <summary>
        /// The frames segment of the timecode.
        /// </summary>
        public int Frames { get; private set; }

        /// <summary>
        /// The total number of frames for this timecode.
        /// </summary>
        public int TotalFrames{get;  private set; }

        /// <summary>
        /// The framerate of this timecode.
        /// </summary>
        public short FrameRate { get; private set; }

        /// <summary>
        /// The total number of seconds in this timecode.
        /// </summary>
        /// <returns></returns>
        public float TotalSeconds { get; private set; }

        public override string ToString()
        {
            return string.Format("{0}:{1}:{2}:{3}",
                PadTimecodeUnit(Hours),
                PadTimecodeUnit(Minutes),
                PadTimecodeUnit(Seconds),
                PadTimecodeUnit(Frames));
        }

        /// <summary>
        /// Pads a number for display in a timecode string.
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="places"></param>
        /// <returns></returns>
        private static string PadTimecodeUnit(int unit, int places = 2)
        {
            return unit.ToString().PadLeft(2, '0');
        }

        /// <summary>
        /// The total number of frames in one day, for this timecode's framerate.
        /// </summary>
        /// <returns></returns>
        private int OneDay()
        {
            return FramesPerDay(this.frameRate);
        }

        /// <summary>
        /// Parses a timecode string of the format "hh:mm:ss:ff".
        /// </summary>
        /// <param name="timecodeStr"></param>
        /// <param name="framerate"></param>
        /// <returns></returns>
        public static Timecode Parse(string timecodeStr, short framerate = 25)
        {
            GroupCollection captureGroups = TimecodeRegex.Match(timecodeStr).Groups;

            int hours = int.Parse(captureGroups["hours"].Value);
            int minutes = int.Parse(captureGroups["minutes"].Value);
            int seconds = int.Parse(captureGroups["seconds"].Value);
            int frames = int.Parse(captureGroups["frames"].Value);

            return new Timecode(hours, minutes, seconds, frames);
        }

        /// <summary>
        /// Adds two timecodes.
        /// </summary>
        /// <param name="timecodeA"></param>
        /// <param name="timecodeB"></param>
        /// <returns></returns>
        public static Timecode operator +(Timecode timecodeA, Timecode timecodeB)
        {
            if (timecodeA.frameRate != timecodeB.frameRate)
                throw new InvalidOperationException("Cannot add two timecodes with different framerates.");

            return new Timecode(timecodeA.totalFrames + timecodeB.totalFrames, timecodeA.frameRate);
        }

        /// <summary>
        /// Adds a given number of frames to the timecode.
        /// </summary>
        /// <param name="timecodeA"></param>
        /// <param name="frames"></param>
        /// <returns></returns>
        public static Timecode operator +(Timecode timecodeA, int frames)
        {
            return new Timecode(timecodeA.totalFrames + frames, timecodeA.frameRate);
        }

        /// <summary>
        /// Subtracts two timecodes.
        /// </summary>
        /// <param name="timecodeA"></param>
        /// <param name="timecodeB"></param>
        /// <returns></returns>
        /// <remarks>If the second timecode has fewer frames than the first, it is presumed to have rolled over the 24 hour mark and therefore be conceptually greater than the first.</remarks>
        public static Timecode operator -(Timecode timecodeA, Timecode timecodeB)
        {
            if (timecodeA.frameRate != timecodeB.frameRate)
                throw new InvalidOperationException("Cannot subtract two timecodes with different framerates.");

            int totalFramesA = timecodeA.totalFrames;
            int totalFramesB = timecodeB.totalFrames;

            if (totalFramesA < totalFramesB)
                totalFramesA += timecodeA.OneDay();

            return new Timecode(totalFramesA - totalFramesB, timecodeA.frameRate);
        }

        /// <summary>
        /// Subtracts frames from the timecode.
        /// </summary>
        /// <param name="timecodeA"></param>
        /// <param name="frames"></param>
        /// <returns></returns>
        public static Timecode operator -(Timecode timecodeA, int frames)
        {
            return new Timecode(timecodeA.totalFrames - frames, timecodeA.frameRate);
        }

        public static bool operator <(Timecode timecodeA, Timecode timecodeB)
        {
            return timecodeA.TotalSeconds < timecodeB.TotalSeconds;
        }

        public static bool operator <=(Timecode timecodeA, Timecode timecodeB)
        {
            return timecodeA.TotalSeconds <= timecodeB.TotalSeconds;
        }

        public static bool operator >(Timecode timecodeA, Timecode timecodeB)
        {
            return timecodeA.TotalSeconds > timecodeB.TotalSeconds;
        }

        public static bool operator >=(Timecode timecodeA, Timecode timecodeB)
        {
            return timecodeA.TotalSeconds >= timecodeB.TotalSeconds;
        }

        private static int FramesPerDay(short framesPerSecond)
        {
            return framesPerSecond * 60 * 60 * 24;
        }
    }
}
