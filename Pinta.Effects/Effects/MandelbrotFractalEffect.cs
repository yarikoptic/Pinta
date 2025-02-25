/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See license-pdn.txt for full licensing and attribution details.             //
//                                                                             //
// Ported to Pinta by: Olivier Dufour <olivier.duff@gmail.com>                 //
/////////////////////////////////////////////////////////////////////////////////

using System;
using Cairo;
using Pinta.Core;
using Pinta.Gui.Widgets;

namespace Pinta.Effects
{
	public class MandelbrotFractalEffect : BaseEffect
	{
		public override string Icon => Pinta.Resources.Icons.EffectsRenderMandelbrotFractal;

		public override string Name {
			get { return Translations.GetString ("Mandelbrot Fractal"); }
		}

		public override bool IsConfigurable {
			get { return true; }
		}

		public override string EffectMenuCategory {
			get { return Translations.GetString ("Render"); }
		}

		public MandelbrotFractalData Data { get { return (MandelbrotFractalData) EffectData!; } } // NRT - Set in constructor

		public MandelbrotFractalEffect ()
		{
			EffectData = new MandelbrotFractalData ();
		}

		public override void LaunchConfiguration ()
		{
			EffectHelper.LaunchSimpleEffectDialog (this);
		}

		#region Algorithm Code Ported From PDN
		private const double max = 100000;
		private static readonly double invLogMax = 1.0 / Math.Log (max);
		private static double zoomFactor = 20.0;
		private const double xOffsetBasis = -0.7;
		private double xOffset = xOffsetBasis;

		private const double yOffsetBasis = -0.29;
		private double yOffset = yOffsetBasis;

		private InvertColorsEffect invert_effect = new ();

		private static double Mandelbrot (double r, double i, int factor)
		{
			int c = 0;
			double x = 0;
			double y = 0;

			while ((c * factor) < 1024 && ((x * x) + (y * y)) < max) {
				double t = x;

				x = x * x - y * y + r;
				y = 2 * t * y + i;

				++c;
			}

			return c - Math.Log (y * y + x * x) * invLogMax;
		}

		public override void Render (ImageSurface src, ImageSurface dst, Core.RectangleI[] rois)
		{
			int w = dst.Width;
			int h = dst.Height;

			double invH = 1.0 / h;
			double zoom = 1 + zoomFactor * Data.Zoom;
			double invZoom = 1.0 / zoom;

			double invQuality = 1.0 / (double) Data.Quality;

			int count = Data.Quality * Data.Quality + 1;
			double invCount = 1.0 / (double) count;
			double angleTheta = (Data.Angle * 2 * Math.PI) / 360;

			Span<ColorBgra> dst_data = dst.GetPixelData ();
			int dst_width = dst.Width;

			foreach (Core.RectangleI rect in rois) {
				for (int y = rect.Top; y <= rect.Bottom; y++) {
					var dst_row = dst_data.Slice (y * dst_width, dst_width);

					for (int x = rect.Left; x <= rect.Right; x++) {
						int r = 0;
						int g = 0;
						int b = 0;
						int a = 0;

						for (double i = 0; i < count; i++) {
							double u = (2.0 * x - w + (i * invCount)) * invH;
							double v = (2.0 * y - h + ((i * invQuality) % 1)) * invH;

							double radius = Math.Sqrt ((u * u) + (v * v));
							double radiusP = radius;
							double theta = Math.Atan2 (v, u);
							double thetaP = theta + angleTheta;

							double uP = radiusP * Math.Cos (thetaP);
							double vP = radiusP * Math.Sin (thetaP);

							double m = Mandelbrot ((uP * invZoom) + this.xOffset, (vP * invZoom) + this.yOffset, Data.Factor);

							double c = 64 + Data.Factor * m;

							r += Utility.ClampToByte (c - 768);
							g += Utility.ClampToByte (c - 512);
							b += Utility.ClampToByte (c - 256);
							a += Utility.ClampToByte (c - 0);
						}

						dst_row[x] = ColorBgra.FromBgra (Utility.ClampToByte (b / count), Utility.ClampToByte (g / count), Utility.ClampToByte (r / count), Utility.ClampToByte (a / count));
					}
				}
			}

			if (Data.InvertColors) {
				invert_effect.Render (dst, dst, rois);
			}
		}
		#endregion

		public class MandelbrotFractalData : EffectData
		{
			[Caption ("Factor"), MinimumValue (1), MaximumValue (10)]
			public int Factor = 1;

			[Caption ("Quality"), MinimumValue (1), MaximumValue (5)]
			public int Quality = 2;

			//TODO double
			[Caption ("Zoom"), MinimumValue (0), MaximumValue (100)]
			public int Zoom = 10;

			[Caption ("Angle")]
			public double Angle = 0;

			[Caption ("Invert Colors")]
			public bool InvertColors = false;

		}
	}
}
