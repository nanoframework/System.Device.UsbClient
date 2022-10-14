// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.

using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Device.Usb
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UsbStream"/> class.
    /// </summary>
    public sealed class UsbStream : System.IO.Stream
    {
        private static bool _streamCreated = false;

#pragma warning disable IDE0052 // required at native code
        private readonly int _streamIndex;
#pragma warning restore IDE0052 // Remove unread private members

        [System.Diagnostics.DebuggerBrowsable(Diagnostics.DebuggerBrowsableState.Never)]
        private readonly UsbDeviceEventListener _useDeviceEventListener;

        private bool _disposed;
        private int _writeTimeout = Timeout.Infinite;
        private int _readTimeout = Timeout.Infinite;

        // default threshold is 1
        private int _receivedBytesThreshold = 1;
        private int _bufferSize = 256;

        /// <summary>
        /// Event occurs when the connection state of the USB device changes.
        /// </summary>
        public event UsbDeviceConnectionChangedEventHandler UsbDeviceConnectionChanged;

        /// <summary>
        /// Gets a value indicating whether the USB device is connected or not.
        /// </summary>
        public static extern bool IsConnected
        {
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }

        /// <summary>
        /// Gets the number of bytes of data in the receive buffer.
        /// </summary>
        /// <returns>The number of bytes of data in the receive buffer.</returns>
        public extern int BytesToRead
        {
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => false;

        /// <inheritdoc/>
        public override bool CanWrite => true;

        /// <inheritdoc/>
        /// <exception cref="PlatformNotSupportedException">This is not support in .NET nanoFramework.</exception>
        public override long Length => throw new PlatformNotSupportedException();

        /// <inheritdoc/>
        /// <exception cref="PlatformNotSupportedException">This is not support in .NET nanoFramework.</exception>
        public override long Position { get => throw new PlatformNotSupportedException(); set => throw new PlatformNotSupportedException(); }

        /// <summary>
        /// Gets or sets the size of the <see cref="UsbStream"/> input buffer.
        /// </summary>
        /// <value>The size of the input buffer. The default is 256.</value>
        /// <exception cref="ArgumentOutOfRangeException">The <see cref="ReadBufferSize"/> value is less than or equal to zero.</exception>
        /// <remarks>
        /// <para>
        /// - There is only one work buffer which is used for transmission and reception.
        /// </para>
        /// <para>
        /// - When the <see cref="UsbStream"/> is <see cref="UsbClient.CreateUsbStream(Guid, string)"/> the driver will try to allocate the requested memory for the buffer. On failure to do so, an <see cref="OutOfMemoryException"/> exception will be throw and the <see cref="UsbClient.CreateUsbStream(Guid, string)"/> operation will fail.
        /// </para>
        /// </remarks>
        public int ReadBufferSize
        {
            get
            {
                return _bufferSize;
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException();
                }

                _bufferSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of milliseconds before a time-out occurs when a read operation does not finish.
        /// </summary>
        /// <exception cref="IOException">If the USB device is not connected.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The read time-out value is less than zero and not equal to <see cref="Timeout.Infinite"/>.</exception>
        public override int ReadTimeout
        {
            get => _readTimeout;

            set
            {
                CheckValidTimeout(value);

                _readTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of bytes in the internal input buffer before a <see cref="DataReceived"/> event occurs.
        /// </summary>
        /// <value>The number of bytes in the internal input buffer before a <see cref="DataReceived"/> event is fired. The default is 1.</value>
        /// <exception cref="ArgumentOutOfRangeException">The <see cref="ReceivedBytesThreshold"/> value is less than or equal
        /// to zero.</exception>
        public int ReceivedBytesThreshold
        {
            get => _receivedBytesThreshold;

            set
            {
                NativeReceivedBytesThreshold(value);
            }
        }

        /// <summary>
        /// Gets or sets the number of milliseconds before a time-out occurs when a write operation does not finish.
        /// </summary>
        /// <exception cref="IOException">If the USB device is not connected.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The read time-out value is less than zero and not equal to <see cref="Timeout.Infinite"/>.</exception>
        public override int WriteTimeout
        {
            get => _writeTimeout;

            set
            {
                CheckValidTimeout(value);

                _writeTimeout = value;
            }
        }

        internal UsbStream(
            Guid classId,
            string name)
        {
            // at this time there is support for a single instance of the UsbStream
            if (_streamCreated)
            {
                throw new InvalidOperationException();
            }

            // need to convert GUID to proper format to help processing at native end
            _streamIndex = NativeOpen(
                $"{{{classId}}}",
                name);

            _useDeviceEventListener = new UsbDeviceEventListener(this);

            _streamCreated = true;
            _disposed = false;
        }

        /// <inheritdoc/>
        ~UsbStream()
        {
            Dispose(false);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                NativeClose();

                _disposed = true;
                _streamCreated = false;
            }
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern override void Flush();

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">This <see cref="UsbStream"/> has been disposed.</exception>
        /// <exception cref="InvalidOperationException">If the USB device is not connected.</exception>
        /// <exception cref="TimeoutException">No bytes were available to read.</exception>
        /// <remarks>Device connectivity can be checked with <see cref="IsConnected"/>. </remarks>
        public override int Read(
            byte[] buffer,
            int offset,
            int count)
        {
            return NativeRead(
                buffer,
                offset,
                count);
        }

        /// <inheritdoc/>
        /// <exception cref="NotImplementedException"></exception>
        /// <exception cref="InvalidOperationException">If the USB device is not connected.</exception>
        /// <exception cref="TimeoutException">No bytes were available to read.</exception>
        public override int Read(SpanByte buffer)
        {
            return Read(
                buffer.ToArray(),
                0,
                buffer.Length);
        }

        /// <inheritdoc/>
        /// <exception cref="PlatformNotSupportedException">This is not support in .NET nanoFramework.</exception>
        public override long Seek(
            long offset,
            SeekOrigin origin) => throw new NotSupportedException();

        /// <inheritdoc/>
        /// <exception cref="PlatformNotSupportedException">This is not support in .NET nanoFramework.</exception>
        public override void SetLength(long value) => throw new PlatformNotSupportedException();

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">This <see cref="UsbStream"/> has been disposed.</exception>
        /// <exception cref="InvalidOperationException">If the USB device is not connected.</exception>
        /// <exception cref="TimeoutException">The operation did not complete before the time-out period ended.</exception>
        public override void Write(
            byte[] buffer,
            int offset,
            int count)
        {
            // developer note: check for "disposed" it's carried out at native code
            NativeWrite(
                buffer,
                offset,
                count);
        }

        private static void CheckValidTimeout(int value)
        {
            if (value < 0 && value != Timeout.Infinite)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        #region event and delegate related methods

        /// <summary>
        /// Indicates that data has been received through the <see cref="UsbClient"/> object.
        /// </summary>
        public event UsbStreamDataReceivedEventHandler DataReceived;

        internal void OnUsbDeviceConnectionChangedInternal(bool isConnected)
        {
            // fire event, if subscribed
            UsbDeviceConnectionChanged?.Invoke(this, new DeviceConnectionEventArgs(isConnected));
        }

        internal void OnUsbStreamDataReceivedInternal()
        {
            // fire event, if subscribed
            DataReceived?.Invoke(this, new UsbStreamDataReceivedEventArgs());
        }

        #endregion

        #region Native Methods

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeClose();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativeOpen(string classId, string name);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeWrite(byte[] buffer, int offset, int count);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativeRead(byte[] buffer, int offset, int count);

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal extern void NativeReceivedBytesThreshold(int value);

        #endregion
    }
}
