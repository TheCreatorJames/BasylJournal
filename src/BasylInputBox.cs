using System;
using System.Windows.Forms;

namespace BasylEncryptionStandard
{
    /// <summary>
    /// Used for getting input easily.
    /// </summary>
    public partial class BasylInputBox : Form
    {
        /// <summary>
        /// Creates an Input Box to Type Stuff into.
        /// </summary>
        /// <param name="question"></param>
        /// <param name="title"></param>
        /// <param name="placeholder"></param>
        /// <param name="passwordChar"></param>
        public BasylInputBox(string question = "", string title="Basyl Input", string placeholder = "", bool passwordChar = false)
        {
            InitializeComponent();
            this.label1.Text = question;
            this.Text = title;
            this.textBox1.Text = placeholder;
            this.KeyDown += BasylInputBox_KeyDown;
            if (passwordChar)
                this.textBox1.UseSystemPasswordChar = true;
            textBox1.KeyDown += BasylInputBox_KeyDown;
        }

        /// <summary>
        /// Ignores certain keystrokes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BasylInputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control || e.Alt)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Gets the answer from the user.
        /// </summary>
        /// <returns></returns>
        public string GetAnswer()
        {
        
            return textBox1.Text;
        }

        private void BasylInputBox_Load(object sender, EventArgs e)
        {
        }


        /// <summary>
        /// If the user hits enter, close the input box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                Close();
            }
        }
    }
}
