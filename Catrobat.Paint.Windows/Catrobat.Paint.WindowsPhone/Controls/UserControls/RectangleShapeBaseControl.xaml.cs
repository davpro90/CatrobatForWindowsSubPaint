using System;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;


// Die Elementvorlage "Benutzersteuerelement" ist unter http://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace Catrobat.Paint.WindowsPhone.Controls.UserControls
{
    public sealed partial class RectangleShapeBaseControl
    {
        private const double m_DefaultGridMainSize = 290.0;
        private const double m_DefaultRectangleForMovementSize = 200.0;
        private const double m_DefaultAreaToDrawSize = 160.0;
        private const double m_MinWidthRectangleToDraw = 20;
        private const double m_MinHeightRectangleToDraw = 20;

        private Point m_CenterPointRotation;
        private readonly TransformGroup m_TransformGridMain;
        private Point m_CornerPoint = new Point(0.0, 0.0);
        private Thickness m_currentGridMainMargin = new Thickness();

        private float m_currentRotationAngle;
        private float m_RotationAngle;

        public Grid AreaToDraw { get; private set; }

        // corner_value = -1
        //   is not initialised
        // corner_value = 0
        //   CenterLeftCorner
        //   TopLeftCorner
        //   TopCenterCorner
        // corner_value = 1
        //   TopRightCorner
        // corner_value = 2
        //   CenterRightCorner
        //   BottomRightCorner
        //   BottomCenterCorner
        // corner_value = 3
        //   LeftBottomCorner
        // HACK: Is needed to resize rotated objects
        private int corners_value = -1;
        private double m_center_x;
        private double m_center_y;

        private enum Orientation
        {
            Top, Bottom, Left, Right, TopLeft, TopRight, BottomRight, BottomLeft
        }

        public RectangleShapeBaseControl()
        {
            InitializeComponent();

            AreaToDraw = AreaToDrawGrid;

            GridMainSelection.RenderTransform = m_TransformGridMain = new TransformGroup();
            m_RotationAngle = 0;

            m_CenterPointRotation = new Point
            {
                X = 0.0,
                Y = 0.0
            };
            IsModifiedRectangleForMovement = false;

            m_center_x = (GridMainSelection.Width / 2.0);
            m_center_y = (GridMainSelection.Height / 2.0);
        }

        public void TodoNeuerName(double center_x, double center_y, Point transformOrigin)
        {
            m_currentGridMainMargin = GridMainSelection.Margin;
            double currentGridMainHeight = GridMainSelection.Height;
            double currentGridMainWidth = GridMainSelection.Width;

            double centerX = m_center_x;
            double centerY = m_center_y;
       
            ResetRectangleShapeBaseControl();
            m_center_x = centerX;
            m_center_y = centerY;

            GridMainSelection.Height = currentGridMainHeight;
            GridMainSelection.Width = currentGridMainWidth;
            GridMainSelection.Margin = m_currentGridMainMargin;
            GridMainSelection.RenderTransformOrigin = transformOrigin;

            MovementRectangle.Height = currentGridMainHeight - 90;
            MovementRectangle.Width = currentGridMainWidth - 90;

            AreaToDrawGrid.Height = currentGridMainHeight - 130;
            AreaToDrawGrid.Width = currentGridMainWidth - 130;

            RotateTransform rotateTransform = GridMainSelection.RenderTransform as RotateTransform;
            var ct = new RotateTransform
            {
                Angle = m_currentRotationAngle,
                CenterX = m_center_x,
                CenterY = m_center_y
            };
            addTransformation(ct);
        }

        public bool IsModifiedRectangleForMovement { get; set; }

        public bool ShouldHeightOfSelectionChanged(double heightOfSelection)
        {
            var result = heightOfSelection >= 1.0;
            return result;
        }
        public bool ShouldWidthOfSelectionChanged(double widthOfSelection)
        {
            var result = widthOfSelection >= 1.0;
            return result;
        }

        public void setCenterxAndCenteryCoordinates(double centerX, double centerY)
        {
            m_center_x = centerX;
            m_center_y = centerY;
        }

        private void TopCenterGrid_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            setCenterxAndCenteryCoordinates((GridMainSelection.Width / 2.0) * -1, (GridMainSelection.Height / 2.0) * -1);
            if (corners_value != 0)
            {
                TodoNeuerName(m_center_x , m_center_y, new Point(1, 1));
                corners_value = 0;
            }
            resizeHeight(e.Delta.Translation.X, e.Delta.Translation.Y, Orientation.Top);
        }

        private void TopLeftGrid_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            setCenterxAndCenteryCoordinates((GridMainSelection.Width / 2.0) * -1, (GridMainSelection.Height / 2.0) * -1);
            if (corners_value != 0)
            {
                TodoNeuerName(m_center_x, m_center_y, new Point(1, 1));
                corners_value = 0;
            }
            else
            {
                resizeHeight(e.Delta.Translation.X, e.Delta.Translation.Y, Orientation.Top);
                resizeWidth(e.Delta.Translation.X, e.Delta.Translation.Y, Orientation.Left);
            }
        }

        private void TopRightGrid_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            setCenterxAndCenteryCoordinates(GridMainSelection.Width / 2.0, (GridMainSelection.Height / 2.0) * -1);
            if (corners_value != 1)
            {
                TodoNeuerName(m_center_x, m_center_y, new Point(0, 1));
                corners_value = 1;
            }
            else
            {
                resizeHeight(e.Delta.Translation.X, e.Delta.Translation.Y, Orientation.Top);
                resizeWidth(e.Delta.Translation.X, e.Delta.Translation.Y, Orientation.Right);
            }
        }

        private void CenterRightGrid_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            setCenterxAndCenteryCoordinates(GridMainSelection.Width / 2.0, GridMainSelection.Height / 2.0);
            if (corners_value != 2)
            {
                TodoNeuerName(m_center_x, m_center_y, new Point(0, 0));
                corners_value = 2;
            }
            resizeWidth(e.Delta.Translation.X, e.Delta.Translation.Y, Orientation.Right);
        }

        private void BottomRightGrid_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            setCenterxAndCenteryCoordinates(GridMainSelection.Width / 2.0, GridMainSelection.Height / 2.0);
            if (corners_value != 2)
            {
                TodoNeuerName(m_center_x, m_center_y, new Point(0, 0));
                corners_value = 2;
            }
            resizeHeight(e.Delta.Translation.X, e.Delta.Translation.Y, Orientation.Bottom);
            resizeWidth(e.Delta.Translation.X, e.Delta.Translation.Y, Orientation.Right);
        }

        private void BottomCenterGrid_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            setCenterxAndCenteryCoordinates(GridMainSelection.Width / 2.0, GridMainSelection.Height / 2.0);
            if (corners_value != 2)
            {
                TodoNeuerName(m_center_x, m_center_y, new Point(0, 0));
                corners_value = 2;
            }
            resizeHeight(e.Delta.Translation.X, e.Delta.Translation.Y, Orientation.Bottom);
        }

        private void BottomLeftGrid_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            setCenterxAndCenteryCoordinates((GridMainSelection.Width / 2.0) * -1, GridMainSelection.Height / 2.0);
            if (corners_value != 3)
            {
                TodoNeuerName(m_center_x, m_center_y, new Point(1, 0));
                corners_value = 3;
            }
            resizeHeight(e.Delta.Translation.X, e.Delta.Translation.Y, Orientation.Bottom);
            resizeWidth(e.Delta.Translation.X, e.Delta.Translation.Y, Orientation.Left);
        }

        private void CenterLeftGrid_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            setCenterxAndCenteryCoordinates((GridMainSelection.Width / 2.0) * -1, (GridMainSelection.Height / 2.0) * -1);
            if (corners_value != 0)
            {
                TodoNeuerName(m_center_x, m_center_y, new Point(1, 1));
                corners_value = 0;
            }
            resizeWidth(e.Delta.Translation.X, e.Delta.Translation.Y, Orientation.Left);
        }

        private void rectEllipseForMovement_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var xVal = e.Delta.Translation.X;
            var yVal = e.Delta.Translation.Y;

            GridMainSelection.Margin = new Thickness(GridMainSelection.Margin.Left + xVal,
                                                     GridMainSelection.Margin.Top + yVal,
                                                     GridMainSelection.Margin.Right - xVal,
                                                     GridMainSelection.Margin.Bottom - yVal);
        }

        private void rectEllipseForMovement_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Debug.Assert(AreaToDraw.Children.Count == 1);
            UIElement elementToDraw = AreaToDraw.Children[0];

            Grid grid = GridMainSelection;
            var centerCoordinateOfGridMain = GetCenterCoordinateOfGridMain();

            Point leftTopPoint = new Point();
            leftTopPoint.X = centerCoordinateOfGridMain.X - AreaToDraw.Width / 2.0;
            leftTopPoint.Y = centerCoordinateOfGridMain.Y - AreaToDraw.Height / 2.0;

            var angle = PocketPaintApplication.GetInstance().angularDegreeOfWorkingSpaceRotation;

            //switch (angle)
            //{
            //    case 0:
            //        coord.X -= coord2.X;
            //        coord.Y -= coord2.Y;
            //        coord.X += (AreaToDraw.Width / 2);
            //        coord.Y += (AreaToDraw.Height / 2);
            //        break;
            //    case 90:
            //        coord.X -= coord2.Y;
            //        coord.Y -= (AreaToDraw.Width - coord2.X);
            //        coord.X += (AreaToDraw.Width / 2);
            //        coord.Y += (AreaToDraw.Height / 2);
            //        break;
            //    case 180:
            //        coord.X += coord2.X;
            //        coord.Y += coord2.Y;
            //        coord.X -= (AreaToDraw.Width / 2);
            //        coord.Y -= (AreaToDraw.Height / 2);
            //        break;
            //    case 270:
            //        coord.X -= (AreaToDraw.Height - coord2.Y);
            //        coord.Y -= coord2.X;
            //        coord.X += (AreaToDraw.Width / 2);
            //        coord.Y += (AreaToDraw.Height / 2);
            //        break;
            //}
            PocketPaintApplication.GetInstance().ToolCurrent.Draw(leftTopPoint);
        }

        private void resizeWidth(double deltaX, double deltaY, Orientation orientation)
        {
            Debug.Assert(orientation == Orientation.Left || orientation == Orientation.Right);

            float rotation = m_RotationAngle;
            while (rotation < 0)
            {
                rotation += 360;
            }

            double rotationRadian = PocketPaintApplication.DegreeToRadian(rotation);
            double deltaXCorrected = Math.Cos(-rotationRadian) * (deltaX)
                    - Math.Sin(-rotationRadian) * (deltaY);

            if (orientation == Orientation.Left)
            {
                System.Diagnostics.Debug.WriteLine("\nhier");
                deltaXCorrected = deltaXCorrected * -1;
            }

            double newWidthRectangleToDraw = AreaToDrawGrid.Width + deltaXCorrected;
            if (newWidthRectangleToDraw < m_MinWidthRectangleToDraw)
            {
                newWidthRectangleToDraw = m_MinWidthRectangleToDraw;
                deltaXCorrected = m_MinWidthRectangleToDraw - AreaToDrawGrid.Width;
            }

            SetWidthOfControl(newWidthRectangleToDraw);

            double deltaXLeft = 0;
            double deltaXRight = 0;
            if (orientation == Orientation.Left)
            {
                deltaXLeft = deltaXCorrected;
                //deltaXRight = deltaXCorrected * -1;
            }
            else if (orientation == Orientation.Right)
            {
                deltaXRight = deltaXCorrected;
            }

            setGridMainSelectionMargin(GridMainSelection.Margin.Left - deltaXLeft,
                                            GridMainSelection.Margin.Top,
                                            GridMainSelection.Margin.Right - deltaXRight,
                                            GridMainSelection.Margin.Bottom);

            PocketPaintApplication.GetInstance().BarRecEllShape.setBtnWidthValue = newWidthRectangleToDraw;
        }

        private void resizeHeight(double deltaX, double deltaY, Orientation orientation)
        {
            Debug.Assert(orientation == Orientation.Top || orientation == Orientation.Bottom);

            float rotation = m_RotationAngle;
            while (rotation < 0)
            {
                rotation += 360;
            }
            double rotationRadian = PocketPaintApplication.DegreeToRadian(rotation);


            double deltaYCorrected = Math.Sin(-rotationRadian) * (deltaX)
                    + Math.Cos(-rotationRadian) * (deltaY);
            if (orientation == Orientation.Top)
            {
                deltaYCorrected = deltaYCorrected * -1;
            }

            double newHeightRectangleToDraw = AreaToDrawGrid.Height + deltaYCorrected;
            if (newHeightRectangleToDraw < m_MinHeightRectangleToDraw)
            {
                newHeightRectangleToDraw = m_MinHeightRectangleToDraw;
                deltaYCorrected = m_MinHeightRectangleToDraw - AreaToDrawGrid.Height;
            }

            SetHeightOfControl(newHeightRectangleToDraw);

            double deltaYTop = 0;
            double deltaYBottom = 0;
            if (orientation == Orientation.Top)
            {
                deltaYTop = deltaYCorrected;
            }
            else if (orientation == Orientation.Bottom)
            {
                deltaYBottom = deltaYCorrected;
            }


            setGridMainSelectionMargin(GridMainSelection.Margin.Left,
                                            GridMainSelection.Margin.Top - deltaYTop,
                                            GridMainSelection.Margin.Right,
                                            GridMainSelection.Margin.Bottom - deltaYBottom);
            PocketPaintApplication.GetInstance().BarRecEllShape.setBtnHeightValue = newHeightRectangleToDraw;
        }

        public Point GetCenterCoordinateOfGridMain()
        {
            double halfScreenHeight = Window.Current.Bounds.Height / 2.0;
            double halfScreenWidth = Window.Current.Bounds.Width / 2.0;

            TranslateTransform lastTranslateTransform = GetLastTranslateTransformation();
            RotateTransform lastRotateTransform = GetLastRotateTransformation();

            double offsetX;
            double offsetY;
            if (lastTranslateTransform != null && lastRotateTransform == null)
            {
                offsetX = lastTranslateTransform.X;
                offsetY = lastTranslateTransform.Y;
            }
            else if (lastTranslateTransform == null && lastRotateTransform != null)
            {
                offsetX = 0.0;
                offsetY = 0.0;
            }
            else if (lastTranslateTransform != null && lastRotateTransform != null)
            {
                offsetX = lastTranslateTransform.X;
                offsetY = lastTranslateTransform.Y;
            }
            else
            {
                offsetX = 0.0;
                offsetY = 0.0;
            }

            double marginOffsetX = GridMainSelection.Margin.Left - GridMainSelection.Margin.Right;
            double marginOffsetY = GridMainSelection.Margin.Top - GridMainSelection.Margin.Bottom;

            double coordinateX = offsetX + halfScreenWidth + (marginOffsetX / 2.0);
            double coordinateY = offsetY + halfScreenHeight + (marginOffsetY / 2.0);

            return new Point(coordinateX, coordinateY);
        }

        public Point GetCenterCoordinateOfFrameworkElement(FrameworkElement e)
        {
            double halfScreenHeight = Window.Current.Bounds.Height / 2.0;
            double halfScreenWidth = Window.Current.Bounds.Width / 2.0;

            TranslateTransform lastTranslateTransform = GetLastTranslateTransformation();
            RotateTransform lastRotateTransform = GetLastRotateTransformation();
            
            double offsetX;
            double offsetY;
            if (lastTranslateTransform != null)
            {
                offsetX = lastTranslateTransform.X;
                offsetY = lastTranslateTransform.Y;
            }
            else
            {
                offsetX = 0.0;
                offsetY = 0.0;
            }

            double marginOffsetX = e.Margin.Left - e.Margin.Right;
            double marginOffsetY = e.Margin.Top - e.Margin.Bottom;

            double coordinateX = offsetX + halfScreenWidth + (marginOffsetX / 2.0);
            double coordinateY = offsetY + halfScreenHeight + (marginOffsetY / 2.0);

            return new Point(coordinateX, coordinateY); 
        }

        public void addTransformation(Transform currentTransform)
        {
            for (int i = 0; i < m_TransformGridMain.Children.Count; i++)
            {
                if (m_TransformGridMain.Children[i].GetType() == currentTransform.GetType())
                {
                    m_TransformGridMain.Children.RemoveAt(i);
                }
            }

            m_TransformGridMain.Children.Add(currentTransform);

            ResetAppBarButtonRectangleSelectionControl(true);
            IsModifiedRectangleForMovement = true;
        }

        public void ResetAppBarButtonRectangleSelectionControl(bool isActivated)
        {
            var paintingAreaView = PocketPaintApplication.GetInstance().PaintingAreaView;
            if (paintingAreaView != null)
            {
                var appBarButtonReset = paintingAreaView.getAppBarResetButton();
                if (appBarButtonReset != null)
                {
                    appBarButtonReset.IsEnabled = isActivated;
                }
            }
        }

        public TranslateTransform GetLastTranslateTransformation()
            {
            return m_TransformGridMain.Children.OfType<TranslateTransform>().Select(t => new TranslateTransform
                {
                X = t.X, 
                Y = t.Y
            }).FirstOrDefault();
        }

        public RotateTransform GetLastRotateTransformation()
        {
            return m_TransformGridMain.Children.OfType<RotateTransform>().Select(t => new RotateTransform
                    {
                CenterX = t.CenterX, 
                CenterY = t.CenterY, 
                Angle = t.Angle
            }).FirstOrDefault();
        }

        public void SetHeightOfControl(double newHeightRectangleToDraw)
        {
            //TODO 1 is maybe too small?
            if (newHeightRectangleToDraw >= 1)
            {
                GridMainSelection.Height = newHeightRectangleToDraw + (GridMainSelection.Height - AreaToDrawGrid.Height);
                MovementRectangle.Height = newHeightRectangleToDraw + (MovementRectangle.Height - AreaToDrawGrid.Height);
                AreaToDrawGrid.Height = newHeightRectangleToDraw;
            }
        }

        // Rename function or change the content of function
        // functionname and content are disagreed
        public double heightOfRectangleToDraw
        {
            get
            {
                return AreaToDrawGrid.Height;
            }
            set
            {
                AreaToDrawGrid.Height = value;
            }
        }

        // Rename function or change the content of function
        // functionname and content are disagreed
        public void SetWidthOfControl(double newWidthRectangleToDraw)
        {
            //TODO 1 is maybe too small?
            if (newWidthRectangleToDraw >= 1)
            {
                GridMainSelection.Width = newWidthRectangleToDraw + (GridMainSelection.Width - AreaToDrawGrid.Width);
                MovementRectangle.Width = newWidthRectangleToDraw + (MovementRectangle.Width - AreaToDrawGrid.Width);
                AreaToDrawGrid.Width = newWidthRectangleToDraw;
            }
        }
        public double widthOfRectangleToDraw
        {
            get
            {
                return AreaToDrawGrid.Width;
            }
            set
            {
                AreaToDrawGrid.Width = value;
            }
        }

        public Thickness CurrentGridMainMargin
        {
            get
            {
                return m_currentGridMainMargin;
            }

            set
            {
                m_currentGridMainMargin = value;
            }
        }

        private void setGridMainSelectionMargin(double leftMargin, double topMargin, double rightMargin, double bottomMargin)
        {        
            GridMainSelection.Margin = new Thickness(leftMargin, topMargin, rightMargin, bottomMargin);
        }

        public void ResetRectangleShapeBaseControl()
        {
            GridMainSelection.Width = m_DefaultGridMainSize;
            GridMainSelection.Height = m_DefaultGridMainSize;

            MovementRectangle.Width = m_DefaultRectangleForMovementSize;
            MovementRectangle.Height = m_DefaultRectangleForMovementSize;

            AreaToDrawGrid.Width = m_DefaultAreaToDrawSize;
            AreaToDrawGrid.Height = m_DefaultAreaToDrawSize;
            PocketPaintApplication.GetInstance().BarRecEllShape.setBtnHeightValue = m_DefaultAreaToDrawSize;
            PocketPaintApplication.GetInstance().BarRecEllShape.setBtnWidthValue = m_DefaultAreaToDrawSize;

            m_TransformGridMain.Children.Clear();
            m_RotationAngle = 0;

            m_CenterPointRotation.X = 0.0;
            m_CenterPointRotation.Y = 0.0;

            m_center_x = GridMainSelection.Width / 2.0;
            m_center_y = GridMainSelection.Height / 2.0;

            m_CornerPoint.X = 0.0;
            m_CornerPoint.Y = 0.0;

            ResetAppBarButtonRectangleSelectionControl(false);
            IsModifiedRectangleForMovement = false;
            GridMainSelection.Margin = new Thickness(0,0,0,0);
        }

        private void RotationTopRight_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            Rotate(e.Position, e.Delta.Translation.X, e.Delta.Translation.Y, Orientation.TopRight); 
        }

        private void RotationTopLeft_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            Rotate(e.Position, e.Delta.Translation.X, e.Delta.Translation.Y, Orientation.TopLeft);
        }

        private void RotationBottomLeft_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            Rotate(e.Position, e.Delta.Translation.X, e.Delta.Translation.Y, Orientation.BottomLeft);
        }

        private void RotationBottomRight_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            Rotate(e.Position, e.Delta.Translation.X, e.Delta.Translation.Y, Orientation.BottomRight);
        }

        private void Rotate(Point position, double deltaX, double deltaY, Orientation orientation)
        {
            Point centerPoint = GetCenterCoordinateOfGridMain();
            m_CornerPoint = GetCornerCoordinate(orientation, centerPoint);

            Point rotationStartingPoint = new Point
            {
                X = m_CornerPoint.X + position.X,
                Y = m_CornerPoint.Y + position.Y
            };

            Point rotationEndPoint = new Point
            {
                X = rotationStartingPoint.X + deltaX,
                Y = rotationStartingPoint.Y + deltaY
            };

            Point directionVectorToOrigin = GetSubtractionOfPoints(rotationStartingPoint, centerPoint);
            Point directionVectorToRotatedPoint = GetSubtractionOfPoints(rotationEndPoint, centerPoint);

            
            double dot = directionVectorToOrigin.X*directionVectorToRotatedPoint.X +
                         directionVectorToOrigin.Y*directionVectorToRotatedPoint.Y;

            double cross = directionVectorToOrigin.X * directionVectorToRotatedPoint.Y -
                           directionVectorToOrigin.Y * directionVectorToRotatedPoint.X;

            
            float angle = (float)Math.Atan2(cross, dot);
            m_RotationAngle += (float)PocketPaintApplication.RadianToDegree(angle) + 360;
            m_RotationAngle = m_RotationAngle % 360;

            if(m_RotationAngle > 180)
            {
                m_RotationAngle = m_RotationAngle - 360;
            }

            var ct = new RotateTransform
            {
                Angle = m_RotationAngle,
                CenterX = m_center_x,
                CenterY = m_center_y
            };
            m_currentRotationAngle = m_RotationAngle;
            addTransformation(ct);
        }

        private Point GetCornerCoordinate(Orientation orientation, Point centerPoint)
        {
            if (orientation == Orientation.TopRight)
            {
                m_CornerPoint = GetCenterCoordinateOfFrameworkElement(TopRightRotationGrid);
                m_CornerPoint.Y -= TopRightRotationGrid.Width / 2;
                m_CornerPoint.X -= TopRightRotationGrid.Height / 2;
            } else if (orientation == Orientation.TopLeft)
            {
                m_CornerPoint = GetCenterCoordinateOfFrameworkElement(TopLeftRotationGrid);
                m_CornerPoint.Y -= TopLeftRotationGrid.Width / 2;
                m_CornerPoint.X -= TopLeftRotationGrid.Height / 2;

            } else if (orientation == Orientation.BottomLeft)
            {
                m_CornerPoint = GetCenterCoordinateOfFrameworkElement(BottomLeftRotationGrid);
                m_CornerPoint.Y -= BottomLeftRotationGrid.Width / 2;
                m_CornerPoint.X -= BottomLeftRotationGrid.Height / 2;

            } else if (orientation == Orientation.BottomRight)
            {
                m_CornerPoint = GetCenterCoordinateOfFrameworkElement(BottomRightRotationGrid);
                m_CornerPoint.Y -= BottomRightRotationGrid.Width / 2;
                m_CornerPoint.X -= BottomRightRotationGrid.Height / 2;
            }
            
            Point rotated_point;
            Point point_to_rotate = GetSubtractionOfPoints(m_CornerPoint, centerPoint);
            
            rotated_point.X = (((Math.Cos(PocketPaintApplication.DegreeToRadian(m_RotationAngle)) * point_to_rotate.X) - (Math.Sin(PocketPaintApplication.DegreeToRadian(m_RotationAngle)) * point_to_rotate.Y)));
            rotated_point.Y = (((Math.Sin(PocketPaintApplication.DegreeToRadian(m_RotationAngle)) * point_to_rotate.X) + (Math.Cos(PocketPaintApplication.DegreeToRadian(m_RotationAngle)) * point_to_rotate.Y)));
           
            rotated_point = GetAdditionOfPoints(centerPoint, rotated_point);
           
            return rotated_point;
        }

        private Point GetSubtractionOfPoints(Point p1, Point p2)
        {
            Point result = new Point(p1.X - p2.X, p1.Y - p2.Y);
            return result;
        }

        private Point GetAdditionOfPoints(Point p1, Point p2)
        {
            Point result = new Point(p1.X + p2.X, p1.Y + p2.Y);
            return result;
        }
    }
}
