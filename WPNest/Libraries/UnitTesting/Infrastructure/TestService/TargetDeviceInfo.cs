using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Silverlight.Testing.Tools
{
    public class TargetDeviceInfo
    {
        private const string WindowsPhone7Platform = "7E6B29E6-AAE1-41dc-9849-049507CBA2B0";

        private const string WindowsPhone7Device = "30F105C9-681E-420b-A277-7C086EAD8A4E";
        private const string WindowsPhone7Emulator = "5E7661DF-D928-40ff-B747-A4B1957194F9";

        public TargetDeviceInfo()
        {
            PlatformId = WindowsPhone7Platform;
            UseEmulator = true;
            Locale = 1033;
        }

        public bool CustomDevice { get; private set; }

        public bool UseEmulator
        {
            get { return DeviceId == WindowsPhone7Emulator; }

            set
            {
                if (WindowsPhone7Platform != PlatformId)
                {
                    throw new InvalidOperationException("UseEmulator cannot be set when a custom PlatformId is specified. Specify a custom DeviceId instead.");
                }

                DeviceId = (value)
                    ? WindowsPhone7Emulator
                    : WindowsPhone7Device;
            }
        }

        /// <summary>
        /// Gets or sets the platform ID to launch. This is an advanced option, <see cref="UseEmulator"/> can be used to specify the default emulator or device.
        /// </summary>
        public string PlatformId { get; set; }

        /// <summary>
        /// Gets or sets the device ID to launch. This is an advanced option, <see cref="UseEmulator"/> can be used to specify the default emulator or device.
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// Gets or sets the locale of the data store that contains device information. This is an advanced option, and can be left with it's default value.
        /// </summary>
        public int Locale { get; set; }
    }
}
