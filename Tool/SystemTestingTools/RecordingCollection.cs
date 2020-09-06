using System.Collections.Generic;

namespace SystemTestingTools
{
    /// <summary>
    /// Collection of recording files
    /// </summary>
    public static class RecordingCollection
    {
        /// <summary>
        /// The list of recordings available for usage
        /// </summary>
        public static List<Recording> Recordings = new List<Recording>();

        internal static void LoadFrom(FolderAbsolutePath folder)
        {
            Recordings.AddRange(Constants.GlobalRecordingManager.GetRecordings(folder));
        }
    }
}
