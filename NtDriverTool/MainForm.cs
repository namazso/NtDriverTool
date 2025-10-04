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

using System.ServiceProcess;
using NtCoreLib;
using Timer = System.Windows.Forms.Timer;

namespace NtDriverTool;

public partial class MainForm : Form
{
    private readonly BindingSource _bindingSource;
    private readonly ToolStripMenuItem _createKeyItem;
    private readonly ToolStripMenuItem _deleteKeyItem;
    private readonly DriverDataAggregator _driverDataAggregator;
    private readonly ToolStripMenuItem _editKeyItem;
    private readonly ContextMenuStrip _headerContextMenu;
    private readonly ToolStripMenuItem _infoItem;
    private readonly ToolStripMenuItem _loadDriverItem;
    private readonly ContextMenuStrip _rowContextMenu;
    private readonly ToolStripMenuItem _startServiceItem;
    private readonly ToolStripMenuItem _stopServiceItem;
    private readonly Timer _typeAheadTimer;
    private readonly ToolStripMenuItem _unloadDriverItem;
    private readonly ToolStripMenuItem _unloadDriverSuperItem;
    private bool _initialColumnSetupComplete;

    // Add variables for type-to-select functionality
    private string _typeAheadText = string.Empty;

    public MainForm()
    {
        InitializeComponent();

        // Enable keyboard events for the form
        KeyPreview = true;
        KeyDown += MainForm_KeyDown;

        // Initialize type-ahead timer
        _typeAheadTimer = new Timer();
        _typeAheadTimer.Interval = 1000; // 1 second timeout
        _typeAheadTimer.Tick += TypeAheadTimer_Tick;

        _driverDataAggregator = new DriverDataAggregator();
        _driverDataAggregator.Refresh();

        // Initialize binding source
        _bindingSource = new BindingSource();
        _bindingSource.DataSource = _driverDataAggregator.Drivers;

        // Configure the DataGridView
        driversGrid.AutoGenerateColumns = true;
        driversGrid.DataSource = _bindingSource;

        // Subscribe to the DataBindingComplete event to configure column sorting
        driversGrid.DataBindingComplete += DriversGrid_DataBindingComplete;

        // Set up context menu for column headers
        _headerContextMenu = new ContextMenuStrip();
        var selectColumnsItem = new ToolStripMenuItem("Select Columns...");
        selectColumnsItem.Click += SelectColumnsItem_Click;
        _headerContextMenu.Items.Add(selectColumnsItem);

        // Set up context menu for rows
        _rowContextMenu = new ContextMenuStrip();

        _infoItem = new ToolStripMenuItem("Details");
        _infoItem.Click += (_, _) =>
        {
            if (driversGrid.SelectedRows.Count <= 0)
                return;
            var driverDataEntry = driversGrid.SelectedRows[0].DataBoundItem as DriverDataAggregator.DriverDataEntry;

            // Show info dialog using the new DriverInfoForm
            using (var infoForm = new DriverInfoForm(driverDataEntry!))
            {
                infoForm.ShowDialog(this);
            }

            RefreshData();
        };
        _rowContextMenu.Items.Add(_infoItem);

        _editKeyItem = new ToolStripMenuItem("Edit service");
        _editKeyItem.Click += (_, _) =>
        {
            if (driversGrid.SelectedRows.Count <= 0)
                return;
            var driverDataEntry = driversGrid.SelectedRows[0].DataBoundItem as DriverDataAggregator.DriverDataEntry;
            try
            {
                // Create a service key for the driver
                using var key = NtKey.GetMachineKey().Open("SYSTEM\\CurrentControlSet\\Services")
                    .Open(driverDataEntry!.GetOriginalData().Identifier);
                DoEditKey(key);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Failed to edit key for driver: {e.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            RefreshData();
        };
        _rowContextMenu.Items.Add(_editKeyItem);

        _startServiceItem = new ToolStripMenuItem("Start service");
        _startServiceItem.Click += (_, _) =>
        {
            if (driversGrid.SelectedRows.Count <= 0)
                return;
            var driverDataEntry = driversGrid.SelectedRows[0].DataBoundItem as DriverDataAggregator.DriverDataEntry;
            try
            {
                var service = driverDataEntry!.GetOriginalData().Service!;
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMilliseconds(1000));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start service: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            RefreshData();
        };
        _rowContextMenu.Items.Add(_startServiceItem);

        _stopServiceItem = new ToolStripMenuItem("Stop service");
        _stopServiceItem.Click += (_, _) =>
        {
            if (driversGrid.SelectedRows.Count <= 0)
                return;
            var driverDataEntry = driversGrid.SelectedRows[0].DataBoundItem as DriverDataAggregator.DriverDataEntry;
            try
            {
                var service = driverDataEntry!.GetOriginalData().Service!;
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMilliseconds(1000));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to stop service: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            RefreshData();
        };
        _rowContextMenu.Items.Add(_stopServiceItem);

        _loadDriverItem = new ToolStripMenuItem("ZwLoadDriver");
        _loadDriverItem.Click += (_, _) =>
        {
            if (driversGrid.SelectedRows.Count <= 0)
                return;
            var driverDataEntry = driversGrid.SelectedRows[0].DataBoundItem as DriverDataAggregator.DriverDataEntry;
            try
            {
                NtSystemInfo.LoadDriver(driverDataEntry!.GetOriginalData().ServiceKey!.FullPath);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Failed to load driver: {e.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            RefreshData();
        };
        _rowContextMenu.Items.Add(_loadDriverItem);

        _unloadDriverItem = new ToolStripMenuItem("ZwUnloadDriver");
        _unloadDriverItem.Click += (_, _) =>
        {
            if (driversGrid.SelectedRows.Count <= 0)
                return;
            var driverDataEntry = driversGrid.SelectedRows[0].DataBoundItem as DriverDataAggregator.DriverDataEntry;
            try
            {
                NtSystemInfo.UnloadDriver(driverDataEntry!.GetOriginalData().ServiceKey!.FullPath);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Failed to unload driver: {e.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            RefreshData();
        };
        _rowContextMenu.Items.Add(_unloadDriverItem);

        _unloadDriverSuperItem = new ToolStripMenuItem("ZwUnloadDriver (Force)");
        _unloadDriverSuperItem.Click += (_, _) =>
        {
            if (driversGrid.SelectedRows.Count <= 0)
                return;
            var driverDataEntry = driversGrid.SelectedRows[0].DataBoundItem as DriverDataAggregator.DriverDataEntry;
            try
            {
                // Create a temporary service key for the driver
                var key = NtKey.GetMachineKey().Open("SYSTEM\\CurrentControlSet\\Services")
                    .Create(driverDataEntry!.GetOriginalData().Identifier);
                try
                {
                    NtSystemInfo.UnloadDriver(key.FullPath);
                }
                finally
                {
                    key.Delete();
                    key.Close();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Failed to force unload driver: {e.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            RefreshData();
        };
        _rowContextMenu.Items.Add(_unloadDriverSuperItem);

        _createKeyItem = new ToolStripMenuItem("Create key");
        _createKeyItem.Click += (_, _) =>
        {
            if (driversGrid.SelectedRows.Count <= 0)
                return;
            var driverDataEntry = driversGrid.SelectedRows[0].DataBoundItem as DriverDataAggregator.DriverDataEntry;
            try
            {
                // Create a service key for the driver
                var key = NtKey.GetMachineKey().Open("SYSTEM\\CurrentControlSet\\Services")
                    .Create(driverDataEntry!.GetOriginalData().Identifier);
                key.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show($"Failed to create key for driver: {e.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            RefreshData();
        };
        _rowContextMenu.Items.Add(_createKeyItem);

        _deleteKeyItem = new ToolStripMenuItem("Delete key");
        _deleteKeyItem.Click += (_, _) =>
        {
            if (driversGrid.SelectedRows.Count <= 0)
                return;
            var driverDataEntry = driversGrid.SelectedRows[0].DataBoundItem as DriverDataAggregator.DriverDataEntry;
            try
            {
                // Create a service key for the driver
                var key = NtKey.GetMachineKey().Open("SYSTEM\\CurrentControlSet\\Services")
                    .Open(driverDataEntry!.GetOriginalData().Identifier);
                key.Delete();
                key.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show($"Failed to delete key for driver: {e.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            RefreshData();
        };
        _rowContextMenu.Items.Add(_deleteKeyItem);

        driversGrid.MouseDown += DriversGrid_MouseDown;
    }

    private void TypeAheadTimer_Tick(object sender, EventArgs e)
    {
        // Reset search text after timeout
        _typeAheadTimer.Stop();
        _typeAheadText = string.Empty;
    }

    // Add this new method to handle the F5 key press
    private void MainForm_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.F5)
        {
            RefreshData();
            e.Handled = true;
            return;
        }

        // Handle type-ahead functionality
        if (e.KeyCode is >= Keys.A and <= Keys.Z or >= Keys.D0 and <= Keys.D9 or >= Keys.NumPad0 and <= Keys.NumPad9)
        {
            // Get the character corresponding to the key
            var keyChar = (char)e.KeyValue;

            // For numeric keypad, convert to number
            if (e.KeyCode is >= Keys.NumPad0 and <= Keys.NumPad9)
                keyChar = (char)(e.KeyCode - Keys.NumPad0 + '0');

            // Append the character to the search text
            _typeAheadText += keyChar;

            // Reset the timer
            _typeAheadTimer.Stop();
            _typeAheadTimer.Start();

            // Search for matching item
            FindAndSelectItem(_typeAheadText);

            e.Handled = true;
        }
    }

    private void FindAndSelectItem(string searchText)
    {
        if (string.IsNullOrEmpty(searchText) || driversGrid.Rows.Count == 0)
            return;

        // Case-insensitive search
        searchText = searchText.ToLower();

        foreach (DataGridViewRow row in driversGrid.Rows)
            if (row.DataBoundItem is DriverDataAggregator.DriverDataEntry entry)
                if (entry.Name.ToLower().StartsWith(searchText))
                {
                    // Select the row
                    driversGrid.ClearSelection();
                    row.Selected = true;

                    // Make sure the row is visible
                    if (row.Index < driversGrid.FirstDisplayedScrollingRowIndex ||
                        row.Index >= driversGrid.FirstDisplayedScrollingRowIndex + driversGrid.DisplayedRowCount(true))
                        driversGrid.FirstDisplayedScrollingRowIndex = row.Index;

                    // Update the context menu items based on the selection
                    UpdateRowContextMenuItems();

                    break;
                }
    }

    private void UpdateRowContextMenuItems()
    {
        if (driversGrid.SelectedRows.Count > 0)
        {
            var driverData = (driversGrid.SelectedRows[0].DataBoundItem as DriverDataAggregator.DriverDataEntry)!
                .GetOriginalData();
            if (driverData.Service != null)
            {
                var service = driverData.Service!;
                _startServiceItem.Enabled = service.Status == ServiceControllerStatus.Stopped;
                _stopServiceItem.Enabled = service.Status == ServiceControllerStatus.Running;
            }
            else
            {
                _startServiceItem.Enabled = false;
                _stopServiceItem.Enabled = false;
            }

            var notLoaded = driverData is { ModuleInformation: null, HasDriverObject: false };
            if (driverData.ServiceKey != null)
            {
                _loadDriverItem.Enabled = notLoaded;
                _unloadDriverItem.Enabled = !notLoaded;
                _unloadDriverSuperItem.Enabled = false;
                _createKeyItem.Enabled = false;
                _deleteKeyItem.Enabled = true;
                _editKeyItem.Enabled = true;
            }
            else
            {
                _loadDriverItem.Enabled = false;
                _unloadDriverItem.Enabled = false;
                _unloadDriverSuperItem.Enabled = true;
                _createKeyItem.Enabled = true;
                _deleteKeyItem.Enabled = false;
                _editKeyItem.Enabled = false;
            }
        }
        else
        {
            _startServiceItem.Enabled = false;
            _stopServiceItem.Enabled = false;
            _loadDriverItem.Enabled = false;
            _unloadDriverItem.Enabled = false;
            _unloadDriverSuperItem.Enabled = false;
        }
    }

    private void SelectColumnsItem_Click(object? sender, EventArgs e)
    {
        using var columnSelector = new ColumnSelectorForm(driversGrid.Columns);
        if (columnSelector.ShowDialog() == DialogResult.OK)
        {
            // The column visibility has been updated in the form
        }
    }

    private void DriversGrid_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right)
        {
            // Get the header rectangle to detect clicks on the header
            var headerRect = driversGrid.DisplayRectangle;
            headerRect.Height = driversGrid.ColumnHeadersHeight;

            if (headerRect.Contains(e.Location))
            {
                _headerContextMenu.Show(driversGrid, e.Location);
            }
            else
            {
                // Get the row under the mouse
                var hitTest = driversGrid.HitTest(e.X, e.Y);
                if (hitTest.Type == DataGridViewHitTestType.Cell)
                {
                    // Select the row
                    driversGrid.ClearSelection();
                    driversGrid.Rows[hitTest.RowIndex].Selected = true;

                    UpdateRowContextMenuItems();

                    // Show the row context menu
                    _rowContextMenu.Show(driversGrid, e.Location);
                }
            }
        }
    }

    private void DriversGrid_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
    {
        // Enable sorting for all columns
        foreach (DataGridViewColumn column in driversGrid.Columns)
        {
            // Only apply default column visibility on first load
            if (!_initialColumnSetupComplete)
                if (column.Name != "Name" && column.Name != "Status" && column.Name != "ImagePath")
                {
                    column.Visible = false;
                    continue;
                }

            column.SortMode = DataGridViewColumnSortMode.Automatic;
        }

        // Auto-size all visible columns based on content
        foreach (DataGridViewColumn column in driversGrid.Columns)
            if (column.Visible)
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

        // Mark initial setup as complete after first binding
        _initialColumnSetupComplete = true;
    }

    private void RefreshData()
    {
        // Save current position
        var firstDisplayedRowIndex = -1;
        if (driversGrid.FirstDisplayedScrollingRowIndex >= 0)
            firstDisplayedRowIndex = driversGrid.FirstDisplayedScrollingRowIndex;

        // Save current selection if any
        List<string> selectedDriverNames = [];
        foreach (DataGridViewRow row in driversGrid.SelectedRows)
            if (row.DataBoundItem is DriverDataAggregator.DriverDataEntry entry)
                // Use Name property as identifier
                selectedDriverNames.Add(entry.Name);

        // Refresh data
        _driverDataAggregator.Refresh();
        _bindingSource.ResetBindings(false);

        // Restore scroll position if previously saved
        if (firstDisplayedRowIndex >= 0 && firstDisplayedRowIndex < driversGrid.RowCount)
            driversGrid.FirstDisplayedScrollingRowIndex = firstDisplayedRowIndex;

        // Restore selection
        if (selectedDriverNames.Count > 0)
        {
            driversGrid.ClearSelection();
            foreach (DataGridViewRow row in driversGrid.Rows)
                if (row.DataBoundItem is DriverDataAggregator.DriverDataEntry entry &&
                    selectedDriverNames.Contains(entry.Name))
                    row.Selected = true;
        }
    }

    private void refreshButton_Click(object sender, EventArgs e)
    {
        RefreshData();
    }

    private void DoEditKey(NtKey key)
    {
        // Read existing registry values

        var imagePath = key.QueryValue("ImagePath", false).GetResultOrDefault(null)?.ToString();
        var displayName = key.QueryValue("DisplayName", false).GetResultOrDefault(null)?.ToString();
        var description = key.QueryValue("Description", false).GetResultOrDefault(null)?.ToString();
        var group = key.QueryValue("Group", false).GetResultOrDefault(null)?.ToString();
        uint? type = key.QueryValue("Type", false).GetResultOrDefault(null) is { } typeVal
            ? BitConverter.ToUInt32(typeVal.Data, 0)
            : null;
        uint? start = key.QueryValue("Start", false).GetResultOrDefault(null) is { } startVal
            ? BitConverter.ToUInt32(startVal.Data, 0)
            : null;
        uint? errorControl = key.QueryValue("ErrorControl", false).GetResultOrDefault(null) is { } errorControlVal
            ? BitConverter.ToUInt32(errorControlVal.Data, 0)
            : null;

        // Create and populate the dialog
        using var editDialog = new EditKeyForm();
        editDialog.KeyName = key.Name;
        editDialog.ImagePath = imagePath ?? string.Empty;
        editDialog.DisplayName = displayName ?? string.Empty;
        editDialog.Description = description ?? string.Empty;
        editDialog.Group = group ?? string.Empty;
        editDialog.Type = type ?? 1; // Default to kernel driver
        editDialog.Start = start ?? 3; // Default to demand start
        editDialog.ErrorControl = errorControl ?? 1; // Default to normal

        // Show the dialog and handle the result
        if (editDialog.ShowDialog() == DialogResult.OK)
        {
            // Update registry values
            if (editDialog.ImagePath != imagePath)
                key.SetValue("ImagePath", editDialog.ImagePath);

            if (!string.IsNullOrEmpty(editDialog.DisplayName))
            {
                if (displayName != editDialog.DisplayName)
                    key.SetValue("DisplayName", editDialog.DisplayName);
            }
            else if (displayName != null)
            {
                key.DeleteValue("DisplayName");
            }

            if (!string.IsNullOrEmpty(editDialog.Description))
            {
                if (description != editDialog.Description)
                    key.SetValue("Description", editDialog.Description);
            }
            else if (description != null)
            {
                key.DeleteValue("Description");
            }

            if (!string.IsNullOrEmpty(editDialog.Group))
            {
                if (group != editDialog.Group)
                    key.SetValue("Group", editDialog.Group);
            }
            else if (group != null)
            {
                key.DeleteValue("Group");
            }

            if (type != editDialog.Type)
                key.SetValue("Type", editDialog.Type);
            if (start != editDialog.Start)
                key.SetValue("Start", editDialog.Start);
            if (errorControl != editDialog.ErrorControl)
                key.SetValue("ErrorControl", editDialog.ErrorControl);
        }
    }

    private void createKeyButton_Click(object sender, EventArgs e)
    {
        using var dialog = new CreateKeyForm();
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            var keyName = dialog.KeyName;
            try
            {
                if (string.IsNullOrWhiteSpace(keyName))
                    throw new Exception("Key name cannot be empty");

                // Create a service key for the driver with the provided name
                using var key = NtKey.GetMachineKey()
                    .Open("SYSTEM\\CurrentControlSet\\Services")
                    .Create(keyName);
                DoEditKey(key);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create key: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            RefreshData();
        }
    }
}