// 
// PasteHistoryItem.cs
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
using Gtk;

namespace Pinta.Core
{
	public class PasteHistoryItem : BaseHistoryItem
	{
		private Cairo.ImageSurface paste_image;
		private DocumentSelection old_selection;

		public override bool CausesDirty { get { return true; } }

		public PasteHistoryItem (Cairo.ImageSurface pasteImage, DocumentSelection oldSelection)
		{
			Text = Translations.GetString ("Paste");
			Icon = Resources.StandardIcons.EditPaste;

			paste_image = pasteImage;
			old_selection = oldSelection;
		}

		public override void Redo ()
		{
			Document doc = PintaCore.Workspace.ActiveDocument;

			// Copy the paste to the temp layer
			doc.Layers.CreateSelectionLayer ();
			doc.Layers.ShowSelectionLayer = true;

			var g = new Cairo.Context (doc.Layers.SelectionLayer.Surface);
			g.SetSourceSurface (paste_image, 0, 0);
			g.Paint ();

			Swap ();

			PintaCore.Workspace.Invalidate ();
			PintaCore.Tools.SetCurrentTool ("MoveSelectedTool");
		}

		public override void Undo ()
		{
			var doc = PintaCore.Workspace.ActiveDocument;

			Swap ();

			doc.Layers.DestroySelectionLayer ();
			PintaCore.Workspace.Invalidate ();
		}

		private void Swap ()
		{
			// Swap the selection paths, and whether the
			// selection path should be visible
			Document doc = PintaCore.Workspace.ActiveDocument;

			DocumentSelection swap_selection = doc.Selection;
			doc.Selection = old_selection;
			old_selection = swap_selection;
		}
	}
}
