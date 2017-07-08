using System.Net;
using Newtonsoft.Json;
using SlackConnector.Connections.Responses;
using SlackConnector.Exceptions;

namespace SlackConnector.Connections.Clients
{
    internal class ResponseVerifier : IResponseVerifier
    {
        public void VerifyResponse(StandardResponse response)
        {
            if (!response.Ok)
            {
                string neededMsg = response.Needed!=null?  $", needed:'{response.Needed}'" : string.Empty;
                throw new CommunicationException($"Error occured while posting message '{response.Error}'{neededMsg}");
            }
        }
    }
}