// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.

namespace System.Device.UsbClient
{
    /// <summary>
    /// Provides static methods for the creation of USB client instances.
    /// </summary>
    public class UsbClient
    {
        /// <summary>
        /// Creates an USB Stream from a WinUSB device that will use the specified name as the device description.
        /// </summary>
        /// <param name="classId"><see cref="Guid"/> for the device class that will be used by WinUSB device.</param>
        /// <param name="name">Name to be used as device description.</param>
        /// <returns>A new UsbStream that was created with the specified name.</returns>
        public static UsbStream CreateUsbStream(
            Guid classId,
            string name)
        {
            return new UsbStream(
                classId,
                name);
        }
    }
}
