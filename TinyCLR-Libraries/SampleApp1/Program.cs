using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;

namespace SampleApp1
{
    class Program
    {
        static void Main()
        {
            var backlight = GpioController.GetDefault().OpenPin(SC20260.GpioPin.PA15);
            backlight.SetDriveMode(GpioPinDriveMode.Output);
            backlight.Write(GpioPinValue.High);
            var displayController = DisplayController.GetDefault();

            // Enter the proper display configurations
            displayController.SetConfiguration(new ParallelDisplayControllerSettings {
                Width = 480,
                Height = 272,
                DataFormat = DisplayDataFormat.Rgb565,
                Orientation = DisplayOrientation.Degrees0, //Rotate display.
                PixelClockRate = 10000000,
                PixelPolarity = false,
                DataEnablePolarity = false,
                DataEnableIsFixed = false,
                HorizontalFrontPorch = 2,
                HorizontalBackPorch = 2,
                HorizontalSyncPulseWidth = 41,
                HorizontalSyncPolarity = false,
                VerticalFrontPorch = 2,
                VerticalBackPorch = 2,
                VerticalSyncPulseWidth = 10,
                VerticalSyncPolarity = false,
            });

            displayController.Enable();
            var myPic = new byte[480 * 272 * 2];

            for (var i = 0; i < myPic.Length; i++) {
                myPic[i] = (byte)(((i % 2) == 0) ? 
                    ((i / 4080) & 0b00000111) << 5 : i / 32640);
            }

            displayController.DrawString("\f");
            displayController.DrawBuffer(0, 0, 0, 0, 480, 272, 480, myPic, 0);
            displayController.DrawString("GHI Electronics\n");
            displayController.DrawString("Low Level Display Demo.");

            for (var x = 20; x < 459; x++) {
                displayController.DrawPixel(x, 50, 0xF800);     //Color is 31,0,0 (RGB565).
                displayController.DrawPixel(x, 51, 0xF800);
            }

            Thread.Sleep(-1);
        }
    }
}
