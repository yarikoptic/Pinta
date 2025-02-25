/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See license-pdn.txt for full licensing and attribution details.             //
//                                                                             //
// Ported to Pinta by: Krzysztof Marecki <marecki.krzysztof@gmail.com>         //
/////////////////////////////////////////////////////////////////////////////////

using System;
using Cairo;
using Pinta.Core;
using Pinta.Gui.Widgets;

namespace Pinta.Effects
{
	public class SharpenEffect : LocalHistogramEffect
	{
		public override string Icon => Pinta.Resources.Icons.EffectsPhotoSharpen;

		public override string Name {
			get { return Translations.GetString ("Sharpen"); }
		}

		public override bool IsConfigurable {
			get { return true; }
		}

		public override string EffectMenuCategory {
			get { return Translations.GetString ("Photo"); }
		}

		public SharpenData Data { get { return (SharpenData) EffectData!; } } // NRT - Set in constructor

		public SharpenEffect ()
		{
			EffectData = new SharpenData ();
		}

		public override void LaunchConfiguration ()
		{
			EffectHelper.LaunchSimpleEffectDialog (this);
		}

		public override void Render (ImageSurface src, ImageSurface dest, Core.RectangleI[] rois)
		{
			foreach (var rect in rois)
				RenderRect (Data.Amount, src, dest, rect);
		}

		public override ColorBgra Apply (in ColorBgra src, int area, Span<int> hb, Span<int> hg, Span<int> hr, Span<int> ha)
		{
			ColorBgra median = GetPercentile (50, area, hb, hg, hr, ha);
			return ColorBgra.Lerp (src, median, -0.5f);
		}
	}

	public class SharpenData : EffectData
	{
		[Caption ("Amount"), MinimumValue (1), MaximumValue (20)]
		public int Amount = 2;
	}
}

