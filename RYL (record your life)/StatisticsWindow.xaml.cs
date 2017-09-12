using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Globalization;

namespace RYL__record_your_life_
{
    /*
     * Вид даты на графике:
     * День.месяц.год,
     * День.месяц,
     * День.год,
     * Год,
     * День,
     * Месяц,
     * Месяц.Год,
     * Ничего
     */
    enum DateTimeStyle
    {
        DayMonthYear,
        DayMonth,
        DayYear,
        Year,
        Day,
        Month,
        MonthYear,
        None
    }

    public partial class StatisticsWindow : Window
    {
        private RYL lifeRecorder;
        private Graph graph;                 // Объект графика
        private Int32 fontSizeOnX = 9;       // Размер шрифта по оси Y
        private Int32 fontSizeOnY = 11;      // Размер шрифта по оси X
        private Boolean isSearchNeed;        // Для поиска
        private Boolean allowChangeSettings; // Разрешение за изменение даты на графике

        public StatisticsWindow(RYL lifeRecorder)
        {
            InitializeComponent();
            this.lifeRecorder = lifeRecorder ?? throw new ArgumentNullException("Null argument");
        }

        #region Events
        private void StatisticsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Closing += StatisticsWindow_Closing;
            CDay.IsChecked = true;
            allowChangeSettings = true;
        }

        private void CSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            if (CSearch.Text == "Поиск..." && CSearch.FontStyle == FontStyles.Italic)
            {
                CSearch.FontStyle = FontStyles.Normal;
                CSearch.Foreground = Brushes.Black;
                CSearch.Clear();
                isSearchNeed = true;
            }
        }
        private void CSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (CSearch.Text == "")
            {
                isSearchNeed = false;
                CSearch.Clear();
                CSearch.Text = "Поиск...";
                CSearch.Foreground = Brushes.Gray;
                CSearch.FontStyle = FontStyles.Italic;
            }
        }
        private void CSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isSearchNeed)
            {
                if (CSearch.Text == "")
                    FillEvents();
                else
                {
                    LifeEvent[] searchResult = lifeRecorder.SearchInEvents(CSearch.Text);
                    if (searchResult != null)
                        FillEvents(searchResult);
                    else
                        CEventsNames.Items.Clear();
                }
            }
        }
        private void CEventsNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DrawGraph();
        }

        private void CPeriod_Changed(object sender, RoutedEventArgs e)
        {
            DrawGraph();
        }
        private void CSettings_Changed(object sender, RoutedEventArgs e)
        {
            if (allowChangeSettings)
                DrawGraph();
        }

        private void CProjectionWidth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (allowChangeSettings)
                DrawGraph();
        }
        private void CFontSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (Int32.TryParse(textBox.Text, out Int32 value))
                {
                    if (value <= 0 || value >= 35790) return;
                    if (textBox.Name == "CFontSizeOnX")
                    {
                        fontSizeOnX = value;
                    }
                    else
                    {
                        fontSizeOnY = value;
                    }
                    DrawGraph();
                }
            }
        }

        private void StatisticsWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
        #endregion
        #region Methods
        private Int32 GetSelectedEventID()
        {
            int ID = lifeRecorder.GetEventIDByName(CEventsNames.SelectedItem.ToString());
            if (ID < 0)
            {
                MessageBox.Show("Ошибка индексации. Обратитесь к разработчику о найденном баге. Приносим извинения.", "Ошибка");
                Application.Current.Shutdown();
            }
            return ID;
        }
        public void FillEvents()
        {
            CEventsNames.Items.Clear();

            String[] names = lifeRecorder.GetEventsNames();
            foreach (String name in names)
            {
                CEventsNames.Items.Add(name);
            }
        }
        private void FillEvents(LifeEvent[] lifeEvents)
        {
            CEventsNames.Items.Clear();

            foreach (LifeEvent lifeEvent in lifeEvents)
            {
                CEventsNames.Items.Add(lifeEvent.Name);
            }
        }

        private void DrawGraph()
        {
            if (CEventsNames.SelectedIndex == -1) return;

            int ID = GetSelectedEventID();
            Int32[] values = null;
            Record[] valuesOnX = null;
            Int32 maxRecordsCount = 0;
            if (CDay.IsChecked ?? false)
            {
                maxRecordsCount = lifeRecorder[ID].GetMaxRecordsOnPeriodCount(Period.Day);
                values = lifeRecorder[ID].GetNumberOfRecordsOnPeriod(Period.Day);
                valuesOnX = lifeRecorder[ID].GetRecordsOnPeriod(Period.Day);
            }
            else if (CMonth.IsChecked ?? false)
            {
                maxRecordsCount = lifeRecorder[ID].GetMaxRecordsOnPeriodCount(Period.Month);
                values = lifeRecorder[ID].GetNumberOfRecordsOnPeriod(Period.Month);
                valuesOnX = lifeRecorder[ID].GetRecordsOnPeriod(Period.Month);
            }
            else if (CYear.IsChecked ?? false)
            {
                maxRecordsCount = lifeRecorder[ID].GetMaxRecordsOnPeriodCount(Period.Year);
                values = lifeRecorder[ID].GetNumberOfRecordsOnPeriod(Period.Year);
                valuesOnX = lifeRecorder[ID].GetRecordsOnPeriod(Period.Year);
            }

            DateTime[] dates = new DateTime[valuesOnX.Length];
            for (int i = 0; i < valuesOnX.Length; i++)
                dates[i] = valuesOnX[i].Date;

            DateTimeStyle dateTimeStyle;
            if (CShowDay.IsChecked ?? false)
            {
                if (CShowMonth.IsChecked ?? false)
                {
                    if (CShowYear.IsChecked ?? false)
                        dateTimeStyle = DateTimeStyle.DayMonthYear;
                    else
                        dateTimeStyle = DateTimeStyle.DayMonth;
                }
                else
                {
                    if (CShowYear.IsChecked ?? false)
                        dateTimeStyle = DateTimeStyle.DayYear;
                    else
                        dateTimeStyle = DateTimeStyle.Day;
                }
            }
            else
            {
                if (CShowMonth.IsChecked ?? false)
                {
                    if (CShowYear.IsChecked ?? false)
                        dateTimeStyle = DateTimeStyle.MonthYear;
                    else
                        dateTimeStyle = DateTimeStyle.Month;
                }
                else
                {
                    if (CShowYear.IsChecked ?? false)
                        dateTimeStyle = DateTimeStyle.Year;
                    else
                        dateTimeStyle = DateTimeStyle.None;
                }
            }
            graph = new Graph(CCanvas, CCanvas.Width - 30, CCanvas.Height - 30, 20, 10, values.Length, maxRecordsCount, dates, dateTimeStyle,
                Int32.Parse(CFontSizeOnX.Text), Int32.Parse(CFontSizeOnY.Text));

            String projectionXValue = CXProjectionWidth.SelectedItem.ToString();
            projectionXValue = projectionXValue.Substring(projectionXValue.IndexOf(':') + 2);
            String projectionYValue = CYProjectionWidth.SelectedItem.ToString();
            projectionYValue = projectionYValue.Substring(projectionYValue.IndexOf(':') + 2);

            graph.DrawChart(values, CXProjection.IsChecked ?? false, CYProjection.IsChecked ?? false,
                Double.Parse(projectionXValue), Double.Parse(projectionYValue));
        }

        public void OnEventsChanged()
        {
            if (CSearch.Foreground == Brushes.Gray)
            {
                FillEvents();
            }
            else
            {
                LifeEvent[] searchResult = lifeRecorder.SearchInEvents(CSearch.Text);
                if (searchResult != null)
                    FillEvents(searchResult);
                else
                    CEventsNames.Items.Clear();
            }
        }
        #endregion
    }

    internal sealed class Graph
    {
        private Canvas canvas;
        private Double graphWidth, graphHeight;
        private Double x1, y1;
        private Int32 segmentXCount, segmentYCount;
        private Double segmentXSize, segmentYSize;
        private DateTimeStyle dateTimeStyle;
        private Int32 fontSizeOnX, fontSizeOnY;
        private DateTime[] valuesOnX;

        private SolidColorBrush color;
        private readonly Double STROKE;
        private readonly Int32 SEGMENTHEIGHT;


        public Graph(Canvas canvas, Double graphWidth, Double graphHeight, Double x1, Double y1, Int32 segmentXCount, Int32 segmentYCount,
            DateTime[] valuesOnX, DateTimeStyle dateTimeStyle, Int32 fontSizeOnX = 9, Int32 fontSizeOnY = 11, Int32 stroke = 1, Int32 segmentHeight = 5)
        {
            if (graphWidth <= 0 || graphHeight <= 0 || segmentXCount < 0 || segmentXCount != valuesOnX.Length || segmentYCount < 0 || stroke <= 0 ||
                segmentHeight <= 0 || fontSizeOnX <= 0 || fontSizeOnY <= 0 || fontSizeOnX >= 35790 || fontSizeOnY >= 35790)
                throw new ArgumentException("Invalid argument(s).");
            this.canvas = canvas ?? throw new ArgumentNullException("Null argument.");
            this.valuesOnX = valuesOnX ?? throw new ArgumentException("Null argument.");

            this.graphWidth = graphWidth;
            this.graphHeight = graphHeight;
            this.x1 = x1;
            this.y1 = y1;
            this.segmentXCount = segmentXCount;
            this.segmentYCount = segmentYCount;
            this.dateTimeStyle = dateTimeStyle;
            this.fontSizeOnX = fontSizeOnX;
            this.fontSizeOnY = fontSizeOnY;

            color = Brushes.Black;
            STROKE = stroke;
            SEGMENTHEIGHT = segmentHeight;

            canvas.Children.Clear();
            DrawAxis();
        }

        // Система координат
        private void DrawAxis()
        {
            // Абсцисса
            DrawAbscissa();

            // Ордината
            DrawOrdinate();

            // Сегменты
            DrawXSegments();
            DrawYSegments();
        }
        // Ось абсцисс.
        private void DrawAbscissa()
        {
            Line ordinate = new Line()
            {
                X1 = x1,
                Y1 = graphHeight + y1,
                X2 = graphWidth + x1,
                Y2 = graphHeight + y1,
                Stroke = color,
                StrokeThickness = STROKE
            };
            canvas.Children.Add(ordinate);
            // Стрелка.
            {
                Line line3 = new Line()
                {
                    X1 = graphWidth + x1,
                    Y1 = graphHeight + y1,
                    X2 = graphWidth + x1 - 10,
                    Y2 = graphHeight + y1 - 5,
                    Stroke = color,
                    StrokeThickness = STROKE
                };
                canvas.Children.Add(line3);
                Line line4 = new Line()
                {
                    X1 = graphWidth + x1,
                    Y1 = graphHeight + y1,
                    X2 = graphWidth + x1 - 10,
                    Y2 = graphHeight + y1 + 5,
                    Stroke = color,
                    StrokeThickness = STROKE
                };
                canvas.Children.Add(line4);
            }
            // Текст.
            {
                TextBlock text2 = new TextBlock()
                {
                    Text = "Дата",
                    FontSize = 10,
                    Foreground = color
                };
                Canvas.SetRight(text2, 1);
                Canvas.SetBottom(text2, 25);
                canvas.Children.Add(text2);
            }
        }
        // Ось ординат.
        private void DrawOrdinate()
        {
            Line abscissa = new Line()
            {
                X1 = x1,
                Y1 = y1,
                X2 = x1,
                Y2 = graphHeight + y1,
                Stroke = color,
                StrokeThickness = STROKE
            };
            canvas.Children.Add(abscissa);
            // Стрелка.
            {
                Line line1 = new Line()
                {
                    X1 = x1,
                    Y1 = y1,
                    X2 = x1 - 5,
                    Y2 = y1 + 10,
                    Stroke = color,
                    StrokeThickness = STROKE
                };
                canvas.Children.Add(line1);

                Line line2 = new Line()
                {
                    X1 = x1,
                    Y1 = y1,
                    X2 = x1 + 5,
                    Y2 = y1 + 10,
                    Stroke = Brushes.Black,
                    StrokeThickness = STROKE
                };
                canvas.Children.Add(line2);
            }
            // Текст.
            {
                TextBlock text1 = new TextBlock()
                {
                    Text = "Кол-во",
                    FontSize = 10,
                    Foreground = color
                };
                Canvas.SetLeft(text1, 1);
                Canvas.SetTop(text1, -2);
                canvas.Children.Add(text1);
            }
        }
        // Сегменты по абсциссе.
        private void DrawXSegments()
        {
            if (segmentXCount > 1)
            {
                segmentXSize = (graphWidth - 10) / (segmentXCount - 1);
                // Координаты очередного сегмента будем высчитывать через эти переменные.
                Double SX = x1 + segmentXSize;
                Double SY = y1 + graphHeight - SEGMENTHEIGHT;

                for (int i = 1; i < segmentXCount; i++)
                {
                    if (i != 1) SX += segmentXSize;
                    Line line = new Line()
                    {
                        X1 = SX,
                        Y1 = SY,
                        X2 = SX,
                        Y2 = SY + SEGMENTHEIGHT * 2,
                        Stroke = color,
                        StrokeThickness = STROKE
                    };
                    canvas.Children.Add(line);
                }
            }

            // Значения на отрезках абсциссы
            Double left = 0;
            Double bottom = 0;
            for (int i = 0; i < segmentXCount; i++)
            {
                String date = valuesOnX[i].ToShortDateString();

                switch (dateTimeStyle)
                {
                    case DateTimeStyle.Day:
                        date = date.Remove(date.IndexOf('.'));
                        break;
                    case DateTimeStyle.DayMonth:
                        date = date.Remove(date.LastIndexOf('.'));
                        break;
                    case DateTimeStyle.DayYear:
                        date = date.Remove(date.IndexOf('.'), 3);
                        break;
                    case DateTimeStyle.Month:
                        String temp = date.Remove(date.LastIndexOf('.'));
                        date = temp.Substring(date.IndexOf('.') + 1);
                        break;
                    case DateTimeStyle.MonthYear:
                        date = date.Substring(date.IndexOf('.') + 1);
                        break;
                    case DateTimeStyle.Year:
                        date = date.Substring(date.LastIndexOf('.') + 1);
                        break;
                    case DateTimeStyle.None:
                        date = "";
                        break;
                }
                TextBlock text3 = new TextBlock()
                {
                    Text = date,
                    FontSize = fontSizeOnX,
                    Foreground = color
                };

                if (i == 0)
                {
                    Double textWidth = MeasureString(text3).Width;
                    Double textHeight = MeasureString(text3).Height;
                    left = 20 - textWidth / 2;
                    bottom = 15 - textHeight;
                }

                Canvas.SetLeft(text3, left);
                Canvas.SetBottom(text3, bottom);
                canvas.Children.Add(text3);

                left += segmentXSize;
            }
        }
        // Сегменты по ординате.
        private void DrawYSegments()
        {
            if (segmentYCount > 0)
            {
                if (segmentYCount == 1)
                    segmentYSize = graphHeight - 10;
                else
                    segmentYSize = (graphHeight - 10) / (segmentYCount);
                // Координаты очередного сегмента будем высчитывать через эти переменные.
                Double SX = x1 - SEGMENTHEIGHT;
                Double SY = y1 + graphHeight - segmentYSize;

                for (int i = 0; i < segmentYCount; i++)
                {
                    if (i != 0) SY -= segmentYSize;
                    Line line = new Line()
                    {
                        X1 = SX,
                        Y1 = SY,
                        X2 = SX + SEGMENTHEIGHT * 2,
                        Y2 = SY,
                        Stroke = color,
                        StrokeThickness = STROKE
                    };
                    canvas.Children.Add(line);
                }
            }

            // Значения на отрезках ординаты
            Double left = 0;
            Double top = y1 + graphHeight - 10;
            // Подгон шрифта под размер оси
            if (segmentYCount > 23)
            {

            }
            for (int i = 0; i <= segmentYCount; i++)
            {
                TextBlock text2 = new TextBlock()
                {
                    Text = i.ToString(),
                    FontSize = fontSizeOnY,
                    RenderTransform = new RotateTransform(270),
                    Foreground = color
                };
                if (i == 0)
                {
                    Double textWidth = MeasureString(text2).Width;
                    Double textHeight = MeasureString(text2).Height;
                    top = (y1 + graphHeight) + textWidth / 2;
                    left = 15 - textHeight;
                }
                Canvas.SetLeft(text2, left);
                Canvas.SetTop(text2, top);
                canvas.Children.Add(text2);
                top -= segmentYSize;
            }
        }

        private void DrawLine(Double x1, Double y1, Double x2, Double y2, SolidColorBrush clr, Double strokeThickness)
        {
            Line line = new Line()
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = clr,
                StrokeThickness = strokeThickness
            };
            canvas.Children.Add(line);
        }

        // График
        public void DrawChart(Int32[] values, Boolean projectionOnX, Boolean projectionOnY, Double projectionOnXSize, Double projectionOnYSize)
        {
            if (values == null)
                throw new ArgumentNullException("Null argument/");
            if (values.Length > segmentXCount)
                throw new ArgumentException("Количество значений превышает заданное.");
            if (segmentXCount < 2)
                return;

            // Рисование непосредственно графика.
            Double X1; Double Y1;
            Double X2 = X1 = x1;
            Double Y2 = Y1 = (y1 + graphHeight) - (values[0] * segmentYSize);
            for (int i = 1; i < values.Length; i++)
            {
                X2 += segmentXSize;
                Y2 = (y1 + graphHeight) - (values[i] * segmentYSize);
                DrawLine(X1, Y1, X2, Y2, color, STROKE);
                X1 = X2; Y1 = Y2;
                // Проекция X
                if (projectionOnX)
                    DrawLine(X1, Y1, X1, y1 + graphHeight, Brushes.Red, projectionOnXSize);
                // Проекция Y
                if (projectionOnY)
                    DrawLine(X1, Y1, x1, Y1, Brushes.Blue, projectionOnYSize);
            }
        }

        // Измеряет размер строки TextBlock'а
        // https://stackoverflow.com/questions/9264398/how-to-calculate-wpf-textblock-width-for-its-known-font-size-and-characters
        private Size MeasureString(TextBlock textBlock)
        {
            var formattedText = new FormattedText(
                textBlock.Text,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),
                textBlock.FontSize,
                Brushes.Black);

            return new Size(formattedText.Width, formattedText.Height);
        }
    }
}