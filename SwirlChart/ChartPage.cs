using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace SwirlChart;

/// <summary>
/// The ActSwirl2 "Chart" screen: swirl angle against generator load, with the swirl
/// axis inverted so 0 deg is at the top and 160 deg at the bottom.
/// </summary>
public class ChartPage : ContentPage
{
    private const double Inset = 12.0;

    private readonly GraphicsView view;

    public ChartPage()
    {
        Title = "Chart";
        BackgroundColor = Color.FromArgb("#f4f6f9");

        view = new GraphicsView
        {
            Drawable = new SwirlCurveDrawable(),
            BackgroundColor = Colors.White,
        };

        Content = new Frame
        {
            Margin = Inset,
            Padding = 2,
            CornerRadius = 8,
            BorderColor = Color.FromArgb("#CCCCCC"),
            BackgroundColor = Colors.White,
            HasShadow = false,
            Content = view,
        };

        // A GraphicsView has no intrinsic size, and inside a Frame it is not reliably
        // constrained by Fill alone - it ends up measured far wider than the window.
        // Size it from the page instead so the plot always matches the visible area.
        SizeChanged += OnPageSizeChanged;
    }

    private void OnPageSizeChanged(object sender, EventArgs e)
    {
        if (Width <= 0 || Height <= 0)
            return;

        view.WidthRequest = Math.Max(120.0, Width - (Inset * 2) - 8);
        view.HeightRequest = Math.Max(120.0, Height - (Inset * 2) - 8);
        view.Invalidate();
    }
}

/// <summary>
/// Plots <see cref="SwirlModel"/> over the full 0-120 MW range in the style of the
/// original chart: inverted swirl axis, grid, and a heavy red curve.
/// </summary>
public class SwirlCurveDrawable : IDrawable
{
    private const double MaxSwirl = 160.0;
    private const double SwirlLabelStep = 10.0;
    private const double SwirlGridStep = 20.0;
    private const double LoadStep = 10.0;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Colors.White;
        canvas.FillRectangle(dirtyRect);

        float w = dirtyRect.Width, h = dirtyRect.Height;
        float font = Math.Max(8f, Math.Min(14f, Math.Min(w / 48f, h / 40f)));
        float left = dirtyRect.Left + font * 7.2f;
        float right = dirtyRect.Right - font * 2.0f;
        float top = dirtyRect.Top + font * 3.2f;
        float bottom = dirtyRect.Bottom - font * 5.6f;

        if (right - left < 40f || bottom - top < 40f)
            return;

        float X(double mw) => left + (float)(mw / SwirlModel.MaxLoadMW) * (right - left);
        float Y(double deg) => top + (float)(deg / MaxSwirl) * (bottom - top);

        // Title.
        canvas.FontColor = Colors.Black;
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontSize = font * 1.45f;
        canvas.DrawString("Swirl Chart  -   MS9001E", left, dirtyRect.Top, right - left, top - dirtyRect.Top,
            HorizontalAlignment.Center, VerticalAlignment.Center);
        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontSize = font;

        // Grid.
        canvas.StrokeSize = 1;
        canvas.StrokeColor = Colors.Black;
        for (double mw = LoadStep; mw < SwirlModel.MaxLoadMW; mw += LoadStep)
            canvas.DrawLine(X(mw), top, X(mw), bottom);
        for (double d = SwirlGridStep; d < MaxSwirl; d += SwirlGridStep)
            canvas.DrawLine(left, Y(d), right, Y(d));

        // Curve, sampled straight from the model so the plot cannot drift from it.
        canvas.StrokeColor = Colors.Red;
        canvas.StrokeSize = Math.Max(2f, font * 0.26f);
        canvas.StrokeLineJoin = LineJoin.Round;
        canvas.StrokeLineCap = LineCap.Round;
        var path = new PathF();
        path.MoveTo(X(0), Y(SwirlModel.GetSwirlFromLoad(0)));
        for (double mw = 0.5; mw <= SwirlModel.MaxLoadMW + 1e-9; mw += 0.5)
            path.LineTo(X(mw), Y(SwirlModel.GetSwirlFromLoad(mw)));
        canvas.DrawPath(path);

        // Border last so the curve and grid cannot overhang it.
        canvas.StrokeColor = Colors.Black;
        canvas.StrokeSize = 2;
        canvas.DrawRectangle(left, top, right - left, bottom - top);

        // Swirl labels down the left, 0 at the top.
        canvas.FontColor = Colors.Black;
        for (double d = SwirlLabelStep; d <= MaxSwirl; d += SwirlLabelStep)
        {
            canvas.DrawString(((int)d).ToString(), left - font * 3.4f, Y(d) - font,
                font * 3.0f, font * 2f, HorizontalAlignment.Right, VerticalAlignment.Center);
        }

        // Load labels along the bottom.
        for (double mw = LoadStep; mw <= SwirlModel.MaxLoadMW; mw += LoadStep)
        {
            canvas.DrawString(((int)mw).ToString(), X(mw) - font * 1.5f, bottom + font * 0.2f,
                font * 3f, font * 1.8f, HorizontalAlignment.Center, VerticalAlignment.Center);
        }

        // Axis titles, in the gutter left of the numbers.
        float gutter = left - dirtyRect.Left - font * 3.6f;
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.DrawString("Swirl", dirtyRect.Left, (top + bottom) / 2f - font * 1.2f, gutter, font * 1.6f,
            HorizontalAlignment.Center, VerticalAlignment.Center);
        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.DrawString("Deg", dirtyRect.Left, (top + bottom) / 2f + font * 0.4f, gutter, font * 1.6f,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontSize = font * 1.3f;
        canvas.DrawString("MW", left, bottom + font * 2.3f, right - left, font * 2f,
            HorizontalAlignment.Center, VerticalAlignment.Center);
        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontSize = font;

        // Footnote.
        canvas.DrawString("Swirl is counter-clockwise", dirtyRect.Left, bottom + font * 2.0f,
            left - dirtyRect.Left + font * 10f, font * 1.5f, HorizontalAlignment.Left, VerticalAlignment.Center);
        canvas.DrawString("looking with flow", dirtyRect.Left, bottom + font * 3.5f,
            left - dirtyRect.Left + font * 10f, font * 1.5f, HorizontalAlignment.Left, VerticalAlignment.Center);
    }
}
