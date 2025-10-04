//  Copyright (c) 2025 namazso <admin@namazso.eu>
//  
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//  
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

namespace NtDriverTool;

partial class MainForm
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
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.driversGrid = new System.Windows.Forms.DataGridView();
        this.refreshButton = new System.Windows.Forms.Button();
        this.createKeyButton = new System.Windows.Forms.Button();
        ((System.ComponentModel.ISupportInitialize)(this.driversGrid)).BeginInit();
        this.SuspendLayout();
        // 
        // driversGrid
        // 
        this.driversGrid.AllowUserToAddRows = false;
        this.driversGrid.AllowUserToDeleteRows = false;
        this.driversGrid.AllowUserToOrderColumns = true;
        this.driversGrid.AllowUserToResizeRows = false;
        this.driversGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
        this.driversGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this.driversGrid.Location = new System.Drawing.Point(12, 41);
        this.driversGrid.MultiSelect = false;
        this.driversGrid.Name = "driversGrid";
        this.driversGrid.ReadOnly = true;
        this.driversGrid.RowHeadersVisible = false;
        this.driversGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this.driversGrid.Size = new System.Drawing.Size(954, 619);
        this.driversGrid.TabIndex = 0;
        // 
        // refreshButton
        // 
        this.refreshButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.refreshButton.Location = new System.Drawing.Point(864, 12);
        this.refreshButton.Name = "refreshButton";
        this.refreshButton.Size = new System.Drawing.Size(102, 23);
        this.refreshButton.TabIndex = 1;
        this.refreshButton.Text = "Refresh";
        this.refreshButton.UseVisualStyleBackColor = true;
        this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
        // 
        // createKeyButton
        // 
        this.createKeyButton.Location = new System.Drawing.Point(12, 12);
        this.createKeyButton.Name = "createKeyButton";
        this.createKeyButton.Size = new System.Drawing.Size(131, 23);
        this.createKeyButton.TabIndex = 2;
        this.createKeyButton.Text = "Create service key";
        this.createKeyButton.UseVisualStyleBackColor = true;
        this.createKeyButton.Click += new System.EventHandler(this.createKeyButton_Click);
        // 
        // MainForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(978, 672);
        this.Controls.Add(this.createKeyButton);
        this.Controls.Add(this.refreshButton);
        this.Controls.Add(this.driversGrid);
        this.Name = "MainForm";
        this.Text = "NtDriverTool";
        ((System.ComponentModel.ISupportInitialize)(this.driversGrid)).EndInit();
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.Button createKeyButton;

    private System.Windows.Forms.Button refreshButton;

    private System.Windows.Forms.DataGridView driversGrid;

    #endregion
}