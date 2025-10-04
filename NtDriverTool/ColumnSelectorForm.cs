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

public sealed class ColumnSelectorForm : Form
{
    private readonly CheckedListBox _columnList;
    private readonly DataGridViewColumnCollection _columns;

    public ColumnSelectorForm(DataGridViewColumnCollection columns)
    {
        _columns = columns;

        Text = "Select Columns";
        Size = new Size(310, 400);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;

        _columnList = new CheckedListBox
        {
            Dock = DockStyle.Fill,
            CheckOnClick = true
        };

        var buttonPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 80
        };

        var okButton = new Button
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Location = new Point(130, 40),
            Size = new Size(75, 23)
        };
        okButton.Click += OkButton_Click;

        var cancelButton = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Location = new Point(210, 40),
            Size = new Size(75, 23)
        };

        var selectAllButton = new Button
        {
            Text = "Select All",
            Location = new Point(10, 10),
            Size = new Size(75, 23)
        };
        selectAllButton.Click += SelectAllButton_Click;

        var clearAllButton = new Button
        {
            Text = "Clear All",
            Location = new Point(10, 40),
            Size = new Size(75, 23)
        };
        clearAllButton.Click += ClearAllButton_Click;

        buttonPanel.Controls.Add(okButton);
        buttonPanel.Controls.Add(cancelButton);
        buttonPanel.Controls.Add(selectAllButton);
        buttonPanel.Controls.Add(clearAllButton);

        Controls.Add(_columnList);
        Controls.Add(buttonPanel);

        AcceptButton = okButton;
        CancelButton = cancelButton;

        LoadColumnData();
    }

    private void LoadColumnData()
    {
        // Add each column to the CheckedListBox
        foreach (DataGridViewColumn column in _columns)
            _columnList.Items.Add(column.HeaderText, column.Visible);
    }

    private void OkButton_Click(object? sender, EventArgs e)
    {
        // Apply column visibility settings
        var visibleIndex = 0;
        for (var i = 0; i < _columnList.Items.Count; i++)
        {
            var column = GetColumnByHeaderText(_columnList.Items[i].ToString());
            if (column != null)
            {
                column.Visible = _columnList.GetItemChecked(i);
                if (column.Visible)
                    column.DisplayIndex = visibleIndex++;
            }
        }
    }

    private void SelectAllButton_Click(object? sender, EventArgs e)
    {
        for (var i = 0; i < _columnList.Items.Count; i++)
            _columnList.SetItemChecked(i, true);
    }

    private void ClearAllButton_Click(object? sender, EventArgs e)
    {
        for (var i = 0; i < _columnList.Items.Count; i++)
            _columnList.SetItemChecked(i, false);
    }

    private DataGridViewColumn? GetColumnByHeaderText(string headerText)
    {
        return _columns.Cast<DataGridViewColumn>().FirstOrDefault(column => column.HeaderText == headerText);
    }
}