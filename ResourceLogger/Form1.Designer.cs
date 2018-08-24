

namespace ResourceLogger
{
    partial class Form1
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
            logger.killThreads();
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        { 
			this.components = new System.ComponentModel.Container();
			System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea3 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
			System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
			System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea4 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
			this.listBox1 = new System.Windows.Forms.ListBox();
			this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.historySummaryLabel = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.historyDatapointsDownLabel = new System.Windows.Forms.Button();
			this.historyDatapointsUpLabel = new System.Windows.Forms.Button();
			this.historyDatapointsLabel = new System.Windows.Forms.Label();
			this.reloadHistoryButton = new System.Windows.Forms.Button();
			this.chart2 = new System.Windows.Forms.DataVisualization.Charting.Chart();
			this.hScrollBar1 = new System.Windows.Forms.HScrollBar();
			((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.chart2)).BeginInit();
			this.SuspendLayout();
			// 
			// listBox1
			// 
			this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listBox1.FormattingEnabled = true;
			this.listBox1.Location = new System.Drawing.Point(12, 680);
			this.listBox1.Name = "listBox1";
			this.listBox1.Size = new System.Drawing.Size(951, 82);
			this.listBox1.TabIndex = 0;
			this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
			// 
			// chart1
			// 
			this.chart1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.chart1.BackColor = System.Drawing.Color.Gainsboro;
			chartArea3.Name = "ChartArea1";
			this.chart1.ChartAreas.Add(chartArea3);
			legend2.Enabled = false;
			legend2.Name = "Legend1";
			this.chart1.Legends.Add(legend2);
			this.chart1.Location = new System.Drawing.Point(6, 6);
			this.chart1.Name = "chart1";
			this.chart1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.chart1.Size = new System.Drawing.Size(931, 385);
			this.chart1.TabIndex = 1;
			this.chart1.Text = "chart1";
			// 
			// timer1
			// 
			this.timer1.Enabled = true;
			this.timer1.Interval = 1000;
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// tabControl1
			// 
			this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Location = new System.Drawing.Point(12, 12);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(951, 662);
			this.tabControl1.TabIndex = 3;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.chart1);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(943, 636);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Live";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.historySummaryLabel);
			this.tabPage2.Controls.Add(this.label1);
			this.tabPage2.Controls.Add(this.historyDatapointsDownLabel);
			this.tabPage2.Controls.Add(this.historyDatapointsUpLabel);
			this.tabPage2.Controls.Add(this.historyDatapointsLabel);
			this.tabPage2.Controls.Add(this.reloadHistoryButton);
			this.tabPage2.Controls.Add(this.chart2);
			this.tabPage2.Controls.Add(this.hScrollBar1);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(943, 636);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "History";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// historySummaryLabel
			// 
			this.historySummaryLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.historySummaryLabel.AutoSize = true;
			this.historySummaryLabel.Location = new System.Drawing.Point(24, 480);
			this.historySummaryLabel.Name = "historySummaryLabel";
			this.historySummaryLabel.Size = new System.Drawing.Size(73, 13);
			this.historySummaryLabel.TabIndex = 15;
			this.historySummaryLabel.Text = "history sumary";
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(749, 471);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(91, 13);
			this.label1.TabIndex = 14;
			this.label1.Text = "History datapoints";
			// 
			// historyDatapointsDownLabel
			// 
			this.historyDatapointsDownLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.historyDatapointsDownLabel.Location = new System.Drawing.Point(647, 543);
			this.historyDatapointsDownLabel.Name = "historyDatapointsDownLabel";
			this.historyDatapointsDownLabel.Size = new System.Drawing.Size(32, 23);
			this.historyDatapointsDownLabel.TabIndex = 13;
			this.historyDatapointsDownLabel.Text = "↓";
			this.historyDatapointsDownLabel.UseVisualStyleBackColor = true;
			this.historyDatapointsDownLabel.Click += new System.EventHandler(this.historyDatapointsDownLabel_Click);
			// 
			// historyDatapointsUpLabel
			// 
			this.historyDatapointsUpLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.historyDatapointsUpLabel.Location = new System.Drawing.Point(647, 471);
			this.historyDatapointsUpLabel.Name = "historyDatapointsUpLabel";
			this.historyDatapointsUpLabel.Size = new System.Drawing.Size(32, 23);
			this.historyDatapointsUpLabel.TabIndex = 12;
			this.historyDatapointsUpLabel.Text = "↑";
			this.historyDatapointsUpLabel.UseVisualStyleBackColor = true;
			this.historyDatapointsUpLabel.Click += new System.EventHandler(this.historyDatapointsUpLabel_Click);
			// 
			// historyDatapointsLabel
			// 
			this.historyDatapointsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.historyDatapointsLabel.AutoSize = true;
			this.historyDatapointsLabel.Location = new System.Drawing.Point(644, 506);
			this.historyDatapointsLabel.Name = "historyDatapointsLabel";
			this.historyDatapointsLabel.Size = new System.Drawing.Size(35, 13);
			this.historyDatapointsLabel.TabIndex = 11;
			this.historyDatapointsLabel.Text = "label4";
			// 
			// reloadHistoryButton
			// 
			this.reloadHistoryButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.reloadHistoryButton.Location = new System.Drawing.Point(862, 607);
			this.reloadHistoryButton.Name = "reloadHistoryButton";
			this.reloadHistoryButton.Size = new System.Drawing.Size(75, 23);
			this.reloadHistoryButton.TabIndex = 2;
			this.reloadHistoryButton.Text = "Reload";
			this.reloadHistoryButton.UseVisualStyleBackColor = true;
			this.reloadHistoryButton.Click += new System.EventHandler(this.reloadHistoryButton_Click);
			// 
			// chart2
			// 
			this.chart2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			chartArea4.Name = "ChartArea1";
			this.chart2.ChartAreas.Add(chartArea4);
			this.chart2.Location = new System.Drawing.Point(6, 6);
			this.chart2.Name = "chart2";
			this.chart2.Size = new System.Drawing.Size(931, 415);
			this.chart2.TabIndex = 1;
			this.chart2.Text = "historyChart";
			// 
			// hScrollBar1
			// 
			this.hScrollBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.hScrollBar1.LargeChange = 1;
			this.hScrollBar1.Location = new System.Drawing.Point(3, 424);
			this.hScrollBar1.Maximum = 10000;
			this.hScrollBar1.Name = "hScrollBar1";
			this.hScrollBar1.Size = new System.Drawing.Size(937, 26);
			this.hScrollBar1.TabIndex = 0;
			this.hScrollBar1.ValueChanged += new System.EventHandler(this.hScrollBar1_ValueChanged);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(975, 774);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.listBox1);
			this.Name = "Form1";
			this.Text = "Form1";
			((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage2.ResumeLayout(false);
			this.tabPage2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.chart2)).EndInit();
			this.ResumeLayout(false);

        }

		#endregion

		//private DoubleBufferedDataGridView dataGridView1;
		private System.Windows.Forms.ListBox listBox1;
		private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.HScrollBar hScrollBar1;
		private System.Windows.Forms.DataVisualization.Charting.Chart chart2;
		private System.Windows.Forms.Button reloadHistoryButton;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button historyDatapointsDownLabel;
		private System.Windows.Forms.Button historyDatapointsUpLabel;
		private System.Windows.Forms.Label historyDatapointsLabel;
		private System.Windows.Forms.Label historySummaryLabel;
	}
}

