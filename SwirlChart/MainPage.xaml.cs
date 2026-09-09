using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Globalization;

namespace SwirlChart;

public partial class MainPage : ContentPage
{
    private readonly SwirlDrawable swirlDrawable = new();

    // Guards the Entry <-> Slider two-way sync so the two handlers do not fight.
    private bool syncing;

    public MainPage()
    {
        InitializeComponent();
        SwirlGraphics.Drawable = swirlDrawable;
        UpdateSwirlFromLoad(LoadSlider.Value);
    }

    private async void OnAboutClicked(object sender, EventArgs e)
    {
        await DisplayAlert("About", "Swirl Chart\nDeveloped With Love and Candy \nEngineer: Abbas :)", "OK");
    }

    private void OnLoadSliderChanged(object sender, ValueChangedEventArgs e)
    {
        if (syncing)
            return;

        syncing = true;
        double load = Math.Round(e.NewValue, 1);
        LoadEntry.Text = load.ToString("0.#", CultureInfo.InvariantCulture);
        UpdateSwirlFromLoad(load);
        syncing = false;
    }

    private void OnLoadEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        if (syncing)
            return;

        if (string.IsNullOrWhiteSpace(e.NewTextValue))
        {
            ResultLabel.Text = $"Enter a load between {SwirlModel.MinLoadMW:F0} and {SwirlModel.MaxLoadMW:F0} MW.";
            return;
        }

        if (!double.TryParse(e.NewTextValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double load))
        {
            ResultLabel.Text = "Invalid load input.";
            return;
        }

        load = Math.Clamp(Math.Round(load, 1), SwirlModel.MinLoadMW, SwirlModel.MaxLoadMW);

        syncing = true;
        // Setting the slider may or may not raise ValueChanged (it does not when the
        // typed value clamps onto the current value), so refresh unconditionally.
        LoadSlider.Value = load;
        UpdateSwirlFromLoad(load);
        syncing = false;
    }

    private void UpdateSwirlFromLoad(double loadMW)
    {
        double swirlAngle = SwirlModel.GetSwirlFromLoad(loadMW);
        swirlDrawable.SetSwirlAngle(swirlAngle);
        SwirlGraphics.Invalidate();
        ResultLabel.Text = $"Load: {loadMW:F1} MW → Swirl angle: {swirlAngle:F2}°";
    }
}

/// <summary>
/// GE Frame 9E (MS9001E) combustion swirl angle as a function of generator load.
/// Piecewise-linear interpolation between the calibration knots
/// (0, 160), (17, 160), (41, 143), (57, 84), (80, 68), (114, 0), (120, 0).
/// </summary>
public static class SwirlModel
{
    public const double MinLoadMW = 0.0;
    public const double MaxLoadMW = 120.0;

    /// <summary>
    /// Swirl angle in degrees for the given generator active power in MW.
    /// Loads outside 0..120 MW are clamped to the ends of the curve.
    /// </summary>
    public static double GetSwirlFromLoad(double loadMW)
    {
        if (double.IsNaN(loadMW))
            return 0.0;

        double p = Math.Clamp(loadMW, MinLoadMW, MaxLoadMW);

        if (p < 17.0) return 160.0;                                  // FSNL plateau
        if (p < 41.0) return 160.0 - (17.0 / 24.0) * (p - 17.0);     // m = -0.708333 deg/MW
        if (p < 57.0) return 143.0 - (59.0 / 16.0) * (p - 41.0);     // m = -3.687500 deg/MW
        if (p < 80.0) return 84.0 - (16.0 / 23.0) * (p - 57.0);      // m = -0.695652 deg/MW
        if (p < 114.0) return 68.0 - 2.0 * (p - 80.0);               // m = -2.000000 deg/MW
        return 0.0;                                                  // base load, axial flow
    }
}

/// <summary>
/// Draws the exhaust thermocouple ring, the combustor can ring, and the back-trace
/// lines that map each T/C to the can it sees, for the current swirl angle.
/// </summary>
public class SwirlDrawable : IDrawable
{
    private const int TcCount = 24;
    private const int CanCount = 14;

    private const double CanSpacing = 360.0 / CanCount;   // 25.714286 deg
    private const double TcSpacing = 360.0 / TcCount;     // 15.000000 deg

    // Screen bearings are measured clockwise from 12 o'clock. 0 deg sits on the
    // midpoint between Can 14 / Can 1 and between T/C 24 / T/C 1, so both rings are
    // offset by half of their own spacing. Both are numbered counter-clockwise.
    private const double CanZeroOffset = CanSpacing / 2.0;  // 12.857143 deg
    private const double TcZeroOffset = TcSpacing / 2.0;    // 7.500000 deg

    private double swirlAngle;

    public void SetSwirlAngle(double angle) => swirlAngle = angle;

    /// <summary>Screen bearing of a 1-based combustor can.</summary>
    private static double CanAngle(int can) =>
        Mod(-(CanZeroOffset + (can - 1) * CanSpacing), 360.0);

    /// <summary>Screen bearing of a 1-based exhaust thermocouple.</summary>
    private static double TcAngle(int tc) =>
        Mod(-(TcZeroOffset + (tc - 1) * TcSpacing), 360.0);

    /// <summary>
    /// Back-traces a thermocouple to its suspect can: Angle_Can = (Angle_TC - swirl) mod 360,
    /// then snaps to the nearest can position.
    /// </summary>
    private static int BackTraceCan(double tcAngle, double swirl)
    {
        double target = Mod(tcAngle - swirl, 360.0);
        int index = (int)Math.Round(Mod(-target - CanZeroOffset, 360.0) / CanSpacing);
        return Mod(index, CanCount) + 1;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Color.FromArgb("#f4f6f9");
        canvas.FillRectangle(dirtyRect);

        float centerX = dirtyRect.Center.X;
        float centerY = dirtyRect.Center.Y;
        float radius = Math.Min(centerX, centerY) - 20;
        float canRadius = radius * 0.8f;

        canvas.StrokeSize = 1;
        canvas.StrokeColor = Colors.Black;
        canvas.DrawCircle(centerX, centerY, radius);

        canvas.StrokeColor = Color.FromArgb("#CCCCCC");
        canvas.DrawCircle(centerX, centerY, canRadius);

        // Back-trace lines first so the markers and labels draw on top of them.
        canvas.StrokeColor = Color.FromArgb("#0078D4");
        for (int tc = 1; tc <= TcCount; tc++)
        {
            double tcBearing = TcAngle(tc);
            int can = BackTraceCan(tcBearing, swirlAngle);

            PointF from = Polar(centerX, centerY, radius, tcBearing);
            PointF to = Polar(centerX, centerY, canRadius, CanAngle(can));
            canvas.DrawLine(from.X, from.Y, to.X, to.Y);
        }

        canvas.FillColor = Colors.Gray;
        for (int can = 1; can <= CanCount; can++)
        {
            PointF p = Polar(centerX, centerY, canRadius, CanAngle(can));
            canvas.DrawString($"C{can}", p.X - 12, p.Y - 10, 24, 20,
                HorizontalAlignment.Center, VerticalAlignment.Center);
        }

        for (int tc = 1; tc <= TcCount; tc++)
        {
            PointF p = Polar(centerX, centerY, radius, TcAngle(tc));
            canvas.FillColor = Colors.Red;
            canvas.FillCircle(p.X, p.Y, 5);
            canvas.DrawString($"{tc}", p.X + 5, p.Y + 5, 30, 20,
                HorizontalAlignment.Left, VerticalAlignment.Top);
        }
    }

    private static PointF Polar(float cx, float cy, float r, double bearingDeg)
    {
        double rad = DegToRad(bearingDeg - 90);
        return new PointF(cx + (float)(r * Math.Cos(rad)), cy + (float)(r * Math.Sin(rad)));
    }

    private static double DegToRad(double deg) => deg * Math.PI / 180.0;

    private static double Mod(double a, double m)
    {
        double r = a % m;
        return r < 0 ? r + m : r;
    }

    private static int Mod(int a, int m)
    {
        int r = a % m;
        return r < 0 ? r + m : r;
    }
}
