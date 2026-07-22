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
            linkLabel1 = new LinkLabel();
            linkLabel2 = new LinkLabel();
            cmbFavorites = new ComboBox();
            btnFav = new Button();
            RemoveFavBtn = new Button();
            label1 = new Label();
            label2 = new Label();
            richTextBox1 = new RichTextBox();
            textBox1 = new TextBox();
            SuspendLayout();
            // 
            // txtCity
            // 
            txtCity.Location = new Point(75, 126);
            txtCity.Name = "txtCity";
            txtCity.Size = new Size(229, 23);
            txtCity.TabIndex = 0;
            txtCity.TextChanged += textBox1_TextChanged;
            // 
            // btnSearch
            // 
            btnSearch.BackColor = Color.LightSkyBlue;
            btnSearch.Location = new Point(341, 103);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(70, 46);
            btnSearch.TabIndex = 1;
            btnSearch.Text = "調べる";
            btnSearch.UseVisualStyleBackColor = false;
            btnSearch.Click += btnSearch_Click;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.BackColor = Color.RosyBrown;
            lblStatus.Location = new Point(80, 272);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(55, 15);
            lblStatus.TabIndex = 2;
            lblStatus.Text = "　　　　";
            // 
            // linkLabel1
            // 
            linkLabel1.AutoSize = true;
            linkLabel1.Location = new Point(341, 395);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new Size(60, 15);
            linkLabel1.TabIndex = 7;
            linkLabel1.TabStop = true;
            linkLabel1.Text = "linkLabel1";
            // 
            // linkLabel2
            // 
            linkLabel2.AutoSize = true;
            linkLabel2.Location = new Point(75, 395);
            linkLabel2.Name = "linkLabel2";
            linkLabel2.Size = new Size(60, 15);
            linkLabel2.TabIndex = 8;
            linkLabel2.TabStop = true;
            linkLabel2.Text = "linkLabel2";
            // 
            // cmbFavorites
            // 
            cmbFavorites.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFavorites.FormattingEnabled = true;
            cmbFavorites.Location = new Point(75, 207);
            cmbFavorites.Name = "cmbFavorites";
            cmbFavorites.Size = new Size(149, 23);
            cmbFavorites.TabIndex = 10;
            cmbFavorites.SelectedIndexChanged += cmbFavorites_SelectedIndexChanged;
            // 
            // btnFav
            // 
            btnFav.BackColor = Color.LightSkyBlue;
            btnFav.Location = new Point(432, 103);
            btnFav.Name = "btnFav";
            btnFav.Size = new Size(70, 46);
            btnFav.TabIndex = 11;
            btnFav.Text = "保存";
            btnFav.UseVisualStyleBackColor = false;
            btnFav.Click += btnFav_Click;
            // 
            // RemoveFavBtn
            // 
            RemoveFavBtn.BackColor = Color.Tomato;
            RemoveFavBtn.Image = Properties.Resources.delete_24dp_E3E3E3_FILL0_wght400_GRAD0_opsz24;
            RemoveFavBtn.Location = new Point(341, 184);
            RemoveFavBtn.Name = "RemoveFavBtn";
            RemoveFavBtn.Size = new Size(70, 46);
            RemoveFavBtn.TabIndex = 12;
            RemoveFavBtn.UseVisualStyleBackColor = false;
            RemoveFavBtn.Click += RemoveFavBtn_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(75, 98);
            label1.Name = "label1";
            label1.Size = new Size(43, 15);
            label1.TabIndex = 13;
            label1.Text = "都市名";
            label1.Click += label1_Click_1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(75, 184);
            label2.Name = "label2";
            label2.Size = new Size(82, 15);
            label2.TabIndex = 14;
            label2.Text = "お気に入り都市";
            // 
            // richTextBox1
            // 
            richTextBox1.BackColor = Color.MistyRose;
            richTextBox1.Location = new Point(292, 272);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(248, 96);
            richTextBox1.TabIndex = 15;
            richTextBox1.Text = "";
            // 
            // textBox1
            // 
            textBox1.BackColor = Color.Salmon;
            textBox1.Location = new Point(80, 324);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(100, 23);
            textBox1.TabIndex = 16;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(587, 454);
            Controls.Add(textBox1);
            Controls.Add(richTextBox1);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(RemoveFavBtn);
            Controls.Add(btnFav);
            Controls.Add(cmbFavorites);
            Controls.Add(linkLabel2);
            Controls.Add(linkLabel1);
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
        private LinkLabel linkLabel1;
        private LinkLabel linkLabel2;
        private ComboBox cmbFavorites;
        private Button btnFav;
        private Button RemoveFavBtn;
        private Label label1;
        private Label label2;
        private RichTextBox richTextBox1;
        private TextBox textBox1;
        //private Label linkOpenMeteo;
    }
}
