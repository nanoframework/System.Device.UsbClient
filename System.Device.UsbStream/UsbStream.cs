// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;

namespace System.Device.UsbClient
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UsbStream"/> class.
    /// </summary>
    public sealed class UsbStream : System.IO.Stream
    {
        private readonly int _streamIndex;
        private bool _disposed;

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

        internal UsbStream(string name)
        {
            _streamIndex = NativeOpen(name);

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
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException();
            }

            return NativeRead(buffer, offset, count);
        }

        /// <inheritdoc/>
        /// <exception cref="NotImplementedException"></exception>
        public override int Read(SpanByte buffer)
        {
            return Read(buffer.ToArray(), 0, buffer.Length);
        }

        /// <inheritdoc/>
        /// <exception cref="PlatformNotSupportedException">This is not support in .NET nanoFramework.</exception>
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        /// <inheritdoc/>
        /// <exception cref="PlatformNotSupportedException">This is not support in .NET nanoFramework.</exception>
        public override void SetLength(long value) => throw new PlatformNotSupportedException();

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">This <see cref="UsbStream"/> has been disposed.</exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException();
            }

            NativeWrite(buffer, offset, count);
        }

        #region Native Methods

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeClose();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativeOpen(string name);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeWrite(byte[] buffer, int offset, int count);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativeRead(byte[] buffer, int offset, int count);

        #endregion
    }
}
