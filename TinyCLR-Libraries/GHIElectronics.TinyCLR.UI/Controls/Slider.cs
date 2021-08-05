using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Media.Imaging;

namespace GHIElectronics.TinyCLR.UI.Controls {
   public class Slider : ContentControl, IDisposable {
        private BitmapImage button_Up ;
        private BitmapImage button_Down ;
        public ushort Alpha { get; set; } = 0xC8;
        public string Name { get; set; } = "Slider1";
        private bool dragging = false;
        private string direction = SliderDirection.Horizontal;
        private Rectangle knob;
        private int knobSize;
        private int lineSize;
        private int tickInterval;
        private double tickSize;
        private int snapInterval;
        private double snapSize;
        private double pixelsPerValue;
        private double min;
        private double max;
        private double valueSlider;
        //private Thread touchThread;
        private void InitResource() {
            this.button_Up = BitmapImage.FromGraphics(Graphics.FromImage(Resources.GetBitmap(Resources.BitmapResources.Button_Up)));
            this.button_Down = BitmapImage.FromGraphics(Graphics.FromImage(Resources.GetBitmap(Resources.BitmapResources.Button_Down)));
        }
    /// <summary>
    /// Creates a new Slider component.
    /// </summary>
    /// <param name="name">Name</param>
    /// <param name="alpha">Alpha</param>
    /// <param name="x">X-axis position.</param>
    /// <param name="y">Y-axis position.</param>
    /// <param name="width">Width</param>
    /// <param name="height">Height</param>
    public Slider() {
            // int x, int y, int width, int height
            this.InitResource();
            /*
            this.Name = name;
            this.Alpha = alpha;

            X = x;
            Y = y;
            Width = width;
            Height = height;
            */
            this.Height = 50;
            this.Width = 100;
            this.Background = new SolidColorBrush(Colors.Gray);
            // Default
            this.KnobSize = 20;
            this.SnapInterval = 10;
            this.TickInterval = 10;
            this.Minimum = 0;
            this.Maximum = 100;
            this.Value = 0;

        }

        /// <summary>
        /// Value changed event.
        /// </summary>
        public event ValueChangedEventHandler ValueChangedEvent;

        /// <summary>
        /// Triggers a value changed event.
        /// </summary>
        /// <param name="sender">Object associated with the event.</param>
        public void TriggerValueChangedEvent(object sender) => this.ValueChangedEvent?.Invoke(sender, new ValueChangedEventArgs(this.Value));

        private void RenderKnob(Point localPoint) {
            //var localPoint = new Point(globalPoint.X - Rect.X, globalPoint.Y - Rect.Y);

            if (this.direction == SliderDirection.Horizontal) {
                var half = this.knob.Width / 2;
                var maxX = this.Width - this.knob.Width;

                if (localPoint.X < half)
                    this.knob.X = 0;
                else if (localPoint.X - half > maxX)
                    this.knob.X = maxX;
                else
                    this.knob.X = localPoint.X - half;

                var interval = (int)System.Math.Round(this.knob.X / this.snapSize);

                if (this.SnapInterval > 0)
                    this.knob.X = (int)(interval * this.snapSize);

                this.Value = this.knob.X / this.pixelsPerValue;
            }
            else {
                var half = this.knob.Height / 2;
                var maxY = this.Height - this.knob.Height;

                Debug.WriteLine(localPoint.ToString());

                if (localPoint.Y < half)
                    this.knob.Y = 0;
                else if (localPoint.Y - half > maxY)
                    this.knob.Y = maxY;
                else
                    this.knob.Y = localPoint.Y - half;

                var interval = (int)System.Math.Round(this.knob.Y / this.snapSize);

                if (this.SnapInterval > 0)
                    this.knob.Y = (int)(interval * this.snapSize);

                this.Value = this.max - (this.knob.Y / this.pixelsPerValue);
            }
            if (this.Parent != null)
                this.Invalidate();
        }

        /// <summary>
        /// Renders the Button onto it's parent container's graphics.
        /// </summary>
        //public override void Render() {
        public override void OnRender(DrawingContext dc) {
            var x = 0;// Rect.X;
            var y = 0;// Rect.Y;

            // HACK: To prevent image/color retention.
            //dc.DrawRectangle(Rect, TinyCLR2.Glide.Ext.Colors.Black, 255);
            var brush = new SolidColorBrush(Colors.Black);
            var pen = new Media.Pen(Colors.Black);
            //dc.DrawRectangle(brush, pen,0,0,this.Width, this.Height);

            //((Window)Parent).FillRect(Rect); //!update
            var thickPen = new Media.Pen(this.tickColor, 1);
            if (this.direction == SliderDirection.Horizontal) {
                var lineY = y + (this.Height / 2);
                var offsetX = this.knob.Width / 2;
                var knobY = this.Height - this.knob.Height;
                int tickX;
                var tickHeight = (int)System.Math.Ceiling(this.Height * 0.05);
               
                dc.DrawLine(thickPen,  x + offsetX, lineY, x + offsetX + this.lineSize, lineY);

                if (this.TickInterval > 1) {
                    for (var i = 0; i < this.TickInterval + 1; i++) {
                        tickX = x + offsetX + (int)(i * this.tickSize);
                        dc.DrawLine(thickPen, tickX, y, tickX, y + tickHeight);
                    }
                }

                if (this.dragging)
                    dc.Scale9Image(x + this.knob.X, y + knobY, this.knob.Width, this.knob.Height, this.button_Down, 5, 5, 5, 5, this.Alpha);
                else                                                                              
                    dc.Scale9Image(x + this.knob.X, y + knobY, this.knob.Width, this.knob.Height, this.button_Up, 5, 5, 5, 5, this.Alpha);
            }
            else {
                var lineX = x + (this.Width / 2);
                var offsetY = this.knob.Height / 2;
                var knobX = this.Width - this.knob.Width;
                int tickY;
                var tickWidth = (int)System.Math.Ceiling(this.Width * 0.05);

                dc.DrawLine(thickPen, lineX, y + offsetY, lineX, y + offsetY + this.lineSize);

                if (this.TickInterval > 1) {
                    for (var i = 0; i < this.TickInterval + 1; i++) {
                        tickY = y + offsetY + (int)(i * this.tickSize);
                        dc.DrawLine(thickPen, x, tickY, x + tickWidth, tickY);
                    }
                }

                if (this.dragging)
                    dc.Scale9Image(x + knobX, y + this.knob.Y, this.knob.Width, this.knob.Height, this.button_Down, 5, 5, 5, 5, this.Alpha);
                else
                    dc.Scale9Image(x + knobX, y + this.knob.Y, this.knob.Width, this.knob.Height, this.button_Up, 5, 5, 5, 5, this.Alpha);
            }
        }
        bool RectContains(Rectangle rect, Point point) {

            if(point.X>rect.X && point.X<rect.X+rect.Width && point.Y > rect.Y && point.Y < rect.Y + rect.Height) {
                return true;
            }
            return false;
        }

        bool Contains(Point point) {
            if(point.X>0 && point.Y>0 && point.X<this.Width && point.Y < this.Height) {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Handles the touch down event.
        /// </summary>
        /// <param name="e">Touch event arguments.</param>
        /// <returns>Touch event arguments.</returns>
        //public override TouchEventArgs OnTouchDown(TouchEventArgs e) {
        protected override void OnTouchDown(TouchEventArgs e) {
            // Global coordinates to local coordinates
            //var localPoint = new Point(e.Point.X - Rect.X, e.Point.Y - Rect.Y);
            e.GetPosition(this, 0, out var ax, out var ay);
            var localPoint = new Point(ax, ay);

            if (this.RectContains(this.knob,localPoint)) {
                this.dragging = true;
                if(this.Parent!=null)
                    this.Invalidate();

                //disable touch thread
                /*
                if (this.touchThread == null || (this.touchThread != null && !this.touchThread.IsAlive)) {
                    //GlideTouch.IgnoreAllEvents = true; //!update
                    this.touchThread = new Thread(this.TouchThread) {
                        Priority = ThreadPriority.Highest
                    };
                    this.touchThread.Start();
                }
                */

                //e.StopPropagation(); //!update
                //return e;
            }

            if (this.Contains(localPoint)) {
                this.RenderKnob(localPoint);
                //e.StopPropagation(); //!update
            }

            //return e;
        }

        /// <summary>
        /// Handles the touch up event.
        /// </summary>
        /// <param name="e">Touch event arguments.</param>
        /// <returns>Touch event arguments.</returns>
        //public override TouchEventArgs OnTouchUp(TouchEventArgs e) {
        protected override void OnTouchUp(TouchEventArgs e) {
            //Point localPoint = new Point(e.Point.X - Rect.X, e.Point.Y - Rect.Y);
            e.GetPosition(this, 0, out var ax, out var ay);
            var localPoint = new Point(ax, ay);

            if (this.RectContains( this.knob,localPoint)) {
                if (this.dragging) {
                    this.dragging = false;
                    if(this.Parent!=null)
                        this.Invalidate();
                    //e.StopPropagation(); //!update
                }
            }
            else {
                if (this.dragging) {
                    this.dragging = false;
                    if (this.Parent != null)
                        this.Invalidate();
                }
            }
            //return e;
        }

        /// <summary>
        /// Handles the touch move event.
        /// </summary>
        /// <param name="e">Touch event arguments.</param>
        /// <returns>Touch event arguments.</returns>
        //public override TouchEventArgs OnTouchMove(TouchEventArgs e) {
        protected override void OnTouchMove(TouchEventArgs e) {
            e.GetPosition(this, 0, out var ax, out var ay);
            var localPoint = new Point(ax, ay);
            if (this.dragging)
                this.RenderKnob(localPoint);
            //return e;
        }

        private void Init() {
            if (direction == SliderDirection.Horizontal) {
                knob = new Rectangle(0, 0, knobSize, (int)((double)Height / 1.2));
                lineSize = Width - knobSize;
            }
            else {
                knob = new Rectangle(0, 0, (int)((double)Width / 1.2), knobSize);
                lineSize = Height - knobSize;
            }

            if (tickInterval < 0)
                tickInterval = 0;
            else if (tickInterval > lineSize)
                tickInterval = lineSize;

            if (tickInterval > 0)
                tickSize = (double)lineSize / TickInterval;
            else
                tickSize = (double)lineSize / lineSize;

            if (snapInterval < 0)
                snapInterval = 0;
            else if (snapInterval > lineSize)
                snapInterval = lineSize;

            if (snapInterval > 0)
                snapSize = (double)lineSize / snapInterval;
            else
                snapSize = (double)lineSize / lineSize;

            if (max > 0)
                pixelsPerValue = lineSize / (max - min);
        }

        /*
        private void TouchThread() {
            // These are used for the touch up event
            var lastX = 0;
            var lastY = 0;

            // These store the current X and Y
            var x = 0;
            var y = 0;

            // Keeps track of whether the panel was touched or not
            var isTouched = false;

            // Create touch inputs that are used as arguments
            var touches = new TouchInput[] { new TouchInput() };
            
            // Begin touch panel polling
            while (dragging) {
                GlideTouch.GetLastTouchPoint(ref x, ref y);
                
                if (x >= 0 && x <= Glide.LCD.Width && y >= 0 && y <= Glide.LCD.Height) {
                    if (isTouched == false) {
                        // Touch down
                        touches[0].X = x;
                        touches[0].Y = y;
                        GlideTouch.RaiseTouchDownEvent(this, new TouchEventArgs(touches));

                        lastX = x;
                        lastY = y;
                        isTouched = true;
                    }
                    else {
                        // Filter finger movements to avoid spamming
                        if (System.Math.Abs(x - lastX) > 2 || System.Math.Abs(y - lastY) > 2) {
                            // Touch move
                            touches[0].X = x;
                            touches[0].Y = y;
                            GlideTouch.RaiseTouchMoveEvent(this, new TouchEventArgs(touches));

                            lastX = x;
                            lastY = y;
                        }
                    }
                }
                else {
                    if (isTouched == true) {
                        // Touch up
                        touches[0].X = lastX;
                        touches[0].Y = lastY;
                        GlideTouch.RaiseTouchUpEvent(this, new TouchEventArgs(touches));

                        isTouched = false;
                    }
                }

                Thread.Sleep(30);
            }
            
            // Allow other threads to run so we dont get double touch events
            // once the message box closes.
            Thread.Sleep(0);

            //GlideTouch.IgnoreAllEvents = false; //!update
        }*/

        /// <summary>
        /// Direction of the slider; horizontal or vertical.
        /// </summary>
        public string Direction {
            get => this.direction;
            set {
                if (value != this.direction) {
                    this.direction = value.ToLower();
                    this.Init();
                }
            }
        }

        /// <summary>
        /// Size of the knob.
        /// </summary>
        public int KnobSize {
            get => this.knobSize;
            set {
                this.knobSize = value;
                this.Init();
            }
        }

        /// <summary>
        /// Maximum value.
        /// </summary>
        public double Maximum {
            get => this.max;
            set {
                this.max = value;
                this.Init();
            }
        }

        /// <summary>
        /// Minimum value.
        /// </summary>
        public double Minimum {
            get => this.min;
            set {
                this.min = value;
                this.Init();
            }
        }

        /// <summary>
        /// Increment by which the value is increased or decreased as the user slides the knob.
        /// </summary>
        public int SnapInterval {
            get => this.snapInterval;
            set {
                this.snapInterval = value;
                this.Init();
            }
        }

        /// <summary>
        /// Tick mark spacing relative to the maximum value.
        /// </summary>
        public int TickInterval {
            get => this.tickInterval;
            set {
                this.tickInterval = value;
                this.Init();
            }
        }

        /// <summary>
        /// Gets or sets the current value.
        /// </summary>
        public double Value {
            get => this.valueSlider;
            set {
                var oldValue = this.valueSlider;
                this.valueSlider = value;

                if (oldValue != this.valueSlider)
                    this.TriggerValueChangedEvent(this);

                if (this.direction == SliderDirection.Horizontal)
                    this.knob.X = (int)((this.valueSlider - this.min) * this.pixelsPerValue);
                else
                    this.knob.Y = this.lineSize - (int)((this.valueSlider - this.min) * this.pixelsPerValue);
            }
        }
        private bool disposed;

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {

                this.button_Down.graphics.Dispose();
                this.button_Down.graphics.Dispose();
                /*
                if (this.Parent != null && this.isTouchParentAssigned) {
                    this.Parent.TouchUp -= this.OnParentTouchUp;
                }
                */
                this.disposed = true;
            }
        }

        ~Slider() {
            this.Dispose(false);
        }
        /// <summary>
        /// Tick color.
        /// </summary>
        public Media.Color tickColor = Colors.Black;
    }

    /// <summary>
    /// The orientation of the Slider component.
    /// </summary>
    public struct SliderDirection {
        /// <summary>
        /// Horizontal
        /// </summary>
        public const string Horizontal = "horizontal";

        /// <summary>
        /// Vertical
        /// </summary>
        public const string Vertical = "vertical";
    }
}
