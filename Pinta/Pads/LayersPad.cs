// 
// LayersPad.cs
//  
// Author:
//       Jonathan Pobst <monkey@jpobst.com>
// 
// Copyright (c) 2011 Jonathan Pobst
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

using Gtk;
using Pinta.Core;
using Pinta.Docking;
using Pinta.Gui.Widgets;

namespace Pinta
{
	public class LayersPad : IDockPad
	{
		public void Initialize (Dock workspace, Application app, Gio.Menu padMenu)
		{
			var layers = new LayersListView ();
			DockItem layers_item = new DockItem (layers, "Layers", icon_name: Pinta.Resources.Icons.LayerDuplicate) {
				Label = Translations.GetString ("Layers")
			};

			var layers_tb = layers_item.AddToolBar ();
			layers_tb.Append (PintaCore.Actions.Layers.AddNewLayer.CreateDockToolBarItem ());
			layers_tb.Append (PintaCore.Actions.Layers.DeleteLayer.CreateDockToolBarItem ());
			layers_tb.Append (PintaCore.Actions.Layers.DuplicateLayer.CreateDockToolBarItem ());
			layers_tb.Append (PintaCore.Actions.Layers.MergeLayerDown.CreateDockToolBarItem ());
			layers_tb.Append (PintaCore.Actions.Layers.MoveLayerUp.CreateDockToolBarItem ());
			layers_tb.Append (PintaCore.Actions.Layers.MoveLayerDown.CreateDockToolBarItem ());

			workspace.AddItem (layers_item, DockPlacement.Right);

			var show_layers = new ToggleCommand ("layers", Translations.GetString ("Layers"), null, Resources.Icons.LayerMergeDown) {
				Value = true
			};
			app.AddAction (show_layers);
			padMenu.AppendItem (show_layers.CreateMenuItem ());

			show_layers.Toggled += (val) => {
				if (val)
					layers_item.Maximize ();
				else
					layers_item.Minimize ();
			};
			layers_item.MaximizeClicked += (_, _) => show_layers.Value = true;
			layers_item.MinimizeClicked += (_, _) => show_layers.Value = false;
		}
	}
}
