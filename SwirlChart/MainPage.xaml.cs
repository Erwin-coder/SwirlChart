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

    private async void OnInfo1Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new InfoPage(InfoContent.Info1Title, InfoContent.Info1));
    }

    private async void OnInfo2Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new InfoPage(InfoContent.Info2Title, InfoContent.Info2));
    }

    private async void OnChartClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ChartPage());
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
/// Draws the MS9001E swirl chart in the style of the original ActSwirl2 tool: a fixed
/// ring of 14 combustor cans on stalks outside the turbine circle, and 24 exhaust
/// thermocouple spokes that rotate with the swirl angle. The suspect can for a given
/// T/C is read off the spoke that points at it.
/// </summary>
public class SwirlDrawable : IDrawable
{
    private const int TcCount = 24;
    private const int CanCount = 14;

    private const double CanSpacing = 360.0 / CanCount;   // 25.714286 deg
    private const double TcSpacing = 360.0 / TcCount;     // 15.000000 deg

    // Screen bearings run clockwise from 12 o'clock. 0 deg sits on the midpoint between
    // Can 14 and Can 1, so the can ring carries a half-pitch offset; the T/C ring does
    // not - T/C 24 sits exactly on 0 deg at zero swirl, matching ActSwirl2.
    private const double CanZeroOffset = CanSpacing / 2.0;  // 12.857143 deg

    // Radii as multiples of the turbine circle. Measured by pixel analysis of the
    // original ActSwirl2 chart (9E, 0 MW, 160 deg), where the circle is R = 179.5 px.
    private const float CanRingRadius = 1.475f;    // can bubble centres
    private const float CanBubbleRadius = 0.163f;
    private const float TcSpokeInner = 0.66f;      // spokes leave a wide clear hub
    private const float TcSpokeOuter = 1.44f;      // and reach almost to the cans
    private const float TcLabelRadius = 1.71f;
    private const float DrawnExtent = 1.95f;

    private double swirlAngle;

    public void SetSwirlAngle(double angle) => swirlAngle = angle;

    /// <summary>Screen bearing of a 1-based combustor can. The can ring never moves.</summary>
    private static double CanAngle(int can) =>
        Mod(-(CanZeroOffset + (can - 1) * CanSpacing), 360.0);

    /// <summary>Screen bearing of a 1-based thermocouple, carried round by the swirl.</summary>
    private double TcAngle(int tc) => Mod(swirlAngle - TcSpacing * tc, 360.0);

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Color.FromArgb("#f4f6f9");
        canvas.FillRectangle(dirtyRect);

        float centerX = dirtyRect.Center.X;
        float centerY = dirtyRect.Center.Y;
        float radius = (Math.Min(centerX, centerY) - 4) / DrawnExtent;
        float font = Math.Max(9f, radius * 0.085f);

        canvas.FontSize = font;

        // Turbine circle.
        canvas.FillColor = Colors.White;
        canvas.FillCircle(centerX, centerY, radius);

        // T/C spokes, drawn before the circle outline so the outline stays unbroken.
        canvas.StrokeSize = 1;
        canvas.StrokeColor = Colors.Red;
        for (int tc = 1; tc <= TcCount; tc++)
        {
            double bearing = TcAngle(tc);
            PointF a = Polar(centerX, centerY, radius * TcSpokeInner, bearing);
            PointF b = Polar(centerX, centerY, radius * TcSpokeOuter, bearing);
            canvas.DrawLine(a.X, a.Y, b.X, b.Y);
        }

        canvas.StrokeSize = Math.Max(2f, radius * 0.022f);
        canvas.StrokeColor = Colors.Black;
        canvas.DrawCircle(centerX, centerY, radius);

        // Cans: a stalk out from the circle to a numbered bubble.
        float bubble = radius * CanBubbleRadius;
        for (int can = 1; can <= CanCount; can++)
        {
            double bearing = CanAngle(can);
            PointF root = Polar(centerX, centerY, radius, bearing);
            PointF hub = Polar(centerX, centerY, radius * CanRingRadius, bearing);

            canvas.StrokeSize = Math.Max(3f, radius * 0.040f);
            canvas.StrokeColor = Colors.Black;
            canvas.DrawLine(root.X, root.Y, hub.X, hub.Y);

            canvas.FillColor = Colors.White;
            canvas.FillCircle(hub.X, hub.Y, bubble);
            canvas.StrokeSize = Math.Max(2f, radius * 0.021f);
            canvas.DrawCircle(hub.X, hub.Y, bubble);

            canvas.FontColor = Colors.Black;
            DrawCentred(canvas, $"#{can}", hub, bubble * 2f);
        }

        // T/C numbers, outside the can ring.
        canvas.FontColor = Colors.Red;
        for (int tc = 1; tc <= TcCount; tc++)
        {
            PointF p = Polar(centerX, centerY, radius * TcLabelRadius, TcAngle(tc));
            DrawCentred(canvas, $"<{tc}>", p, font * 3.4f);
        }

        DrawCentred(canvas, "MS9001E", new PointF(centerX, centerY), font * 7f);
    }

    private static void DrawCentred(ICanvas canvas, string text, PointF at, float box)
    {
        canvas.DrawString(text, at.X - box / 2f, at.Y - box / 2f, box, box,
            HorizontalAlignment.Center, VerticalAlignment.Center);
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
}
