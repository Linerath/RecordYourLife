using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Effects;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace RYL__record_your_life_
{
    public delegate void EventsChangedHandler();

    public delegate void NewEventHandler();
    public delegate void EventDeletedHandler();
    public delegate void AddEventDelegate(String name, String description, Repeatability rate);

    public partial class MainWindow : Window
    {
        private bool arw1t, sw1t, sew1t; // Не первый ли раз будет открыта форма (для addRecordWindow, staticsticsWindow и showEventsWindows).
        private RYL lifeRecorder;
        private AddRecordWindow addRecordWindow;
        private StatisticsWindow statisticsWindow;
        private AddEventWindow addEventWindow;
        private ShowEventsWindow showEventsWindow;

        private BlurEffect blur = new BlurEffect() { Radius = 3 };

        public MainWindow()
        {
            InitializeComponent();

            LoadEvents();

            addEventWindow = new AddEventWindow(lifeRecorder.AddEvent);
            statisticsWindow = new StatisticsWindow(lifeRecorder);
            addRecordWindow = new AddRecordWindow(lifeRecorder);
            showEventsWindow = new ShowEventsWindow(lifeRecorder);

            // Эти события необходимы для динамического отображения измененний в событиях в ListBox'ах.
            // Если сделать событие в классе "RYL", то придется помечать все формы атрибутом сериализации. И базовый класс Window тоже.
            addRecordWindow.Closing += ChildWindow_Closing;

            statisticsWindow.Closing += ChildWindow_Closing;

            addEventWindow.Closing += ChildWindow_Closing;
            addEventWindow.EventAdded += statisticsWindow.OnEventsChanged;
            addEventWindow.EventAdded += addRecordWindow.OnEventsChanged;
            addEventWindow.EventAdded += showEventsWindow.OnEventsChanged;

            showEventsWindow.Closing += ChildWindow_Closing;
            showEventsWindow.EventDeleted += addRecordWindow.OnEventsChanged;
            showEventsWindow.EventDeleted += statisticsWindow.OnEventsChanged;
            showEventsWindow.EventReplaced += addRecordWindow.OnEventsChanged;
            showEventsWindow.EventReplaced += statisticsWindow.OnEventsChanged;

            CAddRecord.Effect = blur;
            CShowStatistics.Effect = blur;
            CAddEvent.Effect = blur;
            CShowEvents.Effect = blur;
        }

        #region Events
        private void CAddRecord_Click(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;
            if (!arw1t)
            {
                addRecordWindow.FillNames();
                arw1t = true;
            }
            addRecordWindow.Show();
        }

        private void CShowStatistics_Click(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;
            if (!sw1t)
            {
                statisticsWindow.FillEvents();
                sw1t = true;
            }
            statisticsWindow.Show();
        }

        private void CAddEvent_Click(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;
            addEventWindow.Show();
        }

        private void CShowEvents_Click(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;
            if (!sew1t)
            {
                showEventsWindow.FillEvents();
                sew1t = true;
            }
            showEventsWindow.Show();
        }

        private void CBlurAnimation_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                DropShadowEffect shadow = new DropShadowEffect()
                {
                    ShadowDepth = 2,
                    BlurRadius = 10
                };
                button.Effect = shadow;
            }

        }
        private void CBlurAnimation_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                button.Effect = blur;
            }
        }

        private void ChildWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IsEnabled = true;
        }

        private void RYL_Loaded(object sender, RoutedEventArgs e)
        {
            addEventWindow.Owner = this;
            addRecordWindow.Owner = this;
            showEventsWindow.Owner = this;
            statisticsWindow.Owner = this;
        }
        private void RYL_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveEvents();
        }
        #endregion

        #region Methods
        public void SaveEvents()
        {
            BinaryFormatter bf = new BinaryFormatter();
            try
            {
                using (BinaryWriter bw = new BinaryWriter(new FileStream("error.ryl", FileMode.Create)))
                    bw.Write(false);
                using (FileStream fs = new FileStream("events.ryl", FileMode.Create))
                    bf.Serialize(fs, lifeRecorder);
            }
            catch (SerializationException e)
            {
                MessageBox.Show("Ошибка сериализации. Причина: " + e.Message, "Ошибка");
            }
        }
        public void LoadEvents()
        {
            BinaryFormatter bf = new BinaryFormatter();
            try
            {
                using (BinaryReader bw = new BinaryReader(new FileStream("error.ryl", FileMode.Open)))
                {
                    if (bw.ReadBoolean() == true)
                        lifeRecorder = new RYL();
                    else
                    {
                        using (FileStream fs = new FileStream("events.ryl", FileMode.Open))
                            lifeRecorder = (RYL)bf.Deserialize(fs);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Системный файл не найден. Все события обнулены.", "Ошибка");
                using (BinaryWriter bw = new BinaryWriter(new FileStream("error.ryl", FileMode.Create)))
                    bw.Write(true);
                Application.Current.Shutdown();
            }
            catch (SerializationException e)
            {
                MessageBox.Show("Ошибка десериализации. Причина: " + e.Message, "Ошибка");
                MessageBoxResult answer = MessageBox.Show("Создать новый файл?", "Ошибка загрузки", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (answer == MessageBoxResult.Yes)
                {
                    using (BinaryWriter bw = new BinaryWriter(new FileStream("error.ryl", FileMode.Create)))
                        bw.Write(true);
                    Application.Current.Shutdown();
                }
            }
        }
        #endregion
    }
}