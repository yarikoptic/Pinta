//  
// Author:
//       Cameron White <cameronwhite91@gmail.com>
// 
// Copyright (c) 2020 Cameron White
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
using Pinta.Resources;

namespace Pinta.Docking
{
	/// <summary>
	/// A dock item contains a single child widget, and can be docked at
	/// various locations.
	/// </summary>
	public class DockItem : Box
	{
		private Label label_widget;
		private Stack button_stack;
		private Button minimize_button;
		private Button maximize_button;

		/// <summary>
		/// Unique identifier for the dock item. Used e.g. when saving the dock layout to disk.
		/// </summary>
		public string UniqueName { get; private set; }

		/// <summary>
		/// Icon name for the dock item, used when minimized.
		/// </summary>
		public string IconName { get; private set; }

		/// <summary>
		/// Visible label for the dock item.
		/// </summary>
		public string Label { get => label_widget.GetLabel (); set => label_widget.SetLabel (value); }

		/// <summary>
		/// Triggered when the minimize button is pressed.
		/// </summary>
		public event EventHandler? MinimizeClicked;

		/// <summary>
		/// Triggered when the maximize button is pressed.
		/// </summary>
		public event EventHandler? MaximizeClicked;

		public DockItem (Widget child, string unique_name, string icon_name, bool locked = false)
		{
			SetOrientation (Orientation.Vertical);

			UniqueName = unique_name;
			IconName = icon_name;

			minimize_button = Button.NewFromIconName (StandardIcons.WindowMinimize);
			minimize_button.AddCssClass (Pinta.Core.AdwaitaStyles.Flat);
			maximize_button = Button.NewFromIconName (StandardIcons.WindowMaximize);
			maximize_button.AddCssClass (Pinta.Core.AdwaitaStyles.Flat);

			button_stack = new Stack ();
			button_stack.AddChild (minimize_button);
			button_stack.AddChild (maximize_button);

			label_widget = new Label ();
			if (!locked) {
				const int padding = 8;
				var title_layout = Box.New (Orientation.Horizontal, 0);
				label_widget.MarginStart = label_widget.MarginEnd = padding;
				label_widget.Hexpand = true;
				label_widget.Halign = Align.Start;
				title_layout.Append (label_widget);

				title_layout.Append (button_stack);

				minimize_button.OnClicked += (o, args) => Minimize ();
				maximize_button.OnClicked += (o, args) => Maximize ();

				Append (title_layout);
			}

			child.Valign = Align.Fill;
			child.Vexpand = true;
			Append (child);

			// TODO - support dragging into floating panel?
		}

		/// <summary>
		/// Create a toolbar and add it to the bottom of the dock item.
		/// </summary>
		public Gtk.Box AddToolBar ()
		{
			var toolbar = GtkExtensions.CreateToolBar ();
			Append (toolbar);
			return toolbar;
		}

		/// <summary>
		/// Minimize the dock item.
		/// </summary>
		public void Minimize ()
		{
			if (button_stack.VisibleChild != maximize_button) {
				button_stack.VisibleChild = maximize_button;
				MinimizeClicked?.Invoke (this, new EventArgs ());
			}
		}

		/// <summary>
		/// Maximize the dock item.
		/// </summary>
		public void Maximize ()
		{
			if (button_stack.VisibleChild != minimize_button) {
				button_stack.VisibleChild = minimize_button;
				MaximizeClicked?.Invoke (this, new EventArgs ());
			}
		}
	}
}
