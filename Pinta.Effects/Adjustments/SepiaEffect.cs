/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See license-pdn.txt for full licensing and attribution details.             //
//                                                                             //
// Ported to Pinta by: Jonathan Pobst <monkey@jpobst.com>                      //
/////////////////////////////////////////////////////////////////////////////////

using System;
using Cairo;
using Pinta.Core;

namespace Pinta.Effects
{
	public class SepiaEffect : BaseEffect
	{
		UnaryPixelOp desat = new UnaryPixelOps.Desaturate ();
		UnaryPixelOp level = new UnaryPixelOps.Desaturate ();

		public override string Icon {
			get { return Pinta.Resources.Icons.AdjustmentsSepia; }
		}

		public override string Name {
			get { return Translations.GetString ("Sepia"); }
		}

		public override string AdjustmentMenuKey {
			get { return "E"; }
		}

		public SepiaEffect ()
		{
			desat = new UnaryPixelOps.Desaturate ();
			level = new UnaryPixelOps.Level (
				ColorBgra.Black,
				ColorBgra.White,
				new float[] { 1.2f, 1.0f, 0.8f },
				ColorBgra.Black,
				ColorBgra.White);
		}

		public override void Render (ImageSurface src, ImageSurface dest, Core.RectangleI[] rois)
		{
			desat.Apply (dest, src, rois);
			level.Apply (dest, dest, rois);
		}
	}
}
