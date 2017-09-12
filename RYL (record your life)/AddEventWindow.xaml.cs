using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace RYL__record_your_life_
{
    public partial class AddEventWindow : Window
    {
        public event EventsChangedHandler EventAdded;
        private AddEventDelegate addEvent;

        public AddEventWindow(AddEventDelegate addEvent)
        {
            InitializeComponent();

            CEventName.Focus();
            if (addEvent == null) return;
            this.addEvent = addEvent;
        }

        #region Events
        private void AddEventWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Closing += AddEventWindow_Closing;
        }

        private void CEventDescription_GotFocus(object sender, RoutedEventArgs e)
        {
            string text = "-1";
            try
            {
                text = new TextRange(CEventDescription.Document.ContentStart, CEventDescription.Document.ContentEnd).Text;
            }
            catch (StackOverflowException)
            {
                MessageBox.Show("Stack Overflow!", "Exception");
            }

            if (text == "Описание...\r\n" && CEventDescription.FontStyle == FontStyles.Italic)
            {
                CEventDescription.FontStyle = FontStyles.Normal;
                CEventDescription.Foreground = Brushes.Black;
                CEventDescription.Document.Blocks.Clear();
            }
        }
        private void CEventDescription_LostFocus(object sender, RoutedEventArgs e)
        {
            string text = "-1";
            try
            {
                text = new TextRange(CEventDescription.Document.ContentStart, CEventDescription.Document.ContentEnd).Text;
            }
            catch (StackOverflowException)
            {
                MessageBox.Show("Stack Overflow!", "Exception");
            }

            text = text.Trim('\n', '\t', '\r', '\f', '\v', '\\', ' ');

            if (text == "")
            {
                CEventDescription.Document.Blocks.Clear();
                CEventDescription.Document.Blocks.Add(new Paragraph(new Run("Описание...")));
                CEventDescription.Foreground = Brushes.Gray;
                CEventDescription.FontStyle = FontStyles.Italic;
            }
        }

        private void CAdd_Click(object sender, RoutedEventArgs e)
        {
            if (TryAddEvent())
            {
                ClearFields();
                ImageAnimationDisappearance();
            }
        }

        private void CBack_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
            Close();
        }

        private void AddEventWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
        #endregion

        #region Methods
        private bool TryAddEvent()
        {
            string name = CEventName.Text.Trim('\t', '\r', '\f', ' ');
            if (name == "")
            {
                MessageBox.Show("Заполните название.", "Предупреждение");
                CEventName.Clear();
                CEventName.Focus();
                return false;
            }

            string text;
            try
            {
                text = new TextRange(CEventDescription.Document.ContentStart, CEventDescription.Document.ContentEnd).Text;
            }
            catch (Exception exp)
            {
                MessageBox.Show("Ошибка. Причина: " + exp.Message, "Ошибка");
                return false;
            }

            String tempText = text.Trim('\n', '\t', '\r', '\f', '\v', '\\', ' ');
            if (tempText == "Описание...")
                tempText = "";

            Repeatability rate;
            if (COnce.IsChecked ?? false)
                rate = Repeatability.Once;
            else if (CSometimes.IsChecked ?? false)
                rate = Repeatability.Sometimes;
            else
                rate = Repeatability.Constantly;

            try
            {
                addEvent(name, tempText, rate);
            }
            catch (NotUniqueNameException exp)
            {
                MessageBox.Show(exp.Message, "Ошибка");
                CEventName.Clear();
                CEventName.Focus();
                return false;
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Exception");
                ClearFields();
                return false;
            }

            EventAdded?.Invoke();
            return true;
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
            COKImage.BeginAnimation(Image.OpacityProperty, animation);
        }

        private void AnimationDisappearance_Completed(object sender, EventArgs e)
        {
            COKImage.BeginAnimation(Image.OpacityProperty, null);
            COKImage.Opacity = 1;
            COKImage.Visibility = Visibility.Hidden;
            CAdd.IsEnabled = true;
        }

        private void ClearFields()
        {
            CEventName.Text = "";
            CEventName.Focus();

            CEventDescription.Document.Blocks.Clear();
            CEventDescription.Document.Blocks.Add(new Paragraph(new Run("Описание...")));
            CEventDescription.Foreground = Brushes.Gray;
            CEventDescription.FontStyle = FontStyles.Italic;
        }
        #endregion
    }
}