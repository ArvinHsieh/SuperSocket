using System;
using System.Buffers;
using System.Collections.Specialized;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;
using SuperSocket.Server;

namespace SuperSocket.WebSocket.Server
{
    public class WebSocketSession : AppSession, IHandshakeRequiredSession
    {
        public bool Handshaked { get; internal set; }

        public HttpHeader HttpHeader { get; internal set; }

        public string Path
        {
            get { return HttpHeader.Path; }
        }

        public string SubProtocol { get; internal set; }

        internal ISubProtocolHandler SubProtocolHandler { get; set; }

        public DateTime CloseHandshakeStartTime { get; private set; }

        public event EventHandler CloseHandshakeStarted;

        internal CloseStatus CloseStatus { get; set; }

        private static readonly IPackageEncoder<WebSocketMessage> _messageEncoder = new WebSocketEncoder();

        public virtual ValueTask SendAsync(WebSocketMessage message)
        {
            return this.Channel.SendAsync(_messageEncoder, message);
        }

        public virtual ValueTask SendAsync(string message)
        {
            return SendAsync(new WebSocketMessage
            {
                OpCode = OpCode.Text,
                Message = message,
            });
        }

        public virtual ValueTask SendAsync(ReadOnlyMemory<byte> data)
        {
            return SendAsync(new WebSocketMessage
            {
                OpCode = OpCode.Binary,
                Data = new ReadOnlySequence<byte>(data),
            });
        }

        public ValueTask CloseAsync(CloseReason reason, string reasonText = null)
        {
            var closeReasonCode = (short)reason;

            var closeStatus = new CloseStatus
            {
                Reason = reason
            };

            var textEncodedLen = 0;

            if (!string.IsNullOrEmpty(reasonText))
                textEncodedLen = Encoding.UTF8.GetMaxByteCount(reasonText.Length);

            var buffer = new byte[textEncodedLen + 2];

            buffer[0] = (byte) (closeReasonCode / 256);
            buffer[1] = (byte) (closeReasonCode % 256);

            var length = 2;

            if (!string.IsNullOrEmpty(reasonText))
            {
                closeStatus.ReasonText = reasonText;
                var span = new Span<byte>(buffer, 2, buffer.Length - 2);
                length += Encoding.UTF8.GetBytes(reasonText.AsSpan(), span);
            }

            CloseStatus = closeStatus;

            CloseHandshakeStartTime = DateTime.Now;
            OnCloseHandshakeStarted();

            return SendAsync(new WebSocketMessage
            {
                OpCode = OpCode.Close,
                Data = new ReadOnlySequence<byte>(buffer, 0, length)
            });
        }

        private void OnCloseHandshakeStarted()
        {
            CloseHandshakeStarted?.Invoke(this, EventArgs.Empty);
        }

        internal void CloseWithoutHandshake()
        {
            base.CloseAsync().DoNotAwait();
        }

        public override async ValueTask CloseAsync()
        {
            if (CloseStatus != null)
            {
                await base.CloseAsync();
                return;
            }

            try
            {
                await CloseAsync(CloseReason.NormalClosure);
            }
            catch
            {

            }
        }
    }
}