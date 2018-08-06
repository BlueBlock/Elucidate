﻿using System.Windows.Forms;

namespace Elucidate.Controls
{
    partial class Recover
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnLoadFiles = new System.Windows.Forms.Button();
            this.btnRecoverFiles = new System.Windows.Forms.Button();
            this.btnClearFiles = new System.Windows.Forms.Button();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.timerTreeViewFill = new System.Windows.Forms.Timer(this.components);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.liveRunLogControl = new Elucidate.Controls.LiveRunLogControl();
            this.timerTreeViewRecover = new System.Windows.Forms.Timer(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.treeView1, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1113, 267);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.btnLoadFiles);
            this.flowLayoutPanel1.Controls.Add(this.btnRecoverFiles);
            this.flowLayoutPanel1.Controls.Add(this.btnClearFiles);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(4, 4);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1105, 36);
            this.flowLayoutPanel1.TabIndex = 3;
            // 
            // btnLoadFiles
            // 
            this.btnLoadFiles.Location = new System.Drawing.Point(4, 4);
            this.btnLoadFiles.Margin = new System.Windows.Forms.Padding(4);
            this.btnLoadFiles.Name = "btnLoadFiles";
            this.btnLoadFiles.Size = new System.Drawing.Size(192, 28);
            this.btnLoadFiles.TabIndex = 0;
            this.btnLoadFiles.Text = "View Recoverable Files";
            this.btnLoadFiles.UseVisualStyleBackColor = true;
            this.btnLoadFiles.Click += new System.EventHandler(this.btnLoadFiles_Click);
            // 
            // btnRecoverFiles
            // 
            this.btnRecoverFiles.Location = new System.Drawing.Point(204, 4);
            this.btnRecoverFiles.Margin = new System.Windows.Forms.Padding(4);
            this.btnRecoverFiles.Name = "btnRecoverFiles";
            this.btnRecoverFiles.Size = new System.Drawing.Size(151, 28);
            this.btnRecoverFiles.TabIndex = 1;
            this.btnRecoverFiles.Text = "Run Recovery";
            this.btnRecoverFiles.UseVisualStyleBackColor = true;
            this.btnRecoverFiles.Click += new System.EventHandler(this.btnRecoverFiles_Click);
            // 
            // btnClearFiles
            // 
            this.btnClearFiles.Location = new System.Drawing.Point(363, 4);
            this.btnClearFiles.Margin = new System.Windows.Forms.Padding(4);
            this.btnClearFiles.Name = "btnClearFiles";
            this.btnClearFiles.Size = new System.Drawing.Size(100, 28);
            this.btnClearFiles.TabIndex = 2;
            this.btnClearFiles.Text = "Clear";
            this.btnClearFiles.UseVisualStyleBackColor = true;
            // 
            // treeView1
            // 
            this.treeView1.CheckBoxes = true;
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.Location = new System.Drawing.Point(4, 48);
            this.treeView1.Margin = new System.Windows.Forms.Padding(4);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(1105, 216);
            this.treeView1.TabIndex = 4;
            this.treeView1.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterCheck);
            this.treeView1.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView1_NodeMouseDoubleClick);
            // 
            // timerTreeViewFill
            // 
            this.timerTreeViewFill.Tick += new System.EventHandler(this.timerTreeViewFill_Tick);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tableLayoutPanel1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.liveRunLogControl);
            this.splitContainer1.Size = new System.Drawing.Size(1113, 534);
            this.splitContainer1.SplitterDistance = 267;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 1;
            // 
            // liveRunLogControl
            // 
            this.liveRunLogControl.AutoSize = true;
            this.liveRunLogControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.liveRunLogControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.liveRunLogControl.IsRunning = false;
            this.liveRunLogControl.Location = new System.Drawing.Point(0, 0);
            this.liveRunLogControl.Margin = new System.Windows.Forms.Padding(5);
            this.liveRunLogControl.MinimumSize = new System.Drawing.Size(67, 62);
            this.liveRunLogControl.Name = "liveRunLogControl";
            this.liveRunLogControl.Size = new System.Drawing.Size(1113, 262);
            this.liveRunLogControl.TabIndex = 5;
            // 
            // timerTreeViewRecover
            // 
            this.timerTreeViewRecover.Interval = 250;
            this.timerTreeViewRecover.Tick += new System.EventHandler(this.timerTreeViewRecover_Tick);
            // 
            // Recover
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Recover";
            this.Size = new System.Drawing.Size(1113, 534);
            this.Load += new System.EventHandler(this.Recover_Load);
            this.Resize += new System.EventHandler(this.Recover_Resize);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button btnLoadFiles;
        private System.Windows.Forms.Button btnRecoverFiles;
        private System.Windows.Forms.Button btnClearFiles;
        private System.Windows.Forms.Timer timerTreeViewFill;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private LiveRunLogControl liveRunLogControl;
        private TreeView treeView1;
        private Timer timerTreeViewRecover;
    }
}
