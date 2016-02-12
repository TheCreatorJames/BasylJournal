using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using BasylEncryptionStandard;
using System.Text;
using System.Security.Cryptography;
using System.IO.Compression;

namespace BasylJournal
{
    /// <summary>
    /// This is the Basyl Journal Algorithm. 
    /// It allows you to keep entries private and encrypted.
    /// 
    /// To build this project, you'll need to reference the BESLibrary, which is available at
    /// http://www.github.com/TheCreatorJames/BESLibrary 
    /// </summary>
    public partial class BasylJournalForm : Form
    {
        private const char SEPARATOR = '\uFFFE'; //constant separator character.

        private List<BasylNote> noteList; //stores a list of notes, not technically necessary because of the list box. I might refractor this out later on.
        private string pass; //The current password.
        private string name; //hashed password name for the file.

        private bool changed; //used to prevent certain events.

        //Used for some Randomization Vectors.
        private RandomNumberGenerator random = RNGCryptoServiceProvider.Create(); 

        /// <summary>
        /// Creates the Main Form of the Application.
        /// </summary>
        public BasylJournalForm()
        {
            InitializeComponent();
            noteList = new List<BasylNote>();

            //Runs to open initial journal.
            openJournalToolStripMenuItem_Click(null, null);
        }
        

        /// <summary>
        /// Creates a New Note and adds it to the list of notes.
        /// it places it at the top of the list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newNoteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BasylNote note = new BasylNote();
            noteListbox.Items.Insert(0, note);
            noteList.Insert(0, note);
            noteListbox.SelectedIndex = 0;
        }

        private void openJournalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changed = true;
            noteTextbox.Text = "";
         
            //Acquire a password from the user.
            var input = new BasylEncryptionStandard.BasylInputBox("Journal password?", "Basyl Input", "", true);
            input.ShowDialog();
            var ans = input.GetAnswer();

            //If there is no password put in, close the program.
            if(ans.Length == 0)
            {
                Environment.Exit(0);
            }

            //next step is to create a hash of sorts from the password.
            byte[] arr = BasylEncryptionStandard.BasylHashAlgorithms.BasylHashUno("BasylHash" + ans, ans, 32, 65535, 250, 1000, "Basyl_3" + ans);
            var simplify = Regex.Replace(Convert.ToBase64String(arr), "[^A-Za-z0-9]", "");
            pass = ans;
            name = simplify;
            

            //clears the list of notes.
            noteList.Clear();
            noteListbox.Items.Clear();


            //open file if it exists, or create it :).
            if (File.Exists(GetPath(simplify)))
            {
                //opens the file
                Stream stream = File.OpenRead(GetPath(simplify));

                //Acquire Hash and Randomizers.
                byte[] hash = new byte[32], k1 = new byte[4], k2 = new byte[4];
                int l = (int)stream.Length;
                stream.Read(hash, 0, 32);

                stream.Read(k2, 0, 4);
                stream.Read(k1, 0, 4);
            
                byte[] rest = new byte[l - 40];
                stream.Read(rest, 0, (l-40));
                stream.Close();

                //Create the Key Generator and Cipher
                BasylKeyGenerator bkg = new BasylKeyGenerator(pass, BasylKeyGenerator.INITIAL/2, BasylKeyGenerator.ROUNDS*2, BasylKeyGenerator.LEFTOFF, BasylKeyGenerator.EXPANSION, BasylKeyGenerator.ADDITIONALKEY, hash, k1, k2, true);
                BESCipher cipher = new BESCipher(bkg);

                //Decrypts the Bytes.
                cipher.EncryptRight(ref rest);

                //Decompresses and Decrypts the Journal
                string journal = GetString(Decompress(rest));

                //Splits using a separator character.
                string[] notes =  journal.Split(SEPARATOR);

                //Loads Journal into Application.
                foreach (string note in notes)
                {
                   
                    if(note.Trim().Length == 0)
                    {
                        continue;
                    }
                    BasylNote cNote = new BasylNote(note);
                    noteList.Add(cNote);
                    noteListbox.Items.Add(cNote);

                }
                
                //Destroy the Key Generator.
                noteListbox.SelectedIndex = 0;
                bkg.Drop();
                GC.Collect();
            }
         
        }

        /// <summary>
        /// Gets the Path of the Current Journal File.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private String GetPath(string name)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return path  + Path.DirectorySeparatorChar + name + ".ebn";
        }


        /// <summary>
        /// Deletes the current Journal Entry.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            noteList.Remove(((BasylNote)noteListbox.SelectedItem));
            noteListbox.Items.Remove(noteListbox.SelectedItem);

            changed = true;
            noteTextbox.Text = "";
        }

        private void noteTextbox_TextChanged(object sender, EventArgs e)
        {
            //In some situations, when the program switches the entry, or something like that
            //you don't want it to modify anything. You only want to modify upon user input.
            //this flag fixes it.
            if (changed) 
            {
                changed = false;
                return;
            }

            noteTextbox.Text = noteTextbox.Text.Replace("" + SEPARATOR, "");

            //If there is a current entry selected, modify it.
            if (noteListbox.SelectedItem != null)
            {

                var item = noteListbox.SelectedItem;
                //places the most recently modified entry on the top of the list.
                if (noteListbox.SelectedIndex != 0)
                {
                    ((BasylNote)item).SetNote(noteTextbox.Text);

                    noteListbox.Items.Remove(item);
                    noteListbox.Items.Insert(0, item);

                    noteList.Remove((BasylNote)item);
                    noteList.Insert(0, (BasylNote)item);

                    noteListbox.SelectedIndex = 0;
                }
                else
                {
                    //adjusts it.
                    ((BasylNote)item).SetNote(noteTextbox.Text);
                    //updates the listbox.
                    noteListbox.Items[noteListbox.SelectedIndex] = noteListbox.SelectedItem;
                }
                

            }
           
        }

        /// <summary>
        /// Gets the Bytes from the String.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        static byte[] GetBytes(string str)
        {

            byte[] bytes = Encoding.UTF8.GetBytes(str);
            return bytes;
        }

        /// <summary>
        /// Switches Entries.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notelistBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (noteListbox.SelectedItem != null)
            {
                changed = true;
                noteTextbox.Text = ((BasylNote)noteListbox.SelectedItem).GetNote();
                
            }
        }

        /// <summary>
        /// Code to execute when the BasylJournalForm loads.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BasylJournalForm_Load(object sender, EventArgs e)
        {
            
        }

        /// <summary>
        /// Creates a New Note
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newNoteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            newNoteToolStripMenuItem_Click(sender, e);
        }


        /// <summary>
        /// Short Cut Keys
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BasylJournalForm_KeyDown(object sender, KeyEventArgs e)
        {
           
            if (e.Control)
            {
                //Save
                if (e.KeyCode == Keys.S)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    saveJournalToolStripMenuItem_Click(sender, e);
                
                }
                else
                //Open
                if (e.KeyCode == Keys.O)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    openJournalToolStripMenuItem_Click(null, null);
                    noteListbox.Focus();

                }
                else
                //New
                if (e.KeyCode == Keys.N)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    newNoteToolStripMenuItem1_Click(null, null);
                    noteListbox.Focus();
                }
                else 
                //Delete - bug: doesn't seem to work properly
                if (e.KeyCode == Keys.Delete)
                {
                    deleteToolStripMenuItem_Click(null, null);
                }


            }

            //Select All
            if (e.Control && (e.KeyCode == Keys.A))
            {
                if (sender != null && sender is TextBox)
                    ((TextBox)sender).SelectAll();
                e.Handled = true;
            }
        }
        
        /// <summary>
        /// Gets the String from the Bytes
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        static string GetString(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }


        /// <summary>
        /// Decompresses Bytes passed in.
        /// </summary>
        /// <param name="gzip"></param>
        /// <returns></returns>
        static byte[] Decompress(byte[] gzip)
        {
            // Create a GZIP stream with decompression mode.
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
            {
                //read into a buffer
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            //write into a dynamically growing memory stream.
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }

        /// <summary>
        /// Compresses the Bytes passed in.
        /// </summary>
        /// <param name="raw"></param>
        /// <returns></returns>
        private static byte[] Compress(byte[] raw)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (GZipStream cstream = new GZipStream(stream, CompressionMode.Compress, true))
                {
                    cstream.Write(raw, 0, raw.Length);
                }
                return stream.ToArray();
            }
        }


        /// <summary>
        /// Saves the journal.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveJournalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String journal = "";

            //Formats it with a seperation character.
            foreach (BasylNote x in noteList)
            {
                journal += x.GetNote() + SEPARATOR;
            }


            //Computes Randomization Vectors
            byte[] hash = SHA256Managed.Create().ComputeHash(Encoding.UTF8.GetBytes(journal));
            byte[] key1 = new byte[4], key2 = new byte[4];


            random.GetBytes(key1);
            random.GetBytes(key2);

            //makes key cipher stuff to encrypt with.
            BasylKeyGenerator bkg = new BasylKeyGenerator(pass, BasylKeyGenerator.INITIAL/2, BasylKeyGenerator.ROUNDS*2, BasylKeyGenerator.LEFTOFF, BasylKeyGenerator.EXPANSION, BasylKeyGenerator.ADDITIONALKEY, hash, (byte[])key1.Clone(), key2, true);
            BESCipher cipher = new BESCipher(bkg);




            //compresses the data into a byte array.
            byte[] ec = Compress(GetBytes(journal));

            //encrypts in the left direction,
            cipher.EncryptLeft(ref ec);
            
            //if the file exists, delete it.
            if (File.Exists(GetPath(name))) File.Delete(GetPath(name));

            //write the file 
            Stream stream = File.OpenWrite(GetPath(name));
            stream.Write(hash, 0, 32);
            stream.Write(key2, 0, 4);
            stream.Write(key1, 0, 4);
            stream.Write(ec, 0, ec.Length);

            //clean up all the stuff.
            bkg.Drop();
            GC.Collect();
            stream.Close();
        }

        /// <summary>
        /// Delete the Current Note
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            deleteToolStripMenuItem_Click(null, null);
        }
    }
}
