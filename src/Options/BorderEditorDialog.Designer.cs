namespace SolutionColors
{
    partial class BorderEditorDialog
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
            this.btnTop = new System.Windows.Forms.Button();
            this.btnBottom = new System.Windows.Forms.Button();
            this.btnLeft = new System.Windows.Forms.Button();
            this.btnRight = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.numTop = new System.Windows.Forms.NumericUpDown();
            this.numRight = new System.Windows.Forms.NumericUpDown();
            this.numLeft = new System.Windows.Forms.NumericUpDown();
            this.numBottom = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numTop)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBottom)).BeginInit();
            this.SuspendLayout();
            // 
            // btnTop
            // 
            this.btnTop.Location = new System.Drawing.Point(90, 15);
            this.btnTop.Name = "btnTop";
            this.btnTop.Size = new System.Drawing.Size(120, 40);
            this.btnTop.TabIndex = 0;
            this.btnTop.Text = "Top";
            this.btnTop.UseVisualStyleBackColor = true;
            this.btnTop.Click += new System.EventHandler(this.btnTop_Click);
            // 
            // btnBottom
            // 
            this.btnBottom.Location = new System.Drawing.Point(90, 195);
            this.btnBottom.Name = "btnBottom";
            this.btnBottom.Size = new System.Drawing.Size(120, 40);
            this.btnBottom.TabIndex = 1;
            this.btnBottom.Text = "Bottom";
            this.btnBottom.UseVisualStyleBackColor = true;
            this.btnBottom.Click += new System.EventHandler(this.btnBottom_Click);
            // 
            // btnLeft
            // 
            this.btnLeft.Location = new System.Drawing.Point(15, 55);
            this.btnLeft.Name = "btnLeft";
            this.btnLeft.Size = new System.Drawing.Size(50, 140);
            this.btnLeft.TabIndex = 2;
            this.btnLeft.Text = "Left";
            this.btnLeft.UseVisualStyleBackColor = true;
            this.btnLeft.Click += new System.EventHandler(this.btnLeft_Click);
            // 
            // btnRight
            // 
            this.btnRight.Location = new System.Drawing.Point(235, 55);
            this.btnRight.Name = "btnRight";
            this.btnRight.Size = new System.Drawing.Size(50, 140);
            this.btnRight.TabIndex = 3;
            this.btnRight.Text = "Right";
            this.btnRight.UseVisualStyleBackColor = true;
            this.btnRight.Click += new System.EventHandler(this.btnRight_Click);
            // 
            // btnOk
            // 
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(210, 255);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 28);
            this.btnOk.TabIndex = 4;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(115, 255);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 28);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // numTop
            // 
            this.numTop.Location = new System.Drawing.Point(130, 60);
            this.numTop.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.numTop.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numTop.Name = "numTop";
            this.numTop.Size = new System.Drawing.Size(45, 20);
            this.numTop.TabIndex = 6;
            this.numTop.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numTop.ValueChanged += new System.EventHandler(this.numTop_ValueChanged);
            // 
            // numRight
            // 
            this.numRight.Location = new System.Drawing.Point(175, 115);
            this.numRight.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.numRight.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numRight.Name = "numRight";
            this.numRight.Size = new System.Drawing.Size(45, 20);
            this.numRight.TabIndex = 7;
            this.numRight.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numRight.ValueChanged += new System.EventHandler(this.numRight_ValueChanged);
            // 
            // numLeft
            // 
            this.numLeft.Location = new System.Drawing.Point(80, 115);
            this.numLeft.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.numLeft.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numLeft.Name = "numLeft";
            this.numLeft.Size = new System.Drawing.Size(45, 20);
            this.numLeft.TabIndex = 8;
            this.numLeft.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numLeft.ValueChanged += new System.EventHandler(this.numLeft_ValueChanged);
            // 
            // numBottom
            // 
            this.numBottom.Location = new System.Drawing.Point(130, 170);
            this.numBottom.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.numBottom.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numBottom.Name = "numBottom";
            this.numBottom.Size = new System.Drawing.Size(45, 20);
            this.numBottom.TabIndex = 9;
            this.numBottom.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numBottom.ValueChanged += new System.EventHandler(this.numBottom_ValueChanged);
            // 
            // BorderEditorDialog
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(300, 300);
            this.Controls.Add(this.numBottom);
            this.Controls.Add(this.numLeft);
            this.Controls.Add(this.numRight);
            this.Controls.Add(this.numTop);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnRight);
            this.Controls.Add(this.btnLeft);
            this.Controls.Add(this.btnBottom);
            this.Controls.Add(this.btnTop);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BorderEditorDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Border settings";
            this.Load += new System.EventHandler(this.BorderEditorDialog_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numTop)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBottom)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnTop;
        private System.Windows.Forms.Button btnBottom;
        private System.Windows.Forms.Button btnLeft;
        private System.Windows.Forms.Button btnRight;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.NumericUpDown numTop;
        private System.Windows.Forms.NumericUpDown numRight;
        private System.Windows.Forms.NumericUpDown numLeft;
        private System.Windows.Forms.NumericUpDown numBottom;
    }
}
