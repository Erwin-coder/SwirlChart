using System;
using System.IO;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace SwirlChart
{
	public partial class App : Application
	{
		public App()
		{
			InitializeComponent();
		}

		// MAUI's Windows title bar is its own control rather than the system caption
		// (MauiWinUIWindow always sets ExtendsContentIntoTitleBar), and the text it
		// draws comes from Window.Title. Leaving that unset is what left the bar blank.
		protected override Window CreateWindow(IActivationState activationState) =>
			new Window(new AppShell()) { Title = "Swirl Chart" };
	}
}