namespace DefLab
{
    partial class Form1
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
            this.rendererCtrl1 = new DefLab.renderer.RendererCtrl();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // rendererCtrl1
            // 
            this.rendererCtrl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rendererCtrl1.Location = new System.Drawing.Point(0, 0);
            this.rendererCtrl1.Name = "rendererCtrl1";
            this.rendererCtrl1.Size = new System.Drawing.Size(765, 441);
            this.rendererCtrl1.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(581, 57);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(765, 441);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.rendererCtrl1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private renderer.RendererCtrl rendererCtrl1;
        private System.Windows.Forms.Button button1;
    }
}

