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

/// <summary>
///     Dialog for creating a new registry key
/// </summary>
public sealed class CreateKeyForm : Form
{
    private readonly TextBox _keyNameTextBox;

    public CreateKeyForm()
    {
        // Form settings
        Text = "Create service key";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        Size = new Size(350, 150);
        AutoScaleMode = AutoScaleMode.Font;

        // Label
        var label = new Label
        {
            Text = "Enter new service name:",
            AutoSize = true,
            Location = new Point(12, 15)
        };

        // TextBox
        _keyNameTextBox = new TextBox
        {
            Location = new Point(12, 35),
            Width = 310,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        // OK Button
        var okButton = new Button
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Location = new Point(166, 70),
            Width = 75,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right
        };
        okButton.Click += (_, _) => Close();

        // Cancel Button
        var cancelButton = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Location = new Point(247, 70),
            Width = 75,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right
        };
        cancelButton.Click += (_, _) => Close();

        // Add controls to form
        Controls.Add(label);
        Controls.Add(_keyNameTextBox);
        Controls.Add(okButton);
        Controls.Add(cancelButton);

        // Set default button and cancel button
        AcceptButton = okButton;
        CancelButton = cancelButton;
    }

    public string KeyName => _keyNameTextBox.Text.Trim();
}