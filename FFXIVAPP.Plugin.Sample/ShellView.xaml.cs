﻿// FFXIVAPP.Plugin.Sample
// ShellView.xaml.cs
// 
// © 2013 Ryan Wilson

namespace FFXIVAPP.Plugin.Sample
{
    /// <summary>
    ///     Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView
    {
        public static ShellView View;

        public ShellView()
        {
            InitializeComponent();
            View = this;
        }
    }
}
