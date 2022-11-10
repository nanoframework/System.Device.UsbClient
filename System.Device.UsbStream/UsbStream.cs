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

        private int _bufferSize = 256;

        /// <summary>
        /// Event occurs when the connection state of the USB device changes.
        /// </summary>
        public event UsbDeviceConnectionChangedEventHandler UsbDeviceConnectionChanged;

        /// <summary>
        /// Gets a value indicating whether the USB device is connected or not.
        /// </summary>
        public extern bool IsConnected
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
        /// <exception cref="PlatformNotSupportedException">This is not support in .NET nanoFramework.</exception>
        public override void Flush() => throw new PlatformNotSupportedException();

        /// <summary>Reads a number of bytes from the USB device and writes those bytes into a byte array at the specified offset.</summary>
        /// <param name="buffer">The byte array to write the input to.</param>
        /// <param name="offset">The <paramref name="offset"/> in <paramref name="buffer"/> at which to write the bytes.</param>
        /// <param name="count">The maximum number of bytes to read. Fewer bytes are read if <paramref name="count"/> is greater than the number of bytes in the input buffer.</param>
        /// <exception cref="ObjectDisposedException">This <see cref="UsbStream"/> has been disposed.</exception>
        /// <exception cref="InvalidOperationException">If the USB device is not connected.</exception>
        /// <exception cref="TimeoutException">No bytes were available to read.</exception>
        /// <exception cref="ArgumentNullException">The buffer passed is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="offset"/> or <paramref name="count"/> parameters are outside a valid region of the <paramref name="buffer"/> being passed. Either <paramref name="offset"/> or <paramref name="count"/> is less than zero.</exception>
        /// <exception cref="ArgumentException"><paramref name="offset"/> plus <paramref name="count"/> is greater than the length of the <paramref name="buffer"/>.</exception>
        /// <remarks>Device connectivity can be checked with <see cref="IsConnected"/>. </remarks>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern override int Read(
            byte[] buffer,
            int offset,
            int count);

        /// <summary>Reads a number of bytes from the USB device and writes those bytes into a byte array.</summary>
        /// <param name="buffer">The byte array to write the input to.</param>
        /// <exception cref="ObjectDisposedException">This <see cref="UsbStream"/> has been disposed.</exception>
        /// <exception cref="InvalidOperationException">If the USB device is not connected.</exception>
        /// <exception cref="TimeoutException">No bytes were available to read.</exception>
        /// <exception cref="ArgumentNullException">The buffer passed is <see langword="null"/>.</exception>
        /// <remarks>Device connectivity can be checked with <see cref="IsConnected"/>. </remarks>
        public int Read(byte[] buffer)
        { 
            return Read(
                buffer,
                0,
                buffer.Length);
        }

        /// <inheritdoc/>
        /// <exception cref="NotImplementedException"></exception>
        public override int Read(SpanByte buffer) => throw new NotImplementedException();

        /// <inheritdoc/>
        /// <exception cref="PlatformNotSupportedException">This is not support in .NET nanoFramework.</exception>
        public override long Seek(
            long offset,
            SeekOrigin origin) => throw new NotSupportedException();

        /// <inheritdoc/>
        /// <exception cref="PlatformNotSupportedException">This is not support in .NET nanoFramework.</exception>
        public override void SetLength(long value) => throw new PlatformNotSupportedException();

        /// <summary>Writes a specified number of bytes to the USB device using data from a buffer.</summary>
        /// <param name="buffer">The byte array that contains the data to write to the USB device.</param>
        /// <param name="offset">The zero-based byte offset in the <paramref name="buffer"/> parameter at which to begin copying bytes to the USB device.</param>
        /// <param name="count">The number of bytes to write.</param>
        /// <exception cref="ObjectDisposedException">This <see cref="UsbStream"/> has been disposed.</exception>
        /// <exception cref="InvalidOperationException">If the USB device is not connected.</exception>
        /// <exception cref="TimeoutException">The operation did not complete before the time-out period ended.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="offset"/> or <paramref name="count"/> parameters are outside a valid region of the <paramref name="buffer"/> being passed. Either <paramref name="offset"/> or <paramref name="count"/> is less than zero.</exception>
        /// <exception cref="ArgumentException"><paramref name="offset"/> plus <paramref name="count"/> is greater than the length of the <paramref name="buffer"/>.</exception>
        // developer note: check for "disposed" it's carried out at native code
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern override void Write(
            byte[] buffer,
            int offset,
            int count);

        /// <summary>Writes the number of bytes in <paramref name="buffer"/> parameter to the USB device using data from a buffer.</summary>
        /// <param name="buffer">The byte array that contains the data to write to the USB device.</param>
        /// <exception cref="ObjectDisposedException">This <see cref="UsbStream"/> has been disposed.</exception>
        /// <exception cref="InvalidOperationException">If the USB device is not connected.</exception>
        /// <exception cref="TimeoutException">The operation did not complete before the time-out period ended.</exception>
        public void Write(byte[] buffer)
        {
            Write(
                buffer,
                0,
                buffer.Length);
        }

        private static void CheckValidTimeout(int value)
        {
            if (value < 0 && value != Timeout.Infinite)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        #region event and delegate related methods

        internal void OnUsbDeviceConnectionChangedInternal(bool isConnected)
        {
            // fire event, if subscribed
            UsbDeviceConnectionChanged?.Invoke(this, new DeviceConnectionEventArgs(isConnected));
        }

        #endregion

        #region Native Methods

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeClose();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativeOpen(string classId, string name);

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal extern void NativeReceivedBytesThreshold(int value);

        #endregion
    }
}
