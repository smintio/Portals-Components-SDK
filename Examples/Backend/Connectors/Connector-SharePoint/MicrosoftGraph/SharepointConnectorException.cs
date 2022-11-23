using System;

namespace SmintIo.Portals.Connector.SharePoint.MicrosoftGraph
{
    internal class SharepointConnectorException : Exception
    {
        public SharepointConnectorException(string s) : base(s)
        {
        }
    }
}