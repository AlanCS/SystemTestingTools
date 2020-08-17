namespace SystemTestingTools
{
    /// <summary>
    /// Configuration of the interception
    /// </summary>
    public class InterceptionConfiguration
    {
        /// <summary>
        /// Where stubs can be found / recorded
        /// Will use to "App_Data/SystemTestingTools" if null 
        /// </summary>
        public FolderAbsolutePath RootStubsFolder = null;

        /// <summary>
        /// Expose stubs and recordings (if configured) as this URL, to enable this feature you need to call IApplicationBuilder.ExposeStubsForDirectoryBrowsing()
        /// </summary>
        public RelativeUri ExposeStubsAs = "Stubs";

        /// <summary>
        /// Forward headers that start with this prefix to downstream systems, so we can use it to drive stubbing downstream
        /// </summary>
        public string ForwardHeadersPrefix = "SystemTestingTools";
    }
}
