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
	public class HueSaturationEffect : BaseEffect
	{
		UnaryPixelOp? op;

		public override string Icon {
			get { return Pinta.Resources.Icons.AdjustmentsHueSaturation; }
		}

		public override string Name {
			get { return Translations.GetString ("Hue / Saturation"); }
		}

		public override bool IsConfigurable {
			get { return true; }
		}

		public override string AdjustmentMenuKey {
			get { return "U"; }
		}

		public HueSaturationEffect ()
		{
			EffectData = new HueSaturationData ();
		}

		public override void LaunchConfiguration ()
		{
			EffectHelper.LaunchSimpleEffectDialog (this);
		}

		public override void Render (ImageSurface src, ImageSurface dest, Core.RectangleI[] rois)
		{
			int hue_delta = Data.Hue;
			int sat_delta = Data.Saturation;
			int lightness = Data.Lightness;

			if (op == null) {
				if (hue_delta == 0 && sat_delta == 100 && lightness == 0)
					op = new UnaryPixelOps.Identity ();
				else
					op = new UnaryPixelOps.HueSaturationLightness (hue_delta, sat_delta, lightness);
			}

			op.Apply (dest, src, rois);
		}

		private HueSaturationData Data { get { return (HueSaturationData) EffectData!; } } // NRT - Set in constructor

		private class HueSaturationData : EffectData
		{
			[Caption ("Hue"), MinimumValue (-180), MaximumValue (180)]
			public int Hue = 0;

			[Caption ("Saturation"), MinimumValue (0), MaximumValue (200)]
			public int Saturation = 100;

			[Caption ("Lightness")]
			public int Lightness = 0;

			[Skip]
			public override bool IsDefault {
				get { return Hue == 0 && Saturation == 100 && Lightness == 0; }
			}
		}
	}
}
