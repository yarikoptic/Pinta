/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See license-pdn.txt for full licensing and attribution details.             //
/////////////////////////////////////////////////////////////////////////////////

using System;
using Cairo;

namespace Pinta.Core
{
	/// <summary>
	/// ColorDifferenctEffect is a base class for my difference effects
	/// that have floating point (double) convolution filters.
	/// its architecture is just like ConvolutionFilterEffect, adding a
	/// function (RenderColorDifferenceEffect) called from Render in each
	/// derived class.
	/// It is also limited to 3x3 kernels.
	/// (Chris Crosetto)
	/// </summary>
	public abstract class ColorDifferenceEffect : BaseEffect
	{
		public void RenderColorDifferenceEffect (double[][] weights, ImageSurface src, ImageSurface dest, RectangleI[] rois)
		{
			RectangleI src_rect = src.GetBounds ();

			// Cache these for a massive performance boost
			var src_data = src.GetReadOnlyPixelData ();
			var dst_data = dest.GetPixelData ();
			int src_width = src.Width;

			foreach (RectangleI rect in rois) {
				// loop through each line of target rectangle
				for (int y = rect.Y; y < rect.Y + rect.Height; ++y) {
					int fyStart = 0;
					int fyEnd = 3;

					if (y == src_rect.Y)
						fyStart = 1;
					if (y == src_rect.Y + src_rect.Height - 1)
						fyEnd = 2;

					// loop through each point in the line
					var dst_row = dst_data.Slice (y * src_width);

					for (int x = rect.X; x < rect.X + rect.Width; ++x) {
						int fxStart = 0;
						int fxEnd = 3;

						if (x == src_rect.X)
							fxStart = 1;

						if (x == src_rect.X + src_rect.Width - 1)
							fxEnd = 2;

						// loop through each weight
						double rSum = 0.0;
						double gSum = 0.0;
						double bSum = 0.0;

						for (int fy = fyStart; fy < fyEnd; ++fy) {
							for (int fx = fxStart; fx < fxEnd; ++fx) {
								double weight = weights[fy][fx];
								ref readonly ColorBgra c = ref src_data[(y - 1 + fy) * src_width + (x - 1 + fx)];

								rSum += weight * (double) c.R;
								gSum += weight * (double) c.G;
								bSum += weight * (double) c.B;
							}
						}

						int iRsum = (int) rSum;
						int iGsum = (int) gSum;
						int iBsum = (int) bSum;

						if (iRsum > 255)
							iRsum = 255;

						if (iGsum > 255)
							iGsum = 255;

						if (iBsum > 255)
							iBsum = 255;

						if (iRsum < 0)
							iRsum = 0;

						if (iGsum < 0)
							iGsum = 0;

						if (iBsum < 0)
							iBsum = 0;

						dst_row[x] = ColorBgra.FromBgra ((byte) iBsum, (byte) iGsum, (byte) iRsum, 255);
					}
				}
			}
		}
	}
}
