// 
// HistoryTreeView.cs
// 
// Copyright (c) 2010 Jonathan Pobst
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using Gtk;
using Pinta.Core;

namespace Pinta.Gui.Widgets
{
	// GObject subclass for use with Gio.ListStore
	public class HistoryListViewItem : GObject.Object
	{
		private BaseHistoryItem item;

		public HistoryListViewItem (BaseHistoryItem item) : base (true, Array.Empty<GObject.ConstructArgument> ())
		{
			ArgumentNullException.ThrowIfNullOrEmpty (item.Text);
			ArgumentNullException.ThrowIfNullOrEmpty (item.Icon);

			this.item = item;
		}

		public string Label => item.Text!;
		public string IconName => item.Icon!;
		public bool Active => item.State == HistoryItemState.Undo;
	}

	public class HistoryItemWidget : Box
	{
		private Gtk.Image image;
		private Gtk.Label label;

		public HistoryItemWidget ()
		{
			Spacing = 6;
			this.SetAllMargins (2);
			SetOrientation (Orientation.Horizontal);

			image = Gtk.Image.New ();
			Append (image);

			label = Gtk.Label.New (string.Empty);
			label.Halign = Align.Start;
			Append (label);
		}

		// Set the widget's contents to the provided history item.
		public void Update (HistoryListViewItem item)
		{
			image.IconName = item.IconName;
			label.SetText (item.Label);

			if (item.Active)
				RemoveCssClass (AdwaitaStyles.DimLabel);
			else
				AddCssClass (AdwaitaStyles.DimLabel);
		}
	}
}
