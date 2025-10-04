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

public sealed class EditKeyForm : Form
{
    // Service type options
    private static readonly Dictionary<uint, string> ServiceTypes = new()
    {
        { 1, "Kernel Driver" },
        { 2, "File System Driver" },
        { 4, "Adapter" },
        { 8, "Recognizer Driver" },
        { 16, "Win32 Own Process" },
        { 32, "Win32 Share Process" },
        { 272, "Win32 Interactive Own Process" },
        { 288, "Win32 Interactive Share Process" }
    };

    // Service start options
    private static readonly Dictionary<uint, string> StartTypes = new()
    {
        { 0, "Boot" },
        { 1, "System" },
        { 2, "Auto" },
        { 3, "Demand" },
        { 4, "Disabled" }
    };

    // Error control options
    private static readonly Dictionary<uint, string> ErrorControlTypes = new()
    {
        { 0, "Ignore" },
        { 1, "Normal" },
        { 2, "Severe" },
        { 3, "Critical" }
    };

    private readonly ComboBox _cmbErrorControl;
    private readonly ComboBox _cmbStart;
    private readonly ComboBox _cmbType;
    private readonly TextBox _txtDescription;
    private readonly TextBox _txtDisplayName;
    private readonly TextBox _txtGroup;
    private readonly TextBox _txtImagePath;

    // Form controls
    private readonly TextBox _txtKeyName;

    public EditKeyForm()
    {
        _txtImagePath = new TextBox();
        _txtDisplayName = new TextBox();
        _txtDescription = new TextBox();
        _txtGroup = new TextBox();
        _cmbType = new ComboBox();
        _cmbStart = new ComboBox();
        _cmbErrorControl = new ComboBox();
        SuspendLayout();

        // Form setup
        Text = "Edit Service";
        Size = new Size(500, 400);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;

        var yPos = 0;
        const int xPad = 10;
        const int yPad = 30;
        const int labelWidth = 120;
        const int controlWidth = 485 - 2 * xPad;
        const int buttonWidth = 100;
        const int controlHeight = 24;

        // Key Name (read-only)
        var lblKeyName = new Label
        {
            Text = "Key Name:",
            Location = new Point(xPad, yPos),
            Size = new Size(labelWidth, controlHeight),
            TextAlign = ContentAlignment.MiddleLeft
        };

        Controls.Add(lblKeyName);
        yPos += controlHeight;

        _txtKeyName = new TextBox
        {
            Location = new Point(xPad, yPos),
            Size = new Size(controlWidth, controlHeight),
            ReadOnly = true
        };
        Controls.Add(_txtKeyName);
        yPos += yPad;

        // Image Path
        var lblImagePath = new Label
        {
            Text = "Image Path:",
            Location = new Point(xPad, yPos),
            Size = new Size(labelWidth, controlHeight),
            TextAlign = ContentAlignment.MiddleLeft
        };
        Controls.Add(lblImagePath);
        yPos += controlHeight;

        _txtImagePath = new TextBox
        {
            Location = new Point(xPad, yPos),
            Size = new Size(controlWidth - buttonWidth - 10, controlHeight)
        };
        Controls.Add(_txtImagePath);

        var btnBrowse = new Button
        {
            Text = "Browse...",
            Location = new Point(xPad + controlWidth - buttonWidth, yPos),
            Size = new Size(buttonWidth, controlHeight)
        };
        btnBrowse.Click += BtnBrowse_Click;
        Controls.Add(btnBrowse);
        yPos += yPad;

        // Display Name
        var lblDisplayName = new Label
        {
            Text = "Display Name:",
            Location = new Point(xPad, yPos),
            Size = new Size(labelWidth, controlHeight),
            TextAlign = ContentAlignment.MiddleLeft
        };
        Controls.Add(lblDisplayName);
        yPos += controlHeight;

        _txtDisplayName = new TextBox
        {
            Location = new Point(xPad, yPos),
            Size = new Size(controlWidth, controlHeight)
        };
        Controls.Add(_txtDisplayName);
        yPos += yPad;

        // Description
        var lblDescription = new Label
        {
            Text = "Description:",
            Location = new Point(xPad, yPos),
            Size = new Size(labelWidth, controlHeight),
            TextAlign = ContentAlignment.MiddleLeft
        };
        Controls.Add(lblDescription);
        yPos += controlHeight;

        _txtDescription = new TextBox
        {
            Location = new Point(xPad, yPos),
            Size = new Size(controlWidth, controlHeight)
        };
        Controls.Add(_txtDescription);
        yPos += yPad;

        // Group
        var lblGroup = new Label
        {
            Text = "Group:",
            Location = new Point(xPad, yPos),
            Size = new Size(labelWidth, controlHeight),
            TextAlign = ContentAlignment.MiddleLeft
        };
        Controls.Add(lblGroup);
        yPos += controlHeight;

        _txtGroup = new TextBox
        {
            Location = new Point(xPad, yPos),
            Size = new Size(controlWidth, controlHeight)
        };
        Controls.Add(_txtGroup);
        yPos += yPad;

        // Type
        var lblType = new Label
        {
            Text = "Service Type:",
            Location = new Point(xPad, yPos),
            Size = new Size(labelWidth, controlHeight),
            TextAlign = ContentAlignment.MiddleLeft
        };
        Controls.Add(lblType);
        yPos += controlHeight;

        _cmbType = new ComboBox
        {
            Location = new Point(xPad, yPos),
            Size = new Size(controlWidth, controlHeight),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        Controls.Add(_cmbType);
        yPos += yPad;

        // Start
        var lblStart = new Label
        {
            Text = "Start Type:",
            Location = new Point(xPad, yPos),
            Size = new Size(labelWidth, controlHeight),
            TextAlign = ContentAlignment.MiddleLeft
        };
        Controls.Add(lblStart);
        yPos += controlHeight;

        _cmbStart = new ComboBox
        {
            Location = new Point(xPad, yPos),
            Size = new Size(controlWidth, controlHeight),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        Controls.Add(_cmbStart);
        yPos += yPad;

        // Error Control
        var lblErrorControl = new Label
        {
            Text = "Error Control:",
            Location = new Point(xPad, yPos),
            Size = new Size(labelWidth, controlHeight),
            TextAlign = ContentAlignment.MiddleLeft
        };
        Controls.Add(lblErrorControl);
        yPos += controlHeight;

        _cmbErrorControl = new ComboBox
        {
            Location = new Point(xPad, yPos),
            Size = new Size(controlWidth, controlHeight),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        Controls.Add(_cmbErrorControl);
        yPos += yPad * 2;

        // OK and Cancel buttons
        var btnOk = new Button
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Location = new Point(Width - 2 * buttonWidth - 40, yPos),
            Size = new Size(buttonWidth, controlHeight)
        };
        Controls.Add(btnOk);

        var btnCancel = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Location = new Point(Width - buttonWidth - 20, yPos),
            Size = new Size(buttonWidth, controlHeight)
        };
        Controls.Add(btnCancel);

        AcceptButton = btnOk;
        CancelButton = btnCancel;

        btnOk.Click += (s, e) =>
        {
            // Save values before closing
            SaveFormValues();
        };

        // Make form taller to fit all controls
        Height = yPos + 80;

        Load += EditKeyForm_Load;
        ResumeLayout(false);

        PopulateComboBoxes();
    }

    // Properties for binding to form controls
    public string KeyName { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public uint Type { get; set; } = 1;
    public uint Start { get; set; } = 3;
    public uint ErrorControl { get; set; } = 1;

    private void EditKeyForm_Load(object sender, EventArgs e)
    {
        // Set form values from properties
        var keyNameControl = Controls.OfType<TextBox>().First();
        keyNameControl.Text = KeyName;
        _txtImagePath.Text = ImagePath;
        _txtDisplayName.Text = DisplayName;
        _txtDescription.Text = Description;
        _txtGroup.Text = Group;

        // Select appropriate combo box items
        SelectComboBoxItemByValue(_cmbType, Type);
        SelectComboBoxItemByValue(_cmbStart, Start);
        SelectComboBoxItemByValue(_cmbErrorControl, ErrorControl);
    }

    private void BtnBrowse_Click(object sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog();
        dialog.Filter = "Driver files (*.sys)|*.sys|All files (*.*)|*.*";
        dialog.Title = "Select Driver File";
        var key = _txtKeyName.Text;
        var imagePath = string.IsNullOrWhiteSpace(_txtImagePath.Text) ? null : _txtImagePath.Text;
        var browsePath = DriverDataAggregator.BuildFullDriverPath(imagePath, key);
        var systemRoot = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        if (browsePath.StartsWith("\\??\\"))
            browsePath = browsePath.Substring(4);
        else if (browsePath.StartsWith("\\SystemRoot\\"))
            browsePath = systemRoot + "\\" + browsePath.Substring(12);

        if (File.Exists(browsePath))
        {
            dialog.InitialDirectory = Path.GetDirectoryName(browsePath);
            dialog.FileName = Path.GetFileName(browsePath);
        }
        else if (Directory.Exists(browsePath))
        {
            dialog.InitialDirectory = browsePath;
        }
        else
        {
            dialog.InitialDirectory = systemRoot + "\\System32\\drivers";
        }

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            var newPath = dialog.FileName;
            if (newPath.StartsWith(systemRoot, StringComparison.OrdinalIgnoreCase))
                newPath = "\\SystemRoot\\" + newPath.Substring(systemRoot.Length).TrimStart('\\');
            else if (!newPath.StartsWith("\\"))
                newPath = "\\??\\" + newPath;

            _txtImagePath.Text = newPath;
        }
    }

    private void PopulateComboBoxes()
    {
        // Populate Type ComboBox
        foreach (var type in ServiceTypes)
            _cmbType.Items.Add(new ComboBoxItem(type.Value, type.Key));

        // Populate Start ComboBox
        foreach (var start in StartTypes)
            _cmbStart.Items.Add(new ComboBoxItem(start.Value, start.Key));

        // Populate ErrorControl ComboBox
        foreach (var errorControl in ErrorControlTypes)
            _cmbErrorControl.Items.Add(new ComboBoxItem(errorControl.Value, errorControl.Key));
    }

    private void SelectComboBoxItemByValue(ComboBox comboBox, uint value)
    {
        foreach (ComboBoxItem item in comboBox.Items)
            if (item.Value.Equals(value))
            {
                comboBox.SelectedItem = item;
                return;
            }

        // If no match found, select the first item
        if (comboBox.Items.Count > 0)
            comboBox.SelectedIndex = 0;
    }

    private void SaveFormValues()
    {
        // Save form values to properties
        ImagePath = _txtImagePath.Text;
        DisplayName = _txtDisplayName.Text;
        Description = _txtDescription.Text;
        Group = _txtGroup.Text;

        if (_cmbType.SelectedItem is ComboBoxItem typeItem)
            Type = (uint)typeItem.Value;

        if (_cmbStart.SelectedItem is ComboBoxItem startItem)
            Start = (uint)startItem.Value;

        if (_cmbErrorControl.SelectedItem is ComboBoxItem errorItem)
            ErrorControl = (uint)errorItem.Value;
    }

    // Helper class for combo box items
    private class ComboBoxItem
    {
        public ComboBoxItem(string text, object value)
        {
            Text = text;
            Value = value;
        }

        public string Text { get; }
        public object Value { get; }

        public override string ToString()
        {
            return Text;
        }
    }
}