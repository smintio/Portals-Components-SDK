using System.IO;

namespace SmintIo.Portals.Connector.SharePoint.Helpers
{
    /// <summary>
    /// The Path helper class.
    /// </summary>
    public static class PathHelpers
    {
        /// <summary>
        /// File extensions of known documents formats.
        /// </summary>
        public static readonly string[] KnownDocumentFormats =
        {
            ".csv",
            ".log",
            ".sql",
            ".xml",
            ".html",
            ".htm",
            ".js",
            ".css",
            ".ods",
            ".odt",
            ".xls",
            ".xlr",
            ".xlsm",
            ".xlsx",
            ".msg",
            ".ini",
            ".cfg",
            ".doc",
            ".docx",
            ".ppt",
            ".pptx",
            ".potx",
            ".pdf",
            ".pages",
            ".odp",
            ".rtf",
            ".txt",
            ".tex",
            ".wpd",
            ".c",
            ".class",
            ".cpp",
            ".cs",
            ".dtd",
            ".fla",
            ".h",
            ".java",
            ".lua",
            ".pl",
            ".py",
            ".sh",
            ".sln",
            ".swift",
            ".vb",
            ".vcxproj",
            ".xcodeproj",
            ".asp",
            ".aspx",
            ".cer",
            ".cfm",
            ".csr",
            ".jsp",
            ".php",
            ".rss",
            ".xhtml"
        };

        /// <summary>
        /// Returns the invalid file name chars.
        /// </summary>
        public static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();
    }
}
