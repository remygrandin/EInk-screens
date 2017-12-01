namespace GrayScaleBenchmark
{
    partial class Form1
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.PCBX_Output = new System.Windows.Forms.PictureBox();
            this.LSTB_Files = new System.Windows.Forms.ListBox();
            this.CMBX_GrayScale = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.CMBX_Method = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.CMBX_Dithering = new System.Windows.Forms.ComboBox();
            this.CHKB_Serpentine = new System.Windows.Forms.CheckBox();
            this.BTN_GenAll = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.PCBX_Output)).BeginInit();
            this.SuspendLayout();
            // 
            // PCBX_Output
            // 
            this.PCBX_Output.Location = new System.Drawing.Point(219, 26);
            this.PCBX_Output.Name = "PCBX_Output";
            this.PCBX_Output.Size = new System.Drawing.Size(1006, 731);
            this.PCBX_Output.TabIndex = 3;
            this.PCBX_Output.TabStop = false;
            // 
            // LSTB_Files
            // 
            this.LSTB_Files.FormattingEnabled = true;
            this.LSTB_Files.Location = new System.Drawing.Point(12, 50);
            this.LSTB_Files.Name = "LSTB_Files";
            this.LSTB_Files.Size = new System.Drawing.Size(201, 95);
            this.LSTB_Files.TabIndex = 4;
            this.LSTB_Files.SelectedIndexChanged += new System.EventHandler(this.LSTB_Files_SelectedIndexChanged);
            // 
            // CMBX_GrayScale
            // 
            this.CMBX_GrayScale.FormattingEnabled = true;
            this.CMBX_GrayScale.Items.AddRange(new object[] {
            "2",
            "4",
            "8",
            "16",
            "32",
            "64",
            "128",
            "256"});
            this.CMBX_GrayScale.Location = new System.Drawing.Point(76, 151);
            this.CMBX_GrayScale.Name = "CMBX_GrayScale";
            this.CMBX_GrayScale.Size = new System.Drawing.Size(137, 21);
            this.CMBX_GrayScale.TabIndex = 5;
            this.CMBX_GrayScale.Text = "4";
            this.CMBX_GrayScale.SelectedIndexChanged += new System.EventHandler(this.CMBX_GrayScale_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "File :";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 154);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Scale :";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 181);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(49, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Method :";
            // 
            // CMBX_Method
            // 
            this.CMBX_Method.FormattingEnabled = true;
            this.CMBX_Method.Location = new System.Drawing.Point(76, 178);
            this.CMBX_Method.Name = "CMBX_Method";
            this.CMBX_Method.Size = new System.Drawing.Size(137, 21);
            this.CMBX_Method.TabIndex = 8;
            this.CMBX_Method.SelectedIndexChanged += new System.EventHandler(this.CMBX_Method_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 208);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Dithering :";
            // 
            // CMBX_Dithering
            // 
            this.CMBX_Dithering.FormattingEnabled = true;
            this.CMBX_Dithering.Location = new System.Drawing.Point(76, 205);
            this.CMBX_Dithering.Name = "CMBX_Dithering";
            this.CMBX_Dithering.Size = new System.Drawing.Size(137, 21);
            this.CMBX_Dithering.TabIndex = 10;
            this.CMBX_Dithering.SelectedIndexChanged += new System.EventHandler(this.CMBX_Dithering_SelectedIndexChanged);
            // 
            // CHKB_Serpentine
            // 
            this.CHKB_Serpentine.AutoSize = true;
            this.CHKB_Serpentine.Location = new System.Drawing.Point(76, 232);
            this.CHKB_Serpentine.Name = "CHKB_Serpentine";
            this.CHKB_Serpentine.Size = new System.Drawing.Size(77, 17);
            this.CHKB_Serpentine.TabIndex = 12;
            this.CHKB_Serpentine.Text = "Serpentine";
            this.CHKB_Serpentine.UseVisualStyleBackColor = true;
            this.CHKB_Serpentine.CheckedChanged += new System.EventHandler(this.CHKB_Serpentine_CheckedChanged);
            // 
            // BTN_GenAll
            // 
            this.BTN_GenAll.Location = new System.Drawing.Point(12, 307);
            this.BTN_GenAll.Name = "BTN_GenAll";
            this.BTN_GenAll.Size = new System.Drawing.Size(201, 23);
            this.BTN_GenAll.TabIndex = 13;
            this.BTN_GenAll.Text = "Generate all possibilities";
            this.BTN_GenAll.UseVisualStyleBackColor = true;
            this.BTN_GenAll.Click += new System.EventHandler(this.BTN_GenAll_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 336);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(201, 23);
            this.button1.TabIndex = 14;
            this.button1.Text = "Bench";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1237, 769);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.BTN_GenAll);
            this.Controls.Add(this.CHKB_Serpentine);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.CMBX_Dithering);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.CMBX_Method);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.CMBX_GrayScale);
            this.Controls.Add(this.LSTB_Files);
            this.Controls.Add(this.PCBX_Output);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.PCBX_Output)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.PictureBox PCBX_Output;
        private System.Windows.Forms.ListBox LSTB_Files;
        private System.Windows.Forms.ComboBox CMBX_GrayScale;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox CMBX_Method;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox CMBX_Dithering;
        private System.Windows.Forms.CheckBox CHKB_Serpentine;
        private System.Windows.Forms.Button BTN_GenAll;
        private System.Windows.Forms.Button button1;
    }
}

