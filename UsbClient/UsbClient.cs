// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using nanoFramework.Runtime.Events;

namespace System.Device.UsbClient
{
    /// <summary>
    /// Indicates a change in the connection state of the USB device.
    /// </summary>
    /// <param name="sender">Specifies the object that sent the USB device connection state changed event. </param>
    /// <param name="e">Contains the connection changed event arguments. </param>
    public delegate void UsbDeviceConnectionChangedEventHandler(object sender, DeviceConnectionEventArgs e);

    /// <summary>
    /// Provides static methods for the creation of USB client instances.
    /// </summary>
    public class UsbClient
    {
        /// <summary>
        /// Gets a value indicating whether the USB device is connected or not.
        /// </summary>
        public extern bool IsConnected
        {
            [MethodImpl(MethodImplOptions.InternalCall)]
            get; 
        }

        static UsbClient()
        {
            UsbDeviceEventListener useDeviceEventListener = new ();

            EventSink.AddEventProcessor(EventCategory.Usb, useDeviceEventListener);
            EventSink.AddEventListener(EventCategory.Usb, useDeviceEventListener);
        }

        /// <summary>
        /// Event occurs when the connection state of the USB device changes.
        /// </summary>
        public static event UsbDeviceConnectionChangedEventHandler UsbDeviceConnectionChanged;

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

        internal class UsbDeviceEventListener : IEventProcessor, IEventListener
        {
            public BaseEvent ProcessEvent(uint data1, uint data2, DateTime time)
            {
                return new UsbDeviceEvent()
                {
                    // Data1 is packed by PostManagedEvent, so we need to unpack the high word.
                    EventType = (UsbEventType)(data1 & 0xFF),

                    // Data2 - Low 8 bits are the interface index
                    InterfaceIndex = (ushort)(data2 & 0xff)
                };
            }

            public void InitializeForEventSource()
            {
                // This method has to exist
            }

            public bool OnEvent(BaseEvent ev)
            {
                if (ev is UsbDeviceEvent myEvent)
                {
                    switch (myEvent.EventType)
                    {
                        case UsbEventType.DeviceDisconnected:

                            // fire event, if subscribed
                            UsbDeviceConnectionChanged?.Invoke(this, new DeviceConnectionEventArgs(false));

                            break;

                        case UsbEventType.DeviceConnected:

                            // fire event, if subscribed
                            UsbDeviceConnectionChanged?.Invoke(this, new DeviceConnectionEventArgs(true));

                            break;
                    }
                }

                return true;
            }
        }
    }
}
