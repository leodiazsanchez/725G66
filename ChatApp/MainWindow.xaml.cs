using ChatApp.ViewModel;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;


namespace ChatApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel viewModel;
        public MainWindow()
        {
            InitializeComponent();
            viewModel = new MainViewModel();
            DataContext = viewModel;
            viewModel.ShakeRequested += (s, e) => Shake();
        }


        public void Shake()
        {
            double currentLeft = this.Left;

            var shakeAnimation = new DoubleAnimation
            {
                From = currentLeft,
                To = currentLeft + 10,
                Duration = TimeSpan.FromMilliseconds(50),
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(5)
            };

            shakeAnimation.Completed += (s, e) =>
            {
                this.Left = currentLeft;
                this.BeginAnimation(Window.LeftProperty, null);
            };

            this.BeginAnimation(Window.LeftProperty, shakeAnimation);
        }

        public async Task ShakeWindowAsync()
        {

            var shakeAnimation = new DoubleAnimation
            {
                From = this.Left - 10,
                To = this.Left + 10,
                Duration = TimeSpan.FromMilliseconds(50),
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(10)
            };

            this.BeginAnimation(Window.LeftProperty, shakeAnimation);

            await Task.Delay(shakeAnimation.Duration.TimeSpan.Milliseconds * 20);

        }

    }
}
