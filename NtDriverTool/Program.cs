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

using NtCoreLib;
using NtCoreLib.Security.Token;

namespace NtDriverTool;

internal static class Program
{
    private static void TryEnablePrivilege(NtToken token, TokenPrivilegeValue privilege)
    {
        try
        {
            if (!token.SetPrivilege(privilege, PrivilegeAttributes.Enabled) && token.Elevated)
                MessageBox.Show($"Failed to enable {privilege} privilege", "NtDriverTool", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
        }
        catch (NtException e)
        {
            MessageBox.Show($"Unexpected error while enabling {privilege} privilege: {e.Status} ({e.Message})",
                "NtDriverTool", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private static void TryEnablePrivileges()
    {
        using var token = NtProcess.Current.OpenToken();
        TryEnablePrivilege(token, TokenPrivilegeValue.SeDebugPrivilege);
        TryEnablePrivilege(token, TokenPrivilegeValue.SeLoadDriverPrivilege);
    }


    /// <summary>
    ///     The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        TryEnablePrivileges();
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}