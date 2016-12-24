using Catrobat.Paint.WindowsPhone.Command;
using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Catrobat.Paint.WindowsPhone.Tool
{
    class MoveZoomTool : ToolBase
    {
        private TransformGroup _transforms;
        private double DISPLAY_HEIGHT_HALF;
        private double DISPLAY_WIDTH_HALF;
        private Point startScale;
        private TransformGroup _tempTransforms;

        public void setGridWorkingSpaceRenderTransform()
        {
            if (PocketPaintApplication.GetInstance().GridWorkingSpace.RenderTransform.GetType() == typeof(TransformGroup))
            {
                _transforms = PocketPaintApplication.GetInstance().GridWorkingSpace.RenderTransform as TransformGroup;
            }

            if (_transforms == null)
            {
                PocketPaintApplication.GetInstance().GridWorkingSpace.RenderTransform = _transforms = new TransformGroup();
            }
        }

        public MoveZoomTool(ToolType toolType=ToolType.Zoom)
        {
            ToolType = toolType; 

            setGridWorkingSpaceRenderTransform();
            ResetCanvas();
         
            _tempTransforms = new TransformGroup();
            DISPLAY_WIDTH_HALF = PocketPaintApplication.GetInstance().GridWorkingSpace.ActualWidth / 2.0;
            DISPLAY_HEIGHT_HALF = PocketPaintApplication.GetInstance().GridWorkingSpace.ActualHeight / 2.0;

            startScale.X = _transforms.Value.M11;
            startScale.Y = _transforms.Value.M22;       
        }

        public override void HandleDown(object arg)
        {
            System.Diagnostics.Debug.WriteLine(ToolType.Zoom.ToString() + "Handle-Down-Method");
            startScale.X = ((Point)arg).X;
            startScale.Y = ((Point)arg).Y;
        }

        public override void HandleMove(object arg)
        {
            System.Diagnostics.Debug.WriteLine(ToolType.Zoom.ToString() + "Handle-Move-Method");
            if (arg is ScaleTransform)
            {
                var toScaleValue = (ScaleTransform)arg;

                bool isScaleAllowed = checkIfScalingAllowed(toScaleValue);
                if (isScaleAllowed)
                {
                    var fixedaspection = 0.0;
                    fixedaspection = toScaleValue.ScaleX > toScaleValue.ScaleY ? toScaleValue.ScaleX : toScaleValue.ScaleY;
                    toScaleValue.ScaleX = fixedaspection;
                    toScaleValue.ScaleY = fixedaspection;
                    if (PocketPaintApplication.GetInstance().isZoomButtonClicked)
                    {
                        toScaleValue.CenterX = DISPLAY_WIDTH_HALF;
                        toScaleValue.CenterY = DISPLAY_HEIGHT_HALF;
                    }
                    else
                    {
                        toScaleValue.CenterX = startScale.X;
                        toScaleValue.CenterY = startScale.Y;
                    }
                    _transforms.Children.Add(toScaleValue);
                    _tempTransforms.Children.Add(toScaleValue);
            }
                }
            else if (arg is TranslateTransform)
            {
                var move = (TranslateTransform)arg;
                _transforms.Children.Add(move);
                _tempTransforms.Children.Add(move);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("MoveZoomTool Should Not Reach this!");
                return;
            }

            setResetButtonInPaintingAreaView(true);
        }
        
        public override void HandleUp(object arg)
        {
            System.Diagnostics.Debug.WriteLine(ToolType.Zoom.ToString() + "Handle-Up-Method");
        }

        public override void Draw(object o)
        {
            System.Diagnostics.Debug.WriteLine(ToolType.Zoom.ToString() + "Draw-Method");
        }

        public void setResetButtonInPaintingAreaView(bool resetButtuonValue)
        {
            AppBarButton appBarButtonReset = PocketPaintApplication.GetInstance().PaintingAreaView.GetAppBarResetButton();
            if (appBarButtonReset != null)
            {
                appBarButtonReset.IsEnabled = resetButtuonValue;
                PocketPaintApplication.GetInstance().PaintingAreaView.isWorkingSpaceMoved(resetButtuonValue);
            }
        }
        public override void ResetDrawingSpace()
        {
            _transforms.Children.Clear();
            setResetButtonInPaintingAreaView(false);
            PocketPaintApplication.GetInstance().PaintingAreaView.AlignPositionOfGridWorkingSpace(null);
        }

        public bool checkIfScalingAllowed(ScaleTransform toScaleValue)
        {
            bool isScaleAllowed = false;

            if ((toScaleValue.ScaleX > 1.0) && ((Math.Abs(_transforms.Value.M11) < 10.0) && (Math.Abs(_transforms.Value.M12) < 10.0)))
            {
                isScaleAllowed = true;
            }
            else if ((toScaleValue.ScaleX < 1.0) && ((Math.Abs(_transforms.Value.M11) > 0.5) || Math.Abs(_transforms.Value.M12) > 0.5))
            {
                isScaleAllowed = true;
            }
            return isScaleAllowed;
        }

        public ScaleTransform getLastScaleTransformation()
        {
            for (int i = 0; i < _transforms.Children.Count; i++)
            {
                if (_transforms.Children[i].GetType() == typeof(ScaleTransform))
                {
                    ScaleTransform scaleTransform = new ScaleTransform();
                    scaleTransform.ScaleX = ((ScaleTransform)_transforms.Children[i]).ScaleX;
                    scaleTransform.ScaleY = ((ScaleTransform)_transforms.Children[i]).ScaleY;
                    scaleTransform.CenterX = ((ScaleTransform)_transforms.Children[i]).CenterX;
                    scaleTransform.CenterY = ((ScaleTransform)_transforms.Children[i]).CenterY;
                    return scaleTransform;
                }
            }

            return null;
        }

        public void addTransformation(Transform currentTransform)
        {
            for (int i = 0; i < _transforms.Children.Count; i++)
            {
                if (_transforms.Children[i].GetType() == currentTransform.GetType())
                {
                    _transforms.Children.RemoveAt(i);
                }
            }
            _transforms.Children.Add(currentTransform);
        }

        public override void ResetUsedElements()
        {
        }
    }
}
