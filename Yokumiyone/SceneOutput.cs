using System.Windows.Controls;

namespace Yokumiyone
{
    internal class SceneOutput
    {
        private TextBox framerateTextBox = new TextBox();
        public string Framerate
        {
            get { return framerateTextBox.Text; }
            set { framerateTextBox.Text = value; }
        }

        public void SetControls(TextBox framerate)
        {
            framerateTextBox = framerate;
            framerateTextBox.Focus();
        }
    }
}
