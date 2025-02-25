// 
// BaseEffect.cs
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
using Mono.Addins;
using Mono.Addins.Localization;
using Pinta.Core;

namespace Pinta.Core
{
	/// <summary>
	/// The base class for all effects and adjustments.
	/// </summary>
	[Mono.Addins.TypeExtensionPoint]
	public abstract class BaseEffect
	{
		/// <summary>
		/// Returns the name of the effect, displayed to the user in the Adjustments/Effects menu and history pad.
		/// </summary>
		public abstract string Name { get; }

		/// <summary>
		/// Returns the icon to use for the effect in the Adjustments/Effects menu and history pad.
		/// </summary>
		public virtual string Icon => Pinta.Resources.Icons.EffectsDefault;

		/// <summary>
		/// Returns whether this effect can display a configuration dialog to the user. (Implemented by LaunchConfiguration ().)
		/// </summary>
		public virtual bool IsConfigurable { get { return false; } }

		/// <summary>
		/// Returns the keyboard shortcut for this adjustment. Only affects adjustments, not effects. Default is no shortcut.
		/// </summary>
		public virtual string? AdjustmentMenuKey { get { return null; } }

		/// <summary>
		/// Returns the modifier(s) to the keyboard shortcut. Only affects adjustments, not effects. Default is Primary+Shift.
		/// </summary>
		public virtual string AdjustmentMenuKeyModifiers { get { return "<Primary><Shift>"; } }

		/// <summary>
		/// Returns the menu category for an effect. Only affects effects, not adjustments. Default is "General".
		/// </summary>
		public virtual string EffectMenuCategory { get { return "General"; } }

		/// <summary>
		/// The user configurable data this effect uses.
		/// </summary>
		public EffectData? EffectData { get; protected set; }

		/// <summary>
		/// Launches the configuration dialog for this effect/adjustment.
		/// If IsConfigurable is true, the ConfigDialogResponse event will be invoked when the user accepts or cancels the dialog.
		/// </summary>
		public virtual void LaunchConfiguration ()
		{
			if (IsConfigurable)
				throw new NotImplementedException (string.Format ("{0} is marked as configurable, but has not implemented LaunchConfiguration", this.GetType ()));
		}

		/// <summary>
		/// Launches the standard configuration dialog for this effect.
		/// </summary>
		/// <param name="localizer">
		/// The localizer for the effect add-in. This is used to fetch translations for the
		/// strings in the dialog.
		/// </param>
		protected void LaunchSimpleEffectDialog (AddinLocalizer localizer)
		{
			PintaCore.Chrome.LaunchSimpleEffectDialog (this, new AddinLocalizerWrapper (localizer));
		}

		/// <summary>
		/// Emitted when the configuration dialog is accepted or cancelled by the user.
		/// </summary>
		public event EventHandler<ConfigDialogResponseEventArgs>? ConfigDialogResponse;

		/// <summary>
		/// Notify that the configuration dialog was accepted or cancelled by the user.
		/// </summary>
		public void OnConfigDialogResponse (bool accepted)
		{
			ConfigDialogResponse?.Invoke (this, new ConfigDialogResponseEventArgs (accepted));
		}

		#region Overrideable Render Methods
		/// <summary>
		/// Performs the actual work of rendering an effect. Do not call base.Render ().
		/// </summary>
		/// <param name="src">The source surface. DO NOT MODIFY.</param>
		/// <param name="dst">The destination surface.</param>
		/// <param name="rois">An array of rectangles of interest (roi) specifying the area(s) to modify. Only these areas should be modified.</param>
		public virtual void Render (ImageSurface src, ImageSurface dst, RectangleI[] rois)
		{
			foreach (var rect in rois)
				Render (src, dst, rect);
		}

		/// <summary>
		/// Performs the actual work of rendering an effect. Do not call base.Render ().
		/// </summary>
		/// <param name="src">The source surface. DO NOT MODIFY.</param>
		/// <param name="dst">The destination surface.</param>
		/// <param name="roi">A rectangle of interest (roi) specifying the area to modify. Only these areas should be modified</param>
		protected virtual void Render (ImageSurface src, ImageSurface dst, RectangleI roi)
		{
			var src_data = src.GetReadOnlyPixelData ();
			var dst_data = dst.GetPixelData ();
			int src_width = src.Width;
			int dst_width = dst.Width;

			for (int y = roi.Y; y <= roi.Bottom; ++y) {
				Render (src_data.Slice (y * src_width + roi.X, roi.Width),
					dst_data.Slice (y * dst_width + roi.X, roi.Width));
			}
		}

		/// <summary>
		/// Performs the actual work of rendering an effect. This overload represent a single line of the image. Do not call base.Render ().
		/// </summary>
		/// <param name="src">The source surface. DO NOT MODIFY.</param>
		/// <param name="dst">The destination surface.</param>
		/// <param name="length">The number of pixels to render.</param>
		protected virtual void Render (ReadOnlySpan<ColorBgra> src, Span<ColorBgra> dst)
		{
			for (int i = 0; i < src.Length; ++i)
				dst[i] = Render (src[i]);
		}

		/// <summary>
		/// Performs the actual work of rendering an effect. This overload represent a single pixel of the image.
		/// </summary>
		/// <param name="color">The color of the source surface pixel.</param>
		/// <returns>The color to be used for the destination pixel.</returns>
		protected virtual ColorBgra Render (in ColorBgra color)
		{
			return color;
		}
		#endregion

		// Effects that have any configuration state which is changed
		// during live preview, and this this state is stored in
		// non-value-type fields should override this method.
		// Generally this state should be stored in the effect data
		// class, not in the effect.
		/// <summary>
		/// Clones this effect so the live preview system has a copy that won't change while it is working.  Only override this when a MemberwiseClone is not enough.
		/// </summary>
		/// <returns>An identical copy of this effect.</returns>
		public virtual BaseEffect Clone ()
		{
			var effect = (BaseEffect) this.MemberwiseClone ();

			if (effect.EffectData != null)
				effect.EffectData = EffectData?.Clone ();

			return effect;
		}

		public class ConfigDialogResponseEventArgs : EventArgs
		{
			public ConfigDialogResponseEventArgs (bool accepted) { Accepted = accepted; }

			public bool Accepted { get; private set; }
		}
	}

	/// <summary>
	/// Holds the user configurable data used by this effect.
	/// </summary>
	public abstract class EffectData : ObservableObject
	{
		// EffectData classes that have any state stored in non-value-type
		// fields must override this method, and clone those members.
		/// <summary>
		/// Clones this EffectData so the live preview system has a copy that won't change while it is working.  Only override this when a MemberwiseClone is not enough.
		/// </summary>
		/// <returns>An identical copy of this EffectData.</returns>
		public virtual EffectData Clone ()
		{
			return (EffectData) this.MemberwiseClone ();
		}

		/// <summary>
		/// Fires the PropertyChanged event for this ObservableObject.
		/// </summary>
		/// <param name="propertyName">The name of the property that changed.</param>
		public new void FirePropertyChanged (string? propertyName)
		{
			base.FirePropertyChanged (propertyName);
		}

		/// <summary>
		/// Returns true if the current values of this EffectData do not modify the image. Returns false if current values modify the image.
		/// </summary>
		public virtual bool IsDefault { get { return false; } }
	}

	/// <summary>
	/// Wrapper around the AddinLocalizer of an add-in.
	/// </summary>
	internal class AddinLocalizerWrapper : IAddinLocalizer
	{
		private AddinLocalizer localizer;

		public AddinLocalizerWrapper (AddinLocalizer localizer)
		{
			this.localizer = localizer;
		}

		public string GetString (string msgid)
		{
			return localizer.GetString (msgid);
		}
	};
}
