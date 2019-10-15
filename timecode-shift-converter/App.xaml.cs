using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace timecode_shift_converter
{
    /// <summary>
    /// Logika interakcji dla klasy App.xaml
    /// </summary>
    public partial class App : Application
    {

        public string startFile = "00:00:00:00";
        public string startBeta = "00:00:00:00";
        public string timecodeFile = "00:00:00:00";
        public string timecodeBeta = "00:00:00:00";

        private Timecode[] TcTable = new Timecode[3];

        public void showResult()
        {
            if (valuesAreCorrect())
            {
                for (int i = 0; i <= 3; i++)
                {

                }
            }
        }

        
    }
}
