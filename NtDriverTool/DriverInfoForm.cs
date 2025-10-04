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

public sealed class DriverInfoForm : Form
{
    public DriverInfoForm(DriverDataAggregator.DriverDataEntry driverData)
    {
        InitializeComponent();

        // Set form properties
        Text = $"Driver Details: {driverData.Name}";
        Size = new Size(600, 600);
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowIcon = false;
        ShowInTaskbar = false;
        FormBorderStyle = FormBorderStyle.FixedDialog;

        // Create property grid
        var propertyGrid = new PropertyGrid
        {
            Dock = DockStyle.Fill,
            SelectedObject = driverData,
            PropertySort = PropertySort.NoSort,
            HelpVisible = false,
            ToolbarVisible = false,
            CommandsVisibleIfAvailable = false
        };
        propertyGrid.DisabledItemForeColor = propertyGrid.ViewForeColor;

        // Create button panel
        var buttonPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 50
        };

        // Create OK button
        var okButton = new Button
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
            Location = new Point(buttonPanel.Width - 100, 15),
            Size = new Size(80, 25)
        };

        // Add controls to form
        buttonPanel.Controls.Add(okButton);
        Controls.Add(propertyGrid);
        Controls.Add(buttonPanel);

        AcceptButton = okButton;
    }

    private void InitializeComponent()
    {
        SuspendLayout();
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(600, 500);
        Name = "DriverInfoForm";
        ResumeLayout(false);
    }
}