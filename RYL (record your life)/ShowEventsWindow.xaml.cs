using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RYL__record_your_life_
{
    public partial class ShowEventsWindow : Window
    {
        public event EventsChangedHandler EventDeleted;
        public event EventsChangedHandler EventReplaced;

        private RYL lifeRecorder;
        private bool isSearchNeed; // Необходимо для поля ввода с поиском.

        public ShowEventsWindow(RYL lifeRecorder)
        {
            InitializeComponent();
            this.lifeRecorder = lifeRecorder ?? throw new ArgumentNullException("Null argument.");
        }

        #region Events
        private void ShowEventsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Closing += ShowEventsWindow_Closing;
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
                    {
                        CEventsNames.Items.Clear();
                        CUp.IsEnabled = false;
                        CDown.IsEnabled = false;
                    }
                }
            }
        }
        private void CEventsNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CEventsNames.SelectedIndex == -1)
            {
                TextBlockRecords.Content = "Записи";
                CRecordsNames.Items.Clear();
            }
            else
            {
                int ID = GetSelectedEventID();
                Record[] records = lifeRecorder.GetEventByID(ID).GetRecords();
                if (records != null)
                    FillRecords(records);
            }
        }

        private void CRecordsNames_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (CRecordsNames.SelectedIndex == -1)
                CRecordsNames.Items.Clear();
            else
            {
                Int32 ID = GetSelectedEventID();
                Record record = lifeRecorder.GetEventByID(GetSelectedEventID()).GetRecords()[GetSelectedRecordID()];
                if (record.Description.Length > 0)
                    MessageBox.Show("Событие: " + lifeRecorder.GetEventNameByID(ID) + "\nДата: " + record.Date.ToString() + "\nОписание: " + record.Description, "Record");
                else
                    MessageBox.Show("Событие: " + lifeRecorder.GetEventNameByID(ID) + "\nДата: " + record.Date.ToString(), "Record");
            }
        }
        private void CEventsNames_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (CEventsNames.SelectedIndex == -1)
                return;

            int ID = GetSelectedEventID();
            if (lifeRecorder[ID].Description.Length > 0)
                MessageBox.Show("Событие: " + lifeRecorder[ID].Name + "\nОписание: " + lifeRecorder[ID].Description + "\nПовторяемость: " + lifeRecorder[ID].Rate, "Event");
            else
                MessageBox.Show("Событие: " + lifeRecorder[ID].Name + "\nПовторяемость: " + lifeRecorder[ID].Rate, "Event");
        }

        private void CDelete_Click(object sender, RoutedEventArgs e)
        {
            if (CEventsNames.SelectedIndex == -1)
                return;

            int ID = GetSelectedEventID();

            MessageBoxResult answer = MessageBox.Show($"Вы уверены, что хотите удалить \"{lifeRecorder[ID].Name}\"?", "Удаление события", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (answer == MessageBoxResult.No)
                return;

            lifeRecorder.DeleteEvent(ID);
            EventDeleted?.Invoke();
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
        private void CUp_Click(object sender, RoutedEventArgs e)
        {
            if (CEventsNames.SelectedIndex == -1)
                return;

            Int32 selected = CEventsNames.SelectedIndex;
            Int32 ID = GetSelectedEventID();
            if (lifeRecorder.TryMoveEventUp(ID))
            {
                FillEvents();
                CEventsNames.Focus();
                CEventsNames.SelectedIndex = selected-1;
                EventReplaced?.Invoke();
            }
        }
        private void CDown_Click(object sender, RoutedEventArgs e)
        {
            if (CEventsNames.SelectedIndex == -1)
                return;

            Int32 selected = CEventsNames.SelectedIndex;
            Int32 ID = GetSelectedEventID();
            if (lifeRecorder.TryMoveEventDown(ID))
            {
                FillEvents();
                CEventsNames.Focus();
                CEventsNames.SelectedIndex=selected+1;
                EventReplaced?.Invoke();
            }
        }

        private void ShowEventsWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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
        private Int32 GetSelectedRecordID()
        {
            int ID = lifeRecorder.GetEventByID(GetSelectedEventID()).GetRecords()[CRecordsNames.SelectedIndex].ID;
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

            CUp.IsEnabled = true;
            CDown.IsEnabled = true;

            String[] names = lifeRecorder.GetEventsNames();
            foreach (String name in names)
            {
                CEventsNames.Items.Add(name);
            }
        }
        private void FillEvents(LifeEvent[] lifeEvents)
        {
            CEventsNames.Items.Clear();

            CUp.IsEnabled = false;
            CDown.IsEnabled = false;

            foreach (LifeEvent lifeEvent in lifeEvents)
            {
                CEventsNames.Items.Add(lifeEvent.Name);
            }
        }
        private void FillRecords(Record[] records)
        {
            CRecordsNames.Items.Clear();

            TextBlockRecords.Content = $"Записи ({records.Length})";

            foreach (Record record in records)
            {
                if (record.Description.Length > 0)
                    CRecordsNames.Items.Add(record.Date.ToShortDateString() + " - " + record.Description);
                else
                    CRecordsNames.Items.Add(record.Date.ToShortDateString());
            }
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

        private void ShowRecords(Record[] records)
        {
            CRecordsNames.Items.Clear();
        }
        #endregion
    }
}