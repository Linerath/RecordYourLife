using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace RYL__record_your_life_
{
    public partial class AddRecordWindow : Window
    {
        private Boolean isSearchNeed;
        private RYL lifeRecorder;

        public AddRecordWindow(RYL lifeRecorder)
        {
            InitializeComponent();

            this.lifeRecorder = lifeRecorder ?? throw new ArgumentNullException("Null argument.");
        }

        #region Events
        private void AddRecordWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Closing += AddRecordWindow_Closing;
        }

        private void CRecordDescription_GotFocus(object sender, RoutedEventArgs e)
        {
            string text = "-1";
            try
            {
                text = new TextRange(CRecordDescription.Document.ContentStart, CRecordDescription.Document.ContentEnd).Text;
            }
            catch (StackOverflowException)
            {
                MessageBox.Show("Stack Overflow!");
            }

            if (text == "Описание...\r\n" && CRecordDescription.FontStyle == FontStyles.Italic)
            {
                CRecordDescription.FontStyle = FontStyles.Normal;
                CRecordDescription.Foreground = Brushes.Black;
                CRecordDescription.Document.Blocks.Clear();
            }
        }
        private void CRecordDescription_LostFocus(object sender, RoutedEventArgs e)
        {
            string text = "-1";
            try
            {
                text = new TextRange(CRecordDescription.Document.ContentStart, CRecordDescription.Document.ContentEnd).Text;
            }
            catch (StackOverflowException)
            {
                MessageBox.Show("Stack Overflow!");
            }

            text = text.Trim('\n', '\t', '\r', '\f', '\v', '\\', ' ');

            if (text == "")
            {
                CRecordDescription.Document.Blocks.Clear();
                CRecordDescription.Document.Blocks.Add(new Paragraph(new Run("Описание...")));
                CRecordDescription.Foreground = System.Windows.Media.Brushes.Gray;
                CRecordDescription.FontStyle = FontStyles.Italic;
            }
        }
        private void CSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            if (CSearch.Text == "Поиск..." && CSearch.FontStyle==FontStyles.Italic)
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
                    FillNames();
                else
                {
                    LifeEvent[] searchResult = lifeRecorder.SearchInEvents(CSearch.Text);
                    if (searchResult != null)
                        FillNames(searchResult);
                    else
                        CEventsNames.Items.Clear();
                }
            }
        }

        private void CAdd_Click(object sender, RoutedEventArgs e)
        {
            if (TryAddRecord())
            {
                ClearFields();
                ImageAnimationDisappearance();
            }
        }

        private void CBack_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AddRecordWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
        #endregion

        #region Methods
        private Boolean TryAddRecord()
        {
            int index = CEventsNames.SelectedIndex;
            if (CEventsNames.Items.Count > 0 && CEventsNames.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите событие.", "Предупреждение");
                return false;
            }
            else if (CEventsNames.SelectedIndex == -1)
            {
                MessageBox.Show("События отсутствуют.", "Предупреждение");
                return false;
            }

            string text;
            try
            {
                text = new TextRange(CRecordDescription.Document.ContentStart, CRecordDescription.Document.ContentEnd).Text;
            }
            catch (Exception exp)
            {
                MessageBox.Show("Ошибка. Причина: " + exp.Message, "Ошибка");
                return false;
            }

            string tempText = text.Trim('\n', '\t', '\r', '\f', '\v', '\\', ' ');
            if (tempText == "Описание...")
                tempText = "";

            // Добавление записи.
            try
            {
                lifeRecorder.AddRecord(CDate.SelectedDate??DateTime.Now, tempText, GetSelectedItemID());
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
            return true;
        }
        private Int32 GetSelectedItemID()
        {
            // Определение ID.
            int ID = lifeRecorder.GetEventIDByName(CEventsNames.SelectedItem.ToString());
            if (ID < 0)
            {
                MessageBox.Show("Ошибка индексации. Обратитесь к разработчику о найденном баге. Приносим извинения.", "Ошибка");
                Application.Current.Shutdown();
            }
            return ID;
        }

        public void OnEventsChanged()
        {
            if (CSearch.Foreground == Brushes.Gray)
            {
                FillNames();
            }
            else
            {
                LifeEvent[] searchResult = lifeRecorder.SearchInEvents(CSearch.Text);
                if (searchResult != null)
                    FillNames(searchResult);
                else
                    CEventsNames.Items.Clear();
            }
        }

        public void FillNames()
        {
            CEventsNames.Items.Clear();

            String[] names = lifeRecorder.GetEventsNames();
            foreach (String name in names)
            {
                CEventsNames.Items.Add(name);
            }
        }
        private void FillNames(LifeEvent[] lifeEvents)
        {
            CEventsNames.Items.Clear();

            foreach (LifeEvent lifeEvent in lifeEvents)
            {
                CEventsNames.Items.Add(lifeEvent.Name);
            }
        }
        
        private void ClearFields()
        {
            CRecordDescription.Document.Blocks.Clear();
            CRecordDescription.Document.Blocks.Add(new Paragraph(new Run("Описание...")));
            CRecordDescription.Foreground = Brushes.Gray;
            CRecordDescription.FontStyle = FontStyles.Italic;
        }

        private void ImageAnimationDisappearance()
        {
            COKImage.Visibility = Visibility.Visible;
            CAdd.IsEnabled = false;

            DoubleAnimation animation = new DoubleAnimation()
            {
                From = COKImage.Opacity,
                To = 0,
                Duration = TimeSpan.FromSeconds(1.0),
            };
            animation.Completed += AnimationDisappearance_Completed;
            COKImage.BeginAnimation(OpacityProperty, animation);
        }
        private void AnimationDisappearance_Completed(object sender, EventArgs e)
        {
            COKImage.BeginAnimation(OpacityProperty, null);
            COKImage.Opacity = 1;
            COKImage.Visibility = Visibility.Hidden;
            CAdd.IsEnabled = true;
        }
        #endregion
    }
}