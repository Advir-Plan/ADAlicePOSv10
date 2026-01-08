namespace ADAlicePOSv10
{
    partial class DefenicoesAlice
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblLicenseExpiry;
        private System.Windows.Forms.Label lblDiasRestantes;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DefenicoesAlice));
            this.groupBoxConfig = new System.Windows.Forms.GroupBox();
            this.lblMaxPollingTime = new System.Windows.Forms.Label();
            this.lblPollingInterval = new System.Windows.Forms.Label();
            this.lblPassword = new System.Windows.Forms.Label();
            this.lblUser = new System.Windows.Forms.Label();
            this.lblBaseUrl = new System.Windows.Forms.Label();
            this.numMaxPollingTime = new System.Windows.Forms.NumericUpDown();
            this.numPollingInterval = new System.Windows.Forms.NumericUpDown();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.txtUser = new System.Windows.Forms.TextBox();
            this.txtBaseUrl = new System.Windows.Forms.TextBox();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblLicenseExpiry = new System.Windows.Forms.Label();
            this.lblDiasRestantes = new System.Windows.Forms.Label();
            this.groupBoxConfig.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxPollingTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPollingInterval)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxConfig
            // 
            this.groupBoxConfig.Controls.Add(this.lblMaxPollingTime);
            this.groupBoxConfig.Controls.Add(this.lblPollingInterval);
            this.groupBoxConfig.Controls.Add(this.lblPassword);
            this.groupBoxConfig.Controls.Add(this.lblUser);
            this.groupBoxConfig.Controls.Add(this.lblBaseUrl);
            this.groupBoxConfig.Controls.Add(this.numMaxPollingTime);
            this.groupBoxConfig.Controls.Add(this.numPollingInterval);
            this.groupBoxConfig.Controls.Add(this.txtPassword);
            this.groupBoxConfig.Controls.Add(this.txtUser);
            this.groupBoxConfig.Controls.Add(this.txtBaseUrl);
            this.groupBoxConfig.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.groupBoxConfig.Location = new System.Drawing.Point(17, 60);
            this.groupBoxConfig.Name = "groupBoxConfig";
            this.groupBoxConfig.Size = new System.Drawing.Size(556, 280);
            this.groupBoxConfig.TabIndex = 1;
            this.groupBoxConfig.TabStop = false;
            this.groupBoxConfig.Text = "Configuração do Terminal Alice";
            // 
            // lblMaxPollingTime
            // 
            this.lblMaxPollingTime.AutoSize = true;
            this.lblMaxPollingTime.Location = new System.Drawing.Point(290, 200);
            this.lblMaxPollingTime.Name = "lblMaxPollingTime";
            this.lblMaxPollingTime.Size = new System.Drawing.Size(176, 15);
            this.lblMaxPollingTime.TabIndex = 8;
            this.lblMaxPollingTime.Text = "Tempo Máximo de Polling (ms):";
            // 
            // lblPollingInterval
            // 
            this.lblPollingInterval.AutoSize = true;
            this.lblPollingInterval.Location = new System.Drawing.Point(20, 200);
            this.lblPollingInterval.Name = "lblPollingInterval";
            this.lblPollingInterval.Size = new System.Drawing.Size(139, 15);
            this.lblPollingInterval.TabIndex = 6;
            this.lblPollingInterval.Text = "Intervalo de Polling (ms):";
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(20, 145);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(60, 15);
            this.lblPassword.TabIndex = 4;
            this.lblPassword.Text = "Password:";
            // 
            // lblUser
            // 
            this.lblUser.AutoSize = true;
            this.lblUser.Location = new System.Drawing.Point(20, 90);
            this.lblUser.Name = "lblUser";
            this.lblUser.Size = new System.Drawing.Size(60, 15);
            this.lblUser.TabIndex = 2;
            this.lblUser.Text = "Utilizador:";
            // 
            // lblBaseUrl
            // 
            this.lblBaseUrl.AutoSize = true;
            this.lblBaseUrl.Location = new System.Drawing.Point(20, 35);
            this.lblBaseUrl.Name = "lblBaseUrl";
            this.lblBaseUrl.Size = new System.Drawing.Size(95, 15);
            this.lblBaseUrl.TabIndex = 0;
            this.lblBaseUrl.Text = "URL Base da API:";
            // 
            // numMaxPollingTime
            // 
            this.numMaxPollingTime.Font = new System.Drawing.Font("Consolas", 9F);
            this.numMaxPollingTime.Increment = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numMaxPollingTime.Location = new System.Drawing.Point(293, 218);
            this.numMaxPollingTime.Maximum = new decimal(new int[] {
            600000,
            0,
            0,
            0});
            this.numMaxPollingTime.Minimum = new decimal(new int[] {
            30000,
            0,
            0,
            0});
            this.numMaxPollingTime.Name = "numMaxPollingTime";
            this.numMaxPollingTime.Size = new System.Drawing.Size(150, 22);
            this.numMaxPollingTime.TabIndex = 9;
            this.numMaxPollingTime.Value = new decimal(new int[] {
            300000,
            0,
            0,
            0});
            // 
            // numPollingInterval
            // 
            this.numPollingInterval.Font = new System.Drawing.Font("Consolas", 9F);
            this.numPollingInterval.Location = new System.Drawing.Point(23, 218);
            this.numPollingInterval.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.numPollingInterval.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numPollingInterval.Name = "numPollingInterval";
            this.numPollingInterval.Size = new System.Drawing.Size(150, 22);
            this.numPollingInterval.TabIndex = 7;
            this.numPollingInterval.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            // 
            // txtPassword
            // 
            this.txtPassword.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtPassword.Location = new System.Drawing.Point(23, 163);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(250, 22);
            this.txtPassword.TabIndex = 5;
            this.txtPassword.UseSystemPasswordChar = true;
            // 
            // txtUser
            // 
            this.txtUser.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtUser.Location = new System.Drawing.Point(23, 108);
            this.txtUser.Name = "txtUser";
            this.txtUser.Size = new System.Drawing.Size(250, 22);
            this.txtUser.TabIndex = 3;
            // 
            // txtBaseUrl
            // 
            this.txtBaseUrl.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtBaseUrl.Location = new System.Drawing.Point(23, 53);
            this.txtBaseUrl.Name = "txtBaseUrl";
            this.txtBaseUrl.Size = new System.Drawing.Size(510, 22);
            this.txtBaseUrl.TabIndex = 1;
            // 
            // btnGuardar
            // 
            this.btnGuardar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.btnGuardar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnGuardar.ForeColor = System.Drawing.Color.White;
            this.btnGuardar.Location = new System.Drawing.Point(367, 360);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(100, 35);
            this.btnGuardar.TabIndex = 2;
            this.btnGuardar.Text = "Guardar";
            this.btnGuardar.UseVisualStyleBackColor = false;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this.btnCancelar.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnCancelar.ForeColor = System.Drawing.Color.White;
            this.btnCancelar.Location = new System.Drawing.Point(473, 360);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(100, 35);
            this.btnCancelar.TabIndex = 3;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = false;
            // 
            // lblTitulo
            //
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.Location = new System.Drawing.Point(12, 15);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(249, 30);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Configurações da Alice";
            //
            // lblLicenseExpiry
            //
            this.lblLicenseExpiry.AutoSize = true;
            this.lblLicenseExpiry.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblLicenseExpiry.ForeColor = System.Drawing.Color.Green;
            this.lblLicenseExpiry.Location = new System.Drawing.Point(300, 23);
            this.lblLicenseExpiry.Name = "lblLicenseExpiry";
            this.lblLicenseExpiry.Size = new System.Drawing.Size(150, 15);
            this.lblLicenseExpiry.TabIndex = 4;
            this.lblLicenseExpiry.Text = "Validade da Licença: --/--/----";
            //
            // lblDiasRestantes
            //
            this.lblDiasRestantes.AutoSize = true;
            this.lblDiasRestantes.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblDiasRestantes.ForeColor = System.Drawing.Color.Gray;
            this.lblDiasRestantes.Location = new System.Drawing.Point(14, 390);
            this.lblDiasRestantes.Name = "lblDiasRestantes";
            this.lblDiasRestantes.Size = new System.Drawing.Size(100, 13);
            this.lblDiasRestantes.TabIndex = 5;
            this.lblDiasRestantes.Text = "-- dias restantes";
            //
            // DefenicoesAlice
            //
            this.AcceptButton = this.btnGuardar;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.btnCancelar;
            this.ClientSize = new System.Drawing.Size(590, 415);
            this.Controls.Add(this.lblDiasRestantes);
            this.Controls.Add(this.lblLicenseExpiry);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.groupBoxConfig);
            this.Controls.Add(this.lblTitulo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DefenicoesAlice";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configurações Alice";
            this.groupBoxConfig.ResumeLayout(false);
            this.groupBoxConfig.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxPollingTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPollingInterval)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.GroupBox groupBoxConfig;
        private System.Windows.Forms.Label lblBaseUrl;
        private System.Windows.Forms.TextBox txtBaseUrl;
        private System.Windows.Forms.Label lblUser;
        private System.Windows.Forms.TextBox txtUser;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label lblPollingInterval;
        private System.Windows.Forms.NumericUpDown numPollingInterval;
        private System.Windows.Forms.Label lblMaxPollingTime;
        private System.Windows.Forms.NumericUpDown numMaxPollingTime;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Button btnCancelar;
    }
}