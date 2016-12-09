﻿using System;
using Catrobat.Paint.WindowsPhone.Command;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Catrobat.Paint.WindowsPhone.Controls.UserControls;

namespace Catrobat.Paint.WindowsPhone.Tool
{
    public class RectangleTool : RectangleShapeBaseTool
    {
        private Path m_path;

        public RectangleTool()
        {
            this.ToolType = ToolType.Rect;
            this.RectangleShapeBase = PocketPaintApplication.GetInstance().RectangleSelectionControl
                .RectangleShapeBase;
        }

        public override void Draw(object o)
        {
            var coordinate = (Point)o;
            var strokeThickness = PocketPaintApplication.GetInstance().PaintData.strokeThickness;

            RectangleSelectionControl currentRectangleSelectionControl = PocketPaintApplication.GetInstance().RectangleSelectionControl;
            Rectangle rectangleToDraw = currentRectangleSelectionControl.RectangleToDraw;

            double widthOfRectangleToDraw = rectangleToDraw.Width - strokeThickness;
            double heightOfRectangleToDraw = rectangleToDraw.Height - strokeThickness;

            Rect rect = new Rect();
            rect.X = coordinate.X + strokeThickness / 2;
            rect.Y = coordinate.Y + strokeThickness / 2;
            var angleOfWorkingspace = PocketPaintApplication.GetInstance().angularDegreeOfWorkingSpaceRotation;

            switch (angleOfWorkingspace)
            {
                case 0:
                case 180:
                    rect.Width = widthOfRectangleToDraw;
                    rect.Height = heightOfRectangleToDraw;
                    break;
                case 90:
                case 270:
                    rect.Width = heightOfRectangleToDraw;
                    rect.Height = widthOfRectangleToDraw;
                    break;
            }

            RectangleGeometry myRectGeometry = new RectangleGeometry();
            myRectGeometry.Rect = rect;

            RotateTransform lastRotateTransform = this.RectangleShapeBase.GetLastRotateTransformation();
            if (lastRotateTransform != null)
            {
                RotateTransform rotateTransform = new RotateTransform();
                rotateTransform.CenterX = currentRectangleSelectionControl.GetControlCenterPoint().X;
                rotateTransform.CenterY = currentRectangleSelectionControl.GetControlCenterPoint().Y;
                rotateTransform.Angle = lastRotateTransform.Angle;
                myRectGeometry.Transform = rotateTransform;
            }

            m_path = new Path
            {
                Fill = PocketPaintApplication.GetInstance().PaintData.colorSelected,
                Stroke = PocketPaintApplication.GetInstance().PaintData.strokeColorSelected,
                StrokeThickness = strokeThickness ,
                StrokeLineJoin =
                    PocketPaintApplication.GetInstance().RectangleSelectionControl.StrokeLineJoinOfRectangleToDraw,
                Data = myRectGeometry
            };
            PocketPaintApplication.GetInstance().PaintingAreaView.addElementToPaintingAreCanvas(m_path);

            var rectangleGeometry = new RectangleGeometry
            {
                Rect = new Rect(0, 0, PocketPaintApplication.GetInstance().PaintingAreaCanvas.ActualWidth,
                PocketPaintApplication.GetInstance().PaintingAreaCanvas.ActualHeight)
            };
            m_path.Clip = rectangleGeometry;
            m_path.InvalidateArrange();
            m_path.InvalidateMeasure();

            CommandManager.GetInstance().CommitCommand(new RectangleCommand(m_path));
        }

        public override void ResetDrawingSpace()
        {
            PocketPaintApplication.GetInstance().RectangleSelectionControl.ResetRectangleSelectionControl();
        }
    }
}
