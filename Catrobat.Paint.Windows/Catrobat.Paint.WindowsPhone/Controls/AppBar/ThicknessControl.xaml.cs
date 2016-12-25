using System;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

// Die Elementvorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkID=390556 dokumentiert.

namespace Catrobat.Paint.WindowsPhone.Controls.AppBar
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet werden kann oder auf die innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class ThicknessControl
    {
        Int32 slider_thickness_textbox_last_value = 1;
        double width_multiplicator = PocketPaintApplication.GetInstance().size_width_multiplication;
        double height_multiplicator = PocketPaintApplication.GetInstance().size_width_multiplication;

        public ThicknessControl()
        {
            this.InitializeComponent();
            setLayout();
        }

        private void setLayout()
        {
            GrdLayoutRoot.Width *= width_multiplicator;
            GrdLayoutRoot.Height *= height_multiplicator;

            GrdSliderThickness.Width *= width_multiplicator;
            GrdSliderThickness.Height *= height_multiplicator;
            GrdSliderThickness.Margin = new Thickness(
                                            GrdSliderThickness.Margin.Left * width_multiplicator,
                                            GrdSliderThickness.Margin.Top * height_multiplicator,
                                            GrdSliderThickness.Margin.Right * width_multiplicator,
                                            GrdSliderThickness.Margin.Bottom * height_multiplicator);

            foreach (Object obj in GrdLayoutRoot.Children.Concat(GrdBrushType.Children.Concat(GrdSlider.Children)))
            {
                if (obj.GetType() == typeof(Button))
                {
                    Button button = ((Button)obj);
                    button.Height *= height_multiplicator;
                    button.Width *= width_multiplicator;

                    button.Margin = new Thickness(
                                            button.Margin.Left * width_multiplicator,
                                            button.Margin.Top * height_multiplicator,
                                            button.Margin.Right * width_multiplicator,
                                            button.Margin.Bottom * height_multiplicator);

                    button.FontSize *= height_multiplicator;

                    var buttonContent = ((Button)obj).Content;
                    if (buttonContent != null && buttonContent.GetType() == typeof(Image))
                    {
                        Image contentImage = (Image)buttonContent;
                        contentImage.Height *= height_multiplicator;
                        contentImage.Width *= width_multiplicator;

                        contentImage.Margin = new Thickness(
                                                contentImage.Margin.Left * width_multiplicator,
                                                contentImage.Margin.Top * height_multiplicator,
                                                contentImage.Margin.Right * width_multiplicator,
                                                contentImage.Margin.Bottom * height_multiplicator);
                    }
                }
                else if (obj.GetType() == typeof(Slider))
                {
                    Slider slider = (Slider)obj;
                    slider.Height *= height_multiplicator;
                    slider.Width *= width_multiplicator;

                    slider.Margin = new Thickness(
                                            slider.Margin.Left * width_multiplicator,
                                            slider.Margin.Top * height_multiplicator,
                                            slider.Margin.Right * width_multiplicator,
                                            slider.Margin.Bottom * height_multiplicator);
                }
                else if(obj.GetType() == typeof(TextBox))
                {
                    TextBox textbox = (TextBox)obj;
                    textbox.Height *= height_multiplicator;
                    textbox.Width *= width_multiplicator;

                    textbox.Margin = new Thickness(
                                            textbox.Margin.Left * width_multiplicator,
                                            textbox.Margin.Top * height_multiplicator,
                                            textbox.Margin.Right * width_multiplicator,
                                            textbox.Margin.Bottom * height_multiplicator);
                }
            }

            SliderThickness.Value = PocketPaintApplication.GetInstance().PaintData.thicknessSelected;
        }

        public void CheckAndSetPenLineCap(PenLineCap penLineCap)
        {
            SolidColorBrush brushGray = new SolidColorBrush(Colors.Gray);
            SolidColorBrush brushWhite = new SolidColorBrush(Colors.Black);
            if (penLineCap == PenLineCap.Round)
            {
                BtnRoundImage.BorderBrush = brushWhite;
                BtnSquareImage.BorderBrush = brushGray;
                BtnTriangleImage.BorderBrush = brushGray;
            }
            else if (penLineCap == PenLineCap.Square)
            {
                BtnRoundImage.BorderBrush = brushGray;
                BtnSquareImage.BorderBrush = brushWhite;
                BtnTriangleImage.BorderBrush = brushGray;
            }
            else
            {
                BtnRoundImage.BorderBrush = brushGray;
                BtnSquareImage.BorderBrush = brushGray;
                BtnTriangleImage.BorderBrush = brushWhite;
            }
        }

        public void RoundButton_OnClick(object sender, RoutedEventArgs e)
        {
            var penLineCap = PenLineCap.Round;
            PocketPaintApplication.GetInstance().PaintData.penLineCapSelected = penLineCap;
            PocketPaintApplication.GetInstance().cursorControl.changeCursorType(penLineCap);
            CheckAndSetPenLineCap(penLineCap);
        }

        public void SquareButton_OnClick(object sender, RoutedEventArgs e)
        {
            var penLineCap = PenLineCap.Square;
            PocketPaintApplication.GetInstance().PaintData.penLineCapSelected = penLineCap;
            PocketPaintApplication.GetInstance().cursorControl.changeCursorType(penLineCap);
            CheckAndSetPenLineCap(penLineCap);
        }

        public void SetValueBtnBrushThickness(int value)
        {
            tbBrushThickness.Text = value.ToString();
        }

        public void SetValueSliderThickness(double value)
        {
            SliderThickness.Value = value;
        }

        public void TriangleButton_OnClick(object sender, RoutedEventArgs e)
        {
            var penLineCap = PenLineCap.Triangle;
            PocketPaintApplication.GetInstance().PaintData.penLineCapSelected = penLineCap;
            PocketPaintApplication.GetInstance().cursorControl.changeCursorType(penLineCap);
            CheckAndSetPenLineCap(penLineCap);
        }

        private void SliderThickness_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (SliderThickness != null)
            {
                tbBrushThickness.Text = Convert.ToInt32(SliderThickness.Value).ToString();
                slider_thickness_textbox_last_value = Convert.ToInt32(SliderThickness.Value);
                PocketPaintApplication.GetInstance().PaintData.thicknessSelected = Convert.ToInt32(SliderThickness.Value);
                if (PocketPaintApplication.GetInstance().cursorControl != null)
                {
                    PocketPaintApplication.GetInstance().cursorControl.changeCursorsize();
                }
            }
        }
    }
}