// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using SharpDX;

namespace Project
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        public ProjectGame game;
        public MainMenu mainMenu;
        public EndPage endPage;
        public bool displayerDebug;
         
        public MainPage()
        {
            InitializeComponent();
            //Loaded += OnLoaded;
            
            
            game = new ProjectGame(this);
            game.Run(this);
            
            mainMenu = new MainMenu(this);
            //endPage = new EndPage(this);

            this.Children.Add(mainMenu);
            InGameButton(Visibility.Collapsed);
            txtCameraPos.Visibility = Visibility.Collapsed;
            txtCameraTarget.Visibility = Visibility.Collapsed;
            txtPlayerPos.Visibility = Visibility.Collapsed;
        }

        // TASK 1: Update the game's score
        public void UpdateScore(int score)
        {
            txtScore.Text = "Score: " + score.ToString();
        }

        private void OnLoaded(object s, RoutedEventArgs e)
        {
            game = new ProjectGame(this);
            game.Run(this);
        }

        public void Reload()
        {
            //InitializeComponent();
            Loaded += OnLoaded;
            //mainMenu = new MainMenu(this);
            //endPage = new EndPage(this);

            this.Children.Add(mainMenu);
            InGameButton(Visibility.Collapsed);
        }

        public void UpdateShootButton(bool canShoot)
        {
            if (canShoot)
                buttonShoot.Background = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(100,0,255,0));
            else
                buttonShoot.Background = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(100, 255, 0, 0));
        }

        public void DisplayCameraPlayerPos(Vector3 cameraPos, Vector3 cameraTarget, Vector3 playerPos, float noindex)
        {
            
            if (displayerDebug)
            {
                txtCameraPos.Visibility = Visibility.Visible;
                txtCameraTarget.Visibility = Visibility.Visible;
                txtPlayerPos.Visibility = Visibility.Visible;
                txtCameraPos.Text = "Camera Position: " + cameraPos.ToString();
                txtCameraTarget.Text = "Camera Target: " + cameraTarget.ToString();
                txtPlayerPos.Text = "Player Position: " + playerPos.ToString();
            }
            else
            {
                txtCameraPos.Visibility = Visibility.Collapsed;
                txtCameraTarget.Visibility = Visibility.Collapsed;
                txtPlayerPos.Visibility = Visibility.Collapsed;
            }
            
        }


        private void Jump(object sender, RoutedEventArgs e)
        {
            game.player.Jump();
        }

        private void Shoot(object sender, RoutedEventArgs e)
        {
            game.player.Shoot();
        }



        // TASK 2: Starts the game.  Not that it seems easier to simply move the game.Run(this) command to this function,
        // however this seems to result in a reduction in texture quality on some machines.  Not sure why this is the case
        // but this is an easy workaround.  Not we are also making the command button invisible after it is clicked
        public void StartGame()
        {
            
            this.Children.Remove(mainMenu);
            InGameButton(Visibility.Visible);
            game.started = true;
        }

        public void InGameButton(Visibility status)
        {
            buttonJump.Visibility = status;
            buttonShoot.Visibility = status;
        }

        public void EndGame(int score)
        {
            endPage = new EndPage(this, score);
            //game.UnloadContent();
            //game.Dispose();
            //this.Children.Remove(this);
            this.Children.Add(endPage);
        }
    }
}
