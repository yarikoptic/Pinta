//
// CurvesDialog.cs
//  
// Author:
//      Krzysztof Marecki <marecki.krzysztof@gmail.com>
// 
// Copyright (c) 2010 Krzysztof Marecki
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
using System.Diagnostics.CodeAnalysis;
using Cairo;
using Gtk;
using Pinta.Core;

namespace Pinta.Effects
{

	public class CurvesDialog : Gtk.Dialog
	{
		private ComboBoxText comboMap;
		private Label labelPoint;
		private DrawingArea drawing;
		private CheckButton checkRed;
		private CheckButton checkGreen;
		private CheckButton checkBlue;
		private Button buttonReset;
		private Label labelTip;

		private class ControlPointDrawingInfo
		{
			public Cairo.Color Color { get; set; }
			public bool IsActive { get; set; }
		}

		//drawing area width and height
		private const int size = 256;
		//control point radius
		private const int radius = 6;

		private int channels;
		//last added control point x;
		private int last_cpx;
		private PointI last_mouse_pos = new (0, 0);

		//control points for luminosity transfer mode
		private SortedList<int, int>[] luminosity_cps = null!; // NRT - Set via code flow
								       //control points for rg transfer mode
		private SortedList<int, int>[] rgb_cps = null!;

		public SortedList<int, int>[] ControlPoints {
			get {
				return (Mode == ColorTransferMode.Luminosity) ? luminosity_cps : rgb_cps;
			}
			set {
				if (Mode == ColorTransferMode.Luminosity)
					luminosity_cps = value;
				else
					rgb_cps = value;
			}
		}

		public ColorTransferMode Mode {
			get {
				return (comboMap.Active == 0) ?
						ColorTransferMode.Rgb :
						ColorTransferMode.Luminosity;
			}
		}

		public CurvesData EffectData { get; private set; }

		public CurvesDialog (CurvesData effectData)
		{
			Title = Translations.GetString ("Curves");
			TransientFor = PintaCore.Chrome.MainWindow;
			Modal = true;
			this.AddCancelOkButtons ();
			this.SetDefaultResponse (ResponseType.Ok);

			Build ();

			EffectData = effectData;

			comboMap.OnChanged += HandleComboMapChanged;
			buttonReset.OnClicked += HandleButtonResetClicked;
			checkRed.OnToggled += HandleCheckToggled;
			checkGreen.OnToggled += HandleCheckToggled;
			checkBlue.OnToggled += HandleCheckToggled;

			drawing.SetDrawFunc ((area, context, width, height) => HandleDrawingDrawnEvent (context));

			var motion_controller = Gtk.EventControllerMotion.New ();
			motion_controller.OnMotion += HandleDrawingMotionNotifyEvent;
			motion_controller.OnLeave += (_, _) => InvalidateDrawing ();
			drawing.AddController (motion_controller);

			var click_controller = Gtk.GestureClick.New ();
			click_controller.SetButton (0); // Handle all buttons
			click_controller.OnPressed += HandleDrawingButtonPressEvent;
			drawing.AddController (click_controller);

			ResetControlPoints ();
		}

		private void UpdateLivePreview (string propertyName)
		{
			if (EffectData != null) {
				EffectData.ControlPoints = ControlPoints;
				EffectData.Mode = Mode;
				EffectData.FirePropertyChanged (propertyName);
			}
		}

		private void HandleCheckToggled (object? o, EventArgs args)
		{
			InvalidateDrawing ();
		}

		void HandleButtonResetClicked (object? sender, EventArgs e)
		{
			ResetControlPoints ();
			InvalidateDrawing ();
		}

		private void ResetControlPoints ()
		{
			channels = (Mode == ColorTransferMode.Luminosity) ? 1 : 3;
			ControlPoints = new SortedList<int, int>[channels];

			for (int i = 0; i < channels; i++) {
				SortedList<int, int> list = new SortedList<int, int> ();

				list.Add (0, 0);
				list.Add (size - 1, size - 1);
				ControlPoints[i] = list;
			}

			UpdateLivePreview ("ControlPoints");
		}

		private void HandleComboMapChanged (object? sender, EventArgs e)
		{
			if (ControlPoints == null)
				ResetControlPoints ();
			else
				UpdateLivePreview ("Mode");

			bool visible = (Mode == ColorTransferMode.Rgb);
			checkRed.Visible = checkGreen.Visible = checkBlue.Visible = visible;

			InvalidateDrawing ();
		}

		private void InvalidateDrawing ()
		{
			//to invalidate whole drawing area
			drawing.QueueDraw ();
		}

		private IEnumerable<SortedList<int, int>> GetActiveControlPoints ()
		{
			if (Mode == ColorTransferMode.Luminosity)
				yield return ControlPoints[0];
			else {
				if (checkRed.Active)
					yield return ControlPoints[0];

				if (checkGreen.Active)
					yield return ControlPoints[1];

				if (checkBlue.Active)
					yield return ControlPoints[2];
			}
		}

		private void AddControlPoint (int x, int y)
		{
			foreach (var controlPoints in GetActiveControlPoints ()) {
				controlPoints[x] = size - 1 - y;
			}

			last_cpx = x;

			UpdateLivePreview ("ControlPoints");
		}

		private void HandleDrawingMotionNotifyEvent (EventControllerMotion controller, EventControllerMotion.MotionSignalArgs args)
		{
			int x = (int) args.X;
			int y = (int) args.Y;

			last_mouse_pos = new (x, y);

			if (x < 0 || x >= size || y < 0 || y >= size)
				return;

			if (controller.GetCurrentEventState () == Gdk.ModifierType.Button1Mask) {
				// first and last control point cannot be removed
				if (last_cpx != 0 && last_cpx != size - 1) {
					foreach (var controlPoints in GetActiveControlPoints ()) {
						if (controlPoints.ContainsKey (last_cpx))
							controlPoints.Remove (last_cpx);
					}
				}

				AddControlPoint (x, y);
			}

			InvalidateDrawing ();
		}

		private void HandleDrawingButtonPressEvent (GestureClick controller, GestureClick.PressedSignalArgs args)
		{
			int x = (int) args.X;
			int y = (int) args.Y;

			if (controller.GetCurrentMouseButton () == MouseButton.Left) {
				AddControlPoint (x, y);
			}

			// user pressed right button
			if (controller.GetCurrentMouseButton () == MouseButton.Right) {
				foreach (var controlPoints in GetActiveControlPoints ()) {
					for (int i = 0; i < controlPoints.Count; i++) {
						int cpx = controlPoints.Keys[i];
						int cpy = size - 1 - (int) controlPoints.Values[i];

						//we cannot allow user to remove first or last control point
						if (cpx == 0 && cpy == size - 1)
							continue;
						if (cpx == size - 1 && cpy == 0)
							continue;

						if (CheckControlPointProximity (cpx, cpy, x, y)) {
							controlPoints.RemoveAt (i);
							break;
						}
					}
				}
			}

			InvalidateDrawing ();
		}

		private void DrawBorder (Context g)
		{
			g.Rectangle (0, 0, size - 1, size - 1);
			g.LineWidth = 1;
			g.Stroke ();
		}

		private void DrawPointerCross (Context g)
		{
			int x = last_mouse_pos.X;
			int y = last_mouse_pos.Y;

			if (x >= 0 && x < size && y >= 0 && y < size) {
				g.LineWidth = 0.5;
				g.MoveTo (x, 0);
				g.LineTo (x, size);
				g.MoveTo (0, y);
				g.LineTo (size, y);
				g.Stroke ();

				this.labelPoint.SetText (string.Format ("({0}, {1})", x, y));
			} else
				this.labelPoint.SetText (string.Empty);
		}

		private void DrawGrid (Context g)
		{
			g.SetDash (new double[] { 4, 4 }, 2);
			g.LineWidth = 1;

			for (int i = 1; i < 4; i++) {
				g.MoveTo (i * size / 4, 0);
				g.LineTo (i * size / 4, size);
				g.MoveTo (0, i * size / 4);
				g.LineTo (size, i * size / 4);
			}

			g.MoveTo (0, size - 1);
			g.LineTo (size - 1, 0);
			g.Stroke ();

			g.SetDash (new double[] { }, 0);
		}

		//cpx, cpyx - control point's x and y coordinates
		private bool CheckControlPointProximity (int cpx, int cpy, int x, int y)
		{
			return (Math.Sqrt (Math.Pow (cpx - x, 2) + Math.Pow (cpy - y, 2)) < radius);
		}

		private IEnumerator<ControlPointDrawingInfo> GetDrawingInfos ()
		{
			if (Mode == ColorTransferMode.Luminosity) {
				drawing.GetStyleContext ().GetColor (out var fg_color);
				yield return new ControlPointDrawingInfo () {
					Color = fg_color,
					IsActive = true
				};
			} else {
				yield return new ControlPointDrawingInfo () {
					Color = new Color (0.9, 0, 0),
					IsActive = checkRed.Active
				};
				yield return new ControlPointDrawingInfo () {
					Color = new Color (0, 0.9, 0),
					IsActive = checkGreen.Active
				};
				yield return new ControlPointDrawingInfo () {
					Color = new Color (0, 0, 0.9),
					IsActive = checkBlue.Active
				};
			}
		}

		private void DrawControlPoints (Context g)
		{
			int x = last_mouse_pos.X;
			int y = last_mouse_pos.Y;

			var infos = GetDrawingInfos ();

			foreach (var controlPoints in ControlPoints) {

				infos.MoveNext ();
				var info = infos.Current;

				for (int i = 0; i < controlPoints.Count; i++) {
					int cpx = controlPoints.Keys[i];
					int cpy = size - 1 - (int) controlPoints.Values[i];
					RectangleD rect;

					if (info.IsActive) {
						if (CheckControlPointProximity (cpx, cpy, x, y)) {
							rect = new RectangleD (cpx - (radius + 2) / 2, cpy - (radius + 2) / 2, radius + 2, radius + 2);
							g.DrawEllipse (rect, new Color (0.2, 0.2, 0.2), 2);
							rect = new RectangleD (cpx - radius / 2, cpy - radius / 2, radius, radius);
							g.FillEllipse (rect, new Color (0.9, 0.9, 0.9));
						} else {
							rect = new RectangleD (cpx - radius / 2, cpy - radius / 2, radius, radius);
							g.DrawEllipse (rect, info.Color, 2);
						}
					}

					rect = new RectangleD (cpx - (radius - 2) / 2, cpy - (radius - 2) / 2, radius - 2, radius - 2);
					g.FillEllipse (rect, info.Color);
				}
			}

			g.Stroke ();
		}

		private void DrawSpline (Context g)
		{
			var infos = GetDrawingInfos ();
			g.Save ();

			foreach (var controlPoints in ControlPoints) {

				int points = controlPoints.Count;
				SplineInterpolator interpolator = new SplineInterpolator ();
				IList<int> xa = controlPoints.Keys;
				IList<int> ya = controlPoints.Values;
				PointD[] line = new PointD[size];

				for (int i = 0; i < points; i++) {
					interpolator.Add (xa[i], ya[i]);
				}

				for (int i = 0; i < line.Length; i++) {
					line[i].X = (float) i;
					line[i].Y = (float) (Utility.Clamp (size - 1 - interpolator.Interpolate (i), 0, size - 1));

				}

				g.LineWidth = 2;
				g.LineJoin = LineJoin.Round;

				g.MoveTo (line[0].X, line[0].Y);
				for (int i = 1; i < line.Length; i++)
					g.LineTo (line[i].X, line[i].Y);

				infos.MoveNext ();
				var info = infos.Current;

				g.SetSourceColor (info.Color);
				g.LineWidth = info.IsActive ? 2 : 1;
				g.Stroke ();
			}

			g.Restore ();
		}

		private void HandleDrawingDrawnEvent (Context g)
		{
			drawing.GetStyleContext ().GetColor (out var fg_color);
			g.SetSourceColor (fg_color);

			DrawBorder (g);
			DrawPointerCross (g);
			DrawSpline (g);
			DrawGrid (g);
			DrawControlPoints (g);
		}

		[MemberNotNull (nameof (comboMap), nameof (labelPoint), nameof (labelTip), nameof (checkRed), nameof (checkGreen), nameof (checkBlue), nameof (buttonReset), nameof (drawing))]
		private void Build ()
		{
			Resizable = false;

			var content_area = this.GetContentAreaBox ();
			content_area.SetAllMargins (12);
			content_area.Spacing = 6;

			const int spacing = 6;
			var hbox1 = new Box () { Spacing = spacing };
			hbox1.SetOrientation (Orientation.Horizontal);
			hbox1.Append (Label.New (Translations.GetString ("Transfer Map")));

			comboMap = new ComboBoxText ();
			comboMap.AppendText (Translations.GetString ("RGB"));
			comboMap.AppendText (Translations.GetString ("Luminosity"));
			comboMap.Active = 1;
			hbox1.Append (comboMap);

			labelPoint = Label.New ("(256, 256)");
			labelPoint.Hexpand = true;
			labelPoint.Halign = Align.End;
			hbox1.Append (labelPoint);
			content_area.Append (hbox1);

			drawing = new DrawingArea () {
				WidthRequest = 256,
				HeightRequest = 256,
				CanFocus = true
			};
			drawing.SetAllMargins (8);
			content_area.Append (drawing);

			var hbox2 = new Box ();
			hbox2.SetOrientation (Orientation.Horizontal);
			checkRed = new CheckButton () { Label = Translations.GetString ("Red  "), Active = true };
			checkGreen = new CheckButton () { Label = Translations.GetString ("Green"), Active = true };
			checkBlue = new CheckButton () { Label = Translations.GetString ("Blue "), Active = true };
			hbox2.Prepend (checkRed);
			hbox2.Prepend (checkGreen);
			hbox2.Prepend (checkBlue);

			buttonReset = new Button () {
				WidthRequest = 81,
				HeightRequest = 30,
				Label = Translations.GetString ("Reset"),
				Halign = Align.End,
				Hexpand = true
			};
			hbox2.Append (buttonReset);
			content_area.Append (hbox2);

			labelTip = Label.New (Translations.GetString ("Tip: Right-click to remove control points."));
			content_area.Append (labelTip);

			checkRed.Hide ();
			checkGreen.Hide ();
			checkBlue.Hide ();
		}
	}
}
