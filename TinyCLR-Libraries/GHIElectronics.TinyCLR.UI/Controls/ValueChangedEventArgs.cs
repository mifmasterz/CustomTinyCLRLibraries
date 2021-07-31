using System;

namespace GHIElectronics.TinyCLR.UI.Controls {
    public class ValueChangedEventArgs : EventArgs {
        //public readonly int PreviousSelectedIndex;
        public readonly double SliderValue;

        public ValueChangedEventArgs(double newValue) => this.SliderValue = newValue;
    }
}


