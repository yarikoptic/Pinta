/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See license-pdn.txt for full licensing and attribution details.             //
//                                                                             //
// Ported to Pinta by: Jonathan Pobst <monkey@jpobst.com>                      //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Security.Cryptography;
using Cairo;
using Pinta.Core;
using Pinta.Gui.Widgets;

namespace Pinta.Effects
{
	public class GaussianBlurEffect : BaseEffect
	{
		public override string Icon => Pinta.Resources.Icons.EffectsBlursGaussianBlur;

		public override string Name {
			get { return Translations.GetString ("Gaussian Blur"); }
		}

		public override bool IsConfigurable {
			get { return true; }
		}

		public override string EffectMenuCategory {
			get { return Translations.GetString ("Blurs"); }
		}

		public GaussianBlurData Data { get { return (GaussianBlurData) EffectData!; } } // NRT - Set in constructor

		public GaussianBlurEffect ()
		{
			EffectData = new GaussianBlurData ();
		}

		public override void LaunchConfiguration ()
		{
			EffectHelper.LaunchSimpleEffectDialog (this);
		}

		#region Algorithm Code Ported From PDN
		public static int[] CreateGaussianBlurRow (int amount)
		{
			int size = 1 + (amount * 2);
			int[] weights = new int[size];

			for (int i = 0; i <= amount; ++i) {
				// 1 + aa - aa + 2ai - ii
				weights[i] = 16 * (i + 1);
				weights[weights.Length - i - 1] = weights[i];
			}

			return weights;
		}

		public override void Render (ImageSurface src, ImageSurface dest, Core.RectangleI[] rois)
		{
			if (Data.Radius == 0) {
				// Copy src to dest
				return;
			}

			int r = Data.Radius;
			int[] w = CreateGaussianBlurRow (r);
			int wlen = w.Length;

			Span<long> waSums = stackalloc long[wlen];
			Span<long> wcSums = stackalloc long[wlen];
			Span<long> aSums = stackalloc long[wlen];
			Span<long> bSums = stackalloc long[wlen];
			Span<long> gSums = stackalloc long[wlen];
			Span<long> rSums = stackalloc long[wlen];

			// Cache these for a massive performance boost
			int src_width = src.Width;
			int src_height = src.Height;
			ReadOnlySpan<ColorBgra> src_data = src.GetReadOnlyPixelData ();
			Span<ColorBgra> dst_data = dest.GetPixelData ();

			foreach (var rect in rois) {
				if (rect.Height >= 1 && rect.Width >= 1) {
					for (int y = rect.Top; y <= rect.Bottom; ++y) {
						long waSum = 0;
						long wcSum = 0;
						long aSum = 0;
						long bSum = 0;
						long gSum = 0;
						long rSum = 0;

						var dst_row = dst_data.Slice (y * src_width, src_width);

						for (int wx = 0; wx < wlen; ++wx) {
							int srcX = rect.Left + wx - r;
							waSums[wx] = 0;
							wcSums[wx] = 0;
							aSums[wx] = 0;
							bSums[wx] = 0;
							gSums[wx] = 0;
							rSums[wx] = 0;

							if (srcX >= 0 && srcX < src_width) {
								for (int wy = 0; wy < wlen; ++wy) {
									int srcY = y + wy - r;

									if (srcY >= 0 && srcY < src_height) {
										ColorBgra c = src.GetColorBgra (src_data, src_width, srcX, srcY).ToStraightAlpha ();
										int wp = w[wy];

										waSums[wx] += wp;
										wp *= c.A + (c.A >> 7);
										wcSums[wx] += wp;
										wp >>= 8;

										if (c.A > 0) {
											aSums[wx] += wp * c.A;
											bSums[wx] += wp * c.B;
											gSums[wx] += wp * c.G;
											rSums[wx] += wp * c.R;
										}
									}
								}

								int wwx = w[wx];
								waSum += wwx * waSums[wx];
								wcSum += wwx * wcSums[wx];
								aSum += wwx * aSums[wx];
								bSum += wwx * bSums[wx];
								gSum += wwx * gSums[wx];
								rSum += wwx * rSums[wx];
							}
						}

						wcSum >>= 8;

						if (waSum == 0 || wcSum == 0) {
							dst_row[rect.Left].Bgra = 0;
						} else {
							byte alpha = (byte) (aSum / waSum);
							byte blue = (byte) (bSum / wcSum);
							byte green = (byte) (gSum / wcSum);
							byte red = (byte) (rSum / wcSum);

							dst_row[rect.Left].Bgra = ColorBgra.FromBgra (blue, green, red, alpha).ToPremultipliedAlpha ().Bgra;
						}

						for (int x = rect.Left + 1; x <= rect.Right; ++x) {
							for (int i = 0; i < wlen - 1; ++i) {
								waSums[i] = waSums[i + 1];
								wcSums[i] = wcSums[i + 1];
								aSums[i] = aSums[i + 1];
								bSums[i] = bSums[i + 1];
								gSums[i] = gSums[i + 1];
								rSums[i] = rSums[i + 1];
							}

							waSum = 0;
							wcSum = 0;
							aSum = 0;
							bSum = 0;
							gSum = 0;
							rSum = 0;

							int wx;
							for (wx = 0; wx < wlen - 1; ++wx) {
								long wwx = (long) w[wx];
								waSum += wwx * waSums[wx];
								wcSum += wwx * wcSums[wx];
								aSum += wwx * aSums[wx];
								bSum += wwx * bSums[wx];
								gSum += wwx * gSums[wx];
								rSum += wwx * rSums[wx];
							}

							wx = wlen - 1;

							waSums[wx] = 0;
							wcSums[wx] = 0;
							aSums[wx] = 0;
							bSums[wx] = 0;
							gSums[wx] = 0;
							rSums[wx] = 0;

							int srcX = x + wx - r;

							if (srcX >= 0 && srcX < src_width) {
								for (int wy = 0; wy < wlen; ++wy) {
									int srcY = y + wy - r;

									if (srcY >= 0 && srcY < src_height) {
										ColorBgra c = src.GetColorBgra (src_data, src_width, srcX, srcY).ToStraightAlpha ();
										int wp = w[wy];

										waSums[wx] += wp;
										wp *= c.A + (c.A >> 7);
										wcSums[wx] += wp;
										wp >>= 8;

										if (c.A > 0) {
											aSums[wx] += wp * (long) c.A;
											bSums[wx] += wp * (long) c.B;
											gSums[wx] += wp * (long) c.G;
											rSums[wx] += wp * (long) c.R;
										}
									}
								}

								int wr = w[wx];
								waSum += (long) wr * waSums[wx];
								wcSum += (long) wr * wcSums[wx];
								aSum += (long) wr * aSums[wx];
								bSum += (long) wr * bSums[wx];
								gSum += (long) wr * gSums[wx];
								rSum += (long) wr * rSums[wx];
							}

							wcSum >>= 8;

							if (waSum == 0 || wcSum == 0) {
								dst_row[x].Bgra = 0;
							} else {
								byte alpha = (byte) (aSum / waSum);
								byte blue = (byte) (bSum / wcSum);
								byte green = (byte) (gSum / wcSum);
								byte red = (byte) (rSum / wcSum);

								dst_row[x].Bgra = ColorBgra.FromBgra (blue, green, red, alpha).ToPremultipliedAlpha ().Bgra;
							}
						}
					}
				}
			}
		}
		#endregion

		public class GaussianBlurData : EffectData
		{
			[Caption ("Radius"), MinimumValue (0), MaximumValue (200)]
			public int Radius = 2;

			[Skip]
			public override bool IsDefault { get { return Radius == 0; } }
		}
	}
}
