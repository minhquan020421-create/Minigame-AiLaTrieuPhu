namespace AILATRIEUPHU__MINH_QUAN_
{
    partial class frmMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            lblTitle = new Label();
            btnStart = new Button();
            btnGuide = new Button();
            btnAchievement = new Button();
            btnSound = new Button();
            btnExit = new Button();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.BackColor = Color.Transparent;
            lblTitle.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblTitle.Location = new Point(203, 25);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(748, 46);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "💰 BẠN CÓ MUỐN GIÀU SAU MỘT ĐÊM ? 💰";
            lblTitle.Click += lblTitle_Click;
            // 
            // btnStart
            // 
            btnStart.Anchor = AnchorStyles.None;
            btnStart.BackColor = SystemColors.Highlight;
            btnStart.Font = new Font("Arial", 15F, FontStyle.Bold, GraphicsUnit.Point, 163);
            btnStart.ForeColor = Color.Yellow;
            btnStart.Location = new Point(493, 414);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(223, 56);
            btnStart.TabIndex = 2;
            btnStart.Text = "BẮT ĐẦU ▶";
            btnStart.UseVisualStyleBackColor = false;
            btnStart.Click += btnStart_Click;
            // 
            // btnGuide
            // 
            btnGuide.BackColor = SystemColors.Highlight;
            btnGuide.Font = new Font("Arial", 15F, FontStyle.Bold, GraphicsUnit.Point, 163);
            btnGuide.ForeColor = Color.Yellow;
            btnGuide.Location = new Point(493, 571);
            btnGuide.Name = "btnGuide";
            btnGuide.Size = new Size(223, 56);
            btnGuide.TabIndex = 3;
            btnGuide.Text = "HƯỚNG DẪN 📖";
            btnGuide.UseVisualStyleBackColor = false;
            btnGuide.Click += btnGuide_Click;
            // 
            // btnAchievement
            // 
            btnAchievement.BackColor = SystemColors.Highlight;
            btnAchievement.Font = new Font("Arial", 15F, FontStyle.Bold, GraphicsUnit.Point, 163);
            btnAchievement.ForeColor = Color.Yellow;
            btnAchievement.Location = new Point(493, 490);
            btnAchievement.Name = "btnAchievement";
            btnAchievement.Size = new Size(223, 56);
            btnAchievement.TabIndex = 4;
            btnAchievement.Text = "THÀNH TÍCH 🏆";
            btnAchievement.UseVisualStyleBackColor = false;
            // 
            // btnSound
            // 
            btnSound.BackColor = SystemColors.Highlight;
            btnSound.Font = new Font("Arial", 15F, FontStyle.Bold, GraphicsUnit.Point, 163);
            btnSound.ForeColor = Color.Yellow;
            btnSound.Location = new Point(35, 571);
            btnSound.Name = "btnSound";
            btnSound.Size = new Size(63, 56);
            btnSound.TabIndex = 5;
            btnSound.Text = "🔊";
            btnSound.UseVisualStyleBackColor = false;
            btnSound.Click += btnSound_Click;
            // 
            // btnExit
            // 
            btnExit.BackColor = SystemColors.Highlight;
            btnExit.Font = new Font("Arial", 15F, FontStyle.Bold, GraphicsUnit.Point, 163);
            btnExit.ForeColor = Color.Yellow;
            btnExit.Location = new Point(1084, 571);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(63, 56);
            btnExit.TabIndex = 6;
            btnExit.Text = "➡️";
            btnExit.UseVisualStyleBackColor = false;
            // 
            // frmMain
            // 
            AutoScaleDimensions = new SizeF(8F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoValidate = AutoValidate.EnablePreventFocusChange;
            BackColor = Color.FromArgb(10, 20, 65);
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Zoom;
            ClientSize = new Size(1182, 653);
            Controls.Add(btnExit);
            Controls.Add(btnSound);
            Controls.Add(btnAchievement);
            Controls.Add(btnGuide);
            Controls.Add(btnStart);
            Controls.Add(lblTitle);
            Font = new Font("Microsoft Sans Serif", 8.25F);
            ForeColor = Color.Gold;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "AI LÀ TRIỆU PHÚ";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblTitle;
        private Button btnStart;
        private Button btnGuide;
        private Button btnAchievement;
        private Button btnSound;
        private Button btnExit;
    }
}
 
