using System;
using System.Collections.Generic;
using System.Text;

namespace TSBTool2
{
    public class SNES_TSB3_ScheduleHelper : SNES_ScheduleHelper
    {
        private int[] weeks = { 
            0x16F00C, 0x16F02A, 0x16F048, 0x16F066, 0x16F084, 0x16F0A2, 0x16F0C0, 0x16F0DE, 
            0x16F0FC, 0x16F11A, 0x16F138, 0x16F156, 0x16F174, 0x16F192, 0x16F1B0, 0x16F1CE, 
            0x16F1EC
        };

        public SNES_TSB3_ScheduleHelper(ITecmoTool tool):base(tool) { }

        protected override int GameLocation(int week, int gameOfweek)
        {
            int location = weeks[week];
            int retVal = location + (2 * gameOfweek);
            return retVal;
        }
    }
}
