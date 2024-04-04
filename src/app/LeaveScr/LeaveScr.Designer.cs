namespace LeaveScr
{
    partial class LeaveScr
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            mtbPassword = new MaskedTextBox();
            lblUsername = new Label();
            llblResetPassword = new LinkLabel();
            llblSigninOptions = new LinkLabel();
            tlpControls = new TableLayoutPanel();
            pPassword = new Panel();
            pbSubmit = new PictureBox();
            pbUser = new PictureBox();
            lblError = new Label();
            tlpControls.SuspendLayout();
            pPassword.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pbSubmit).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pbUser).BeginInit();
            SuspendLayout();
            // 
            // mtbPassword
            // 
            mtbPassword.Anchor = AnchorStyles.None;
            mtbPassword.BorderStyle = BorderStyle.FixedSingle;
            mtbPassword.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            mtbPassword.Location = new Point(59, 14);
            mtbPassword.Margin = new Padding(4, 3, 4, 3);
            mtbPassword.Name = "mtbPassword";
            mtbPassword.Size = new Size(233, 26);
            mtbPassword.TabIndex = 1;
            mtbPassword.UseSystemPasswordChar = true;
            mtbPassword.KeyUp += mtbPassword_KeyUp;
            // 
            // lblUsername
            // 
            lblUsername.BackColor = Color.Transparent;
            lblUsername.Dock = DockStyle.Fill;
            lblUsername.Font = new Font("Segoe UI Semilight", 39.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblUsername.ForeColor = Color.White;
            lblUsername.Location = new Point(4, 206);
            lblUsername.Margin = new Padding(4, 0, 4, 0);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(810, 122);
            lblUsername.TabIndex = 1;
            lblUsername.Text = "User";
            lblUsername.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // llblResetPassword
            // 
            llblResetPassword.ActiveLinkColor = Color.DarkGray;
            llblResetPassword.Dock = DockStyle.Bottom;
            llblResetPassword.Font = new Font("Segoe UI Light", 12F);
            llblResetPassword.LinkBehavior = LinkBehavior.NeverUnderline;
            llblResetPassword.LinkColor = Color.White;
            llblResetPassword.Location = new Point(4, 432);
            llblResetPassword.Margin = new Padding(4, 17, 4, 0);
            llblResetPassword.Name = "llblResetPassword";
            llblResetPassword.Size = new Size(810, 27);
            llblResetPassword.TabIndex = 2;
            llblResetPassword.TabStop = true;
            llblResetPassword.Text = "Reset password";
            llblResetPassword.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // llblSigninOptions
            // 
            llblSigninOptions.ActiveLinkColor = Color.DarkGray;
            llblSigninOptions.Dock = DockStyle.Bottom;
            llblSigninOptions.Font = new Font("Segoe UI Light", 12F);
            llblSigninOptions.LinkBehavior = LinkBehavior.NeverUnderline;
            llblSigninOptions.LinkColor = Color.White;
            llblSigninOptions.Location = new Point(4, 476);
            llblSigninOptions.Margin = new Padding(4, 17, 4, 0);
            llblSigninOptions.Name = "llblSigninOptions";
            llblSigninOptions.Size = new Size(810, 58);
            llblSigninOptions.TabIndex = 3;
            llblSigninOptions.TabStop = true;
            llblSigninOptions.Text = "Sign-in options";
            llblSigninOptions.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // tlpControls
            // 
            tlpControls.Anchor = AnchorStyles.None;
            tlpControls.AutoSize = true;
            tlpControls.BackColor = Color.Transparent;
            tlpControls.ColumnCount = 1;
            tlpControls.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpControls.Controls.Add(pPassword, 0, 2);
            tlpControls.Controls.Add(llblSigninOptions, 0, 5);
            tlpControls.Controls.Add(llblResetPassword, 0, 4);
            tlpControls.Controls.Add(lblUsername, 0, 1);
            tlpControls.Controls.Add(pbUser, 0, 0);
            tlpControls.Controls.Add(lblError, 0, 3);
            tlpControls.Location = new Point(49, 297);
            tlpControls.Margin = new Padding(4, 3, 4, 3);
            tlpControls.Name = "tlpControls";
            tlpControls.RowCount = 7;
            tlpControls.RowStyles.Add(new RowStyle());
            tlpControls.RowStyles.Add(new RowStyle());
            tlpControls.RowStyles.Add(new RowStyle());
            tlpControls.RowStyles.Add(new RowStyle(SizeType.Absolute, 23F));
            tlpControls.RowStyles.Add(new RowStyle());
            tlpControls.RowStyles.Add(new RowStyle());
            tlpControls.RowStyles.Add(new RowStyle(SizeType.Absolute, 23F));
            tlpControls.Size = new Size(818, 590);
            tlpControls.TabIndex = 2;
            // 
            // pPassword
            // 
            pPassword.Anchor = AnchorStyles.None;
            pPassword.Controls.Add(pbSubmit);
            pPassword.Controls.Add(mtbPassword);
            pPassword.Location = new Point(225, 331);
            pPassword.Margin = new Padding(4, 3, 4, 3);
            pPassword.Name = "pPassword";
            pPassword.Size = new Size(368, 58);
            pPassword.TabIndex = 3;
            // 
            // pbSubmit
            // 
            pbSubmit.Image = Properties.Resources.SubmitIcon;
            pbSubmit.Location = new Point(293, 14);
            pbSubmit.Margin = new Padding(4, 3, 4, 3);
            pbSubmit.Name = "pbSubmit";
            pbSubmit.Size = new Size(30, 30);
            pbSubmit.SizeMode = PictureBoxSizeMode.StretchImage;
            pbSubmit.TabIndex = 2;
            pbSubmit.TabStop = false;
            pbSubmit.Click += pbSubmit_Click;
            // 
            // pbUser
            // 
            pbUser.Anchor = AnchorStyles.None;
            pbUser.BackgroundImageLayout = ImageLayout.None;
            pbUser.Image = Properties.Resources.UserIcon;
            pbUser.Location = new Point(309, 3);
            pbUser.Margin = new Padding(4, 3, 4, 3);
            pbUser.Name = "pbUser";
            pbUser.Size = new Size(200, 200);
            pbUser.SizeMode = PictureBoxSizeMode.AutoSize;
            pbUser.TabIndex = 4;
            pbUser.TabStop = false;
            pbUser.DoubleClick += pbUser_Click;
            // 
            // lblError
            // 
            lblError.Dock = DockStyle.Bottom;
            lblError.Font = new Font("Segoe UI Light", 12F);
            lblError.ForeColor = Color.White;
            lblError.Location = new Point(4, 392);
            lblError.Margin = new Padding(4, 0, 4, 0);
            lblError.Name = "lblError";
            lblError.Size = new Size(810, 23);
            lblError.TabIndex = 5;
            lblError.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // LeaveScr
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(933, 936);
            ControlBox = false;
            Controls.Add(tlpControls);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "LeaveScr";
            ShowIcon = false;
            ShowInTaskbar = false;
            Text = "Windows Logon Application";
            WindowState = FormWindowState.Maximized;
            FormClosing += Screen_FormClosing;
            Load += Screen_Load;
            tlpControls.ResumeLayout(false);
            tlpControls.PerformLayout();
            pPassword.ResumeLayout(false);
            pPassword.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pbSubmit).EndInit();
            ((System.ComponentModel.ISupportInitialize)pbUser).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.MaskedTextBox mtbPassword;
        private System.Windows.Forms.PictureBox pbUser;
        private System.Windows.Forms.LinkLabel llblResetPassword;
        private System.Windows.Forms.LinkLabel llblSigninOptions;
        private System.Windows.Forms.Panel pPassword;
        private System.Windows.Forms.PictureBox pbSubmit;
        private System.Windows.Forms.TableLayoutPanel tlpControls;
        private System.Windows.Forms.Label lblError;
        private System.Windows.Forms.Label lblUsername;
    }
}

