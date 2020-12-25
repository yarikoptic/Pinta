// 
// LayerManager.cs
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Linq;
using Cairo;

namespace Pinta.Core
{
	public class LayerManager
	{
		#region Public Properties
		public UserLayer this[int index]
		{
			get { return PintaCore.Workspace.ActiveDocument.Layers.UserLayers[index]; }
		}

		public UserLayer CurrentLayer
		{
			get { return PintaCore.Workspace.ActiveDocument.Layers.CurrentUserLayer; }
		}

		public int Count {
			get { return PintaCore.Workspace.ActiveDocument.Layers.UserLayers.Count; }
		}

		public Layer ToolLayer {
			get { return PintaCore.Workspace.ActiveDocument.Layers.ToolLayer; }
		}

		public Layer SelectionLayer {
			get { return PintaCore.Workspace.ActiveDocument.Layers.SelectionLayer; }
		}

		public int CurrentLayerIndex {
			get { return PintaCore.Workspace.ActiveDocument.Layers.CurrentUserLayerIndex; }
		}
		
		public bool ShowSelectionLayer {
			get { return PintaCore.Workspace.ActiveDocument.Layers.ShowSelectionLayer; }
			set { PintaCore.Workspace.ActiveDocument.Layers.ShowSelectionLayer = value; }
		}
		#endregion

		#region Public Methods
		public List<Layer> GetLayersToPaint ()
		{
			return PintaCore.Workspace.ActiveDocument.Layers.GetLayersToPaint ();
		}

		public void SetCurrentLayer (int i)
		{
			PintaCore.Workspace.ActiveDocument.Layers.SetCurrentUserLayer (i);
		}

		public void SetCurrentLayer(UserLayer layer)
		{
			PintaCore.Workspace.ActiveDocument.Layers.SetCurrentUserLayer (layer);
		}

		public void FinishSelection ()
		{
			PintaCore.Workspace.ActiveDocument.FinishSelection ();
		}
		
		// Adds a new layer above the current one
		public UserLayer AddNewLayer(string name)
		{
			return PintaCore.Workspace.ActiveDocument.Layers.AddNewLayer (name);
		}
		
		// Adds a new layer above the current one
		public void Insert(UserLayer layer, int index)
		{
			PintaCore.Workspace.ActiveDocument.Layers.Insert (layer, index);
		}

		public int IndexOf(UserLayer layer)
		{
			return PintaCore.Workspace.ActiveDocument.Layers.IndexOf (layer);
		}

		// Delete the current layer
		public void DeleteCurrentLayer ()
		{
			PintaCore.Workspace.ActiveDocument.Layers.DeleteCurrentLayer ();
		}

		// Delete the layer
		public void DeleteLayer (int index, bool dispose)
		{
			PintaCore.Workspace.ActiveDocument.Layers.DeleteLayer (index, dispose);
		}

		// Duplicate current layer
		public Layer DuplicateCurrentLayer ()
		{
			return PintaCore.Workspace.ActiveDocument.Layers.DuplicateCurrentLayer ();
		}

		// Flatten current layer
		public void MergeCurrentLayerDown ()
		{
			PintaCore.Workspace.ActiveDocument.Layers.MergeCurrentLayerDown ();
		}

		// Move current layer up
		public void MoveCurrentLayerUp ()
		{
			PintaCore.Workspace.ActiveDocument.Layers.MoveCurrentLayerUp ();
		}

		// Move current layer down
		public void MoveCurrentLayerDown ()
		{
			PintaCore.Workspace.ActiveDocument.Layers.MoveCurrentLayerDown ();
		}

		// Flip image horizontally
		public void FlipImageHorizontal ()
		{
			PintaCore.Workspace.ActiveDocument.FlipImageHorizontal ();
		}

		// Flip image vertically
		public void FlipImageVertical ()
		{
			PintaCore.Workspace.ActiveDocument.FlipImageVertical ();
		}

		// Rotate image 180 degrees (flip H+V)
		public void RotateImage180 ()
		{
			PintaCore.Workspace.ActiveDocument.RotateImage180 ();
		}
		
		public void RotateImageCW ()
		{
			PintaCore.Workspace.ActiveDocument.RotateImageCW ();
		}
	
		public void RotateImageCCW ()
		{
			PintaCore.Workspace.ActiveDocument.RotateImageCCW ();
		}
			
		// Flatten image
		public void FlattenImage ()
		{
			PintaCore.Workspace.ActiveDocument.Layers.FlattenLayers ();
		}
		
		public void CreateSelectionLayer ()
		{
			PintaCore.Workspace.ActiveDocument.Layers.CreateSelectionLayer ();
		}
		
		public void DestroySelectionLayer ()
		{
			PintaCore.Workspace.ActiveDocument.Layers.DestroySelectionLayer ();
		}

		public void ResetSelectionPath ()
		{
			PintaCore.Workspace.ActiveDocument.ResetSelectionPaths ();
		}
		#endregion

		#region Protected Methods
		protected internal void OnLayerAdded ()
		{
			if (LayerAdded != null)
				LayerAdded.Invoke (this, EventArgs.Empty);
		}

		protected internal void OnLayerRemoved ()
		{
			if (LayerRemoved != null)
				LayerRemoved.Invoke (this, EventArgs.Empty);
		}

		protected internal void OnSelectedLayerChanged ()
		{
			if (SelectedLayerChanged != null)
				SelectedLayerChanged.Invoke (this, EventArgs.Empty);
		}	
		#endregion

		#region Private Methods
		public Layer CreateLayer ()
		{
			return PintaCore.Workspace.ActiveDocument.Layers.CreateLayer ();
		}

		public Layer CreateLayer (string name, int width, int height)
		{
			return PintaCore.Workspace.ActiveDocument.Layers.CreateLayer (name, width, height);
		}
		
		internal void RaiseLayerPropertyChangedEvent (object? sender, PropertyChangedEventArgs e)
		{
			if (LayerPropertyChanged != null)
				LayerPropertyChanged (sender, e);
			
			//TODO Get the workspace to subscribe to this event, and invalidate itself.
			PintaCore.Workspace.Invalidate ();
		}
		#endregion

		#region Events
		public event EventHandler? LayerAdded;
		public event EventHandler? LayerRemoved;
		public event EventHandler? SelectedLayerChanged;
		public event PropertyChangedEventHandler? LayerPropertyChanged;
		#endregion
	}
}
