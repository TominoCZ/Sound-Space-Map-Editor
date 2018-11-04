namespace Blox_Saber_Editor
{
    partial class IndexSelector
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // IndexSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.DoubleBuffered = true;
            this.Name = "IndexSelector";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.IndexSelector_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.IndexSelector_MouseDown);
            this.MouseLeave += new System.EventHandler(this.IndexSelector_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.IndexSelector_MouseMove);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
