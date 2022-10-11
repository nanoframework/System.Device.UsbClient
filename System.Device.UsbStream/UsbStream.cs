// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.

using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Device.UsbClient
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UsbStream"/> class.
    /// </summary>
    public sealed class UsbStream : System.IO.Stream
    {
#pragma warning disable IDE0052 // required at native code
        private readonly int _streamIndex;
#pragma warning restore IDE0052 // Remove unread private members

        private bool _disposed;
        private int _writeTimeout = Timeout.Infinite;
        private int _readTimeout = Timeout.Infinite;

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
            // need to convert GUID to proper format to help processing at native end
            _streamIndex = NativeOpen(
                $"{{{classId}}}",
                name);

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
            }
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern override void Flush();

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">This <see cref="UsbStream"/> has been disposed.</exception>
        /// <exception cref="InvalidOperationException">If the USB device is not connected.</exception>
        /// <remarks>Device connectivity can be checked with </remarks>
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

        #region Native Methods

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeClose();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativeOpen(string classId, string name);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeWrite(byte[] buffer, int offset, int count);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativeRead(byte[] buffer, int offset, int count);

        #endregion
    }
}
