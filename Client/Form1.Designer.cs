namespace Client
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
            textBox1 = new TextBox();
            button1 = new Button();
            label1 = new Label();
            textBox2 = new TextBox();
            label2 = new Label();
            comboBox1 = new ComboBox();
            button2 = new Button();
            SuspendLayout();
            // 
            // textBox1
            // 
            textBox1.Location = new Point(12, 46);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(198, 27);
            textBox1.TabIndex = 0;
            // 
            // button1
            // 
            button1.Location = new Point(216, 44);
            button1.Name = "button1";
            button1.Size = new Size(119, 29);
            button1.TabIndex = 1;
            button1.Text = "Узнать курс";
            button1.UseVisualStyleBackColor = true;
            button1.Click += SendRequestButtonEvent;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(519, 23);
            label1.Name = "label1";
            label1.Size = new Size(115, 20);
            label1.TabIndex = 2;
            label1.Text = "Выберите пару";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(12, 146);
            textBox2.Multiline = true;
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(323, 292);
            textBox2.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 113);
            label2.Name = "label2";
            label2.Size = new Size(109, 20);
            label2.TabIndex = 4;
            label2.Text = "Ответ сервера";
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(519, 46);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(151, 28);
            comboBox1.TabIndex = 5;
            comboBox1.SelectedValueChanged += SelectPare;
            // 
            // button2
            // 
            button2.Location = new Point(519, 80);
            button2.Name = "button2";
            button2.Size = new Size(151, 64);
            button2.TabIndex = 6;
            button2.Text = "Получить пары с сервера";
            button2.UseVisualStyleBackColor = true;
            button2.Click += GetParesFromServer;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(button2);
            Controls.Add(comboBox1);
            Controls.Add(label2);
            Controls.Add(textBox2);
            Controls.Add(label1);
            Controls.Add(button1);
            Controls.Add(textBox1);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBox1;
        private Button button1;
        private Label label1;
        private TextBox textBox2;
        private Label label2;
        private ComboBox comboBox1;
        private Button button2;
    }
}
