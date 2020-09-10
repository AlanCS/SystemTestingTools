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

        /// <summary>
        /// Add recordings found in this folder to 'Recordings' property
        /// </summary>
        /// <param name="folder">the root folder where recordings will be found, deep folder search will be performed</param>
        public static void LoadFrom(FolderAbsolutePath folder)
        {            
            Recordings.AddRange(new RecordingManager().GetRecordings(folder));
        }
    }
}
