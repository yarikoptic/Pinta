// 
// SimpleHistoryItem.cs
//  
// Author:
//       Jonathan Pobst <monkey@jpobst.com>
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
using Cairo;

namespace Pinta.Core
{
	public class SimpleHistoryItem : BaseHistoryItem
	{
		private SurfaceDiff? surface_diff;
		ImageSurface? old_surface;
		int layer_index;

		public SimpleHistoryItem (string icon, string text, ImageSurface oldSurface, int layerIndex) : base (icon, text)
		{
			var doc = PintaCore.Workspace.ActiveDocument;

			layer_index = layerIndex;
			surface_diff = SurfaceDiff.Create (oldSurface, doc.Layers[layer_index].Surface);

			// If the diff was too big, store the original surface, else, dispose it
			if (surface_diff == null)
				old_surface = oldSurface;
		}

		public SimpleHistoryItem (string icon, string text) : base (icon, text)
		{
		}

		public override void Undo ()
		{
			Swap ();
		}

		public override void Redo ()
		{
			Swap ();
		}

		private void Swap ()
		{
			var doc = PintaCore.Workspace.ActiveDocument;

			// Grab the original surface
			ImageSurface surf = doc.Layers[layer_index].Surface;

			if (surface_diff != null) {
				surface_diff.ApplyAndSwap (surf);
				PintaCore.Workspace.Invalidate (surface_diff.GetBounds ());
			} else {
				// Undo to the "old" surface
				doc.Layers[layer_index].Surface = old_surface!; // NRT - Will be not-null if surface_diff is null

				// Store the original surface for Redo
				old_surface = surf;

				PintaCore.Workspace.Invalidate ();
			}
		}

		public void TakeSnapshotOfLayer (int layerIndex)
		{
			var doc = PintaCore.Workspace.ActiveDocument;

			layer_index = layerIndex;
			old_surface = doc.Layers[layerIndex].Surface.Clone ();
		}

		public void TakeSnapshotOfLayer (UserLayer layer)
		{
			var doc = PintaCore.Workspace.ActiveDocument;

			layer_index = doc.Layers.IndexOf (layer);
			old_surface = layer.Surface.Clone ();
		}
	}
}
