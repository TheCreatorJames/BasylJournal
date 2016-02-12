namespace BasylJournal
{
    /// <summary>
    /// Used to store the text and format a title in the listbox.
    /// </summary>
    class BasylNote
    {
        //variable to store note text.
        private string noteText;

        /// <summary>
        /// Creates Empty Note
        /// </summary>
        public BasylNote() : this("")
        {

        }

        /// <summary>
        /// Creates Note
        /// </summary>
        /// <param name="msg"></param>
        public BasylNote(string msg)
        {
            this.noteText = msg;
        }

        /// <summary>
        /// Sets the Note.
        /// </summary>
        /// <param name="noteText"></param>
        public void SetNote(string noteText)
        {
            this.noteText = noteText;
        }

        /// <summary>
        /// Gets the Note
        /// </summary>
        /// <returns></returns>
        public string GetNote()
        {
            return noteText;
        }

        /// <summary>
        /// Gets the top line to use as title.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string[] lines = noteText.Split('\n');
            string result = "";

            //search for a line that isn't blank.
            foreach(string line in lines)
            {
                if (line.Trim().Length != 0)
                {
                    result = line.Trim();
                    break;
                } 
            }

            //if there is no non-blank line, then say "new note" is the title.
            if (result.Trim().Length == 0) result = "New Note";

            return result;
        }
    }
}
