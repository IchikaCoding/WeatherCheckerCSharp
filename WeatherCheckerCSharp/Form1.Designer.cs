namespace WeatherCheckerCSharp
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtCity = new TextBox();
            btnSearch = new Button();
            lblStatus = new Label();
            txtRaw = new TextBox();
            SuspendLayout();
            // 
            // txtCity
            // 
            txtCity.Location = new Point(52, 146);
            txtCity.Name = "txtCity";
            txtCity.Size = new Size(100, 23);
            txtCity.TabIndex = 0;
            txtCity.TextChanged += textBox1_TextChanged;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(52, 233);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(134, 68);
            btnSearch.TabIndex = 1;
            btnSearch.Text = "調べる";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(288, 52);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(38, 15);
            lblStatus.TabIndex = 2;
            lblStatus.Text = "label1";
            // 
            // txtRaw
            // 
            txtRaw.Location = new Point(288, 278);
            txtRaw.Multiline = true;
            txtRaw.Name = "txtRaw";
            txtRaw.Size = new Size(100, 23);
            txtRaw.TabIndex = 3;
            txtRaw.TextChanged += txtRaw_TextChanged;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(txtRaw);
            Controls.Add(lblStatus);
            Controls.Add(btnSearch);
            Controls.Add(txtCity);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtCity;
        private Button btnSearch;
        private Label lblStatus;
        private TextBox txtRaw;
    }
}
