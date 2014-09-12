﻿#region --- License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2006-2008 the OpenTK Team.
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using OpenTK;
using OpenTK.Input;


public static class Utilities
{
    /// <summary>
    /// Converts a System.Drawing.Color to a System.Int32.
    /// </summary>
    /// <param name="c">The System.Drawing.Color to convert.</param>
    /// <returns>A System.Int32 containing the R, G, B, A values of the
    /// given System.Drawing.Color in the Rbga32 format.</returns>
    public static int ColorToRgba32(Color c)
    {
        return (int)((c.A << 24) | (c.B << 16) | (c.G << 8) | c.R);
    }

    /// <summary>
    /// Sets the window title to the name of the sample.
    /// </summary>
    /// <param name="window"></param>
    public static void SetWindowTitle(GameWindow window)
    {
        window.Title = String.Format("OpenTK | {0} {1}: {2}", 1, 2, 3);
        //window.Icon = OpenTK.Examples.Properties.Resources.Game;
    }

    /// <summary>
    /// Sets the window title to the name of the sample.
    /// </summary>
    /// <param name="window"></param>
    public static void SetWindowTitle(System.Windows.Forms.Form window)
    {
        window.Text = String.Format("OpenTK | {0} {1}: {2}", 1, 2, 3);
        //window.Icon = OpenTK.Examples.Properties.Resources.Game;
    }
}
