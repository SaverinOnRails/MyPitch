using Avalonia.Controls;
using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyPitch.Controls;

internal class NumericUpDownCustom : NumericUpDown
{
    protected override void OnGotFocus(GotFocusEventArgs e)
    {
       // base.OnGotFocus(e);
    }
}
