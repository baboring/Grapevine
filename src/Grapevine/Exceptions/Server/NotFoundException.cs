using System;
using Grapevine.Interfaces.Server;

namespace Grapevine.Exceptions.Server
{
    /// <summary>
    /// 
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }

    /// <summary>
    /// 
    /// </summary>
    public class FileNotFoundException : NotFoundException
    {
        public FileNotFoundException(IHttpContext context) : base($"{context.Request.PathInfo} not found") { }
    }

    /// <summary>
    /// Thrown when no routes are found for the provided context.
    /// </summary>
    public class RouteNotFoundException : NotFoundException
    {
        public RouteNotFoundException(IHttpContext context) : base($"Route Not Found For {context.Request.HttpMethod} {context.Request.PathInfo}") { }
    }
}
