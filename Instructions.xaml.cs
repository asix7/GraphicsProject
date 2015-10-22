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
    // TASK 4: Instructions Page
    public sealed partial class Instructions
    {
        private MainPage parent;
        public Instructions(MainPage parent)
        {
            InitializeComponent();
            this.parent = parent;

            txtInstructions.Text = ("Jump around, dodge enemy projectile, don't die\n\n" + 
                "Blue Tile gives extra speed, Red Tile gives extra vertical, Yellow tile gives permission to shoot\n\n" +
                "You don't need to aim, sophisticated path finding algorithm is implemented on the projectile, to boost your gameplay and performance\n" +
                "All you need to do is P2W (press to win), press Jump to jump, press Shoot to shoot\n\n" +
                "If Shoot button is Red, you can't shoot, make sure you are in Yellow Tile, and it will turn green, and you shoot\n\n"+
                "Enjoy the game scrub, L2P if you die, stop hacking if you don't die...");

            bluehelpblock.Text = "Blue Tile: Extra speed.";
            redhelpblock.Text = "Red Tile: Reduce speed.";
            yellowblock.Text = "Yellow Tile: Shooting available.";
        }

       
        private void GoBack(object sender, RoutedEventArgs e)
        {
            parent.Children.Add(parent.mainMenu);
            parent.Children.Remove(this);
        }
    }
}
