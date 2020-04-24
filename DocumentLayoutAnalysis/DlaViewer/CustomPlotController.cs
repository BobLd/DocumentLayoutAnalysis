namespace DlaViewer
{
    using OxyPlot;

    public class CustomPlotController : PlotController
    {
        private readonly OxyModifierKeys _zoomOxyModifierKeys = OxyModifierKeys.Control;

        public CustomPlotController() : base()
        {
            this.BindMouseDown(OxyMouseButton.Left, PanZoomAt);
            this.BindMouseEnter(OxyPlot.PlotCommands.HoverSnapTrack);
            this.BindMouseDown(OxyMouseButton.Left, _zoomOxyModifierKeys, OxyPlot.PlotCommands.ZoomRectangle);
            this.BindMouseDown(OxyMouseButton.Left, OxyModifierKeys.None, 2, OxyPlot.PlotCommands.ResetAt);

            this.UnbindMouseDown(OxyMouseButton.Middle);
            this.UnbindMouseDown(OxyMouseButton.Right);
            this.UnbindKeyDown(OxyKey.C, OxyModifierKeys.Control | OxyModifierKeys.Alt);
            this.UnbindKeyDown(OxyKey.R, OxyModifierKeys.Control | OxyModifierKeys.Alt);
            this.UnbindKeyDown(OxyKey.Up);
            this.UnbindKeyDown(OxyKey.Down);
            this.UnbindKeyDown(OxyKey.Left);
            this.UnbindKeyDown(OxyKey.Right);

            this.UnbindKeyDown(OxyKey.Up, OxyModifierKeys.Control);
            this.UnbindKeyDown(OxyKey.Down, OxyModifierKeys.Control);
            this.UnbindKeyDown(OxyKey.Left, OxyModifierKeys.Control);
            this.UnbindKeyDown(OxyKey.Right, OxyModifierKeys.Control);
            this.UnbindMouseWheel();
        }

        private static readonly IViewCommand<OxyMouseDownEventArgs> PanZoomAt = new DelegatePlotCommand<OxyMouseDownEventArgs>(
            (view, controller, args) => controller.AddMouseManipulator(view, new PanZoomManipulator(view), args));
    }

    public class PanZoomManipulator : MouseManipulator
    {
        public PanZoomManipulator(IPlotView plotView) : base(plotView)
        { }

        private ScreenPoint PreviousPosition { get; set; }
        private DataPoint PreviousPositionShortTerm { get; set; }
        private bool IsPanEnabled { get; set; }

        public override void Completed(OxyMouseEventArgs e)
        {
            base.Completed(e);

            if (!this.IsPanEnabled)
            {
                return;
            }

            this.View.SetCursorType(CursorType.Default);
            e.Handled = true;
        }

        public override void Delta(OxyMouseEventArgs e)
        {
            base.Delta(e);
            if (this.PreviousPosition.Equals(e.Position))
            {
                e.Handled = true;
                return;
            }

            if (!this.IsPanEnabled)
            {
                e.Handled = true;
                return;
            }

            DataPoint current = this.InverseTransform(e.Position.X, e.Position.Y);
            const double inScale = 1.03;
            const double outScale = 0.97;

            if (this.XAxis != null && this.YAxis != null)
            {
                // this is pan
                this.XAxis.Pan(this.PreviousPosition, e.Position);
                this.YAxis.Pan(this.PreviousPosition, e.Position);
            }
            else
            {
                double scale;
                // this is zoom
                if (this.YAxis != null && this.YAxis.IsZoomEnabled)
                {
                    if (this.PreviousPositionShortTerm.Y - current.Y > 0)
                    {
                        scale = outScale;
                    }
                    else if (this.PreviousPositionShortTerm.Y - current.Y < 0)
                    {
                        scale = inScale;
                    }
                    else
                    {
                        scale = 1;
                    }

                    PreviousPositionShortTerm = this.InverseTransform(e.Position.X, e.Position.Y);
                    this.YAxis.ZoomAt(scale, current.Y);
                }

                if (this.XAxis != null && this.XAxis.IsZoomEnabled)
                {
                    if (this.PreviousPositionShortTerm.X - current.X > 0)
                    {
                        scale = inScale;
                    }
                    else if (this.PreviousPositionShortTerm.X - current.X < 0)
                    {
                        scale = outScale;
                    }
                    else
                    {
                        scale = 1;
                    }
                    PreviousPositionShortTerm = this.InverseTransform(e.Position.X, e.Position.Y);
                    this.XAxis.ZoomAt(scale, current.X);
                }
            }
            this.PlotView.InvalidatePlot(false);
            this.PreviousPosition = e.Position;
            e.Handled = true;
        }

        public override void Started(OxyMouseEventArgs e)
        {
            base.Started(e);
            this.PreviousPosition = e.Position;

            this.IsPanEnabled = (this.XAxis != null && this.XAxis.IsPanEnabled)
                || (this.YAxis != null && this.YAxis.IsPanEnabled);

            if (this.IsPanEnabled)
            {
                if (this.XAxis != null && this.YAxis != null)
                {
                    this.View.SetCursorType(CursorType.Pan);
                }
                else if (this.XAxis == null && this.YAxis != null)
                {
                    this.View.SetCursorType(CursorType.ZoomVertical);
                }
                else if (this.XAxis != null && this.YAxis == null)
                {
                    this.View.SetCursorType(CursorType.ZoomHorizontal);
                }
                e.Handled = true;
            }
        }
    }
}
