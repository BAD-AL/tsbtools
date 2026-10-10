using System;

namespace TSBTool2
{
    /// <summary>
    /// The TSB2/TSB3 player form's and the converter's Auto Update Sim. Both games now use the models
    /// fitted to Tecmo's own sim values (TsbSimAutoUpdater); the old formula file is gone.
    /// AutoUpdateSimData (the player form) also works out TSB2's team SimData; AutoUpdatePlayerSimData (the
    /// converter) leaves team values as they are.
    /// </summary>
    public class TSBXSimAutoUpdater
    {
        public static string AutoUpdateSimData(string text, TSBTool.TSBContentType gameVersion)
        {
            CheckVersion(gameVersion);
            return TsbSimAutoUpdater.AutoUpdateSimData(text, gameVersion);
        }

        private static void CheckVersion(TSBTool.TSBContentType gameVersion)
        {
            if (gameVersion != TSBTool.TSBContentType.TSB2 && gameVersion != TSBTool.TSBContentType.TSB3)
                throw new ArgumentException("TSBXSimAutoUpdater: Incorrect version " + gameVersion.ToString());
        }

        public static string AutoUpdatePlayerSimData(string text, TSBTool.TSBContentType gameVersion)
        {
            switch (gameVersion)
            {
                case TSBTool.TSBContentType.TSB2:
                case TSBTool.TSBContentType.TSB3:
                    return TsbSimAutoUpdater.AutoUpdatePlayerSimData(text, gameVersion);
                default:
                    throw new ArgumentException("TSBXSimAutoUpdater: Incorrect version " + gameVersion.ToString());
            }
        }
    }
}
