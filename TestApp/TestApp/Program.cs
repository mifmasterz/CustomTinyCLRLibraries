using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c;
//using GHIElectronics.TinyCLR.DUE;
//using GHIElectronics.TinyCLR.Native;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using GHIElectronics.TinyCLR.Drivers.FocalTech.FT5xx6;
using TestApp.Properties;
using System;

namespace TestApp
{
    class Program : Application
    {
        public Program(DisplayController d) : base(d)
        {
        }
        static Program app;

        static void Main()
        {
            GpioPin backlight = GpioController.GetDefault().OpenPin(SC20260.GpioPin.PA15);
            backlight.SetDriveMode(GpioPinDriveMode.Output);
            backlight.Write(GpioPinValue.High);
            var display = DisplayController.GetDefault();

            var controllerSetting = new
                GHIElectronics.TinyCLR.Devices.Display.ParallelDisplayControllerSettings
            {
                Width = 800,
                Height = 480,
                DataFormat = DisplayDataFormat.Rgb565,
                Orientation = DisplayOrientation.Degrees0, //Rotate Display.
                PixelClockRate = 24000000,
                PixelPolarity = false,
                DataEnablePolarity = false,
                DataEnableIsFixed = false,
                HorizontalFrontPorch = 16,
                HorizontalBackPorch = 46,
                HorizontalSyncPulseWidth = 1,
                HorizontalSyncPolarity = false,
                VerticalFrontPorch = 7,
                VerticalBackPorch = 23,
                VerticalSyncPulseWidth = 1,
                VerticalSyncPolarity = false,
            };

            display.SetConfiguration(controllerSetting);
            display.Enable();

            var screen = Graphics.FromHdc(display.Hdc);
            var i2cController = I2cController.FromName(SC20100.I2cBus.I2c1);
            var gpioController = GpioController.GetDefault();

            var touch = new FT5xx6Controller(i2cController.GetDevice(FT5xx6Controller.GetConnectionSettings()),gpioController.OpenPin(SC20260.GpioPin.PJ14));
            touch.Orientation = FT5xx6Controller.TouchOrientation.Degrees0; //Rotate touch coordinates.

            font = Resources.GetFont(Resources.FontResources.NinaB);
            GHIElectronics.TinyCLR.UI.OnScreenKeyboard.Font = font;
            app = new Program(display);
            touch.TouchMove += (_, e) => {
                app.InputProvider.RaiseTouch(e.X, e.Y, GHIElectronics.TinyCLR.UI.Input.TouchMessages.Move, System.DateTime.UtcNow);
            };
            touch.TouchUp += (_, e) => {
                app.InputProvider.RaiseTouch(e.X, e.Y, GHIElectronics.TinyCLR.UI.Input.TouchMessages.Up, System.DateTime.UtcNow);
            };
            touch.TouchDown += (_, e) => {
                app.InputProvider.RaiseTouch(e.X, e.Y, GHIElectronics.TinyCLR.UI.Input.TouchMessages.Down, System.DateTime.UtcNow);
            };
            app.Run(Program.CreateWindow(display));
            
        }
        private static UIElement Elements3()
        {
            var canvas = new Canvas();
            var border = new Border();

            border.SetBorderThickness(10);
            border.BorderBrush = new SolidColorBrush(Colors.Red);

            Canvas.SetLeft(border, 20);
            Canvas.SetTop(border, 20);

            var txt = new TextBox();
            txt.Mode = TextBox.TextMode.Password;
            txt.Font = font;
            txt.Text = "TinyCLR is Great!";

            border.Child = txt;
            canvas.Children.Add(border);

            return canvas;
        }

        private static UIElement Elements2()
        {
            var panel = new StackPanel(Orientation.Vertical);

            var txt1 = new TextBox()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };

            txt1.Font = font;
            txt1.SetMargin(20);
            txt1.Text = "Hello World!";

            var txt2 = new Text(font, "TinyCLR is Great!")
            {
                ForeColor = Colors.White,
                HorizontalAlignment = HorizontalAlignment.Right,
            };

            txt2.SetMargin(20);

            var rect = new GHIElectronics.TinyCLR.UI.Shapes.Rectangle(200, 10)
            {
                Fill = new SolidColorBrush(Colors.Green),
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            var slider1 = new Slider(300,50);
            slider1.SetMargin(5);
            //slider1.Height = 100;
            slider1.ValueChangedEvent += (object sender, ValueChangedEventArgs args) =>
            {
                Debug.WriteLine("val:" + args.SliderValue);
            };
            GvData = new DataGrid(400,200,30,5);
        
            GvData.AddColumn(new DataGridColumn("Time", 100));
            GvData.AddColumn(new DataGridColumn("Sensor A", 100));
            GvData.AddColumn(new DataGridColumn("Sensor B", 100));
            
            panel.Children.Add(txt1);
            panel.Children.Add(txt2);
            panel.Children.Add(rect);
            panel.Children.Add(slider1);
            panel.Children.Add(GvData);

            return panel;
        }
        static Timer timer;
        static DataGrid GvData;
        static Font font;
        private static UIElement Elements()
        {
            
            var panel = new Panel();

            var txt1 = new TextBox()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };

            txt1.Font = font;
            txt1.SetMargin(20);
            txt1.Text = "Hello World!";

            var txt2 = new Text(font, "TinyCLR is Great!")
            {
                ForeColor = Colors.White,
                HorizontalAlignment = HorizontalAlignment.Right,
            };

            txt2.SetMargin(20);

            var rect = new GHIElectronics.TinyCLR.UI.Shapes.Rectangle(200, 10)
            {
                Fill = new SolidColorBrush(Colors.Green),
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            var slider1 = new Slider();
            slider1.Width = 300;

            //slider1.SetMargin(20);
            slider1.ValueChangedEvent += (object sender, ValueChangedEventArgs args)=>
            {
                Debug.WriteLine("val:" + args.SliderValue);
            };
            panel.Children.Add(txt1);
            panel.Children.Add(txt2);
            panel.Children.Add(rect);
            panel.Children.Add(slider1);

            return panel;
        }

     

        private static Window CreateWindow(DisplayController display)
        {
            var window = new Window
            {
                Height = (int)display.ActiveConfiguration.Height,
                Width = (int)display.ActiveConfiguration.Width
            };

            window.Background = new LinearGradientBrush
                (Colors.Blue, Colors.Teal, 0, 0, window.Width, window.Height);
           
            window.Visibility = Visibility.Visible;
            window.Child = Elements2();
            Random rnd = new Random();
            int counter = 0;
            if (GvData != null)
            {
                timer = new Timer((object o) =>
                {
                    Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(1), _ =>
                    {
                        //insert to db
                        var item = new DataGridItem(new object[] { DateTime.Now.ToString("HH:mm:ss"), $"{(20 + rnd.Next() * 50).ToString("n2")}C", $"{(rnd.Next() * 1000).ToString("n2")}L" });
                        //add data to grid
                        GvData.AddItem(item);
                        GvData.Invalidate();
                        if (counter++ > 4)
                        {
                            counter = 0;
                            GvData.Clear();
                        }
                        return null;
                    }, null);

                }, null, 1000, 1000);
            }
            return window;
        }
    }
}
