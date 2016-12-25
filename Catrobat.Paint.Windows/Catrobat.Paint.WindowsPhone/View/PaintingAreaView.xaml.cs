using Catrobat.Paint.WindowsPhone.Command;
using Catrobat.Paint.WindowsPhone.Listener;
using Catrobat.Paint.WindowsPhone.Tool;
using Catrobat.Paint.WindowsPhone.Ui;
using System;
using System.Diagnostics;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using Catrobat.Paint.WindowsPhone.Data;
using Windows.ApplicationModel.Core;
using Catrobat.Paint.WindowsPhone.Converters;

// Die Elementvorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkID=390556 dokumentiert.
namespace Catrobat.Paint.WindowsPhone.View
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet werden kann oder auf die innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class PaintingAreaView
    {
        static int _rotateCounter;
        static bool _flipVertical;
        static bool _flipHorizontal;
        static bool _isTapLoaded;
        static bool _isFullscreen;
        static bool _isPointerEventLoaded;
        static int _zoomCounter;
        static bool _m_isWorkingSpaceMoved;
        CoreApplicationView _view;

        private const string barCursorText = "barCursor";
        private const string barBrushEraserLineText = "barBrushEraserLine";
        private const string barCropText = "barCrop";
        private const string barFillText = "barFill";
        private const string barPipetteText = "barPipette";
        private const string barImportPngText = "barImportPng";
        private const string barEllipseText = "barEllipse";
        private const string barRectangleText = "barRectangle";
        private const string barMoveText = "barMove";
        private const string barZoomText = "barZoom";
        private const string barRotateText = "barRotate";
        private const string barFlipText = "barFlip";
        private const string barStampText = "barStamp";

        static string current_appbar = barBrushEraserLineText;

        public PaintingAreaView()
        {
            InitializeComponent();
            _rotateCounter = 0;
            _flipHorizontal = false;
            _flipHorizontal = false;
            _isTapLoaded = false;
            _isFullscreen = false;
            _isPointerEventLoaded = false;
            _zoomCounter = 0;
            _m_isWorkingSpaceMoved = false;


            PocketPaintApplication.GetInstance().PaintingAreaCanvas = PaintingAreaCanvas;
            PocketPaintApplication.GetInstance().EraserCanvas = EraserCanvas;
            PocketPaintApplication.GetInstance().PaintingAreaCanvas.RenderTransform = new TransformGroup();
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            PocketPaintApplication.GetInstance().angularDegreeOfWorkingSpaceRotation = 0;

            LayoutRoot.Height = Window.Current.Bounds.Height;
            LayoutRoot.Width = Window.Current.Bounds.Width;
            PocketPaintApplication.GetInstance().PaintingAreaView = this;
            PocketPaintApplication.GetInstance().PaintingAreaLayoutRoot = LayoutRoot;
            PocketPaintApplication.GetInstance().GridWorkingSpace = GridWorkingSpace;
            PocketPaintApplication.GetInstance().CursorControl = ClCursor;
            PocketPaintApplication.GetInstance().CropControl = ctrlCropControl;
            PocketPaintApplication.GetInstance().StampControl = ctrlStampControl;
            PocketPaintApplication.GetInstance().EllipseSelectionControl = ucEllipseSelectionControl;
            PocketPaintApplication.GetInstance().RectangleSelectionControl = ucRectangleSelectionControl;
            PocketPaintApplication.GetInstance().GridInputScopeControl = GridInputScopeControl;
            PocketPaintApplication.GetInstance().GridImportImageSelectionControl = GridImportImageSelectionControl;
            PocketPaintApplication.GetInstance().InfoAboutAndConditionOfUseBox = InfoAboutAndConditionOfUseBox;
            PocketPaintApplication.GetInstance().InfoBoxActionControl = InfoBoxActionControl;
            PocketPaintApplication.GetInstance().PhoneControl = ucPhotoControl;
            PocketPaintApplication.GetInstance().InfoBoxControl = InfoBoxControl;
            PocketPaintApplication.GetInstance().pgPainting = pgPainting;
            PocketPaintApplication.GetInstance().InfoxBasicBoxControl = InfoBasicBoxControl;
            PocketPaintApplication.GetInstance().ProgressRing = progressRing;
            LoadManipulationEraserCanvasEvents();

            //PocketPaintApplication.GetInstance().ToolName = ucToolName;

            Spinner.SpinnerGrid = SpinnerGrid;
            Spinner.SpinnerStoryboard = new Storyboard();

            PocketPaintApplication.GetInstance().MainGrid = LayoutRoot;
            UndoRedoActionbarManager.GetInstance().ApplicationBarTop = PocketPaintApplication.GetInstance().AppbarTop;

            PocketPaintApplication.GetInstance().BarStandard = barStandard;

            PocketPaintApplication.GetInstance().PaintData.toolCurrentChanged += ToolChangedHere;
            PocketPaintApplication.GetInstance().AppbarTop.ToolChangedHere(PocketPaintApplication.GetInstance().ToolCurrent);

            SetPaintingAreaViewLayout();
            PocketPaintApplication.GetInstance().GrdThicknessControlState = Visibility.Collapsed;
            CreateAppBarAndSwitchAppBarContent(current_appbar);

            SetSizeOfGridWorkingSpace((int)Window.Current.Bounds.Height, (int)Window.Current.Bounds.Width);
            AlignPositionOfGridWorkingSpace(null);
            PocketPaintApplication.GetInstance().PaintingAreaCanvas.Height = Window.Current.Bounds.Height;
            PocketPaintApplication.GetInstance().PaintingAreaCanvas.Width = Window.Current.Bounds.Width;
            _view = CoreApplication.GetCurrentView();
            DrawCheckeredBackgroundInCheckeredCanvas(9);

            var test = ucRectangleSelectionControl;
        }

        public void DrawCheckeredBackgroundInCheckeredCanvas(uint sizeOfBoxes)
        {
            uint sizeOfBoxesToDraw = sizeOfBoxes;
            Rectangle rectToDraw;
            CheckeredCanvas.Children.Clear();
            for (int x = 0; x < Math.Floor(PaintingAreaCanvas.Width / sizeOfBoxesToDraw) + 1; x++)
            {
                for (int y = 0; y < Math.Floor(PaintingAreaCanvas.Height / sizeOfBoxesToDraw) + 1; y++)
                {
                    rectToDraw = new Rectangle();
                    if ((x + y) % 2 == 0)
                    {
                        rectToDraw.Fill = new SolidColorBrush(Colors.White);
                    }
                    else
                    {
                        rectToDraw.Fill = new SolidColorBrush(Colors.Gray);
                    }
                    double yCoordToDraw = Window.Current.Bounds.Height - (y * sizeOfBoxesToDraw);
                    if (yCoordToDraw >= 0)
                    {
                        rectToDraw.Height = yCoordToDraw < sizeOfBoxesToDraw ?
                            yCoordToDraw : sizeOfBoxesToDraw;
                    }
                    double xCoordToDraw = Window.Current.Bounds.Width - (x * sizeOfBoxesToDraw);
                    if (xCoordToDraw >= 0)
                    {
                        rectToDraw.Width = xCoordToDraw < sizeOfBoxesToDraw ?
                            xCoordToDraw : sizeOfBoxesToDraw;
                    }

                    Canvas.SetLeft(rectToDraw, x * sizeOfBoxesToDraw);
                    Canvas.SetTop(rectToDraw, y * sizeOfBoxesToDraw);
                    CheckeredCanvas.Children.Add(rectToDraw);
                }
            }
        }

        public void SetSizeOfGridWorkingSpace(int height, int width)
        {
            GridWorkingSpace.Height = height;
            GridWorkingSpace.Width = width;
        }

        public void AlignPositionOfGridWorkingSpace(RotateTransform rtRotation)
        {
            TransformGroup tgGridWorkingSpace = GetGridWorkingSpaceTransformGroup();
            int angularDegreeOfWorkingSpaceRotation = PocketPaintApplication.GetInstance().angularDegreeOfWorkingSpaceRotation;
            if (tgGridWorkingSpace == null)
            {
                return;
            }
            tgGridWorkingSpace.Children.Clear();

            if (rtRotation == null)
            {
                rtRotation = CreateRotateTransform(angularDegreeOfWorkingSpaceRotation, new Point(GridWorkingSpace.Width / 2.0, GridWorkingSpace.Height / 2.0));
            }
            tgGridWorkingSpace.Children.Add(rtRotation);

            GridWorkingSpace.HorizontalAlignment = HorizontalAlignment.Left;
            GridWorkingSpace.VerticalAlignment = VerticalAlignment.Top;

            var toScaleValue = new ScaleTransform();

            toScaleValue.ScaleX = 0.70;
            toScaleValue.ScaleY = 0.70;
            toScaleValue.CenterX = GridWorkingSpace.Width / 2.0;
            toScaleValue.CenterY = GridWorkingSpace.Height / 2.0;
            if (angularDegreeOfWorkingSpaceRotation == 90 || angularDegreeOfWorkingSpaceRotation == 270)
            {
                toScaleValue.ScaleX = 0.5625;
                toScaleValue.ScaleY = 0.5625;
            }

            tgGridWorkingSpace.Children.Add(toScaleValue);

            TranslateTransform tfMiddlePointOfGridWorkingSpaceToGlobalNullPoint = new TranslateTransform();
            var tfLeftTopCornerOfGridWorkingSpaceToNullPoint = CreateTranslateTransform(tgGridWorkingSpace.Value.OffsetX * (-1), tgGridWorkingSpace.Value.OffsetY *(-1));

            double offsetToCenterWorkingSpace;
            if (angularDegreeOfWorkingSpaceRotation == 0)
            {
                offsetToCenterWorkingSpace = 11;
                tfMiddlePointOfGridWorkingSpaceToGlobalNullPoint = CreateTranslateTransform(((GridWorkingSpace.Width / 2.0) * toScaleValue.ScaleX) * (-1),
                                                                                            ((GridWorkingSpace.Height / 2.0) * toScaleValue.ScaleY) * (-1) - offsetToCenterWorkingSpace);
            }
            else if (angularDegreeOfWorkingSpaceRotation == 90)
            {
                tfMiddlePointOfGridWorkingSpaceToGlobalNullPoint = CreateTranslateTransform((GridWorkingSpace.Height / 2.0) * toScaleValue.ScaleY,
                                                                                            ((GridWorkingSpace.Width / 2.0) * toScaleValue.ScaleX) * (-1) - 5.5);
            }
            else if (angularDegreeOfWorkingSpaceRotation == 180)
            {
                offsetToCenterWorkingSpace = 11;
                tfMiddlePointOfGridWorkingSpaceToGlobalNullPoint = CreateTranslateTransform((GridWorkingSpace.Width / 2.0) * toScaleValue.ScaleX,
                                                                                            ((GridWorkingSpace.Height / 2.0) * toScaleValue.ScaleY) - offsetToCenterWorkingSpace);
            }
            else if (angularDegreeOfWorkingSpaceRotation == 270)
            {
                offsetToCenterWorkingSpace = 5.5;
                tfMiddlePointOfGridWorkingSpaceToGlobalNullPoint = CreateTranslateTransform(((GridWorkingSpace.Height / 2.0) * toScaleValue.ScaleY) *(-1),
                                                                            (GridWorkingSpace.Width / 2.0) * toScaleValue.ScaleX - offsetToCenterWorkingSpace);
            }
            var tfMiddlePointOfGridWorkingSpaceToGlobalMiddlePoint = CreateTranslateTransform((Window.Current.Bounds.Width / 2.0), (Window.Current.Bounds.Height / 2.0));

            AddTranslateTransformToGridWorkingSpaceTransformGroup(tfLeftTopCornerOfGridWorkingSpaceToNullPoint);
            AddTranslateTransformToGridWorkingSpaceTransformGroup(tfMiddlePointOfGridWorkingSpaceToGlobalNullPoint);
            AddTranslateTransformToGridWorkingSpaceTransformGroup(tfMiddlePointOfGridWorkingSpaceToGlobalMiddlePoint);
        }

        public TranslateTransform CreateTranslateTransform(double translateX, double translateY)
        {
            TranslateTransform translateTransform = new TranslateTransform();
            translateTransform.X = translateX;
            translateTransform.Y = translateY;
            return translateTransform;
        }

        public RotateTransform CreateRotateTransform(int angle, Point rotationCenter)
        {
            RotateTransform rotateTransform = new RotateTransform();
            rotateTransform.Angle = angle;
            rotateTransform.CenterX = rotationCenter.X;
            rotateTransform.CenterY = rotationCenter.Y;
            return rotateTransform;
        }

        public void AddTranslateTransformToGridWorkingSpaceTransformGroup(TranslateTransform translateTransform)
        {
            TransformGroup tgGridWorkingSpace = GetGridWorkingSpaceTransformGroup();
            if(tgGridWorkingSpace == null)
            {
                return;
            }
            tgGridWorkingSpace.Children.Add(translateTransform);
        }

        public TransformGroup GetGridWorkingSpaceTransformGroup()
        {
            TransformGroup tgGridWorkingSpace = null;
            if (PocketPaintApplication.GetInstance().GridWorkingSpace.RenderTransform is TransformGroup)
            {
                tgGridWorkingSpace = PocketPaintApplication.GetInstance().GridWorkingSpace.RenderTransform as TransformGroup;
            }
            if (tgGridWorkingSpace == null)
            {
                tgGridWorkingSpace = new TransformGroup();
                PocketPaintApplication.GetInstance().GridWorkingSpace.RenderTransform = tgGridWorkingSpace;
            }

            return tgGridWorkingSpace;
        }

        public async void ContinueFileOpenPicker(CoreApplicationView sender, IActivatedEventArgs args1)
        {
            FileOpenPickerContinuationEventArgs args = args1 as FileOpenPickerContinuationEventArgs;

            if (args != null && args.Files.Count > 0)
            {
                _view.Activated -= ContinueFileOpenPicker;

                StorageFile file = args.Files[0];

                BitmapImage image = new BitmapImage();
                await image.SetSourceAsync(await file.OpenAsync(FileAccessMode.Read));

                ImageBrush fillBrush = new ImageBrush();
                fillBrush.ImageSource = image;

                if (PocketPaintApplication.GetInstance().isLoadPictureClicked)
                {
                    PocketPaintApplication.GetInstance().PaintingAreaCanvas.Children.Clear();
                    RectangleGeometry myRectangleGeometry = new RectangleGeometry();
                    myRectangleGeometry.Rect = new Rect(new Point(0, 0), new Point(PaintingAreaCanvas.Width, PaintingAreaCanvas.Height));


                    Path path = new Path();
                    path.Fill = fillBrush;
                    path.Stroke = PocketPaintApplication.GetInstance().PaintData.strokeColorSelected;

                    path.Data = myRectangleGeometry;
                    PocketPaintApplication.GetInstance().PaintingAreaCanvas.Children.Add(path);
                    CommandManager.GetInstance().CommitCommand(new LoadPictureCommand(path));
                    PocketPaintApplication.GetInstance().isLoadPictureClicked = false;
                    ChangeBackgroundColorAndOpacityOfPaintingAreaCanvas(Colors.Transparent, 1.0);
                    ChangeEnabledOfASecondaryAppbarButton("appBarButtonClearWorkingSpace", true);
                    ChangeEnabledOfASecondaryAppbarButton("appbarButtonSave", true);
                }
                else
                {
                    PocketPaintApplication.GetInstance().ImportImageSelectionControl.imageSourceOfRectangleToDraw = fillBrush;
                    PocketPaintApplication.GetInstance().PaintingAreaView.ChangeBackgroundColorAndOpacityOfPaintingAreaCanvas(Colors.Black, 0.5);
                }

                PocketPaintApplication.GetInstance().PaintingAreaView.DisableToolbarsAndPaintingArea(false);
            }
            else
            {
                ChangeVisibilityOfAppBars(Visibility.Visible);
            }
        }

        public void PickAFileButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");

            openPicker.PickSingleFileAndContinue();
            _view.Activated += ContinueFileOpenPicker;
        }

        public async void HideStatusAppBar()
        {
            var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
            await statusBar.HideAsync();
        }

        public async void ShowStatusAppBar()
        {
            var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
            await statusBar.ShowAsync();
        }

        void PaintingAreaCanvas_Tapped(object sender, TappedRoutedEventArgs e)
        {
            PocketPaintApplication currentPpa = PocketPaintApplication.GetInstance();
            if(currentPpa != null)
            {
                bool shouldDrawingModeActivated = !currentPpa.cursorControl.isDrawingActivated();
                currentPpa.cursorControl.setCursorLook(shouldDrawingModeActivated);
            }
        }

        private void SetPaintingAreaViewLayout()
        {
            double heightMultiplicator = PocketPaintApplication.GetInstance().size_width_multiplication;
            double widthMultiplicator = PocketPaintApplication.GetInstance().size_width_multiplication;

            GrdThicknessControl.Height *= heightMultiplicator;
            GrdThicknessControl.Width *= widthMultiplicator;

            GridUserControlRectEll.Height *= heightMultiplicator;
            GridUserControlRectEll.Width *= widthMultiplicator;
            GridUserControlRectEll.Margin = new Thickness(
                                GridUserControlRectEll.Margin.Left * widthMultiplicator,
                                GridUserControlRectEll.Margin.Top * heightMultiplicator,
                                GridUserControlRectEll.Margin.Right * widthMultiplicator,
                                GridUserControlRectEll.Margin.Bottom * heightMultiplicator);
        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            PocketPaintApplication.GetInstance().shouldAppClosedThroughBackButton = false;
            if (Frame.CurrentSourcePageType == typeof(ViewColorPicker))
            {
                e.Handled = true;
                Frame.GoBack();
            }
            else if (Frame.CurrentSourcePageType == typeof(ViewToolPicker))
            {
                Frame.GoBack();
                e.Handled = true;
            }
            else if (_isFullscreen)
            {
                _isFullscreen = false;

                ChangeVisibilityOfAppBars(Visibility.Visible);
                AppBarButton brushThickness = getAppBarButtonBy("ThicknessButton");
                if (brushThickness != null)
                {
                    brushThickness.Icon = bitmapIconFrom("icon_menu_strokes.png");
                }
                AlignPositionOfGridWorkingSpace(null);
                ShowStatusAppBar();
                e.Handled = true;
            }
            else if (InfoAboutAndConditionOfUseBox.Visibility == Visibility.Visible
                    || InfoBoxActionControl.Visibility == Visibility.Visible
                    || InfoBoxControl.Visibility == Visibility.Visible
                    || InfoBasicBoxControl.Visibility == Visibility.Visible
                    || InfoAboutAndConditionOfUseBox.Visibility == Visibility.Visible)
            {
                SetActivityOfToolsControls(true);

                InfoAboutAndConditionOfUseBox.Visibility = Visibility.Collapsed;
                InfoBoxActionControl.Visibility = Visibility.Collapsed;
                InfoBoxControl.Visibility = Visibility.Collapsed;
                InfoBasicBoxControl.Visibility = Visibility.Collapsed;
                InfoAboutAndConditionOfUseBox.Visibility = Visibility.Collapsed;
                ChangeVisibilityOfAppBars(Visibility.Visible);
                ChangeBackgroundColorAndOpacityOfPaintingAreaCanvas(Colors.Transparent, 1.0);
                e.Handled = true;
            }
            else if (ucPhotoControl.Visibility == Visibility.Visible)
            {
                PocketPaintApplication.GetInstance().PhoneControl.closePhoneControl(sender, null);
                e.Handled = true;
            }
            else
            {
                if (PocketPaintApplication.GetInstance().ToolCurrent.GetToolType() != ToolType.Brush)
                {
                    ResetControls();
                    ChangeBackgroundColorAndOpacityOfPaintingAreaCanvas(Colors.Transparent, 1.0);
                    PocketPaintApplication.GetInstance().SwitchTool(ToolType.Brush);
                    PocketPaintApplication.GetInstance().AppbarTop.BtnSelectedColorVisible(true);
                    e.Handled = true;
                }
                else if (GrdThicknessControl.Visibility == Visibility.Visible)
                {
                    GrdThicknessControl.Visibility = Visibility.Collapsed;
                    e.Handled = true;
                }
                else if (PaintingAreaCanvas.Children.Count > 0)
                {
                    PocketPaintApplication.GetInstance().shouldAppClosedThroughBackButton = true;
                    messageBoxNewDrawingSpace_Click("", true);
                    e.Handled = true;
                }
                else
                {
                    //TODO: Close app.
                    Application.Current.Exit();
                }
            }
        }

        public void SetActivityOfToolsControls(bool isActive)
        {
            if (isActive)
            {
                PaintingAreaCanvas.IsHitTestVisible = true;
                ChangeVisibilityOfActiveSelectionControl(Visibility.Visible);
            }
            else
            {
                PaintingAreaCanvas.IsHitTestVisible = false;
                ChangeVisibilityOfSelectionsControls(Visibility.Collapsed);
            }
        }

        public void ChangeVisibilityOfAppBars(Visibility visibility)
        {
            appBarTop.Visibility = visibility;
            if (BottomAppBar != null)
                BottomAppBar.Visibility = visibility;
        }

        /// <summary>
        /// Wird aufgerufen, wenn diese Seite in einem Frame angezeigt werden soll.
        /// </summary>
        /// <param name="e">Ereignisdaten, die beschreiben, wie diese Seite erreicht wurde.
        /// Dieser Parameter wird normalerweise zum Konfigurieren der Seite verwendet.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

        }

        // if there is no object on the paintingareaview and no copy of the workingspace is selected in the stamp tool then reset
        // the stampbarbuttons
        public void CheckAndUpdateStampAppBarButtons()
        {
            if (PocketPaintApplication.GetInstance().ToolCurrent.GetToolType() == ToolType.Stamp && !IsAppBarButtonSelected("appBtnStampCopy"))
            {
                PocketPaintApplication.GetInstance().PaintingAreaView.CreateAppBarAndSwitchAppBarContent("barStamp");
            }
        }

        private BitmapIcon bitmapIconFrom(string iconNameWithExtension)
        {
            BitmapIcon bitmapIcon = new BitmapIcon();
            bitmapIcon.UriSource = new Uri("ms-resource:/Files/Assets/Icons/" + iconNameWithExtension, UriKind.Absolute);
            return bitmapIcon;
        }

        private AppBarButton appBarButtonWith(string iconText, string labelText, string nameText)
        {
            AppBarButton appBarButton = new AppBarButton();
            appBarButton.Icon = bitmapIconFrom(iconText);
            appBarButton.Label = labelText;
            appBarButton.Name = nameText;
            return appBarButton;
        }

        public void CreateAppBarAndSwitchAppBarContent(string type)
        {
            CommandBar cmdBar = new CommandBar();
            SolidColorBrush appBarBackgroundColor = new SolidColorBrush();
            appBarBackgroundColor.Color = Color.FromArgb(255, 25, 165, 184);
            cmdBar.Background = appBarBackgroundColor;

            LoadPointerEvents();
            UnloadTapEvent();
            UnloadManipulationPaintingAreaCanvasEvents();

            if (type == barCursorText || type == barBrushEraserLineText)
            {
                AppBarButton app_btnBrushThickness = appBarButtonWith("icon_menu_strokes.png", "Pinselstärke", "ThicknessButton");
                app_btnBrushThickness.Click += btnThickness_Click;
                cmdBar.PrimaryCommands.Add(app_btnBrushThickness);

                if (type == barCursorText)
                {
                    AppBarButton app_btnResetCursor = appBarButtonWith("icon_menu_cursor.png", "Cursor-Startposition", "appButtonResetCursor");

                    TransformGroup transformGroup = (TransformGroup)ClCursor.RenderTransform;
                    if (transformGroup != null && (transformGroup.Value.OffsetX != 0.0 || transformGroup.Value.OffsetY != 0.0))
                    {
                        app_btnResetCursor.IsEnabled = true;
                    }
                    else
                    {
                        app_btnResetCursor.IsEnabled = false;
                    }
                    app_btnResetCursor.Click += ((CursorTool)PocketPaintApplication.GetInstance().ToolCurrent).app_btnResetCursor_Click;

                    cmdBar.PrimaryCommands.Add(app_btnResetCursor);
                    LoadTapEvent();
                    LoadManipulationPaintingAreaCanvasEvents();
                    UnloadPointerEvents();
                }
            }
            else if (type == barCropText)
            {
                AppBarButton app_btnCropImage = appBarButtonWith("icon_menu_crop_cut.png", "Schneiden", "");
                app_btnCropImage.Click += app_btnCropImage_Click;
                cmdBar.PrimaryCommands.Add(app_btnCropImage);

                AppBarButton app_btnResetSelection = appBarButtonWith("icon_menu_crop_adjust.png", "Ausgangsposition", "appButtonResetCrop");
                app_btnResetSelection.Click += app_btn_reset_Click;
                app_btnResetSelection.IsEnabled = PocketPaintApplication.GetInstance().CropControl.SetIsModifiedRectangleMovement;
                cmdBar.PrimaryCommands.Add(app_btnResetSelection);
            }
            else if (type == barFillText || type == barPipetteText)
            {
                // Do nothing
            }
            else if (type == barImportPngText || type == barEllipseText || type == barRectangleText)
            {
                AppBarButton app_btnBrushThickness = new AppBarButton();
                app_btnBrushThickness.Click += btnThicknessBorder_Click;
                app_btnBrushThickness.Icon = bitmapIconFrom("icon_menu_strokes.png");
                app_btnBrushThickness.Label = "Einstellungen";
                app_btnBrushThickness.Name = "ThicknessProperties";
                cmdBar.PrimaryCommands.Add(app_btnBrushThickness);

                if(type == barImportPngText)
                {
                    AppBarButton app_btnImportPicture = appBarButtonWith("icon_menu_cursor.png", "Bild laden", "");
                    app_btnImportPicture.Click += app_btnImportPicture_Click;
                    cmdBar.PrimaryCommands.Add(app_btnImportPicture);
                }

                AppBarButton app_btnReset = appBarButtonWith("icon_menu_cursor.png", "Ausgangsposition", "appButtonReset");
                app_btnReset.Click += app_btn_reset_Click;
                // TODO: comment in the following line if the importpngcontrol contains the rectangleshapebasecontrol
                // app_btnReset.IsEnabled = ((RectangleShapeBaseTool)PocketPaintApplication.GetInstance().ToolCurrent)
                //                         .RectangleShapeBase.IsModifiedRectangleForMovement;
                // Debug.Assert(
                //            ((RectangleShapeBaseTool)PocketPaintApplication.GetInstance().ToolCurrent)
                //            .RectangleShapeBase != null);
                cmdBar.PrimaryCommands.Add(app_btnReset);

                LoadManipulationPaintingAreaCanvasEvents();
                UnloadPointerEvents();
            }
            else if (type == barMoveText || type == barZoomText)
            {
                AppBarButton app_btnReset = appBarButtonWith("icon_menu_cursor.png", "Ausgangsposition", "appButtonResetZoom");
                app_btnReset.Click += app_btn_reset_Click;

                if (type == barZoomText)
                {
                    AppBarButton app_btnZoomIn = appBarButtonWith("icon_zoom_in.png", "Vergrößern", "");
                    app_btnZoomIn.Click += BtnZoomIn_Click;
                    cmdBar.PrimaryCommands.Add(app_btnZoomIn);

                    AppBarButton app_btnZoomOut = appBarButtonWith("icon_zoom_out.png", "Verkleinern", "");
                    app_btnZoomOut.Click += BtnZoomOut_Click;
                    cmdBar.PrimaryCommands.Add(app_btnZoomOut);
                }

                app_btnReset = appBarButtonWith("icon_menu_cursor.png", "Ausgangsposition", "appButtonResetZoom");
                app_btnReset.Click += app_btn_reset_Click;
                app_btnReset.IsEnabled = _zoomCounter == 0 ? false : true;
                cmdBar.PrimaryCommands.Add(app_btnReset);

                UnloadPointerEvents();
                LoadManipulationPaintingAreaCanvasEvents();
            }
            else if (type == barRotateText)
            {
                AppBarButton app_btnRotate_left = appBarButtonWith("icon_menu_rotate_left.png", "Rechts drehen", "");
                app_btnRotate_left.Click += BtnLeft_OnClick;
                cmdBar.PrimaryCommands.Add(app_btnRotate_left);

                AppBarButton app_btnRotate_right = appBarButtonWith("icon_menu_rotate_right.png", "Links drehen", "");
                app_btnRotate_right.Click += BtnRight_OnClick;
                cmdBar.PrimaryCommands.Add(app_btnRotate_right);

                AppBarButton app_btnReset = appBarButtonWith("icon_menu_cursor.png", "Ausgangsposition", "appButtonResetRotate");
                app_btnReset.Click += app_btn_reset_Click;
                app_btnReset.IsEnabled = _rotateCounter == 0 ? false : true;
                cmdBar.PrimaryCommands.Add(app_btnReset);
            }
            else if (type == barFlipText)
            {
                AppBarButton app_btnHorizontal = appBarButtonWith("icon_menu_flip_horizontal.png", "Horizontal", "");
                app_btnHorizontal.Click += BtnHorizotal_OnClick;
                cmdBar.PrimaryCommands.Add(app_btnHorizontal);

                AppBarButton app_btnVertical = appBarButtonWith("icon_menu_flip_vertical.png", "Vertikal", "");
                app_btnVertical.Click += BtnVertical_OnClick;
                cmdBar.PrimaryCommands.Add(app_btnVertical);

                AppBarButton app_btnReset = appBarButtonWith("icon_menu_cursor.png", "Ausgangsposition", "appButtonResetFlip");
                app_btnReset.Click += app_btn_reset_Click;
                app_btnReset.IsEnabled = _flipHorizontal | _flipVertical;
                cmdBar.PrimaryCommands.Add(app_btnReset);
            }
            else if (type == barStampText)
            {
                AppBarButton app_btnStampCopy = appBarButtonWith("icon_menu_stamp_copy.png", "Auswahl merken", "appBtnStampCopy");
                app_btnStampCopy.Click += app_btnStampCopy_Click;
                cmdBar.PrimaryCommands.Add(app_btnStampCopy);

                AppBarButton app_btnStampClear = appBarButtonWith("icon_menu_stamp_clear.png", "Auswahl zurücksetzen", "appBtnStampReset");
                app_btnStampClear.Click += app_btnStampClear_Click;
                app_btnStampClear.IsEnabled = false;
                cmdBar.PrimaryCommands.Add(app_btnStampClear);

                AppBarButton app_btnStampPaste = appBarButtonWith("icon_menu_stamp_paste.png", "Stempeln", "appBtnStampPaste");
                app_btnStampPaste.Click += app_btnStampPaste_Click;
                app_btnStampPaste.Visibility = Visibility.Collapsed;
                cmdBar.PrimaryCommands.Add(app_btnStampPaste);

                AppBarButton app_btnResetSelection = appBarButtonWith("icon_menu_cursor.png", "Tool zurücksetzen", "appButtonResetStamp");
                app_btnResetSelection.Click += app_btn_reset_Click;
                cmdBar.PrimaryCommands.Add(app_btnResetSelection);

                LoadManipulationPaintingAreaCanvasEvents();
                UnloadPointerEvents();
            }
            else
            {
                return;
            }

            AppBarButton app_btnClearElementsInWorkingSpace = appBarButtonWith("", "Arbeitsfläche löschen", "appBarButtonClearWorkingSpace");
            app_btnClearElementsInWorkingSpace.Click += app_btnClearElementsInWorkingSpace_Click;
            app_btnClearElementsInWorkingSpace.IsEnabled = PaintingAreaCanvas.Children.Count > 0 ? true : false;

            AppBarButton app_btnSave = appBarButtonWith("", "Speichern", "appbarButtonSave");
            app_btnSave.Click += app_btnSave_Click;
            app_btnSave.IsEnabled = PaintingAreaCanvas.Children.Count > 0 ? true : false;

            AppBarButton app_btnSaveCopy = appBarButtonWith("", "Kopie speichern", "");

            AppBarButton app_btnLoad = appBarButtonWith("", "Laden", "");
            app_btnLoad.Click += app_btnLoad_Click;

            AppBarButton app_btnFullScreen = appBarButtonWith("", "Vollbild", "");
            app_btnFullScreen.Click += app_btnFullScreen_Click;

            AppBarButton app_btnMoreInfo = appBarButtonWith("", "Mehr", "");
            app_btnMoreInfo.Click += app_btnMoreInfo_Click;

            AppBarButton app_btnNewPicture = appBarButtonWith("", "Neues Bild", "");            
            app_btnNewPicture.Click += app_btnNewPicture_Click;

            cmdBar.SecondaryCommands.Add(app_btnClearElementsInWorkingSpace);
            cmdBar.SecondaryCommands.Add(app_btnNewPicture);
            cmdBar.SecondaryCommands.Add(app_btnLoad);
            cmdBar.SecondaryCommands.Add(app_btnSave);
            cmdBar.SecondaryCommands.Add(app_btnFullScreen);
            cmdBar.SecondaryCommands.Add(app_btnMoreInfo);

            BottomAppBar = cmdBar;
            current_appbar = type;
        }

        void app_btnCropImage_Click(object sender, RoutedEventArgs e)
        {
            ((CropTool)PocketPaintApplication.GetInstance().ToolCurrent).CropImage();
        }

        public void app_btnStampPaste_Click(object sender, RoutedEventArgs e)
        {
            ((StampTool)PocketPaintApplication.GetInstance().ToolCurrent).StampPaste();
        }

        public bool IsAppBarButtonSelected(string nameOfAppbarbutton)
        {
            CommandBar cmdBar = (CommandBar)BottomAppBar;
            if (cmdBar != null)
            {
                for (int appBarButtonIndex = 0; appBarButtonIndex < cmdBar.PrimaryCommands.Count; appBarButtonIndex++)
                {
                    AppBarButton currentAppBarButton = ((AppBarButton) (cmdBar.PrimaryCommands[appBarButtonIndex]));
                    if (currentAppBarButton.Name == "appBtnStampCopy")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void app_btnStampClear_Click(object sender, RoutedEventArgs e)
        {
            CommandBar cmdBar = (CommandBar)BottomAppBar;
            if (cmdBar != null)
            {
                for (int appBarButtonIndex = 0; appBarButtonIndex < cmdBar.PrimaryCommands.Count; appBarButtonIndex++)
                {
                    AppBarButton currentAppBarButton = ((AppBarButton) (cmdBar.PrimaryCommands[appBarButtonIndex]));
                    if (currentAppBarButton.Name == "appBtnStampCopy")
                    {
                        currentAppBarButton.Visibility = Visibility.Visible;
                    }
                    else if (currentAppBarButton.Name == "appBtnStampPaste")
                    {
                        currentAppBarButton.Visibility = Visibility.Collapsed;
                    }
                    else if (currentAppBarButton.Name == "appBtnStampReset")
                    {
                        currentAppBarButton.IsEnabled = false;
                    }
                }
            }

            ((StampTool)PocketPaintApplication.GetInstance().ToolCurrent).StampClear();
        }

        public void app_btnStampCopy_Click(object sender, RoutedEventArgs e)
        {
            ((StampTool)PocketPaintApplication.GetInstance().ToolCurrent).StampCopy();
            CommandBar cmdBar = (CommandBar)BottomAppBar;

            if (cmdBar != null)
            {
                for (int appBarButtonIndex = 0; appBarButtonIndex < cmdBar.PrimaryCommands.Count; appBarButtonIndex++)
                {
                    AppBarButton currentAppBarButton = ((AppBarButton) (cmdBar.PrimaryCommands[appBarButtonIndex]));
                    if (currentAppBarButton.Name == "appBtnStampCopy")
                    {
                        currentAppBarButton.Visibility = Visibility.Collapsed;
                    }
                    else if (currentAppBarButton.Name == "appBtnStampPaste")
                    {
                        currentAppBarButton.Visibility = Visibility.Visible;
                    }
                    else if (currentAppBarButton.Name == "appBtnStampReset")
                    {
                        currentAppBarButton.IsEnabled = true;
                    }
                }
            }
        }

        void app_btnMoreInfo_Click(object sender, RoutedEventArgs e)
        {
            InfoAboutAndConditionOfUseBox.Visibility = Visibility.Visible;
            ChangeVisibilityOfAppBars(Visibility.Collapsed);
            SetActivityOfToolsControls(false);
        }

        private void app_btnNewPicture_Click(object sender, RoutedEventArgs e)
        {
            // if the working space is empty no message query should come.
            if (PaintingAreaCanvas.Children.Count > 0)
            {
                messageBoxNewDrawingSpace_Click("Neues Bild", false);
            }
            ResetApp();
        }

        public void ChangeEnableOfAppBarButtonResetZoom(bool isEnabled)
        {
            CommandBar cmdBar = (CommandBar)BottomAppBar;

            if (cmdBar != null)
            {
                for (int i = 0; i < cmdBar.PrimaryCommands.Count; i++)
                {
                    if (((AppBarButton) cmdBar.PrimaryCommands[i]).Name == "appButtonResetZoom")
                    {
                        ((AppBarButton) cmdBar.PrimaryCommands[i]).IsEnabled = isEnabled;
                    }
                }
            }
        }

        void app_btnSave_Click(object sender, RoutedEventArgs e)
        {
            PocketPaintApplication.GetInstance().SaveAsPng();
            ShowToastNotification("Bild gespeichert!");
        }

        public void ShowToastNotification(string message)
        {
            ToastTemplateType toastTemplate = ToastTemplateType.ToastImageAndText01;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);
            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode(message));
            ToastNotification toast = new ToastNotification(toastXml);
            ToastNotifier toastNotifier = ToastNotificationManager.CreateToastNotifier();

            toast.ExpirationTime = DateTimeOffset.UtcNow.AddSeconds(1);
            toastNotifier.Show(toast);
        }

        void app_btnImportPicture_Click(object sender, RoutedEventArgs e)
        {
            GridImportImageSelectionControl.Visibility = Visibility.Visible;
            ChangeVisibilityOfAppBars(Visibility.Collapsed);
            InfoBoxActionControl.Visibility = Visibility.Visible;
        }

        void app_btnLoad_Click(object sender, RoutedEventArgs e)
        {
            PocketPaintApplication pocketPaintApplication = PocketPaintApplication.GetInstance();
            pocketPaintApplication.InfoBoxActionControl.Visibility = Visibility.Visible;
            pocketPaintApplication.AppbarTop.Visibility = Visibility.Collapsed;
            if (BottomAppBar != null)
                BottomAppBar.Visibility = Visibility.Collapsed;
            ChangeBackgroundColorAndOpacityOfPaintingAreaCanvas(Colors.Black, 0.5);
            PocketPaintApplication.GetInstance().isLoadPictureClicked = true;
            SetActivityOfToolsControls(false);
        }

        void app_btnFullScreen_Click(object sender, RoutedEventArgs e)
        {
            _isFullscreen = true;

            PocketPaintApplication.GetInstance().AppbarTop.Visibility = Visibility.Collapsed;
            if (BottomAppBar != null)
                BottomAppBar.Visibility = Visibility.Collapsed;
            GrdThicknessControlVisibility = Visibility.Collapsed;
            GridUserControlRectEll.Visibility = Visibility.Collapsed;

            TransformGroup transforms = null;
            if (PocketPaintApplication.GetInstance().GridWorkingSpace.RenderTransform is TransformGroup)
            {
                transforms = PocketPaintApplication.GetInstance().GridWorkingSpace.RenderTransform as TransformGroup;
            }
            if (transforms == null)
            {
                PocketPaintApplication.GetInstance().GridWorkingSpace.RenderTransform = transforms = new TransformGroup();
            }
            transforms.Children.Clear();
            HideStatusAppBar();
        }

        public void DisableToolbarsAndPaintingArea(bool isDisable)
        {
            if (isDisable)
            {
                PaintingAreaCanvas.IsHitTestVisible = false;

                PocketPaintApplication.GetInstance().InfoBoxControl.Visibility = Visibility.Visible;

                PocketPaintApplication.GetInstance().AppbarTop.Visibility = Visibility.Collapsed;
                if (BottomAppBar != null)
                    BottomAppBar.Visibility = Visibility.Collapsed;
            }
            else
            {
                PaintingAreaCanvas.IsHitTestVisible = true;

                PocketPaintApplication.GetInstance().InfoBoxControl.Visibility = Visibility.Collapsed;

                PocketPaintApplication.GetInstance().AppbarTop.Visibility = Visibility.Visible;
                if (BottomAppBar != null)
                    BottomAppBar.Visibility = Visibility.Visible;
            }
        }

        void app_btnClearElementsInWorkingSpace_Click(object sender, RoutedEventArgs e)
        {
            if (PaintingAreaCanvas.Children.Count != 0)
            {
                PaintingAreaCanvas.Children.Clear();
                ChangeEnabledOfASecondaryAppbarButton("appBarButtonClearWorkingSpace", false);
                ChangeEnabledOfASecondaryAppbarButton("appbarButtonSave", false);
                PocketPaintApplication.GetInstance().CropControl.SetCropSelection();
                CommandManager.GetInstance().CommitCommand(new RemoveCommand());
            }
        }

        void app_btn_reset_Click(object sender, RoutedEventArgs e)
        {
            PocketPaintApplication.GetInstance().PaintingAreaManipulationListener.ResetDrawingSpace();
        }

        private void BtnLeft_OnClick(object sender, RoutedEventArgs e)
        {
            EnableResetButtonRotate(-1);
            if (PocketPaintApplication.GetInstance().ToolCurrent.GetToolType() == ToolType.Rotate)
            {
                var rotateTool = (RotateTool)PocketPaintApplication.GetInstance().ToolCurrent;
                rotateTool.RotateLeft();
            }
        }

        public AppBarButton GetAppBarResetButton(string toolName)
        {
            AppBarButton appBarButtonReset = null;
            CommandBar commandBar = (CommandBar)BottomAppBar;

            if (commandBar != null)
            {
                for (int i = 0; i < commandBar.PrimaryCommands.Count; i++)
                {
                    appBarButtonReset = (AppBarButton) (commandBar.PrimaryCommands[i]);
                    string appBarResetName = ("appButtonReset" + toolName);
                    if (appBarButtonReset.Name == appBarResetName)
                    {
                        break;
                    }
                }
            }
            return appBarButtonReset;
        }

        public AppBarButton GetAppBarResetButton()
        {
            AppBarButton appBarButtonReset = null;
            CommandBar commandBar = (CommandBar)BottomAppBar;

            if (commandBar != null)
            {
                for (int i = 0; i < commandBar.PrimaryCommands.Count; i++)
                {
                    appBarButtonReset = (AppBarButton) (commandBar.PrimaryCommands[i]);
                    string appBarResetName = ("appButtonReset");
                    if (appBarButtonReset.Name.Contains(appBarResetName))
                    {
                        break;
                    }
                }
            }
            return appBarButtonReset;
        }

        private void EnableResetButtonFlip(bool isFliped)
        {
            AppBarButton appBarButtonReset = GetAppBarResetButton("Flip");

            if (appBarButtonReset != null)
            {
                if (isFliped)
                {
                    appBarButtonReset.IsEnabled = true;
                }
                else
                {
                    appBarButtonReset.IsEnabled = false;
                }
            }
        }

        public int GetRotationCounter()
        {
            return _rotateCounter;
        }

        public void EnableResetButtonRotate(int number)
        {
            AppBarButton appBarButtonReset = GetAppBarResetButton("Rotate");

            if (appBarButtonReset != null)
            {
                _rotateCounter += number;
                if (_rotateCounter < 0 || _rotateCounter > 3)
                {
                    _rotateCounter = (_rotateCounter < 0) ? 3 : 0;
                }
                appBarButtonReset.IsEnabled = _rotateCounter != 0;
            }
        }

        private void BtnRight_OnClick(object sender, RoutedEventArgs e)
        {
            EnableResetButtonRotate(1);

            if (PocketPaintApplication.GetInstance().ToolCurrent.GetToolType() == ToolType.Rotate)
            {
                var rotateTool = (RotateTool)PocketPaintApplication.GetInstance().ToolCurrent;
                rotateTool.RotateRight();
            }
        }

        private AppBarButton getAppBarButtonBy(string appbarName)
        {
            CommandBar commandBar = (CommandBar)BottomAppBar;
            if (commandBar != null)
            {
                foreach(AppBarButton currentAppbarButton in commandBar.PrimaryCommands)
                { 
                    if (currentAppbarButton.Name == appbarName)
                    {
                        return currentAppbarButton;
                    }
                }
            }
            return null;
        }

        private void EnableResetButtonZoom(int number)
        {
            AppBarButton appBarButtonReset = GetAppBarResetButton("Zoom");

            if (appBarButtonReset != null)
            {
                _zoomCounter += number;
                if (_zoomCounter == 0)
                {
                    appBarButtonReset.IsEnabled = false;
                }
                else
                {
                    appBarButtonReset.IsEnabled = true;
                }
            }
        }

        void BtnZoomOut_Click(object sender, RoutedEventArgs e)
        {
                EnableResetButtonZoom(-1);
                MoveZoomTool tool = new MoveZoomTool();
                ScaleTransform scaletransform = new ScaleTransform();
                scaletransform.ScaleX = 0.9;
                scaletransform.ScaleY = 0.9;
                PocketPaintApplication.GetInstance().isZoomButtonClicked = true;
                tool.HandleMove(scaletransform);
                tool.HandleUp(scaletransform);          
        }

        void BtnZoomIn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                EnableResetButtonZoom(1);
                MoveZoomTool tool = (MoveZoomTool)PocketPaintApplication.GetInstance().ToolCurrent;
                ScaleTransform scaletransform = new ScaleTransform();
                scaletransform.ScaleX = 1.1;
                scaletransform.ScaleY = 1.1;
                PocketPaintApplication.GetInstance().isZoomButtonClicked = true;
                tool.HandleMove(scaletransform);
                tool.HandleUp(scaletransform);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.StackTrace);
            }
        }

        public void ToolChangedHere(ToolBase tool)
        {
            if (tool.GetToolType() == ToolType.Eraser && PocketPaintApplication.GetInstance().isBrushEraser)
            {
                tool = new BrushTool();
            }
            else
            {
                if (PocketPaintApplication.GetInstance().isToolPickerUsed)
                {
                    PocketPaintApplication.GetInstance().isBrushEraser = false;
                }
            }

            ClCursor.Visibility = Visibility.Collapsed;
            GrdThicknessControlVisibility = Visibility.Collapsed;
            VisibilityGridEllRecControl = Visibility.Collapsed;

            switch (tool.GetToolType())
            {
                case ToolType.Brush:
                case ToolType.Cursor:
                case ToolType.Eraser:
                case ToolType.Line:
                    if (tool.GetToolType() == ToolType.Cursor)
                    {
                        CreateAppBarAndSwitchAppBarContent("barCursor");
                        ClCursor.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        CreateAppBarAndSwitchAppBarContent("barBrushEraserLine");
                    }
                    GrdThicknessControlVisibility = PocketPaintApplication.GetInstance().GrdThicknessControlState;
                    break;
                case ToolType.Crop:
                    CreateAppBarAndSwitchAppBarContent("barCrop");
                    break;
                case ToolType.Ellipse:
                    CreateAppBarAndSwitchAppBarContent("barEllipse");
                    VisibilityGridEllRecControl = PocketPaintApplication.GetInstance().GridUcRellRecControlState;
                    break;
                case ToolType.Fill:
                    CreateAppBarAndSwitchAppBarContent("barFill");
                    break;
                case ToolType.Flip:
                    CreateAppBarAndSwitchAppBarContent("barFlip");
                    break;
                case ToolType.ImportPng:
                    CreateAppBarAndSwitchAppBarContent("barImportPng");
                    break;
                case ToolType.Move:
                    CreateAppBarAndSwitchAppBarContent("barMove");
                    break;
                case ToolType.Zoom:
                    CreateAppBarAndSwitchAppBarContent("barZoom");
                    break;
                case ToolType.Pipette:
                    CreateAppBarAndSwitchAppBarContent("barPipette");
                    break;
                case ToolType.Rect:
                    CreateAppBarAndSwitchAppBarContent("barRectangle");
                    VisibilityGridEllRecControl = PocketPaintApplication.GetInstance().GridUcRellRecControlState;
                    break;
                case ToolType.Rotate:
                    CreateAppBarAndSwitchAppBarContent("barRotate");
                    break;
                case ToolType.Stamp:
                    CreateAppBarAndSwitchAppBarContent("barStamp");
                    break;
            }
        }

        public Visibility GrdThicknessControlVisibility
        {
            get
            {
                return GrdThicknessControl.Visibility;
            }
            set
            {
                GrdThicknessControl.Visibility = value;
            }
        }

        public Visibility VisibilityGridEllRecControl
        {
            get
            {
                return GridUserControlRectEll.Visibility;
            }
            set
            {
                GridUserControlRectEll.Visibility = value;
            }
        }
        public void SetRectEllUserControlMargin(Thickness margin)
        {
            GridUserControlRectEll.Margin = margin;
        }

        private void btnThickness_Click(object sender, RoutedEventArgs e)
        {
            UpdateThicknessButtonLayout((AppBarButton)sender);
        }

        private void UpdateThicknessButtonLayout(AppBarButton sender)
        {
            GrdThicknessControlVisibility = GrdThicknessControlVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            Visibility gridThicknessStateInPaintingAreaView = PocketPaintApplication.GetInstance().GrdThicknessControlState;
            gridThicknessStateInPaintingAreaView = gridThicknessStateInPaintingAreaView == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            PocketPaintApplication.GetInstance().GrdThicknessControlState = gridThicknessStateInPaintingAreaView;

            UpdateThicknessControlButton(sender, GrdThicknessControlVisibility);
        }

        private void btnThicknessBorder_Click(object sender, RoutedEventArgs e)
        {
            UpdateThicknessPropertiesButtonLayout((AppBarButton)sender);
        }

        private void UpdateThicknessPropertiesButtonLayout(AppBarButton sender)
        {
            VisibilityGridEllRecControl = VisibilityGridEllRecControl == Visibility.Collapsed
                ? Visibility.Visible : Visibility.Collapsed;
            PocketPaintApplication.GetInstance().GridUcRellRecControlState = VisibilityGridEllRecControl;
            PocketPaintApplication.GetInstance().GridInputScopeControl.Visibility = Visibility.Collapsed;

            UpdateThicknessControlButton(sender, VisibilityGridEllRecControl);
        }

        private void UpdateThicknessControlButton(AppBarButton sender, Visibility vis)
        {
            ToolSettingsTextConverter textConv = new ToolSettingsTextConverter();
            sender.Label = (string)textConv.Convert(vis, null, null, string.Empty);

            ToolSettingsIconConverter iconConv = new ToolSettingsIconConverter();
            var icon = sender.Icon as BitmapIcon;
            if (icon != null) icon.UriSource = (Uri)iconConv.Convert(vis, null, null, string.Empty);
        }

        private void PaintingAreaCanvas_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var point = new Point(Convert.ToInt32(e.GetCurrentPoint(PaintingAreaCanvas).Position.X), Convert.ToInt32(e.GetCurrentPoint(PaintingAreaCanvas).Position.Y));

            //TODO: some bubbling? issue here, fast multiple applicationbartop undos result in triggering this event
            if (point.X < 0 || point.Y < 0 || Spinner.SpinnerActive || e.Handled)
            {
                return;
            }

            PocketPaintApplication.GetInstance().ToolCurrent.HandleDown(point);

            e.Handled = true;
        }

        void PaintingAreaCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var point = new Point(Convert.ToInt32(e.GetCurrentPoint(PaintingAreaCanvas).Position.X), Convert.ToInt32(e.GetCurrentPoint(PaintingAreaCanvas).Position.Y));

            //TODO: some bubbling? issue here, fast multiple applicationbartop undos result in triggering this event
            if (point.X < 0 || point.Y < 0 || Spinner.SpinnerActive || e.Handled)
            {
                return;
            }
            object movezoom;
            movezoom = new TranslateTransform();

            ((TranslateTransform)movezoom).X += point.X;
            ((TranslateTransform)movezoom).Y += point.Y;

            switch (PocketPaintApplication.GetInstance().ToolCurrent.GetToolType())
            {
                case ToolType.Brush:
                case ToolType.Eraser:
                    PocketPaintApplication.GetInstance().ToolCurrent.HandleMove(point);
                    break;
                case ToolType.Cursor:
                case ToolType.Move:
                case ToolType.Zoom:
                    PocketPaintApplication.GetInstance().ToolCurrent.HandleMove(movezoom);
                    break;
                case ToolType.Line:
                    PocketPaintApplication.GetInstance().ToolCurrent.HandleMove(point);
                    break;
            }
        }

        void PaintingAreaCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var point = new Point(Convert.ToInt32(e.GetCurrentPoint(PaintingAreaCanvas).Position.X), Convert.ToInt32(e.GetCurrentPoint(PaintingAreaCanvas).Position.Y));

            //TODO: some bubbling? issue here, fast multiple applicationbartop undos result in triggering this event
            if (point.X < 0 || point.Y < 0 || Spinner.SpinnerActive || e.Handled)
            {
                return;
            }

            PocketPaintApplication.GetInstance().ToolCurrent.HandleUp(point);

            e.Handled = true;
        }

        private void BtnHorizotal_OnClick(object sender, RoutedEventArgs e)
        {
            _flipHorizontal = !_flipHorizontal;

            EnableResetButtonFlip(_flipHorizontal | _flipVertical);

            if (PocketPaintApplication.GetInstance().ToolCurrent.GetToolType() == ToolType.Flip)
            {
                var flipTool = (FlipTool)PocketPaintApplication.GetInstance().ToolCurrent;
                flipTool.FlipHorizontal();
            }
        }

        private void BtnVertical_OnClick(object sender, RoutedEventArgs e)
        {
            _flipVertical = !_flipVertical;

            EnableResetButtonFlip(_flipHorizontal | _flipVertical);

            if (!_flipVertical && !_flipHorizontal)
            {

            }
            if (PocketPaintApplication.GetInstance().ToolCurrent.GetToolType() == ToolType.Flip)
            {
                var flipTool = (FlipTool)PocketPaintApplication.GetInstance().ToolCurrent;
                flipTool.FlipVertical();
            }
        }

        private void LoadTapEvent()
        {
            if (PocketPaintApplication.GetInstance() != null && !_isTapLoaded)
            {
                PaintingAreaCanvas.Tapped += PaintingAreaCanvas_Tapped;
                _isTapLoaded = true;
            }
        }

        private void UnloadTapEvent()
        {
            if (PocketPaintApplication.GetInstance() != null && _isTapLoaded)
            {
                PaintingAreaCanvas.Tapped -= PaintingAreaCanvas_Tapped;
                _isTapLoaded = false;
            }
        }

        private void LoadManipulationPaintingAreaCanvasEvents()
        {
            if (PocketPaintApplication.GetInstance() != null)
            {
                PaintingAreaManipulationListener currentAbl = PocketPaintApplication.GetInstance().PaintingAreaManipulationListener;
                // PaintingAreaCanvas
                PaintingAreaCanvas.ManipulationStarted += currentAbl.ManipulationStarted;
                PaintingAreaCanvas.ManipulationDelta += currentAbl.ManipulationDelta;
                PaintingAreaCanvas.ManipulationCompleted += currentAbl.ManipulationCompleted;
            }
        }


        private void LoadManipulationEraserCanvasEvents()
        {
            if (PocketPaintApplication.GetInstance() != null)
            {
                EraserCanvas.PointerEntered += EraserCanvas_PointerEntered;
                EraserCanvas.PointerMoved += EraserCanvas_PointerMoved;
                EraserCanvas.PointerReleased += EraserCanvas_PointerReleased;
            }
        }

        void EraserCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var point = new Point(Convert.ToInt32(e.GetCurrentPoint(EraserCanvas).Position.X), Convert.ToInt32(e.GetCurrentPoint(EraserCanvas).Position.Y));

            //TODO: some bubbling? issue here, fast multiple applicationbartop undos result in triggering this event
            if (point.X < 0 || point.Y < 0 || Spinner.SpinnerActive || e.Handled)
            {
                return;
            }

            PocketPaintApplication.GetInstance().ToolCurrent.HandleUp(point);

            e.Handled = true;
        }

        private void EraserCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var point = new Point(Convert.ToInt32(e.GetCurrentPoint(EraserCanvas).Position.X), Convert.ToInt32(e.GetCurrentPoint(EraserCanvas).Position.Y));

            //TODO: some bubbling? issue here, fast multiple applicationbartop undos result in triggering this event
            if (point.X < 0 || point.Y < 0 || Spinner.SpinnerActive || e.Handled)
            {
                return;
            }
            PocketPaintApplication.GetInstance().ToolCurrent.HandleMove(point);
        }

        private void EraserCanvas_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var point = new Point(Convert.ToInt32(e.GetCurrentPoint(EraserCanvas).Position.X), Convert.ToInt32(e.GetCurrentPoint(EraserCanvas).Position.Y));

            //TODO: some bubbling? issue here, fast multiple applicationbartop undos result in triggering this event
            if (point.X < 0 || point.Y < 0 || Spinner.SpinnerActive || e.Handled)
            {
                return;
            }

            PocketPaintApplication.GetInstance().ToolCurrent.HandleDown(point);

            e.Handled = true;
        }

        private void UnloadManipulationPaintingAreaCanvasEvents()
        {
            if (PocketPaintApplication.GetInstance() != null)
            {
                PaintingAreaManipulationListener currentAbl = PocketPaintApplication.GetInstance().PaintingAreaManipulationListener;
                PaintingAreaCanvas.ManipulationStarted -= currentAbl.ManipulationStarted;
                PaintingAreaCanvas.ManipulationDelta -= currentAbl.ManipulationDelta;
                PaintingAreaCanvas.ManipulationCompleted -= currentAbl.ManipulationCompleted;
            }
        }

        private void LoadPointerEvents()
        {
            if (!_isPointerEventLoaded)
            {
                PaintingAreaCanvas.PointerEntered += PaintingAreaCanvas_PointerEntered;
                PaintingAreaCanvas.PointerMoved += PaintingAreaCanvas_PointerMoved;
                PaintingAreaCanvas.PointerReleased += PaintingAreaCanvas_PointerReleased;
                _isPointerEventLoaded = true;
            }
        }
        private void UnloadPointerEvents()
        {
            if (_isPointerEventLoaded)
            {
                PaintingAreaCanvas.PointerEntered -= PaintingAreaCanvas_PointerEntered;
                PaintingAreaCanvas.PointerMoved -= PaintingAreaCanvas_PointerMoved;
                PaintingAreaCanvas.PointerReleased -= PaintingAreaCanvas_PointerReleased;
                _isPointerEventLoaded = false;
            }
        }

        private void CursorControl_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //TODO: Empty?
        }

        public Visibility SetVisibilityOfUcRectangleSelectionControl
        {
            get
            {
                return ucRectangleSelectionControl.Visibility;
            }
            set
            {
                ucRectangleSelectionControl.Visibility = value;
            }
        }

        public Visibility SetVisibilityOfUcEllipseSelectionControl
        {
            get
            {
                return ucEllipseSelectionControl.Visibility;
            }
            set
            {
                ucEllipseSelectionControl.Visibility = value;
            }
        }

        public void ChangeBackgroundColorAndOpacityOfPaintingAreaCanvas(Color color, double opacity)
        {
            PaintingAreaCanvas.Background = new SolidColorBrush(color);
            PaintingAreaCanvas.Background.Opacity = opacity;
        }

        public async void messageBoxNewDrawingSpace_Click(string message, bool shouldAppClosed)
        {
            var messageDialog = new MessageDialog("Änderungen speichern?", message);

            messageDialog.Commands.Add(new UICommand(
                "Speichern",
                new UICommandInvokedHandler(SaveChanges)));
            messageDialog.Commands.Add(new UICommand(
                "Verwerfen",
                new UICommandInvokedHandler(DeleteChanges)));

            messageDialog.DefaultCommandIndex = 0;

            await messageDialog.ShowAsync();
        }

        public void SaveChanges(IUICommand command)
        {
            if (PocketPaintApplication.GetInstance().shouldAppClosedThroughBackButton)
            {
                Application.Current.Exit();
            }
            else
            {
                PocketPaintApplication.GetInstance().SaveAsPng();
                CommandManager.GetInstance().clearAllCommands();
                ChangeBackgroundColorAndOpacityOfPaintingAreaCanvas(Colors.Transparent, 1.0);
                UndoRedoActionbarManager.GetInstance().Update(UndoRedoActionbarManager.UndoRedoButtonState.DisableUndo);
                ChangeEnabledOfASecondaryAppbarButton("appBarButtonClearWorkingSpace", false);
                ChangeEnabledOfASecondaryAppbarButton("appbarButtonSave", false);
            }
        }

        public void DeleteChanges(IUICommand command)
        {
            if (PocketPaintApplication.GetInstance().shouldAppClosedThroughBackButton)
            {
                Application.Current.Exit();
            }
            else
            {
                ResetTools();
                CommandManager.GetInstance().clearAllCommands();
                ChangeBackgroundColorAndOpacityOfPaintingAreaCanvas(Colors.Transparent, 1.0);
                UndoRedoActionbarManager.GetInstance().Update(UndoRedoActionbarManager.UndoRedoButtonState.DisableUndo);
                ChangeEnabledOfASecondaryAppbarButton("appBarButtonClearWorkingSpace", false);
                ChangeEnabledOfASecondaryAppbarButton("appbarButtonSave", false);
            }
        }

        public void ResetTools()
        {
            PocketPaintApplication.GetInstance().PaintingAreaCanvas.Children.Clear();
            PocketPaintApplication.GetInstance().PaintingAreaCanvas.RenderTransform = new TransformGroup();
            PocketPaintApplication.GetInstance().PaintingAreaView.AlignPositionOfGridWorkingSpace(null);
            PocketPaintApplication.GetInstance().PaintingAreaView.DisableToolbarsAndPaintingArea(false);
        }

        public void ResetControls()
        {
            Visibility visibility = Visibility.Collapsed;
            PocketPaintApplication.GetInstance().EllipseSelectionControl.Visibility = visibility;
            PocketPaintApplication.GetInstance().GridImportImageSelectionControl.Visibility = visibility;
            PocketPaintApplication.GetInstance().GridInputScopeControl.Visibility = visibility;
            PocketPaintApplication.GetInstance().GridUcRellRecControlState = visibility;
            PocketPaintApplication.GetInstance().InfoBoxActionControl.Visibility = visibility;
            PocketPaintApplication.GetInstance().RectangleSelectionControl.Visibility = visibility;
            PocketPaintApplication.GetInstance().CropControl.Visibility = visibility;
            PocketPaintApplication.GetInstance().StampControl.Visibility = visibility;

            //TODO: Die folgenden Code-zeilen gehören in eine eigene Funktion ausgelagert.
            //PocketPaintApplication.GetInstance().EllipseSelectionControl.IsHitTestVisible = true;
            //PocketPaintApplication.GetInstance().RectangleSelectionControl.IsHitTestVisible = true;
            // PocketPaintApplication.GetInstance().PaintingAreaView.changeBackgroundColorAndOpacityOfPaintingAreaCanvas(Colors.Transparent, 1.0);
        }

        public void ChangeEnabledOfASecondaryAppbarButton(string appBarButtonName, bool isEnabled)
        {
            CommandBar cmdBar = (CommandBar)BottomAppBar;

            if (cmdBar != null)
            {
                for (int i = 0; i < cmdBar.SecondaryCommands.Count; i++)
                {
                    if (((AppBarButton) cmdBar.SecondaryCommands[i]).Name == appBarButtonName)
                    {
                        ((AppBarButton) cmdBar.SecondaryCommands[i]).IsEnabled = isEnabled;
                        break;
                    }
                }
            }
        }

        public void  AddElementToPaintingAreCanvas(Path path)
        {
            if (path != null)
            {
                PaintingAreaCanvas.Children.Add(path);
                ChangeEnabledOfASecondaryAppbarButton("appBarButtonClearWorkingSpace", true);
                ChangeEnabledOfASecondaryAppbarButton("appbarButtonSave", true);
            }
        }

        public void AddElementToPaintingAreCanvas(Rectangle rectangle)
        {
            if (rectangle != null)
            {
                Rectangle rectangleToDraw = rectangle;
                rectangle.Height = rectangle.Height;
                rectangle.Width = rectangle.Width;
                PaintingAreaCanvas.Children.Add(rectangle);
                ChangeEnabledOfASecondaryAppbarButton("appBarButtonClearWorkingSpace", true);
                ChangeEnabledOfASecondaryAppbarButton("appbarButtonSave", true);
            }
        }

        public void addElementToEraserCanvas(Path path)
        {
            if (path != null)
            {
                EraserCanvas.Children.Clear();
                EraserCanvas.Visibility = Visibility.Visible;
                EraserCanvas.Children.Add(path);
                ChangeEnabledOfASecondaryAppbarButton("appBarButtonClearWorkingSpace", true);
                ChangeEnabledOfASecondaryAppbarButton("appbarButtonSave", true);
            }
        }

        public void AddElementToPaintingAreCanvas(Image image, int xCoordinate, int yCoordinate)
        {
            if (image != null)
            {
                Canvas.SetLeft(image, xCoordinate);
                Canvas.SetTop(image, yCoordinate);
                PaintingAreaCanvas.Children.Add(image);
                ChangeEnabledOfASecondaryAppbarButton("appBarButtonClearWorkingSpace", true);
                ChangeEnabledOfASecondaryAppbarButton("appbarButtonSave", true);
            }
        }

        public bool IsASelectionControlSelected()
        {
            bool isSelectionControlSelected = ucEllipseSelectionControl.Visibility == Visibility.Visible
                || ucRectangleSelectionControl.Visibility == Visibility.Visible
                || GridImportImageSelectionControl.Visibility == Visibility.Visible;
            return isSelectionControlSelected;
        }

        public void ChangeVisibilityOfSelectionsControls(Visibility visibility)
        {
            SetVisibilityOfUcEllipseSelectionControl = visibility;
            SetVisibilityOfUcRectangleSelectionControl = visibility;
            GridImportImageSelectionControl.Visibility = visibility;
            ctrlCropControl.Visibility = visibility;
            ctrlStampControl.Visibility = visibility;
        }

        public void ChangeVisibilityOfActiveSelectionControl(Visibility visibility)
        {
            if (PocketPaintApplication.GetInstance().ToolCurrent.GetToolType() == ToolType.Crop)
            {
                ctrlCropControl.Visibility = visibility;
            }
            else if (PocketPaintApplication.GetInstance().ToolCurrent.GetToolType() == ToolType.Ellipse)
            {
                SetVisibilityOfUcEllipseSelectionControl = visibility;
            }
            else if (PocketPaintApplication.GetInstance().ToolCurrent.GetToolType() == ToolType.ImportPng)
            {
                GridImportImageSelectionControl.Visibility = visibility;
            }
            else if (PocketPaintApplication.GetInstance().ToolCurrent.GetToolType() == ToolType.Rect)
            {
                SetVisibilityOfUcRectangleSelectionControl = visibility;
            }
            else if (PocketPaintApplication.GetInstance().ToolCurrent.GetToolType() == ToolType.Stamp)
            {
                ctrlStampControl.Visibility = visibility;
            }
        }

        public void ResetActiveSelectionControl()
        {
            if (PocketPaintApplication.GetInstance().ToolCurrent.GetToolType() == ToolType.Crop
                || PocketPaintApplication.GetInstance().ToolCurrent.GetToolType() == ToolType.Ellipse
                || PocketPaintApplication.GetInstance().ToolCurrent.GetToolType() == ToolType.ImportPng
                || PocketPaintApplication.GetInstance().ToolCurrent.GetToolType() == ToolType.Rect
                || PocketPaintApplication.GetInstance().ToolCurrent.GetToolType() == ToolType.Stamp
                )
            {
                PocketPaintApplication.GetInstance().ToolCurrent.ResetDrawingSpace();
            }
        }

        public void ResetApp()
        {
            PaintData paintData = PocketPaintApplication.GetInstance().PaintData;
            PaintingAreaCanvas.Height = Window.Current.Bounds.Height;
            PaintingAreaCanvas.Width = Window.Current.Bounds.Width;
            AlignPositionOfGridWorkingSpace(null);
            ResetControls();
            PocketPaintApplication.GetInstance().SwitchTool(ToolType.Brush);
            CommandManager.GetInstance().clearAllCommands();
            ChangeBackgroundColorAndOpacityOfPaintingAreaCanvas(Colors.Transparent, 1.0);
            UndoRedoActionbarManager.GetInstance().Update(UndoRedoActionbarManager.UndoRedoButtonState.DisableUndo);
            paintData.colorSelected = new SolidColorBrush(Colors.Black);
            paintData.strokeColorSelected = new SolidColorBrush(Colors.Gray);
            paintData.thicknessSelected = 8;
            paintData.strokeThickness = 3.0;
            GrdThicknessControl.Visibility = Visibility.Collapsed;
            PocketPaintApplication.GetInstance().resetBoolVariables(false, true, false, true, false, false);
            CtrlThicknessControl.SetValueBtnBrushThickness(paintData.thicknessSelected);
            CtrlThicknessControl.SetValueSliderThickness(paintData.thicknessSelected);
            CtrlThicknessControl.CheckAndSetPenLineCap(PenLineCap.Round);

            PocketPaintApplication.GetInstance().angularDegreeOfWorkingSpaceRotation = 0;
            PocketPaintApplication.GetInstance().flipX = 1;
            PocketPaintApplication.GetInstance().flipY = 1;
        }

        public AppBarButton GetAppBarButtonByName(string toolName)
        {
            AppBarButton appBarButton = null;
            CommandBar commandBar = (CommandBar)BottomAppBar;
            string appBarName = toolName;

            if (commandBar != null)
            {
                for (int i = 0; i < commandBar.PrimaryCommands.Count; i++)
                {
                    var curr = (AppBarButton) (commandBar.PrimaryCommands[i]);

                    if (curr.Name == appBarName)
                    {
                        appBarButton = curr;
                        break;
                    }
                }
            }
            return appBarButton;
        }

        private void GridWorkingSpace_ManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
        {
            if (GrdThicknessControlVisibility == Visibility.Visible)
            {
                var button = GetAppBarButtonByName("ThicknessButton");
                UpdateThicknessButtonLayout(button);
            }
            else if(VisibilityGridEllRecControl == Visibility.Visible)
            {
                var button = GetAppBarButtonByName("ThicknessProperties");
                UpdateThicknessPropertiesButtonLayout(button);
            }
        }

        public void isWorkingSpaceMoved(bool isWorkingSpaceMoved)
        {
            _m_isWorkingSpaceMoved = isWorkingSpaceMoved;
        }
        
        public void setFlippedVertical(bool flippedVerticalValue)
        {
            _flipVertical = flippedVerticalValue;
        }

        public void setFlippedHorizontal(bool flippedHorizontalValue)
        {
            _flipHorizontal = flippedHorizontalValue;
        }

    }

}
