using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;

namespace SuperSocket.Client
{
    public interface IEasyClient<TReceivePackage, TSendPackage> : IEasyClient<TReceivePackage>
        where TReceivePackage : class
    {
        ValueTask SendAsync(TSendPackage package);      
    }

    
    public interface IEasyClient<TReceivePackage>
        where TReceivePackage : class
    {
        ValueTask<bool> ConnectAsync(EndPoint remoteEndPoint, CancellationToken cancellationToken = default);

        ValueTask<bool> ConnectAsync(EndPoint remoteEndPoint, ChannelOptions options = default, CancellationToken cancellationToken = default);

        ValueTask<TReceivePackage> ReceiveAsync();

        void StartReceive();

        ValueTask SendAsync(ReadOnlyMemory<byte> data);

        ValueTask SendAsync<TSendPackage>(IPackageEncoder<TSendPackage> packageEncoder, TSendPackage package);

        bool IsConnected { get; }

        EndPoint RemoteEndPoint { get; }

        event EventHandler Closed;

        event EventHandler Connected;

        event EventHandler<ErrorEventArgs> Error;

        event PackageHandler<TReceivePackage> PackageHandler;

        ValueTask CloseAsync();
    }
}