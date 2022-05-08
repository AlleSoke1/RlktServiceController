using System;
using System.Threading;
using System.Threading.Tasks;

namespace LiteNetwork.Server.Abstractions
{
    /// <summary>
    /// Provides a basic abstraction to manage a TCP server.
    /// </summary>
    public interface ILiteServer : IDisposable
    {
        /// <summary>
        /// Gets a boolean value that indicates if the server is running.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Gets the server options.
        /// </summary>
        LiteServerOptions Options { get; }
        
        /// <summary>
        /// Starts the server.
        /// </summary>
        /// <returns></returns>
        Task StartAsync();

        /// <summary>
        /// Starts the server with a cancellation token.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task StartAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Stops the server.
        /// </summary>
        /// <returns></returns>
        Task StopAsync();
    }
}
