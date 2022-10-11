// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.

namespace System.Device.Usb
{
    /// <summary>
    /// Contains argument values for USB device connection events.
    /// </summary>
    public class DeviceConnectionEventArgs : EventArgs
    {
        private readonly bool _isConnected;

        /// <summary>
        /// Gets a value indicating whether the USB device is connected or not.
        /// </summary>
        public bool IsConnected => _isConnected;

        internal DeviceConnectionEventArgs(bool isConnected) => _isConnected = isConnected;
    }
}
