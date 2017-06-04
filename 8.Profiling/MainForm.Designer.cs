namespace Profiling
{
    partial class MainForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.UnmanagedLeakButton = new System.Windows.Forms.Button();
            this.ManagedLeakButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // UnmanagedLeakButton
            // 
            this.UnmanagedLeakButton.Location = new System.Drawing.Point(12, 25);
            this.UnmanagedLeakButton.Name = "UnmanagedLeakButton";
            this.UnmanagedLeakButton.Size = new System.Drawing.Size(150, 30);
            this.UnmanagedLeakButton.TabIndex = 0;
            this.UnmanagedLeakButton.Text = "Start Unmanaged leak";
            this.UnmanagedLeakButton.UseVisualStyleBackColor = true;
            this.UnmanagedLeakButton.Click += new System.EventHandler(this.UnmanagedLeakButton_Click);
            // 
            // ManagedLeakButton
            // 
            this.ManagedLeakButton.Location = new System.Drawing.Point(172, 25);
            this.ManagedLeakButton.Name = "ManagedLeakButton";
            this.ManagedLeakButton.Size = new System.Drawing.Size(150, 30);
            this.ManagedLeakButton.TabIndex = 1;
            this.ManagedLeakButton.Text = "Start Managed leak";
            this.ManagedLeakButton.UseVisualStyleBackColor = true;
            this.ManagedLeakButton.Click += new System.EventHandler(this.ManagedLeakButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 84);
            this.Controls.Add(this.ManagedLeakButton);
            this.Controls.Add(this.UnmanagedLeakButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "Memory leaks generator";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button UnmanagedLeakButton;
        private System.Windows.Forms.Button ManagedLeakButton;
    }
}

