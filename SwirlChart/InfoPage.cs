using System;
using System.Collections.Generic;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace SwirlChart;

/// <summary>
/// Renders one of the ActSwirl2 reference documents from the line markup described on
/// <see cref="InfoContent"/>. Built in code rather than XAML because the layout is
/// driven entirely by the text.
/// </summary>
public class InfoPage : ContentPage
{
    private static readonly Color Accent = Color.FromArgb("#0063B1");
    private static readonly Color Body = Color.FromArgb("#333333");
    private static readonly Color Muted = Color.FromArgb("#6B7280");
    private static readonly Color Rule = Color.FromArgb("#CCCCCC");

    public InfoPage(string title, string markup)
    {
        Title = title;
        BackgroundColor = Color.FromArgb("#f4f6f9");

        var stack = new VerticalStackLayout { Padding = 20, Spacing = 2 };
        Build(stack, markup);

        Content = new ScrollView { Content = stack };
    }

    private static void Build(VerticalStackLayout stack, string markup)
    {
        string[] lines = markup.Replace("\r\n", "\n").Split('\n');
        var table = new List<string[]>();

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].TrimEnd();

            // Table rows accumulate until the run of '|' lines ends.
            if (line.StartsWith("|", StringComparison.Ordinal))
            {
                table.Add(line.Substring(1).Split('|'));
                continue;
            }
            if (table.Count > 0)
            {
                stack.Add(BuildTable(table));
                table.Clear();
            }

            if (line.Length == 0)
            {
                stack.Add(new BoxView { HeightRequest = 10, Color = Colors.Transparent });
            }
            else if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                stack.Add(Text(line.Substring(3), 16, FontAttributes.Bold, Body, new Thickness(0, 12, 0, 2)));
            }
            else if (line.StartsWith("# ", StringComparison.Ordinal))
            {
                stack.Add(Text(line.Substring(2), 19, FontAttributes.Bold, Accent, new Thickness(0, 20, 0, 4)));
                stack.Add(new BoxView { HeightRequest = 1, Color = Rule, Margin = new Thickness(0, 0, 0, 6) });
            }
            else if (line.StartsWith("> ", StringComparison.Ordinal))
            {
                stack.Add(Text(line.Substring(2), 13, FontAttributes.Italic, Muted, new Thickness(0, 6, 0, 0)));
            }
            else if (line.StartsWith("-- ", StringComparison.Ordinal))
            {
                stack.Add(Bullet("–", line.Substring(3), 44, Body, FontAttributes.None));
            }
            else if (line.StartsWith("- ", StringComparison.Ordinal))
            {
                stack.Add(Bullet("–", line.Substring(2), 24, Body, FontAttributes.None));
            }
            else if (line.StartsWith("+ ", StringComparison.Ordinal))
            {
                stack.Add(Bullet("•", line.Substring(2), 4, Body, FontAttributes.Bold));
            }
            else
            {
                stack.Add(Text(line, 15, FontAttributes.None, Body, new Thickness(0, 2, 0, 2)));
            }
        }

        if (table.Count > 0)
            stack.Add(BuildTable(table));
    }

    private static Label Text(string s, double size, FontAttributes attr, Color color, Thickness margin) =>
        new Label { Text = s, FontSize = size, FontAttributes = attr, TextColor = color, Margin = margin };

    private static View Bullet(string marker, string s, double indent, Color color, FontAttributes attr)
    {
        var grid = new Grid
        {
            Margin = new Thickness(indent, 2, 0, 2),
            ColumnSpacing = 6,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(14) },
                new ColumnDefinition { Width = GridLength.Star },
            },
        };
        grid.Add(new Label { Text = marker, FontSize = 15, TextColor = color, FontAttributes = attr }, 0, 0);
        grid.Add(new Label { Text = s, FontSize = 15, TextColor = color, FontAttributes = attr }, 1, 0);
        return grid;
    }

    private static View BuildTable(List<string[]> rows)
    {
        int cols = 0;
        foreach (var r in rows)
            if (r.Length > cols) cols = r.Length;

        var grid = new Grid
        {
            Margin = new Thickness(0, 6, 0, 6),
            ColumnSpacing = 8,
            RowSpacing = 0,
            BackgroundColor = Colors.White,
            Padding = 8,
        };
        for (int c = 0; c < cols; c++)
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        for (int r = 0; r < rows.Count; r++)
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        for (int r = 0; r < rows.Count; r++)
        {
            bool header = r == 0;
            for (int c = 0; c < rows[r].Length; c++)
            {
                grid.Add(new Label
                {
                    Text = rows[r][c].Trim(),
                    FontSize = 12,
                    FontAttributes = header ? FontAttributes.Bold : FontAttributes.None,
                    TextColor = header ? Accent : Body,
                    Padding = new Thickness(0, 5),
                }, c, r);
            }
        }

        return new Frame
        {
            Padding = 0,
            CornerRadius = 8,
            BorderColor = Rule,
            BackgroundColor = Colors.White,
            HasShadow = false,
            Content = grid,
        };
    }
}
